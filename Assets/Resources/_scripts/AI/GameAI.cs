using System;
using System.Reflection;
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
    private bool firstInterchangeDone = false;

    private AiPlayerInterchange currentInterchange;

    private void Start() {

        //init communcation channels 
        textCommChannel = CommunicationChannelFactory.Make2WayTextChannel() as TextCommunicationChannel;
        oneWayCommChannel = CommunicationChannelFactory.MakeOneWayTextChannel() as OneWayTextCommunication;
        roomExitCommChannel = CommunicationChannelFactory.MakeRoomExitPathChannel() as RoomExitPathCommChannel;

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
            if (!firstInterchangeDone) {
                AskPlayerToTouchCorners(true);
                firstInterchangeDone = true;
            }
            else if (UnityEngine.Random.Range(0f, 1.0f) > 0.5) {
                AskPlayerToTouchCorners();                
            }
            else {
                MakeTextRequestToPlayer();
            }
        }

        //Debug.Log(aiCommState.ToString());
    }

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

    public void AskPlayerToTouchCorners(bool firstRequest = false) {
        var pathToFollow = GetPlayerCornerPath();
        var cornerInterchange = new TouchCornersInterchange(new PlayerResponse(pathToFollow, false), firstRequest);
        RequestPlayerToFollowPath(cornerInterchange, roomExitCommChannel);
    }

    private void RequestPlayerToFollowPath(PathInterchange pathInterchange, PathCommuncationChannel channel) {
        currentInterchange = pathInterchange;
        channel.SetPathForPlayer(pathInterchange.expectedResponse.playerPath);
        SendMessageToPlayer(pathInterchange.GetQuestionText(), channel);
        
    }

    public void MakeTextRequestToPlayer() {
        currentInterchange = new TextOnlyInterchange();
        SendMessageToPlayer(currentInterchange.GetQuestionText(), textCommChannel);
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
        //do nothing if there was no text response expected
        if (currentCommChannel.GetType() == typeof(OneWayTextCommunication)) {
            return;
        }

        bool wasResponseCorrect = currentInterchange.CheckIfCorrectResponse(response);
        SendMessageToPlayer(currentInterchange.GetResponseToPlayerText(wasResponseCorrect), oneWayCommChannel);

        maze.OpenDoorsInCell(playerCurrentCoords);
    }
}