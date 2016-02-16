using System;
using UnityEngine;

public class Player : MonoBehaviour {

    private CharacterController physicsController;

    public float speed;
    public Vector3 facingVec;

    void Start() {
        physicsController = GetComponent<CharacterController>();
    }

    void FixedUpdate() {
        if (Input.GetKey(KeyCode.RightArrow)) {
            transform.Rotate(new Vector3(0.0f, 10.0f, 0.0f));
        }
        else if (Input.GetKey(KeyCode.LeftArrow)) {
            transform.Rotate(new Vector3(0.0f, -10.0f, 0.0f));
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float moveGravity = 0.0f;
        if (!physicsController.isGrounded) {
            moveGravity = -0.1f;
        }

        Vector3 movement = new Vector3(moveX, moveGravity, moveZ);

        physicsController.Move(movement * speed);
    }
}
