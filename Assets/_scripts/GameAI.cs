using System;
using UnityEngine;

public class GameAI : MonoBehaviour {
    public Player player;
    public TextCommunicationChannel playerCommChannelPrefab;

    private TextCommunicationChannel playerCommChannel;
    private string receivedPlayerMessage;
    private IntVector2 playerCurrentCoords;

    void Start() {
        playerCommChannel = Instantiate(playerCommChannelPrefab) as TextCommunicationChannel;
        playerCurrentCoords = player.MazeCellCoords;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            ReceivePlayerResponse();
        }
        if (playerCurrentCoords != player.MazeCellCoords) {
            playerCurrentCoords = player.MazeCellCoords;
            SendMessageToPlayer("Welcome to the maze.");
        }
    }

    private void SendMessageToPlayer(string message) {
        playerCommChannel.StartCommunicationWithPlayer(player, message);
    }

    public void ReceivePlayerResponse() {
        receivedPlayerMessage = playerCommChannel.EndCommunicationWithPlayer(player);
    }

    
}