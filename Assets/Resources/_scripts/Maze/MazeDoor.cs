using UnityEngine;


public class MazeDoor : MazeCellEdge {
    private bool open = true;
    private GameObject door;

    void Start() {
        door = transform.Find("Door").gameObject;
    }    

    void FixedUpdate() {
        if (!open && door.transform.localPosition.y < 0.25f) {
            door.transform.localPosition += new Vector3(0, 0.01f, 0);
        }
        else if (open && door.transform.localPosition.y > -0.35) {
            door.transform.localPosition += new Vector3(0, -0.01f, 0);
        }
    }

    public void CloseDoor() {
        if (open) {
            open = false;
        }
    }

    public void OpenDoor() {
        if (!open) {
            open = true;
        }
    }
}

