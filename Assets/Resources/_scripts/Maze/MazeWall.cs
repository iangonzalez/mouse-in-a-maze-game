using UnityEngine;

public class MazeWall : MazeCellEdge {
    public void AddCoordText(IntVector2 coords) {
        TextMesh textMesh = GetComponentInChildren<TextMesh>();
        textMesh.text = "X: " + coords.x + "\n" + "Z: " + coords.z;
    }
}
