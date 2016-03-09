using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class TimedCommChannel : CommunicationChannel {
    protected float timeToWait = 0f;

    //complete communcation on enter
    protected override void Update() {
        if (Input.GetKeyDown(KeyCode.Return) && ai != null && player != null) {
            if (lineIdx < messageLines.Length) {
                DisplayNextLine();
            }
            else {
                DestroyTextBoxes();
            }
        }

        timeToWait -= Time.deltaTime;
        if (timeToWait <= 0) {
            commComplete = true;
        }
        
    }

    public void SetTimeToWait(float seconds) {
        timeToWait = seconds;
    }

    public override void StartCommunicationWithPlayer(Player player, GameAI ai, string message) {
        if (timeToWait <= 0) {
            //default to 10 seconds wait time
            timeToWait = 10.0f;
        }

        InitializeChannelFields(player, ai);

        //change this in derived class if you want text input.
        CreateTextBoxes(withPlayerWordBox: false, withContinuePrompt: true);

        if (aiTextBox == null) {
            Debug.LogError("Could not find one of the text boxes for game AI to use.");
        }

        SplitMessageIntoLines(message);
        DisplayNextLine();
    }

    public override bool IsResponseReceived() {
        return commComplete;
    }


    public override void EndCommuncation() {
        commComplete = false;
        DestroyTextBoxes();

        //reset these fields so channel is dead
        player = null;
        ai = null;
        enabled = false;
    }
}