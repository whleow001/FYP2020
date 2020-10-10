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

    // Components
    private Rigidbody rb;

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
    public bool ReadyForFiring = false;

    // Flags
    [HideInInspector]
    public bool isMoving = false;
    [HideInInspector]
    public bool isAttacking = false;
    [HideInInspector]
    public bool isDodging = false;

    /*
    public Slider slider;
    public Gradient gradient;
    public Image fill;
    */

    public PlayerManager playerManager;
    public GameDirector director;

    private bool fovInstantiated = false;

    void Start() {
      //rb = GetComponent<PlayerManager>().GetPlayerClone().GetComponent<Rigidbody>();
        rb = GetComponent<Rigidbody>();

        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        director = GameObject.Find("Director").GetComponent<GameDirector>();

        raycastOrigin = transform.Find("GunPoint").transform;
    }

    private void Update()
    {
        if (fovInstantiated == false)
        {
            if (PhotonNetwork.IsMasterClient == false)
            {
                GetComponent<PlayerRPC>().CallRPC("AllocateFOV");
                fovInstantiated = true;
            }
        }
    }

    void FixedUpdate() {
      if (turning) {
        transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, rotateSpeed * Time.deltaTime);

        if (Math.Abs(transform.rotation.eulerAngles.y - lookAt.eulerAngles.y) <= 1.0f)
          ReadyForFiring = true;
      }
    }

    public void Move() {
      if (!GetComponent<PlayerRPC>().IsPhotonViewMine()) return;

      Vector2 joystickVector = playerInput.GetJoystickVector();
      turning = false;
      ReadyForFiring = false;

      // Movement
      Vector3 projectedVector = new Vector3(joystickVector.y * speed, rb.velocity.y, joystickVector.x * speed);
      rb.velocity = Quaternion.Euler(0, 45, 0) * projectedVector;

      // Rotation
      if (joystickVector.x != 0 || joystickVector.y != 0)
        angle = Mathf.Atan2(joystickVector.x, -joystickVector.y) * Mathf.Rad2Deg - 45;
      transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
    }
    /*
    public void SpawnCamera(GameObject camera) {
      if (!GetComponent<PlayerRPC>().IsPhotonViewMine()) return;

      GameObject _mainCamera = Instantiate(camera, Vector3.zero, Quaternion.Euler(30, 45, 0));
      _mainCamera.GetComponent<CameraMotor>().SetPlayer(gameObject);
    }
    
    public bool IsPhotonViewMine() {
      return photonView.IsMine;
    }
    */
    public void Dodge() {
      turning = false;
      ReadyForFiring = false;
      rb.AddForce(transform.forward * 10.0f, ForceMode.Impulse);
    }

    // Auto targeting
    public void TurnAndFireNearestTarget() {
      Collider[] targets = Physics.OverlapSphere(transform.position, range, 1 << GetComponent<PlayerManager>().GetDirector().GetOtherFactionLayer());
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
    /*
    private int GetOtherFactionLayer() {
      return gameObject.layer == GOVT_LAYER ? REBEL_LAYER : GOVT_LAYER;
    }
    */

    [PunRPC]
    void Fire() {
      ray.origin = raycastOrigin.transform.position;
      ray.direction = raycastOrigin.forward;

      if (Physics.Raycast(ray, out hitInfo, range)) {
        if (hitInfo.collider.gameObject.layer != gameObject.layer) {
                if (hitInfo.collider.gameObject.tag == "Player")
                    hitInfo.transform.gameObject.GetComponent<PlayerController>().TakeDamage(20, photonView);
                else if (hitInfo.collider.gameObject.tag == "Generator")
                {
                    hitInfo.transform.gameObject.GetComponent<GeneratorHealth>().TakeDamage(20);
                }
        }
      }

      Debug.DrawRay(ray.origin, transform.TransformDirection(Vector3.forward) * range, Color.red, 0.5f);
    }
    /*
    public void SetHealthBar(int value)
    {
        slider.value = value;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void SetMaxHealthBar(int value)
    {
        slider.maxValue = 100;
        slider.value = 100;
        fill.color = gradient.Evaluate(1f);
    }
    */

    // Take Damage
    public void TakeDamage(int damage, PhotonView attacker)
    {
        /*
        if (!GetComponent<PlayerRPC>().IsPhotonViewMine()) return;

        GetComponent<PlayerManager>().TakeDamage(damage, attacker);
        Player victim = GetComponent<PlayerRPC>().GetPhotonView().Owner;
        GetComponent<PlayerRPC>().CallRPC("BroadcastHealth", victim);
        */

        if (!photonView.IsMine) return;

        playerManager.TakeDamage(damage, attacker);
        int VictimID = photonView.ViewID;
        //GetComponent<PlayerRPC>().CallRPC("BroadcastHealth", VictimID);
        photonView.RPC("BroadcastHealth", RpcTarget.All, VictimID);


    }
    /*
    //broadcast health to all clients in the server
    [PunRPC]
    void BroadcastHealth(int VictimID)
    {
        PhotonView PV = PhotonView.Find(VictimID);
        Player victim = PV.Owner;
        Slider mainslider = PV.gameObject.GetComponentInChildren<Slider>();
        Image mainfill = PV.gameObject.transform.Find("Canvas").Find("Healthbar").Find("fill").GetComponent<Image>();
        playerManager.SetHealthBar((int)victim.CustomProperties["Health"], mainslider, mainfill);
    }
    */
    

    [PunRPC]
    void AllocateFOV()
    {
        director.AllocateFOVMask();
    }

    /*
    public void DecrementGeneratorCount() {
      director.DecrementGeneratorCount();
    }
    */
}
