using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState {
    Active,
    Frozen
}

public class Player : MonoBehaviour {

    private CharacterController physicsController;
    private Camera playerCamera;

    public float speed;
    public float turnSpeed;
    public Vector3 facingVec;

    private IntVector2 mazeCellCoords;
    private bool inCell;

    public int maxBreadcrumbs = 11;
    private List<Breadcrumb> breadcrumbsDropped;

    private PlayerState currentState;

    private bool canFrozenStateBeChanged = true;

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

        breadcrumbsDropped = new List<Breadcrumb>(maxBreadcrumbs + 1);
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
    /// Init maze cell coordinates. Should ONLY be called once, when the player is placed into the maze.
    /// </summary>
    /// <param name="coords"></param>
    public void InitializePlayerCoords(IntVector2 coords) {
        mazeCellCoords = coords;
    }

    /// <summary>
    /// Physics update based on arrow key input
    /// </summary>
    void FixedUpdate() {
        if (currentState == PlayerState.Frozen) {
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


    void Update() {
        if (currentState == PlayerState.Frozen) {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            DropBreadCrumb();
        }
    }

    private void DropBreadCrumb() {
        GameObject breadcrumbObj = Instantiate(Resources.Load("prefabs/Breadcrumb") as GameObject);
        if (breadcrumbObj == null) {
            Debug.LogError("breadcrumb was null");
        }
        breadcrumbObj.transform.parent = transform.parent;
        breadcrumbObj.transform.localPosition = transform.localPosition + (ForwardVector * 0.5f);

        Breadcrumb breadcrumb = breadcrumbObj.GetComponent<Breadcrumb>();

        if (breadcrumb != null) {
            breadcrumbsDropped.Add(breadcrumb);
            while (breadcrumbsDropped.Count > maxBreadcrumbs) {
                Destroy(breadcrumbsDropped[0].gameObject);
                breadcrumbsDropped.RemoveAt(0);
            }
        }
    }

    public void DestroyDroppedBreadcrumbs() {
        foreach (var crumb in breadcrumbsDropped) {
            Destroy(crumb.gameObject);
        }
        breadcrumbsDropped = new List<Breadcrumb>(maxBreadcrumbs + 1);
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

    public void PermanentlyFreezePlayer() {
        currentState = PlayerState.Frozen;
        canFrozenStateBeChanged = false;
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

    public void FreezePlayer() {
        if (canFrozenStateBeChanged) {
            currentState = PlayerState.Frozen;
        }
    }

    public void UnfreezePlayer() {
        if (canFrozenStateBeChanged) {
            currentState = PlayerState.Active;
        }
    }
}
