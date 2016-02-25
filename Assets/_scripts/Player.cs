using System;
using UnityEngine;

public enum PlayerState {
    Active,
    InTextCommunication
}

public class Player : MonoBehaviour {

    private CharacterController physicsController;
    private Camera playerCamera;

    public float speed;
    public float turnSpeed;
    public Vector3 facingVec;

    private IntVector2 mazeCellCoords;
    private bool inCell;

    private PlayerState currentState;

    //public accessor for the maze cell coordinates of the player (get only)
    public IntVector2 MazeCellCoords {
        get {
            return mazeCellCoords;
        }
    }

    public bool InCell {
        get {
            return inCell;
        }
    }

    //initializations:
    void Awake() {
        physicsController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        //camera shouldnt be active until the player is placed.
        DisablePlayerCamera();

        //set initial player state
        currentState = PlayerState.Active;
    }

    /// <summary>
    /// On collisions, check for whether the player is touching a cell and update coords and inCell
    /// accordingly.
    /// </summary>
    /// <param name="col"></param>
    void OnControllerColliderHit(ControllerColliderHit col) {
        MazeCell collidedCell = col.gameObject.GetComponentInParent<MazeCell>();
        if (collidedCell != null) {
            inCell = true;
            if (collidedCell.coordinates != mazeCellCoords) {
                mazeCellCoords = collidedCell.coordinates;
            }
        }
        else {
            inCell = false;
        }
    }

    /// <summary>
    /// Physics update based on arrow key input
    /// </summary>
    void FixedUpdate() {
        if (currentState == PlayerState.InTextCommunication) {
            return;
        }


        if (Input.GetKey(KeyCode.D)) {
            transform.Rotate(new Vector3(0.0f, turnSpeed, 0.0f), Space.World);
        }
        else if (Input.GetKey(KeyCode.A)) {
            transform.Rotate(new Vector3(0.0f, -1 * turnSpeed, 0.0f), Space.World);
        }

        if (Input.GetKey(KeyCode.Q) && Vector3.Angle(playerCamera.transform.forward, Vector3.up) < 170f) {
            transform.Rotate(playerCamera.transform.right, turnSpeed, Space.World);
        }
        else if (Input.GetKey(KeyCode.E) && Vector3.Angle(playerCamera.transform.forward, Vector3.up) > 10f) {
            transform.Rotate(playerCamera.transform.right, -1 * turnSpeed, Space.World);
        }
        

        float moveGravity = 0.0f;
        if (!physicsController.isGrounded) {
            moveGravity = -0.1f;
        }

        Vector3 movement = new Vector3(0f, moveGravity, 0f);

        if (Input.GetKey(KeyCode.W)) {
            movement += speed * ForwardVector;
        }
        else if (Input.GetKey(KeyCode.S)) {
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

    public void BeginTextCommunicationWithPlayer() {
        currentState = PlayerState.InTextCommunication;
    }

    public void EndTextCommunicationWithPlayer() {
        currentState = PlayerState.Active;
    }
}
