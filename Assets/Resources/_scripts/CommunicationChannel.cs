using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class CommunicationChannel : MonoBehaviour {
    public GameObject gameCanvas;
    public Text aiTextBoxPrefab;
    public InputField playerWordBoxPrefab;

    protected GameAI ai;
    protected Player player;

    protected Text aiTextBox = null;
    protected InputField playerWordBox = null;

    protected bool commComplete = false;   

    void Awake() {
        gameCanvas = GameObject.Find("Canvas");
        ai = null;
        player = null;
    }

    protected void CreateTextBoxes(bool withPlayerWordBox = true) {
        aiTextBox = Instantiate(aiTextBoxPrefab) as Text;
        aiTextBox.transform.SetParent(gameCanvas.transform, false);

        if (withPlayerWordBox) {
            playerWordBox = Instantiate(playerWordBoxPrefab) as InputField;
            playerWordBox.transform.SetParent(gameCanvas.transform, false);
        }
        
    }

    protected void DestroyTextBoxes() {
        if (playerWordBox != null) {
            DestroyObject(playerWordBox.gameObject);
        }

        if (aiTextBox != null) {
            DestroyObject(aiTextBox.gameObject);
        }
    }

    protected abstract void Update();
    abstract public void StartCommunicationWithPlayer(Player player, GameAI ai, string message);
    abstract public bool IsResponseReceived();
    abstract public PlayerResponse GetResponse();
    abstract public void EndCommuncation();
}

