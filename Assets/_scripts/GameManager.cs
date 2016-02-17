using UnityEngine;

public class GameManager : MonoBehaviour {

    public Maze mazePrefab;
    public Player playerPrefab;

    private Maze mazeInstance;
    private Player playerInstance;
    private Camera mainCam;

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
        else if (Input.GetKeyDown(KeyCode.Q)) {
            SwitchCameraView();
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

        mazeInstance.PlacePlayerInMaze(playerInstance);
        playerInstance.EnablePlayerCamera();
    }
    
    private void RestartGame() {
        StopAllCoroutines();
        Destroy(mazeInstance.gameObject);
        Destroy(playerInstance.gameObject);
        BeginGame();
    }
}
