using System;
using UnityEngine;

public class Player : MonoBehaviour {

    private CharacterController physicsController;
    private Camera playerCamera;

    public float speed;
    public float turnSpeed;
    public Vector3 facingVec;

    //initializations:
    void Awake() {
        physicsController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        //camera shouldnt be active until the player is placed.
        DisablePlayerCamera();
    }

    /// <summary>
    /// Physics update based on arrow key input
    /// </summary>
    void FixedUpdate() {
        if (Input.GetKey(KeyCode.RightArrow)) {
            transform.Rotate(new Vector3(0.0f, turnSpeed, 0.0f));
        }
        else if (Input.GetKey(KeyCode.LeftArrow)) {
            transform.Rotate(new Vector3(0.0f, -1 * turnSpeed, 0.0f));
        }

        float moveGravity = 0.0f;
        if (!physicsController.isGrounded) {
            moveGravity = -0.1f;
        }

        Vector3 movement = new Vector3(0f, moveGravity, 0f);

        if (Input.GetKey(KeyCode.UpArrow)) {
            movement += speed * ForwardVector;
        }
        else if (Input.GetKey(KeyCode.DownArrow)) {
            movement -= speed * ForwardVector;
        }

        physicsController.Move(movement);
    }

    /// <summary>
    /// The direction that the player's camera is facing.
    /// </summary>
    private Vector3 ForwardVector {
        get {
            var ret = playerCamera.transform.forward;
            ret.y = 0f;
            return ret;
        }
    }

    public void MovePlayerToPosition(Vector3 position) {
        transform.localPosition = position;
    }

    public void EnablePlayerCamera() {
        if (playerCamera == null) {
            playerCamera = GetComponentInChildren<Camera>();
        }
        playerCamera.enabled = true;
    }

    public void DisablePlayerCamera() {
        if (playerCamera == null) {
            playerCamera = GetComponentInChildren<Camera>();
        }
        playerCamera.enabled = false;
    }
}
