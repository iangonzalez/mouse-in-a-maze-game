using System;
using UnityEngine;

public enum AICommunicationState {
    NotInCommuncation,
    InCommunication
}

public class GameAI : MonoBehaviour {
    public Player player;
    public TextCommunicationChannel playerCommChannelPrefab;
    public OneWayTextCommunication oneWayCommChannelPrefab;

    private TextCommunicationChannel playerCommChannel;
    private OneWayTextCommunication oneWayCommChannel;
    private string receivedPlayerMessage;
    private IntVector2 playerCurrentCoords;

    private AICommunicationState aiCommState;

    private PlayerResponse playerResponse;

    void Start() {
        playerCommChannel = CommunicationChannelFactory.Make2WayTextChannel() as TextCommunicationChannel;
        oneWayCommChannel = CommunicationChannelFactory.MakeOneWayTextChannel() as OneWayTextCommunication;

        if (playerCommChannel == null) {
            Debug.Log("2 way was null");
        }
        if (oneWayCommChannel == null) {
            Debug.Log("1 way was null");
        }

        playerCurrentCoords = player.MazeCellCoords;
        aiCommState = AICommunicationState.NotInCommuncation;
    }

    void Update() {
        if (aiCommState == AICommunicationState.InCommunication && playerCommChannel.IsResponseReceived()) {
            playerResponse = playerCommChannel.GetResponse();
            playerCommChannel.EndCommuncation();
            aiCommState = AICommunicationState.NotInCommuncation;
            TextResponseHandler();
        }

        else if (playerCurrentCoords != player.MazeCellCoords) {
            playerCurrentCoords = player.MazeCellCoords;
            SendMessageToPlayer("Welcome to the maze.", playerCommChannel);
        }
    }

    private void SendMessageToPlayer(string message, CommunicationChannel channel) {
        if (channel == null) {
            throw new Exception("channel was null");
        }
        aiCommState = AICommunicationState.InCommunication;
        channel.StartCommunicationWithPlayer(player, this, message);        
    }

    private void HandleResponse() {

    }

    private void TextResponseHandler() {
        if (playerResponse.responseStr == "yes") {
            SendMessageToPlayer("Good choice.", oneWayCommChannel);
        }
        else {
            SendMessageToPlayer("Bad idea, bucko.", oneWayCommChannel);
        }
    }
}