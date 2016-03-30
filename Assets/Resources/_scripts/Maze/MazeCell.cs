using UnityEngine;
using System.Collections.Generic;

public class MazeCell : MonoBehaviour {
    public IntVector2 coordinates;
    public int initedEdgeCount = 0;

    public MazeExitLadder exitLadderPrefab;
    public DirectionalSignPost signpostPrefab;

    private MazeCellEdge[] edges = new MazeCellEdge[MazeDirections.DirectionCount];

    private DirectionalSignPost signpostInstance;

    void Start() {
        signpostInstance = null;
    }

    public bool IsFullyInitialized {
        get {
            return initedEdgeCount >= MazeDirections.DirectionCount;
        }
    }

    public MazeDirection RandUninitializedDirection {
        get {
            List<int> uninitDirs = new List<int>();
            for (int i = 0; i < MazeDirections.DirectionCount; i++) {
                if (edges[i] == null) {
                    uninitDirs.Add(i);
                }
            }
            
            if (uninitDirs.Count == 0) {
                throw new System.Exception("No uninitialized direction when trying to get RandUninitializedDirection.");
            }

            return (MazeDirection)uninitDirs[Random.Range(0, uninitDirs.Count)];
        }
    }

    public void SetEdge(MazeDirection dir, MazeCellEdge edge) {
        this.edges[(int)dir] = edge;
        this.initedEdgeCount += 1;
    }

    public MazeCellEdge GetEdge(MazeDirection dir) {
        return this.edges[(int)dir];
    }

    public void TurnLightRed() {
        Light light = GetComponentInChildren<Light>();
        if (light != null) {
            light.color = Color.red;
        }
    }

    public void MakeThisExitCell() {
        MazeExitLadder exitLadder = Instantiate(exitLadderPrefab) as MazeExitLadder;
        exitLadder.transform.parent = transform;
        exitLadder.transform.localPosition = Vector3.zero;

        GameObject cellLamp = transform.Find("lamp_small").gameObject;
        Destroy(cellLamp);
    }

    //initialize a directional signpost pointing in direction dir and in a quadrant of the cell not
    //containing the player located at playerPosition.
    public void AddSignPost(MazeDirection dir, Vector3 playerPosition) {

        if (signpostInstance != null) {
            return;
        }

        signpostInstance = Instantiate(signpostPrefab) as DirectionalSignPost;
        signpostInstance.transform.parent = transform;

        Vector3 relativePos = playerPosition - transform.localPosition;

        signpostInstance.transform.localPosition = new Vector3(
            (relativePos.x <= 0 ? 0.25f : -0.25f), 
            -1.5f,
            (relativePos.z <= 0 ? 0.25f : -0.25f)
        );

        signpostInstance.transform.localRotation *= dir.ToRotation();
    }

    public void RemoveSignPost() {
        if (signpostInstance == null) {
            return;
        }
        else {
            Destroy(signpostInstance.gameObject);
            signpostInstance = null;
        }
    }
}
