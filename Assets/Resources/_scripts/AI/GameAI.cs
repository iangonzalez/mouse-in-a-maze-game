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

    private Dictionary<AIAlignmentState, List<Action>> perStateActionList;

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

    /// <summary>
    /// Initialize the action lists for each alignment state. This gets every 0-param, void-returning method in the object
    /// that starts with the state's name (plus "_") and puts it in a list for that state.
    /// Neutral actions are added to every state.
    /// Note that ALL methods designated as state actions with a State + _ name must be void return type and take no arguments for this to work.
    /// </summary>
    private void InitializeActionLists() {
        perStateActionList = new Dictionary<AIAlignmentState, List<Action>>();

        //get the methods of this type
        var aiMethods = typeof(GameAI).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        //get the actions for Neutral first
        var neutralMethods = FilterMethodInfosByNameStart(aiMethods, "Neutral_");
        var neutralActions = GetActionListFromMethodInfos(neutralMethods);
        neutralActions.Add(NullAction);


        foreach (AIAlignmentState state in Enum.GetValues(typeof(AIAlignmentState))) {
            string stateName = state.ToString();
            
            //Neutral gets only neutral actions
            if (state == AIAlignmentState.Neutral) {
                perStateActionList[state] = neutralActions;
            }
            //all other states get actions that start with their name and Neutral
            else {
                var stateActions = GetActionListFromMethodInfos(FilterMethodInfosByNameStart(aiMethods, stateName + "_"));
                //perStateActionList[state] = new List<Action>(neutralActions.Concat(stateActions));
                perStateActionList[state] = stateActions;
            }
            
        }
    }
    #endregion


    //Main business logic. Contains update function (called every frame), code to initiate actions/communications
    //with player, handle responses, and change alignment states
    #region
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
            maze.CloseDoorsInCell(playerCurrentCoords);
            SendMessageToPlayer(GameLinesTextGetter.OpeningMonologue(), oneWayCommChannel);
            openingDone = true;
        }
        else if (playerCurrentCoords != player.MazeCellCoords) {
            playerCurrentCoords = player.MazeCellCoords;
            if (!firstInterchangeDone) {
                Neutral_AskPlayerToTouchCorners();
                firstInterchangeDone = true;
            }
            else {
                var possibleActions = perStateActionList[aiAlignmentState];
                int randIdx = rng.Next(0, possibleActions.Count);
                //Debug.LogError(possibleActions.Count);
                //Debug.LogError(randIdx);

                Action randAction = possibleActions[randIdx];
                randAction();
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
        SendMessageToPlayer(currentInterchange.GetResponseToPlayerText(wasResponseCorrect.ToBool()), oneWayCommChannel);

        if (wasResponseCorrect != ThreeState.Neutral) {
            StateTransition(wasResponseCorrect.ToBool());
        }
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
    private void Neutral_AskPlayerToTouchCorners() {
        var pathToFollow = GetPlayerCornerPath();
        var cornerInterchange = new TouchCornersInterchange(aiAlignmentState,
                                                            new PlayerResponse(pathToFollow, false), 
                                                            !firstInterchangeDone);
        RequestPlayerToFollowPath(cornerInterchange, roomExitCommChannel);
    }

    private void Neutral_MakeTextRequestToPlayer() {
        maze.CloseDoorsInCell(playerCurrentCoords);
        currentInterchange = new TextOnlyInterchange(aiAlignmentState);
        SendMessageToPlayer(currentInterchange.GetQuestionText(), textCommChannel);
    }

    private void Neutral_LockPlayerInRoom() {
        maze.CloseDoorsInCell(playerCurrentCoords);
        var interchange = new LockPlayerInRoomInterchange(aiAlignmentState);
        interchange.timeLocked = 5.0f;
        currentInterchange = interchange;

        oneWayTimedComm.SetTimeToWait(5.0f);

        SendMessageToPlayer(currentInterchange.GetQuestionText(), oneWayTimedComm);
    }
    #endregion

    //Hostile AI actions
    #region

    private void Hostile_SendAngryMessage() {
        SendMessageToPlayer("I'm so angry at you!", oneWayCommChannel);
    }

    #endregion

    //Friendly AI actions
    #region

    private void Friendly_SendHappyMessage() {
        SendMessageToPlayer("I love you so much!", oneWayCommChannel);
    }

    #endregion
}