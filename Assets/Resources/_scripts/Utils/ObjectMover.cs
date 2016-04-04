using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

public enum MovementType {
    None,
    Spin
}

public class ObjectMover : MonoBehaviour {
    private GameObject objToMove = null;
    private MovementType movement;

    private float degreesToSpin;
    private float rate;

    private Action<GameObject> onFinish;

    public static ObjectMover CreateObjectMover() {
        var obj = Instantiate(Resources.Load("prefabs/ObjectMover")) as GameObject;

        if (obj == null) {
            Debug.LogError("prefab instantiation of object mover was null");
            return null;
        }

        return obj.GetComponent<ObjectMover>();
    }

    private void ResetFields() {
        objToMove = null;
        onFinish = null;
        movement = MovementType.None;

        degreesToSpin = 0f;
        rate = 1.0f;
    }

    void Update() {
        if (movement == MovementType.Spin && objToMove != null) {
            float degreeMovement = Time.deltaTime * rate;
            degreesToSpin -= degreeMovement;
            if (degreesToSpin <= 0) {
                if (onFinish != null) {
                    onFinish(objToMove);
                }

                ResetFields();
            }
            else {
                objToMove.transform.Rotate(new Vector3(0, degreeMovement, 0));
            }
        }
    }

    //return a bool saying if the mover was busy already or not
    public bool SpinObject(GameObject obj, float degrees, float rate, Action<GameObject> onFinish = null) {
        if (objToMove != null) {
            return false;
        }
        

        objToMove = obj;
        movement = MovementType.Spin;
        degreesToSpin = degrees;
        this.rate = rate;
        this.onFinish = onFinish;

        return true;
    }
}