using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

public enum MovementType {
    None,
    Spin,
    Shake,
    LineMovement,
}

public class ObjectMover : MonoBehaviour {
    private GameObject objToMove = null;
    private MovementType movement;

    private float degreesToSpin;
    private float rate;

    private Action<GameObject> onFinish;

    //shake fields
    private bool shakeLeft;
    private int shakeCount = 0;
    private Vector3 shakeAxis;
    private float perShakeDegrees;

    //move along line path fields
    private Vector3 targetPosition;

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

        shakeCount = 0;
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
        else if (movement == MovementType.Shake && objToMove != null) {
            float degreeMovement = Time.deltaTime * rate;
            if (shakeLeft) {
                objToMove.transform.Rotate(degreeMovement * shakeAxis);
            }
            else {
                objToMove.transform.Rotate(-degreeMovement * shakeAxis);
            }

            degreesToSpin -= degreeMovement;
            if (degreesToSpin <= 0) {
                shakeCount--;
                shakeLeft = !shakeLeft;
                if (shakeCount <= 0) {
                    Debug.Log("finished shaking.");

                    if (onFinish != null) {
                        onFinish(objToMove);
                    }

                    ResetFields();
                }
                else {
                    degreesToSpin = 2*perShakeDegrees;
                }
            }
        }
        else if (movement == MovementType.LineMovement && objToMove != null) {
            float movementTowardsTarget = Time.deltaTime * rate;
            Vector3 newPos = Vector3.MoveTowards(objToMove.transform.localPosition, targetPosition, movementTowardsTarget);
            objToMove.transform.localPosition = newPos;

            if (newPos == targetPosition) {
                if (onFinish != null) {
                    onFinish(objToMove);
                }

                ResetFields();
            }
        }
    }

    ///spin the given object the number of degrees at the given rate, call onfinish when done
    ///return a bool saying if the mover was busy already or not
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

    ///shake the given object around the given axis at the given rate, call onfinish when done
    ///return a bool saying if the mover was busy already or not
    public bool ShakeObject(GameObject obj, Vector3 axis, int shakeCount, float shakeRate, 
                            float perShakeDegrees, Action<GameObject> onFinish = null) {
        if (objToMove != null) {
            return false;
        }

        objToMove = obj;
        movement = MovementType.Shake;
        rate = shakeRate;
        this.onFinish = onFinish;

        degreesToSpin = perShakeDegrees;
        shakeLeft = true;
        this.shakeCount = shakeCount;
        shakeAxis = axis;
        this.perShakeDegrees = perShakeDegrees;


        return true;
    }

    public bool MoveObjectStraightLine(GameObject obj, Vector3 targetPosition, float rate, 
                                        Action<GameObject> onFinish = null) {
        if (objToMove != null) {
            return false;
        }

        objToMove = obj;
        movement = MovementType.LineMovement;
        this.rate = rate;
        this.targetPosition = targetPosition;

        return true;
    }
}