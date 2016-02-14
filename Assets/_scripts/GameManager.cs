using UnityEngine;

public class GameManager : MonoBehaviour {

    public Maze mazePrefab;

    private Maze mazeInstance;

	// Use this for initialization
	private void Start () {
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
    }
    
    private void RestartGame() {
        StopAllCoroutines();
        Destroy(mazeInstance.gameObject);
        BeginGame();
    }
}
