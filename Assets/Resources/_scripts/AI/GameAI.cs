using System;
using System.Collections.Generic;
using UnityEngine;

public enum AICommunicationState {
    NotInCommuncation,
    InCommunication
}

public class GameAI : MonoBehaviour {
    public Player player;
    public Maze maze;

    private CommunicationChannel currentCommChannel;

    private TextCommunicationChannel textCommChannel;
    private OneWayTextCommunication oneWayCommChannel;
    private RoomExitPathCommChannel roomExitCommChannel;

    private string receivedPlayerMessage;
    private IntVector2 playerCurrentCoords;

    private AICommunicationState aiCommState;

    private PlayerResponse playerResponse;

    private bool openingDone = false;

    private string[] aiLines = {
        "Welcome to The Maze.",
        "Are you prepared to die in here?",
        "Do you fear me?",
    };

    private string RandomAiLine {
        get {
            return aiLines[UnityEngine.Random.Range(0, aiLines.Length)];
        }
    }

    private void Start() {

        //init communcation channels 
        textCommChannel = CommunicationChannelFactory.Make2WayTextChannel() as TextCommunicationChannel;
        oneWayCommChannel = CommunicationChannelFactory.MakeOneWayTextChannel() as OneWayTextCommunication;
        roomExitCommChannel = CommunicationChannelFactory.MakeRoomExitPathChannel() as RoomExitPathCommChannel;

        if (textCommChannel == null) {
            Debug.Log("2 way was null");
        }
        if (oneWayCommChannel == null) {
            Debug.Log("1 way was null");
        }

        playerCurrentCoords = player.MazeCellCoords;

        //start out not in communcation
        aiCommState = AICommunicationState.NotInCommuncation;
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
            SendMessageToPlayer(GameLinesTextGetter.OpeningMonologue(), oneWayCommChannel);
            openingDone = true;
        }
        else if (playerCurrentCoords != player.MazeCellCoords) {
            playerCurrentCoords = player.MazeCellCoords;
            if (UnityEngine.Random.Range(0f, 1.0f) > 0.5) {
                //Debug.Log(player.MazeCellCoords.x + " " + player.MazeCellCoords.z);
                maze.CloseDoorsInCell(playerCurrentCoords);
                MakeTextRequestToPlayer();
            }
            else {
                AskPlayerToTouchCorners();
            }
        }

        //Debug.Log(aiCommState.ToString());
    }

    private void AskPlayerToTouchCorners() {
        Vector3 localRoomPos = maze.GetCellLocalPosition(playerCurrentCoords.x, playerCurrentCoords.z);

        var pointList = new List<Vector3> {
            localRoomPos + new Vector3(0.5f, 0, 0.5f),
            localRoomPos + new Vector3(-0.5f, 0, 0.5f),
            localRoomPos + new Vector3(0.5f, 0, -0.5f),
            localRoomPos + new Vector3(-0.5f, 0, -0.5f),
        };


        //testing by marking a specific corner with a sphere
        //GameObject testSphere = GameObject.Find("TestSphere");
        //testSphere.transform.parent = maze.transform;
        //testSphere.transform.localPosition = localRoomPos;
        //testSphere.GetComponent<SphereCollider>().enabled = false;


        PlayerPath pathToFollow = new PlayerPath(pointList, initWithListOrder: false);
        roomExitCommChannel.SetPathForPlayer(pathToFollow);
        SendMessageToPlayer("Touch all four corners of this room before moving on.", roomExitCommChannel);
    }

    private void MakeTextRequestToPlayer() {
        SendMessageToPlayer("multiple \n lines", textCommChannel);
    }
    

    /// <summary>
    /// sends the given string message to the player via the given channel. 
    /// Also sets current channel to be given channel.
    /// </summary>
    /// <param name="message"></param>
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
    /// Handle a response from a communcation channel (to be expanded)
    /// </summary>
    /// <param name="response"></param>
    private void HandleResponse(PlayerResponse response) {
        //do nothing if there was no text response
        if (response.responseStr != string.Empty) {
            TextResponseHandler();
        }
        else if (response.playerPath != null) {
            PathResponseHandler();
        }
        maze.OpenDoorsInCell(playerCurrentCoords);
    }

    /// <summary>
    /// Text response handler (to be expanded)
    /// </summary>
    private void TextResponseHandler() {
        if (playerResponse.responseStr == "yes") {
            SendMessageToPlayer("Good choice.", oneWayCommChannel);
        }
        else {
            SendMessageToPlayer(playerResponse.responseStr, oneWayCommChannel);
        }
    }

    private void PathResponseHandler() {
        if (playerResponse.playerPath.WereAllPointsTraversed()) {
            SendMessageToPlayer("Thank you for obeying.", oneWayCommChannel);
        }
        else {
            SendMessageToPlayer("Disobedience will not be tolerated.", oneWayCommChannel);
        }
    }
}