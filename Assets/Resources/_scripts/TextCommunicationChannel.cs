using System;
using UnityEngine;

public class TextCommunicationChannel : CommunicationChannel {
    protected override void Update() {
        if (Input.GetKeyDown(KeyCode.Return) && ai != null && player != null) {
            commComplete = true;
        }
    }

    public override void StartCommunicationWithPlayer(Player player, GameAI ai, string message) {
        this.player = player;
        this.ai = ai;

        //restrict players movements
        player.BeginTextCommunicationWithPlayer();

        CreateTextBoxes();

        if (aiTextBox == null || playerWordBox == null) {
            Debug.LogError("Could not find one of the text boxes for game AI to use.");
        }

        aiTextBox.text = message;
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
        player.EndTextCommunicationWithPlayer();
        player = null;
        ai = null;
    }
}