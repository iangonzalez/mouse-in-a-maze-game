using UnityEngine;

public class StillnessTimedCommChannel : TimedCommChannel {
    protected Vector3 initialPlayerPos;

    protected override void Update() {
        if (Input.GetKeyDown(KeyCode.Return) && ai != null && player != null) {
            if (lineIdx < messageLines.Length) {
                DisplayNextLine();
            }
            else {
                player.UnfreezePlayer(); //this line is changed from parent class
                DestroyTextBoxes();
            }
        }

        timeToWait -= Time.deltaTime;
        if (timeToWait <= 0) {
            commComplete = true;
        }

    }

    public override void StartCommunicationWithPlayer(Player player, GameAI ai, string message) {
        if (timeToWait <= 0) {
            //default to 10 seconds wait time
            timeToWait = 10.0f;
        }

        InitializeChannelFields(player, ai);

        player.FreezePlayer();

        //change this in derived class if you want text input.
        CreateTextBoxes(withPlayerWordBox: false, withContinuePrompt: true);

        if (aiTextBox == null) {
            Debug.LogError("Could not find one of the text boxes for game AI to use.");
        }

        SplitMessageIntoLines(message);
        DisplayNextLine();

        //record initial position of the player
        initialPlayerPos = player.transform.localPosition;
    }

    public override PlayerResponse GetResponse() {
        if (initialPlayerPos.x != player.transform.localPosition.x ||
            initialPlayerPos.z != player.transform.localPosition.z) {
            return new PlayerResponse(true);
        }
        else {
            return new PlayerResponse(false);
        }
    }
}

