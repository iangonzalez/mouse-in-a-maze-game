using System;
using UnityEngine;
using UnityEngine.UI;


public class TextCommunicationChannel : MonoBehaviour {
    public GameObject gameCanvas;
    public Text aiTextBoxPrefab;
    public InputField playerWordBoxPrefab;

    private Text aiTextBox;
    private InputField playerWordBox;

    void Awake() {
        gameCanvas = GameObject.Find("Canvas");
    }

    private void CreateTextBoxes() {
        Debug.Log("create text called");

        aiTextBox = Instantiate(aiTextBoxPrefab) as Text;
        playerWordBox = Instantiate(playerWordBoxPrefab) as InputField;

        aiTextBox.transform.SetParent(gameCanvas.transform, false);
        playerWordBox.transform.SetParent(gameCanvas.transform, false);
    }

    private void DestroyTextBoxes() {
        DestroyObject(playerWordBox.gameObject);
        DestroyObject(aiTextBox.gameObject);
    }

   

    public void StartCommunicationWithPlayer(Player player, string message) {
        //restrict players movements
        player.BeginTextCommunicationWithPlayer();

        CreateTextBoxes();

        if (aiTextBox == null || playerWordBox == null) {
            Debug.LogError("Could not find one of the text boxes for game AI to use.");
        }

        aiTextBox.text = message;
    }

    public string EndCommunicationWithPlayer(Player player) {
        string receivedPlayerMessage = playerWordBox.text;
        DestroyTextBoxes();
        player.EndTextCommunicationWithPlayer();
        return receivedPlayerMessage;
    }
}
