using UnityEngine;

public class GameManager : MonoBehaviour {

    public Maze mazePrefab;
    public Player playerPrefab;

    private Maze mazeInstance;
    private Player playerInstance;
    private Camera mainCam;

    private IntVector2 exitCellCoords;

	// Use this for initialization
	private void Start () {
        mainCam = GetComponentInChildren<Camera>();
        BeginGame();
	}
	
	// Update is called once per frame
	private void Update () {
	    if (Input.GetKeyDown(KeyCode.Space)) {
            RestartGame();
        }
        else if (Input.GetKeyDown(KeyCode.Z)) {
            SwitchCameraView();
        }

        //check if player has reached the end
        if (playerInstance.InCell &&
            playerInstance.MazeCellCoords.x == exitCellCoords.x &&
            playerInstance.MazeCellCoords.z == exitCellCoords.z) {

            EndGame();
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
        Destroy(this);
    }
}
