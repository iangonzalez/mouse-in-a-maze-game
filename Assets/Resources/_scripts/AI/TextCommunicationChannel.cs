using System;
using UnityEngine;

/// <summary>
/// Text communcation channel (two way). Provides a text box where the player can send a response.
/// </summary>
public class TextCommunicationChannel : CommunicationChannel {

    //complete communcation on enter
    protected override void Update() {
        if (Input.GetKeyDown(KeyCode.Return) && ai != null && player != null) {
            commComplete = true;
        }
    }

    /// <summary>
    /// Create the text boxes, set up fields, and add the message text
    /// </summary>
    /// <param name="player"></param>
    /// <param name="ai"></param>
    /// <param name="message"></param>
    public override void StartCommunicationWithPlayer(Player player, GameAI ai, string message) {
        enabled = true;
        this.player = player;
        this.ai = ai;

        //restrict players movements
        player.BeginTextCommunicationWithPlayer();

        CreateTextBoxes();

        if (aiTextBox == null || playerWordBox == null) {
            Debug.LogError("Could not find one of the text boxes for game AI to use.");
        }

        aiTextBox.text = message;

        commComplete = false;
    }

    public override bool IsResponseReceived() {
        return commComplete;
    }

    public override PlayerResponse GetResponse() {
        return new PlayerResponse(playerWordBox.text);
    }

    public override void EndCommuncation() {
        commComplete = false;
        DestroyTextBoxes();

        //allow the player to move again
        player.EndTextCommunicationWithPlayer();

        //reset these fields so channel is dead
        player = null;
        ai = null;
        enabled = false;
    }
}