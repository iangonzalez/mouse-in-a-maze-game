using UnityEngine;
using System.Collections.Generic;

public class MazeCell : MonoBehaviour {
    public IntVector2 coordinates;
    public int initedEdgeCount = 0;

    public MazeExitLadder exitLadderPrefab;

    private MazeCellEdge[] edges = new MazeCellEdge[MazeDirections.DirectionCount];

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
        light.color = Color.red;
    }

    public void MakeThisExitCell() {
        MazeExitLadder exitLadder = Instantiate(exitLadderPrefab) as MazeExitLadder;
        exitLadder.transform.parent = transform;
        exitLadder.transform.localPosition = new Vector3(0, 0, 0);

        GameObject cellLamp = transform.Find("lamp_small").gameObject;
        Destroy(cellLamp);
    }
}
