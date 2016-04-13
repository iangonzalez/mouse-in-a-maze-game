using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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
    private StillnessTimedCommChannel stillnessTimedComm;

    private ObjectMover objectMover;

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

    private bool reactToPlayer = false;

    private void Start() {

        //init communcation channels 
        textCommChannel = CommunicationChannelFactory.Make2WayTextChannel() as TextCommunicationChannel;
        oneWayCommChannel = CommunicationChannelFactory.MakeOneWayTextChannel() as OneWayTextCommunication;
        roomExitCommChannel = CommunicationChannelFactory.MakeRoomExitPathChannel() as RoomExitPathCommChannel;
        oneWayTimedComm = CommunicationChannelFactory.MakeOneWayTimedChannel() as OneWayTimedCommChannel;
        stillnessTimedComm = CommunicationChannelFactory.MakeTimedStillnessChannel() as StillnessTimedCommChannel;

        //init object mover
        objectMover = ObjectMover.CreateObjectMover();

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

    //helper function to turn all of the requests in a given states GameLines directory into
    //Actions that execute the interaction as a generic text interchange.
    private IEnumerable<Action> CreateTextRequestActionList(string stateName) {
        string requestPath = string.Format("requests/text_requests/{0}/", stateName);
        foreach (var interchange in GameLinesTextGetter.ParseAllTextInterchangesInDir(requestPath)) {
            if (interchange != null) {
                yield return (() => ExecTextInterchange(interchange));
            }
        }
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

            //add the generic text requests found in the GameLines folder for this state as actions:
            perStateRequestActionList[state].AddRange(CreateTextRequestActionList(stateName));            
            

            //add the reaction methods to the list for reaction methods for each state
            perStateReactionList[state] = GetActionsByNameStart(aiMethods, stateName + "_Reaction_");
        }
    }
    #endregion


    //Main business logic. Contains update function (called every frame), code to initiate actions/communications
    //with player, handle responses, and change alignment states
    #region
    private float DistanceBetweenPlayerAndRoom(IntVector2 roomCoords) {
        return Vector3.Distance(maze.GetCellLocalPosition(roomCoords.x, roomCoords.z),
                                player.transform.localPosition);
    }

    private void ExecuteRandomAction(List<Action> possibleActions) {
        if (possibleActions.Count == 0) {
            NullAction();
            return;
        }

        int randIdx = rng.Next(0, possibleActions.Count);
        Action randAction = possibleActions[randIdx];
        randAction();

        //testing this feature: remove an action after its done so it cant happen again.
        possibleActions.RemoveAt(randIdx);
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
            maze.CloseDoorsInCell(playerCurrentCoords);
            //Friendly_Request_FeedTheBeast();
            //Friendly_Request_KillAChild();
            //Hostile_Reaction_LengthenHallways();
            Friendly_Reaction_AddGridLocationsToWalls();
            //Hostile_Reaction_LengthenPathToExit();
            //Friendly_Reaction_CreateShortcut();
            //Hostile_Reaction_TheBeastIsNear();
            //Hostile_Reaction_SpinTheMaze();
            //Neutral_Request_AskPlayerToStandStill();
            //SendMessageToPlayer(GameLinesTextGetter.OpeningMonologue(), oneWayCommChannel);
        }
        else if (playerCurrentCoords != player.MazeCellCoords && 
                 DistanceBetweenPlayerAndRoom(player.MazeCellCoords) < 0.3) {

            playerCurrentCoords = player.MazeCellCoords;
            if (!firstInterchangeDone) {
                Neutral_Request_AskPlayerToTouchCorners();
                firstInterchangeDone = true;
            }
            else {
                if (reactToPlayer) {
                    ExecuteRandomAction(perStateReactionList[aiAlignmentState]);
                    reactToPlayer = false;
                }
                else {
                    ExecuteRandomAction(perStateRequestActionList[aiAlignmentState]);

                    //on occasion, prompt a reaction from the AI on the next room
                    reactToPlayer = (UnityEngine.Random.Range(0, 1f) < 0.75f);
                }
            }
        }
    }

    /// <summary>
    /// Handle a response from a communcation channel (to be expanded)
    /// </summary>
    /// <param name="response"></param>
    private void HandleResponse(PlayerResponse response) {
        if (!openingDone) {
            openingDone = true;
            maze.OpenDoorsInCell(playerCurrentCoords);
        }


        //if there was no interchange, no response was expected, so do nothing
        if (currentInterchange == null) {
            return;
        }
        //otherwise, check response, change state, and respond as needed
        else {
            //reopen doors if they were closed
            maze.OpenDoorsInCell(playerCurrentCoords);

            ThreeState wasResponseCorrect = currentInterchange.CheckIfCorrectResponse(response);
            Debug.Log(wasResponseCorrect.ToBool());
            Debug.Log(response.responseStr);
            string responseText = currentInterchange.GetResponseToPlayerText(wasResponseCorrect.ToBool());
            SendMessageToPlayer(responseText, oneWayCommChannel);

            if (wasResponseCorrect != ThreeState.Neutral) {
                StateTransition(wasResponseCorrect.ToBool());
            }
            //reset current interchange to get caught by above conditional.
            currentInterchange = null;
        }
    }

    private void StateTransition(bool responseWasPositive) {
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

    /// <summary>
    /// Given a generic text interchange object, executes the interchange on the 2 way commchannel
    /// and closes doors on the player
    /// </summary>
    /// <param name="interchange"></param>
    private void ExecTextInterchange(GenericTextInterchange interchange) {
        maze.CloseDoorsInCell(playerCurrentCoords);

        currentInterchange = interchange;

        SendMessageToPlayer(currentInterchange.GetQuestionText(), textCommChannel);
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

    private void Neutral_Request_AskPlayerToStandStill() {
        maze.CloseDoorsInCell(playerCurrentCoords);
        currentInterchange = new StayStillInterchange(aiAlignmentState);
        SendMessageToPlayer(currentInterchange.GetQuestionText(), stillnessTimedComm);
    }


    #endregion

    //Hostile AI actions
    #region

    private void Hostile_Request_SendAngryMessage() {
        SendMessageToPlayer("I'm so angry at you!", oneWayCommChannel);
    }
    

    private void Hostile_Request_LockPlayerInRoom() {
        maze.CloseDoorsInCell(playerCurrentCoords);
        var interchange = new LockPlayerInRoomInterchange(aiAlignmentState);
        interchange.timeLocked = 5.0f;
        currentInterchange = interchange;

        oneWayTimedComm.SetTimeToWait(5.0f);

        SendMessageToPlayer(currentInterchange.GetQuestionText(), oneWayTimedComm);
    }

    private void Hostile_Request_NastyLimerickCompletion() {

    }

    private void Hostile_Reaction_TurnLightsRed() {
        maze.RemoveAllSignPosts();
        maze.TurnAllLightsRed();
    }

    private void Hostile_Reaction_SpinTheMaze() {
        oneWayTimedComm.SetTimeToWait(5.0f);
        SendMessageToPlayer(GameLinesTextGetter.SpinMazeText, oneWayTimedComm);

        Action<GameObject> onFinish = (obj => obj.GetComponent<Player>().UnfreezePlayer());

        player.FreezePlayer();
        bool success = objectMover.SpinObject(player.gameObject, 4750f, 300.0f, onFinish);

        if (!success) {
            Debug.LogError("ObjectMover failed to spin the Player, it was already busy.");
        }
    }

    private void Hostile_Reaction_LengthenHallways() {
        SendMessageToPlayer(GameLinesTextGetter.LengthenHallwaysText, oneWayCommChannel);
        maze.ChangeHallwayLength(maze.RoomSeparationDistance + 3.0f, player);
    }

    private void Hostile_Reaction_LengthenPathToExit() {
        MazeDirection? longcutDir = maze.LengthenPathToExitIfPossible(playerCurrentCoords);
        bool longcutPossible = longcutDir != null;

        oneWayTimedComm.SetTimeToWait(5.0f);
        SendMessageToPlayer(GameLinesTextGetter.LongcutText(longcutPossible), oneWayTimedComm);

        if (longcutPossible) {
            maze.AddSignpostToCell(playerCurrentCoords, longcutDir.GetValueOrDefault(), player.transform.localPosition);
        }
    }

    private void Hostile_Reaction_TheBeastIsNear() {
        maze.CloseDoorsInCell(playerCurrentCoords, doItInstantly: true);

        SendMessageToPlayer(GameLinesTextGetter.BeastIsNearText, oneWayCommChannel);

        //realign the object to normal rotation after shaking is done
        Action<GameObject> onFinish =
            (obj) => {
                    obj.transform.localRotation = Quaternion.LookRotation(obj.transform.forward);
                    obj.GetComponentInParent<Maze>().OpenDoorsInCell(playerCurrentCoords);
                };

        objectMover.ShakeObject(maze.GetCell(playerCurrentCoords).gameObject, 
                                new Vector3(0, 0, 1f), 30, 200f, 10f, onFinish);
    }

    private void Hostile_Reaction_GiveFalseHint() {
        SendMessageToPlayer(GameLinesTextGetter.FalseHintText, oneWayCommChannel);

        maze.AddSignpostToCell(playerCurrentCoords, MazeDirections.RandDirection, player.transform.localPosition);
    }

    #endregion

    //Friendly AI actions
    #region

    private void Friendly_Request_SendHappyMessage() {
        SendMessageToPlayer("I love you so much!", oneWayCommChannel);
    }

    private void Friendly_Reaction_GiveHint() {
        List<IntVector2> pathToExit = maze.GetPathToExit(playerCurrentCoords);

        SendMessageToPlayer("Have a hint. You've earned it.", oneWayCommChannel);
        MazeDirection wayToMove = (pathToExit[1] - playerCurrentCoords).ToDirection();
        maze.AddSignpostToCell(playerCurrentCoords, wayToMove, player.transform.localPosition);
    }

    private void Friendly_Reaction_AddGridLocationsToWalls() {
        SendMessageToPlayer("The coordinates of each cell may help you navigate, friend.", oneWayCommChannel);
        maze.AddCoordsToAllCells();
    }

    private void Friendly_Reaction_CreateShortcut() {
        MazeDirection? shortcutDir = maze.CreateShortcutIfPossible(playerCurrentCoords);
        bool shortcutPossible = shortcutDir != null;

        oneWayTimedComm.SetTimeToWait(5.0f);
        SendMessageToPlayer(GameLinesTextGetter.ShortcutText(shortcutPossible), oneWayTimedComm);

        if (shortcutPossible) {
            maze.AddSignpostToCell(playerCurrentCoords, shortcutDir.GetValueOrDefault(), player.transform.localPosition);
        }
    }
    
    #endregion
}