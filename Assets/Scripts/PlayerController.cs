using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun {
    // Input reference
    private PlayerInput playerInput;
    private Transform raycastOrigin;

    // Layer references
    private int GOVT_LAYER = 9;
    private int REBEL_LAYER = 10;

    // Director reference
    private GameDirector director;

    // Components
    Rigidbody rb;

    // Variables
    float speed = 10.0f;
    float angle;

    // firing
    Ray ray;
    RaycastHit hitInfo;
    float range = 10.0f;
    bool turning = false;
    Quaternion lookAt;
    float rotateSpeed = 10.0f;

    // Flags
    [HideInInspector]
    public bool isMoving = false;
    [HideInInspector]
    public bool isAttacking = false;
    [HideInInspector]
    public bool isDodging = false;

    void Start() {
      playerInput = GetComponent<PlayerInput>();
      rb = GetComponent<Rigidbody>();

      director = GameObject.Find("Director").GetComponent<GameDirector>();
      raycastOrigin = transform.Find("GunPoint").transform;
    }

    void FixedUpdate() {
      if (turning) {
        transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, rotateSpeed * Time.deltaTime);

        if (Math.Abs(transform.rotation.eulerAngles.y - lookAt.eulerAngles.y) <= 1.0f)
          photonView.RPC("Fire", RpcTarget.All);
      }
    }

    public void Move() {
      if (!IsPhotonViewMine()) return;

      Vector2 joystickVector = playerInput.GetJoystickVector();
      turning = false;

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

    public void Dodge() {
      turning = false;
      rb.AddForce(transform.forward * 10.0f, ForceMode.Impulse);
    }

    // Auto targeting
    public void TurnAndFireNearestTarget() {
      Collider[] targets = Physics.OverlapSphere(transform.position, range, 1 << GetOtherFactionLayer());
      float nearestDistance = 0.0f;
      Collider nearestTarget = new Collider();

      if (targets.Length > 0) {
        nearestDistance = Vector2.Distance(transform.position, targets[0].transform.position);
        nearestTarget = targets[0];
      }

      foreach (Collider target in targets) {
        float newDistance = Vector2.Distance(transform.position, target.transform.position);

        if (nearestDistance > newDistance) {
          nearestDistance = newDistance;
          nearestTarget = target;
        }
      }

      if (nearestDistance != 0.0f) {
        turning = true;
        lookAt = Quaternion.LookRotation(nearestTarget.transform.position - transform.position);
      }
    }

    private int GetOtherFactionLayer() {
      return gameObject.layer == GOVT_LAYER ? REBEL_LAYER : GOVT_LAYER;
    }

    [PunRPC]
    void Fire() {
      ray.origin = raycastOrigin.transform.position;
      ray.direction = raycastOrigin.forward;

      if (Physics.Raycast(ray, out hitInfo, range)) {
        if (hitInfo.collider.gameObject.layer != gameObject.layer) {
          if (hitInfo.collider.gameObject.tag == "Player")
            hitInfo.transform.gameObject.GetComponent<PlayerManager>().TakeDamage(20, photonView);
          else if (hitInfo.collider.gameObject.tag == "Generator")
            hitInfo.transform.gameObject.GetComponent<GeneratorHealth>().TakeDamage(20);
        }
        /*hitEffect.transform.position = hitInfo.point;
        hitEffect.transform.forward = hitInfo.normal;
        hitEffect.Emit(1);

        bullet.tracer.transform.position = hitInfo.point;
        bullet.time = maxLifeTime;*/
      }

      Debug.DrawRay(ray.origin, transform.TransformDirection(Vector3.forward) * range, Color.red, 0.5f);
    }
}
