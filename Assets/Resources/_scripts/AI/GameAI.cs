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
            Debug.Log("ai is in comm");
            if (currentCommChannel.IsResponseReceived()) {
                Debug.Log("got here");
                playerResponse = currentCommChannel.GetResponse();

                //end communcation and reset state
                currentCommChannel.EndCommuncation();
                aiCommState = AICommunicationState.NotInCommuncation;

                //handle whatever the response was
                HandleResponse(playerResponse);
            }
        }
        else if (playerCurrentCoords != player.MazeCellCoords) {
            playerCurrentCoords = player.MazeCellCoords;
            Vector3 localRoomPos = maze.GetCellLocalPosition(playerCurrentCoords.x, playerCurrentCoords.z);
            PlayerPath pathToFollow = new PlayerPath(new List<Vector3> { localRoomPos }, initWithListOrder: false);

            roomExitCommChannel.SetPathForPlayer(pathToFollow);
            SendMessageToPlayer(RandomAiLine, roomExitCommChannel);
        }

        //Debug.Log(aiCommState.ToString());
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
            Debug.Log("path wasnt null");
            PathResponseHandler();
        }
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
        if (playerResponse.playerPath.ArePointsInCorrectOrder()) {
            SendMessageToPlayer("Thank you for obeying.", oneWayCommChannel);
        }
        else {
            SendMessageToPlayer("Disobedience will not be tolerated.", oneWayCommChannel);
        }
    }
}