using UnityEngine;

public class MazeHallway : MonoBehaviour
{
    public void RotateHallway() {
        transform.localRotation = Quaternion.Euler(0, 90f, 0);
    }

    public void StretchHallway(Vector3 scale) {
        transform.localScale = scale;
        //Transform walls = transform.Find("walls");
        //walls.localScale = scale;

        Transform lamp = transform.Find("lamp_large");
        lamp.localScale = new Vector3(1.0f, scale.y, scale.z);
    }

    public void TurnLightsRed() {
        Light light = GetComponentInChildren<Light>();
        light.color = Color.red;
    }
}

