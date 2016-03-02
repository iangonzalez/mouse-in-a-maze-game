using System;
using UnityEngine;

/// <summary>
/// Text communication channel (one way). Same as parent class, but no input box and empty response.
/// </summary>
public class OneWayTextCommunication : TextCommunicationChannel {
    public override void StartCommunicationWithPlayer(Player player, GameAI ai, string message) {
        enabled = true;

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

    public override PlayerResponse GetResponse() {
        return new PlayerResponse(string.Empty);
    }
}

