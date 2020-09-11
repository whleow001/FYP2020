using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun {
    // Input reference
    private PlayerInput playerInput;
    private Transform raycastOrigin;

    // Components
    Rigidbody rb;

    // Variables
    float speed = 10.0f;
    float angle;

    // firing
    float cd = 2.0f;
    float timer = 0.0f;
    Ray ray;
    RaycastHit hitInfo;
    float distance = 100.0f;

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

      foreach (Transform child in gameObject.transform)
        if (child.name == "GunPoint")
          raycastOrigin = child;
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

    void Update() {
      if (isAttacking) {
        if (timer == 0.0f && photonView.IsMine)
          photonView.RPC("Fire", RpcTarget.All);

        timer += Time.deltaTime;
        if (timer >= cd)
          timer = 0.0f;
      }
      else
        timer = 0.0f;
    }

    [PunRPC]
    void Fire() {
      ray.origin = raycastOrigin.transform.position;
      ray.direction = raycastOrigin.forward;

      if (Physics.Raycast(ray, out hitInfo, distance)) {
        if (hitInfo.collider.gameObject.layer != gameObject.layer && hitInfo.collider.gameObject.tag == "Player") {
          if (hitInfo.transform.gameObject.GetComponent<PlayerManager>().TakeDamage(20)) {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            foreach(GameObject player in players)
              if (photonView == PhotonView.Get(player))
                player.GetComponent<PlayerManager>().Increment("Kills");
          }

        }
        /*hitEffect.transform.position = hitInfo.point;
        hitEffect.transform.forward = hitInfo.normal;
        hitEffect.Emit(1);

        bullet.tracer.transform.position = hitInfo.point;
        bullet.time = maxLifeTime;*/
      }

      Debug.DrawRay(ray.origin, ray.GetPoint(distance), Color.red, 2.0f);
    }
}
