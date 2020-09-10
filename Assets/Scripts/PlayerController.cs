﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    // Input reference
    PlayerInput playerInput;

    // Components
    Rigidbody rb;

    // Variables
    float speed = 10.0f;
    float angle;

    // Flags
    [HideInInspector]
    public bool isMoving = false;
    [HideInInspector]
    public bool isAttacking = false;
    [HideInInspector]
    public bool isDodging = false;

    // Start is called before the first frame update
    void Start() {
      playerInput = GetComponent<PlayerInput>();
      rb = GetComponent<Rigidbody>();

      // Set camera to follow player
      Camera.main.GetComponent<CameraMotor>().SetPlayer(gameObject);
    }

    public void Move() {
      Vector2 joystickVector = playerInput.GetJoystickVector();

      // Movement
      Vector3 projectedVector = new Vector3(joystickVector.y * speed, rb.velocity.y, joystickVector.x * speed);
      rb.velocity = Quaternion.Euler(0, 45, 0) * projectedVector;

      // Rotation
      if (joystickVector.x != 0 || joystickVector.y != 0)
        angle = Mathf.Atan2(joystickVector.x, -joystickVector.y) * Mathf.Rad2Deg - 45;
      transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
    }
}
