using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GraphClasses;


public class MazeDoor : MazeCellEdge {
    private bool open = true;
    private GameObject door;

    void Start() {
        door = transform.Find("Door").gameObject;
    }    

    void FixedUpdate() {
        if (!open && door.transform.localPosition.y < 0.25f) {
            //Debug.Log("doing this thing"); 
            door.transform.localPosition += new Vector3(0, 0.01f, 0);
        }
        else if (open && door.transform.localPosition.y > -0.35) {
            door.transform.localPosition += new Vector3(0, -0.01f, 0);
        }
    }

    public void CloseDoor() {
        Debug.Log("calling close door in exactly this object");
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

