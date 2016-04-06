using UnityEngine;

public abstract class MazeCellEdge : MonoBehaviour {
    public MazeCell cell, otherCell;
    public MazeDirection direction;

    public void Initialize(MazeCell cell,  MazeCell otherCell, MazeDirection dir) {
        this.cell = cell;
        this.otherCell = otherCell;
        this.direction = dir;
        cell.SetEdge(dir, this);
        transform.parent = cell.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = dir.ToRotation();
        transform.localScale = new Vector3(1f, 1f, 1f);
    }
}
