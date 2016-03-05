using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class PathCommuncationChannel : CommunicationChannel {
    PlayerPath playerPath = null;
    IntVector2 playerCoords;

    float waitTimeBeforeResponseCheck = 1.0f;
    protected bool safeToCheckResponse = false;

    public void SetPathForPlayer(PlayerPath path) {
        playerPath = path;
    }

    public void SetPathForPlayer(List<Vector3> path) {
        playerPath = new PlayerPath(path, initWithListOrder: false);
    }

    protected bool IsPlayerNearPoint(Vector3 point, float maxDistance) {
        if (Vector3.Distance(point, player.transform.localPosition) < maxDistance) {
            return true;
        }
        return false;
    }

    protected void TraverseAnyClosePoint() {
        foreach (var pt in playerPath.pointList) {
            if (playerPath.pointOrder[pt] == -1 && IsPlayerNearPoint(pt, 0.3f)) {
                playerPath.TraversePointInPath(pt);
            }
        }
    }

    protected override void Update() {
        if (!safeToCheckResponse) {
            waitTimeBeforeResponseCheck -= Time.deltaTime;

            if (waitTimeBeforeResponseCheck <= 0) {
                safeToCheckResponse = true;
            }
        }
        

        TraverseAnyClosePoint();
    }

    public override void StartCommunicationWithPlayer(Player player, GameAI ai, string message) {
        waitTimeBeforeResponseCheck = 1.0f;
        safeToCheckResponse = false;

        enabled = true;

        this.player = player;
        this.ai = ai;

        //Do NOT restrict players movements
        //player.BeginTextCommunicationWithPlayer();

        playerCoords = player.MazeCellCoords;

        CreateTextBoxes(withPlayerWordBox : false, withContinuePrompt : false);

        if (aiTextBox == null) {
            Debug.LogError("Could not find one of the text boxes for game AI to use.");
        }

        if (playerPath == null) {
            throw new InvalidOperationException("Cannot start path comm channel with null player path. Call SetPathForPlayer() before StartCommuncationWithPlayer()");
        }

        aiTextBox.text = message;        
    }

    public override PlayerResponse GetResponse() {
        return new PlayerResponse(playerPath);
    }

    public override void EndCommuncation() {
        commComplete = false;
        DestroyTextBoxes();

        //reset relevant fields
        playerPath = null;
        player = null;
        ai = null;
        enabled = false;
    }
}

