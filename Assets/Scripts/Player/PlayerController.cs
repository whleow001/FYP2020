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
    private PlayerInput playerInput;

    // Variables
    private float speed = 10.0f;
    private float angle;
    private float dodgeDistance = 7.0f;
    private bool readyForDoding = false;

    // firing
    private Ray ray;
    private RaycastHit hitInfo;
    private float range = 10.0f;
    private bool turning = false;
    private Quaternion lookAt;
    private float rotateSpeed = 10.0f;
    private bool ReadyForFiring = false;

    public enum CharacterState {
      Idle = 0,
      Running = 1,
      Attacking = 2,
      Dodging = 3,
      Dead = 4,
      Victory = 5,
      Defeat = 6
    };

    [HideInInspector]
    public CharacterState characterState = CharacterState.Idle;

    // Flags
    private bool fovInstantiated = false;

    void Start() {
      raycastOrigin = GetComponent<PlayerManager>().GetPlayerClone().transform.Find("GunPoint").transform;
      playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (characterState != CharacterState.Dodging) {
          UpdateState();
          Rotate();
        } else if (readyForDoding)
          GetTransform().position += GetTransform().forward * dodgeDistance * Time.deltaTime;

    }

    private void UpdateState() {
      if (!playerInput.IsJoystickMoving() && !playerInput.IsPressed(PlayerInput.Ability.Attack) && !playerInput.IsPressed(PlayerInput.Ability.Dodge))
        ChangeState(CharacterState.Idle);
      else if (playerInput.IsPressed(PlayerInput.Ability.Dodge))
        ChangeState(CharacterState.Dodging);
      else if (playerInput.IsPressed(PlayerInput.Ability.Attack))
        ChangeState(CharacterState.Attacking);
      else if (playerInput.IsJoystickMoving())
        ChangeState(CharacterState.Running);

      if (GetComponent<PlayerManager>().GetPlayerClone())
        switch (characterState) {
          case CharacterState.Idle:
            Stop();
            break;
          case CharacterState.Running:
            Move();
            break;
          case CharacterState.Dodging:
            Stop();
            StartCoroutine(Dodge());
            //Dodge();
            break;
          case CharacterState.Attacking:
            Stop();
            Attack();
            break;
          default:
            break;
        }
    }

    private Rigidbody GetRigidbody() {
      return GetComponent<PlayerManager>().GetPlayerAvatar().GetComponent<Rigidbody>();
    }

    private Transform GetTransform() {
      return GetComponent<PlayerManager>().GetPlayerAvatar().transform;
    }

    private void ChangeState(CharacterState _characterState) {
      if (characterState == _characterState)
        return;
      characterState = _characterState;
    }

    private void Stop() {
      GetRigidbody().velocity = Vector3.zero;
    }

    private void Move() {
      GetRigidbody().velocity = GetTransform().forward * speed;
    }

    private void Rotate() {
      if (playerInput.IsJoystickMoving())
        angle = playerInput.GetJoystickAngle();
      GetTransform().rotation = Quaternion.Euler(new Vector3(0, angle, 0));
    }

    private IEnumerator Dodge() {
      yield return new WaitForSeconds(.3f);
      readyForDoding = true;
    }

    private void Attack() {

    }

    void FixedUpdate() {
      /*if (turning) {
        transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, rotateSpeed * Time.deltaTime);

        if (Math.Abs(transform.rotation.eulerAngles.y - lookAt.eulerAngles.y) <= 1.0f)
          ReadyForFiring = true;
      }*/
    }

    public void OnDodgeAnimationFinish() {
      if (characterState == CharacterState.Dodging) {
        ChangeState(CharacterState.Idle);
        readyForDoding = false;
      }
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
