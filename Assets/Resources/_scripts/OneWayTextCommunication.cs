using System;
using UnityEngine;


public class OneWayTextCommunication : TextCommunicationChannel {
    public override void StartCommunicationWithPlayer(Player player, GameAI ai, string message) {
        this.player = player;
        this.ai = ai;

        //restrict players movements
        player.BeginTextCommunicationWithPlayer();

        CreateTextBoxes(withPlayerWordBox: false);

        if (aiTextBox == null) {
            Debug.LogError("Could not find one of the text boxes for game AI to use.");
        }

        aiTextBox.text = message;
    }
}

