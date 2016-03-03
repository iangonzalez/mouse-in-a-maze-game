using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This is the base class for all communication channel types that pass information between the Game AI and
/// the Player. It handles UI elements such as text boxes and input boxes and provides abstract methods for
/// starting communication, querying communcation status, and getting the response.
/// </summary>
public abstract class CommunicationChannel : MonoBehaviour {
    public GameObject gameCanvas;
    public Text aiTextBoxPrefab;
    public InputField playerWordBoxPrefab;

    protected GameAI ai;
    protected Player player;

    protected Text aiTextBox = null;
    protected Text continueTextBox = null;
    protected InputField playerWordBox = null;

    //internally track whether the communcation has finished
    protected bool commComplete = false;

    //text to prompt player to hit enter
    protected const string continueMessage = "  [Hit ENTER to continue]";

    //methods that have to be implemented by children (provide interface for interaction, see GameAI.cs)
    protected abstract void Update();
    abstract public void StartCommunicationWithPlayer(Player player, GameAI ai, string message);
    abstract public bool IsResponseReceived();
    abstract public PlayerResponse GetResponse();
    abstract public void EndCommuncation();

    void Awake() {
        gameCanvas = GameObject.Find("Canvas");
        ai = null;
        player = null;
        enabled = false;
    }

    /// <summary>
    /// Creates the ai text box containing its message, as well as the player's input box depending on parameter.
    /// </summary>
    /// <param name="withPlayerWordBox"></param>
    protected void CreateTextBoxes(bool withPlayerWordBox = true, bool withContinuePrompt = true) {
        aiTextBox = Instantiate(aiTextBoxPrefab) as Text;
        aiTextBox.transform.SetParent(gameCanvas.transform, false);

        if (withContinuePrompt) {
            continueTextBox = (Instantiate(Resources.Load("prefabs/ContinueText")) as GameObject).GetComponent<Text>();
            continueTextBox.transform.SetParent(gameCanvas.transform, false);

            continueTextBox.text = continueMessage;
        }


        if (withPlayerWordBox) {
            playerWordBox = Instantiate(playerWordBoxPrefab) as InputField;
            playerWordBox.transform.SetParent(gameCanvas.transform, false);
        }
        
    }

    /// <summary>
    /// Destroy the text boxes created above
    /// </summary>
    protected void DestroyTextBoxes() {
        if (playerWordBox != null) {
            DestroyObject(playerWordBox.gameObject);
        }

        if (continueTextBox != null) {
            DestroyObject(continueTextBox.gameObject);
        }

        if (aiTextBox != null) {
            DestroyObject(aiTextBox.gameObject);
        }
    }
}

