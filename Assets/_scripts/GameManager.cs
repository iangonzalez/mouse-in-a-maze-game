using UnityEngine;

public class GameManager : MonoBehaviour {

    public Maze mazePrefab;
    public Player playerPrefab;

    private Maze mazeInstance;
    private Player playerInstance;

	// Use this for initialization
	private void Start () {
        if (playerPrefab == null) {
            throw new System.Exception("Player prefab was null\n");
        }
        BeginGame();
	}
	
	// Update is called once per frame
	private void Update () {
	    if (Input.GetKeyDown(KeyCode.Space)) {
            RestartGame();
        }
	}

    private void BeginGame() {
        mazeInstance = Instantiate(mazePrefab) as Maze;
        mazeInstance.Generate();

        playerInstance = Instantiate(playerPrefab) as Player;
        
    }
    
    private void RestartGame() {
        StopAllCoroutines();
        Destroy(mazeInstance.gameObject);
        BeginGame();
    }
}
