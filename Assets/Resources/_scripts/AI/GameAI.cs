using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AICommunicationState {
    NotInCommuncation,
    InCommunication
}

public enum AIAlignmentState {
    Neutral,
    Friendly,
    VeryFriendly,
    Hostile,
    VeryHostile
}

public class GameAI : MonoBehaviour {
    public Player player;
    public Maze maze;

    private CommunicationChannel currentCommChannel;

    private TextCommunicationChannel textCommChannel;
    private OneWayTextCommunication oneWayCommChannel;
    private RoomExitPathCommChannel roomExitCommChannel;
    private OneWayTimedCommChannel oneWayTimedComm;

    private string receivedPlayerMessage;
    private IntVector2 playerCurrentCoords;

    private AICommunicationState aiCommState;
    private AIAlignmentState aiAlignmentState;

    private PlayerResponse playerResponse;

    private bool openingDone = false;
    private bool firstInterchangeDone = false;

    private AiPlayerInterchange currentInterchange;

    private Dictionary<AIAlignmentState, List<Action>> perStateRequestActionList;
    private Dictionary<AIAlignmentState, List<Action>> perStateReactionList;

    private System.Random rng;

    private int numberOfInfractions = 0;

    private void Start() {

        //init communcation channels 
        textCommChannel = CommunicationChannelFactory.Make2WayTextChannel() as TextCommunicationChannel;
        oneWayCommChannel = CommunicationChannelFactory.MakeOneWayTextChannel() as OneWayTextCommunication;
        roomExitCommChannel = CommunicationChannelFactory.MakeRoomExitPathChannel() as RoomExitPathCommChannel;
        oneWayTimedComm = CommunicationChannelFactory.MakeOneWayTimedChannel() as OneWayTimedCommChannel;

        playerCurrentCoords = player.MazeCellCoords;

        //start out not in communcation
        aiCommState = AICommunicationState.NotInCommuncation;
        aiAlignmentState = AIAlignmentState.Neutral;

        //initialize list of possible actions (for each state)
        InitializeActionLists();

        rng = new System.Random();
    }

    //code to intialize the action lists for each state based on the AI's player-affecting methods.
    //consider refactoring this into a superclass that this derives from.
    #region
    //helper function to convert a list of methodInfo objects into Actions
    private List<Action> GetActionListFromMethodInfos(IEnumerable<MethodInfo> methodInfos) {
        return new List<Action> (methodInfos.Select(m => (Action)Delegate.CreateDelegate(typeof(Action), this, m, false)));
    }

    //helper function to filter a list of MethodInfo objects by name
    private IEnumerable<MethodInfo> FilterMethodInfosByNameStart(IEnumerable<MethodInfo> methodInfos, string nameStart) {
        return methodInfos.Where(m => m.Name.StartsWith(nameStart));
    }

    //helper function to combine the above 2 and add any extra needed actions to the list (only null action right now)
    private List<Action> GetActionsByNameStart(IEnumerable<MethodInfo> methodInfos, string nameStart) {
        var actions = GetActionListFromMethodInfos(FilterMethodInfosByNameStart(methodInfos, nameStart));
        actions.Add(NullAction);
        return actions;
    }

    /// <summary>
    /// Initialize the action lists for each alignment state. This gets every 0-param, void-returning method in the object
    /// that starts with the state's name (plus "_") and puts it in a list for that state.
    /// Neutral actions are added to every state.
    /// Note that ALL methods designated as state actions with a State + _ name must be void return type and take no arguments for this to work.
    /// </summary>
    private void InitializeActionLists() {
        perStateRequestActionList = new Dictionary<AIAlignmentState, List<Action>>();
        perStateReactionList = new Dictionary<AIAlignmentState, List<Action>>();

        //get the methods of this type
        var aiMethods = typeof(GameAI).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        
        foreach (AIAlignmentState state in Enum.GetValues(typeof(AIAlignmentState))) {
            string stateName = state.ToString();

            //add the request methods to the list for request methods for each state
            perStateRequestActionList[state] = GetActionsByNameStart(aiMethods, stateName + "_Request_");

            //add the reaction methods to the list for reaction methods for each state
            perStateReactionList[state] = GetActionsByNameStart(aiMethods, stateName + "_Reaction_");
        }
    }
    #endregion


    //Main business logic. Contains update function (called every frame), code to initiate actions/communications
    //with player, handle responses, and change alignment states
    #region
    private void ExecuteRandomAction(List<Action> possibleActions) {
        if (possibleActions.Count == 0) {
            NullAction();
            return;
        }

        int randIdx = rng.Next(0, possibleActions.Count);
        Action randAction = possibleActions[randIdx];
        randAction();
    }

    private void Update() {
        //if in communcation, check for response, call handler on response if there
        if (aiCommState == AICommunicationState.InCommunication) {
            if (currentCommChannel.IsResponseReceived()) {
                playerResponse = currentCommChannel.GetResponse();

                //end communcation and reset state
                currentCommChannel.EndCommuncation();
                aiCommState = AICommunicationState.NotInCommuncation;

                //handle whatever the response was
                HandleResponse(playerResponse);
            }
        }
        else if (!openingDone) {
            //maze.CloseDoorsInCell(playerCurrentCoords);
            //SendMessageToPlayer(GameLinesTextGetter.OpeningMonologue(), oneWayCommChannel);
            //Hostile_Reaction_TurnLightsRed();
            openingDone = true;
        }
        else if (playerCurrentCoords != player.MazeCellCoords) {
            playerCurrentCoords = player.MazeCellCoords;
            if (!firstInterchangeDone) {
                maze.AddSignpostToCell(playerCurrentCoords, MazeDirection.East);
                Neutral_Request_AskPlayerToTouchCorners();
                firstInterchangeDone = true;
            }
            else {
                ExecuteRandomAction(perStateRequestActionList[aiAlignmentState]);
            }
        }
    }

    /// <summary>
    /// Handle a response from a communcation channel (to be expanded)
    /// </summary>
    /// <param name="response"></param>
    private void HandleResponse(PlayerResponse response) {
        maze.OpenDoorsInCell(playerCurrentCoords);

        //do nothing if there was no text response expected
        if (currentCommChannel.GetType() == typeof(OneWayTextCommunication)) {
            return;
        }

        ThreeState wasResponseCorrect = currentInterchange.CheckIfCorrectResponse(response);
        string responseText = currentInterchange.GetResponseToPlayerText(wasResponseCorrect.ToBool());
        SendMessageToPlayer(responseText, oneWayCommChannel);

        if (wasResponseCorrect != ThreeState.Neutral) {
            StateTransition(wasResponseCorrect.ToBool());
        }
        
        ExecuteRandomAction(perStateReactionList[aiAlignmentState]);
    }

    private void StateTransition(bool responseWasPositive) {
        Debug.Log(numberOfInfractions);
        numberOfInfractions += (responseWasPositive ? -1 : 1);
        if (numberOfInfractions <= 2 && numberOfInfractions >= -2) {
            aiAlignmentState = AIAlignmentState.Neutral;
        }
        else if (numberOfInfractions < -2) {
            aiAlignmentState = AIAlignmentState.Friendly;
        }
        else if (numberOfInfractions > 2) {
            aiAlignmentState = AIAlignmentState.Hostile;
        }
    }
    #endregion

    // Ai player communcation helper functions. Functions that handle communicating with player through the AI's
    // communcation channels.
    #region
    private PlayerPath GetPlayerCornerPath() {
        Vector3 localRoomPos = maze.GetCellLocalPosition(playerCurrentCoords.x, playerCurrentCoords.z);

        var pointList = new List<Vector3> {
            localRoomPos + new Vector3(0.5f, 0, 0.5f),
            localRoomPos + new Vector3(-0.5f, 0, 0.5f),
            localRoomPos + new Vector3(0.5f, 0, -0.5f),
            localRoomPos + new Vector3(-0.5f, 0, -0.5f),
        };

        return new PlayerPath(pointList, initWithListOrder: false);
    }

    private void RequestPlayerToFollowPath(PathInterchange pathInterchange, PathCommuncationChannel channel) {
        currentInterchange = pathInterchange;
        channel.SetPathForPlayer(pathInterchange.expectedResponse.playerPath);
        SendMessageToPlayer(pathInterchange.GetQuestionText(), channel);

    }

    /// <summary>
    /// sends the given string message to the player via the given channel. 
    /// Also sets current channel to be given channel.
    /// </summary>
    /// <param name="message"></
    /// <param name="channel"></param>
    private void SendMessageToPlayer(string message, CommunicationChannel channel) {
        if (channel == null) {
            throw new Exception("channel was null");
        }
        aiCommState = AICommunicationState.InCommunication;
        currentCommChannel = channel;
        channel.StartCommunicationWithPlayer(player, this, message);
    }
    #endregion


    /// <summary>
    /// Below this point in the code are the AI's state specific high-level actions. The ai chooses between these
    /// actions randomly depending on its current alignment state.
    /// </summary>

    private void NullAction() {
        return;
    }

    // Neutral AI actions
    #region
    private void Neutral_Request_AskPlayerToTouchCorners() {
        var pathToFollow = GetPlayerCornerPath();
        var cornerInterchange = new TouchCornersInterchange(aiAlignmentState,
                                                            new PlayerResponse(pathToFollow, false), 
                                                            !firstInterchangeDone);
        RequestPlayerToFollowPath(cornerInterchange, roomExitCommChannel);
    }

    private void Neutral_Request_MakeTextRequestToPlayer() {
        maze.CloseDoorsInCell(playerCurrentCoords);
        currentInterchange = new TextOnlyInterchange(aiAlignmentState);
        SendMessageToPlayer(currentInterchange.GetQuestionText(), textCommChannel);
    }

    private void Neutral_Reaction_SayANeutralPhrase() {
        //TODO: Add code for the AI to say some banal thing.
    }

    
    #endregion

    //Hostile AI actions
    #region

    private void Hostile_Request_SendAngryMessage() {
        SendMessageToPlayer("I'm so angry at you!", oneWayCommChannel);
    }

    private void Hostile_Request_MakeTextRequestToPlayer() {
        Neutral_Request_MakeTextRequestToPlayer();
    }

    private void Hostile_Request_LockPlayerInRoom() {
        maze.CloseDoorsInCell(playerCurrentCoords);
        var interchange = new LockPlayerInRoomInterchange(aiAlignmentState);
        interchange.timeLocked = 5.0f;
        currentInterchange = interchange;

        oneWayTimedComm.SetTimeToWait(5.0f);

        SendMessageToPlayer(currentInterchange.GetQuestionText(), oneWayTimedComm);
    }

    private void Hostile_Reaction_TurnLightsRed() {
        maze.TurnAllLightsRed();
    }

    #endregion

    //Friendly AI actions
    #region

    private void Friendly_Request_SendHappyMessage() {
        SendMessageToPlayer("I love you so much!", oneWayCommChannel);
    }

    private void Friendly_Request_MakeTextRequestToPlayer() {
        Neutral_Request_MakeTextRequestToPlayer();
    }

    private void Friendly_Reaction_GiveHint() {
        //TODO: Code here to have the AI give a hint to the player.
        SendMessageToPlayer("Have a hint. You've earned it.", oneWayCommChannel);

    }
    
    #endregion
}