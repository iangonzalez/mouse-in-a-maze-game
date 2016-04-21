using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public Maze mazePrefab;
    public Player playerPrefab;
    public GameAI gameAiPrefab;
    public TextCommunicationChannel commChannel;

    public Text instructionText;

    private Maze mazeInstance = null;
    private Player playerInstance = null;
    private GameAI gameAiInstance = null;
    private TextCommunicationChannel commChannelInstance;
    private Camera mainCam;

    private IntVector2 exitCellCoords;

    public Text aiText;
    public InputField playerWordBox;

    private ScreenFader screenFader;

	// Use this for initialization
	private void Start () {
        mainCam = GetComponentInChildren<Camera>();
        BeginGame();
        screenFader = GetComponentInChildren<ScreenFader>();
    }
	
	// Update is called once per frame
	private void Update () {
	    if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.I)) {
            instructionText.enabled = !instructionText.enabled;
        }

        if (gameAiInstance != null && gameAiInstance.gameOver) {
            if (screenFader != null) {
                if (screenFader.enabled == false) {
                    screenFader.FadeScreenToBlack();
                }
                else if (screenFader.doneFading) {
                    Application.Quit();
                }   
            }
        }
	}

    private void SwitchCameraView() {
        if (mainCam.enabled) {
            mainCam.enabled = false;
            playerInstance.EnablePlayerCamera();
        }
        else {
            mainCam.enabled = true;
            playerInstance.DisablePlayerCamera();
        }
    }

    private void BeginGame() {
        mazeInstance = Instantiate(mazePrefab) as Maze;
        mazeInstance.Generate();
 
        playerInstance = Instantiate(playerPrefab) as Player;

        
        IntVector2 playerStartCoords = mazeInstance.PlacePlayerInMaze(playerInstance);
        exitCellCoords = mazeInstance.PlaceExitCell(playerStartCoords);

        gameAiInstance = Instantiate(gameAiPrefab) as GameAI;
        gameAiInstance.player = playerInstance;
        gameAiInstance.maze = mazeInstance;
        
        playerInstance.EnablePlayerCamera();
    }

    private void RestartGame() {
        StopAllCoroutines();
        Destroy(mazeInstance.gameObject);
        Destroy(playerInstance.gameObject);
        BeginGame();
    }

    private void EndGame() {
        Debug.Log("Game over!");
        StopAllCoroutines();
        Destroy(mazeInstance.gameObject);
        Destroy(playerInstance.gameObject);
        Destroy(gameAiInstance.gameObject);
        Destroy(this);
    }
}
