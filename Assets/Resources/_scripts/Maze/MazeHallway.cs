using UnityEngine;

public class MazeHallway : MonoBehaviour
{
    public bool isRotated = false;

    public void RotateHallway() {
        transform.localRotation *= Quaternion.Euler(0, 90f, 0);
        isRotated = !isRotated;
    }

    public void StretchHallway(Vector3 scale) {
        transform.localScale = scale;

        Transform lamp = transform.Find("lamp_large");
        lamp.localScale = new Vector3(1.0f, scale.y, 1.0f/scale.x);
    }

    public void TurnLightsRed() {
        Light light = GetComponentInChildren<Light>();
        light.color = Color.red;
    }
}

