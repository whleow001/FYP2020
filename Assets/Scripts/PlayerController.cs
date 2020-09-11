using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun {
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
    }

    public void Move() {
      if (!IsPhotonViewMine()) return;

      Vector2 joystickVector = playerInput.GetJoystickVector();

      // Movement
      Vector3 projectedVector = new Vector3(joystickVector.y * speed, rb.velocity.y, joystickVector.x * speed);
      rb.velocity = Quaternion.Euler(0, 45, 0) * projectedVector;

      // Rotation
      if (joystickVector.x != 0 || joystickVector.y != 0)
        angle = Mathf.Atan2(joystickVector.x, -joystickVector.y) * Mathf.Rad2Deg - 45;
      transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
    }

    public void SpawnCamera(GameObject camera) {
      if (!photonView.IsMine) return;

      GameObject _mainCamera = Instantiate(camera, Vector3.zero, Quaternion.Euler(30, 45, 0));
      _mainCamera.GetComponent<CameraMotor>().SetPlayer(gameObject);
    }

    public bool IsPhotonViewMine() {
      return photonView.IsMine;
    }
}
