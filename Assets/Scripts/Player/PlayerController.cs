using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun {
    private Transform raycastOrigin;

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

    // Flags
    [HideInInspector]
    public bool canMove = true;

    private bool fovInstantiated = false;

    void Start() {
      raycastOrigin = GetTransform().Find("GunPoint").transform;
    }

    private void Update()
    {
        if (fovInstantiated == false)
        {
            if (!GetComponent<PlayerRPC>().IsMasterClient())
            {
                Debug.Log("FOV");
                GetComponent<PlayerRPC>().CallRPC("AllocateFOV");
                fovInstantiated = true;
            }
        }

        if (canMove) {

          Vector2 joystickVector = GetComponent<PlayerInput>().GetJoystickVector();

          // Movement
          Vector3 projectedVector = new Vector3(joystickVector.y * speed, GetRigidbody().velocity.y, joystickVector.x * speed);
          GetRigidbody().velocity = Quaternion.Euler(0, 45, 0) * projectedVector;

          // Rotation
          if (GetComponent<PlayerInput>().IsJoystickMoving())
            angle = Mathf.Atan2(joystickVector.x, -joystickVector.y) * Mathf.Rad2Deg - 45;
          GetTransform().rotation = Quaternion.Euler(new Vector3(0, angle, 0));
        }
    }

    private Rigidbody GetRigidbody() {
      return GetComponent<PlayerManager>().GetPlayerClone().GetComponent<Rigidbody>();
    }

    private Transform GetTransform() {
      return GetComponent<PlayerManager>().GetPlayerClone().transform;
    }

    void FixedUpdate() {
      /*if (turning) {
        transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, rotateSpeed * Time.deltaTime);

        if (Math.Abs(transform.rotation.eulerAngles.y - lookAt.eulerAngles.y) <= 1.0f)
          ReadyForFiring = true;
      }*/
    }

    public void Dodge() {
      turning = false;
      ReadyForFiring = false;
      //rb.AddForce(transform.forward * 10.0f, ForceMode.Impulse);
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

        GetComponent<PlayerManager>().TakeDamage(damage, attacker);
        int VictimID = photonView.ViewID;
        //GetComponent<PlayerRPC>().CallRPC("BroadcastHealth", VictimID);
        photonView.RPC("BroadcastHealth", RpcTarget.All, VictimID);


    }
}
