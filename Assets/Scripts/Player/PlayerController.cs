using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviour {

    // Firing
    class Bullet {
      public float time;
      public Vector3 initialPosition;
      public Vector3 initialVelocity;
      public TrailRenderer tracer;
    }

    [SerializeField]
    private TrailRenderer tracerEffect;
    /*[SerializeField]
    private ParticleSystem muzzleFlash;
    [SerializeField]
    private ParticleSystem hitMetalEffect;
    [SerializeField]
    private ParticleSystem hitFleshEffect;*/
    private Transform raycastDestination;
    private Transform raycastOrigins;
    private bool isFiring = false;
    private float bulletSpeed = 70.0f;
    private float bulletDrop = 0.0f;

    Ray ray;
    RaycastHit hitInfo;
    float accumulatedTime;
    List<Bullet> bullets = new List<Bullet>();
    float maxLifetime = 2.0f;

    private PlayerInput playerInput;

    // Movement variables
    private float angle;
    private bool readyForDoding = false;

    public enum CharacterState {
      Idle = 0,
      Running = 1,
      Attacking = 2,
      Dodging = 3,
      Dead = 4,
      Victory = 5,
      Defeat = 6
    };

    private struct Stats {
      public Stats(float _speed, float _dodgeDistance, float _fireRate) {
        speed = _speed;
        dodgeDistance = _dodgeDistance;
        fireRate = _fireRate;
      }

      public float speed { get; }
      public float dodgeDistance { get; }
      public float fireRate { get; }
    }

    private Stats[] stats = {
      new Stats(10, 7, 1),       // Gunslinger
      new Stats(8, 5, 1),        // Sniper
      new Stats(5, 3, 1)         // Juggernaut
    };

    private Stats currentStats;

    [HideInInspector]
    public CharacterState characterState = CharacterState.Idle;

    // Flags
    private bool fovInstantiated = false;

    void Start() {
      ReinitializeGunpoints();
      playerInput = GetComponent<PlayerInput>();
      SetStatsOnRespawn();
    }

    private void Update()
    {
        if (characterState == CharacterState.Dodging)
          return;

        UpdateState();
        UpdateBullets(Time.deltaTime);
        Rotate();
    }

    void FixedUpdate() {
      /*if (turning) {
        transform.rotation = Quaternion.Slerp(transform.rotation, lookAt, rotateSpeed * Time.deltaTime);

        if (Math.Abs(transform.rotation.eulerAngles.y - lookAt.eulerAngles.y) <= 1.0f)
          ReadyForFiring = true;
      }*/

      if (readyForDoding)
        GetTransform().position += GetTransform().forward * currentStats.dodgeDistance * Time.deltaTime;
    }

    public void ReinitializeGunpoints() {
      raycastOrigins = GetComponent<PlayerManager>().GetPlayerClone().transform.Find("RaycastOrigins").transform;
      raycastDestination = GetComponent<PlayerManager>().GetPlayerClone().transform.Find("RaycastDestination").transform;
    }

    public void SetStatsOnRespawn()
    {
        currentStats = stats[GetComponent<PlayerManager>().getSelectedCharacterIndex() - 1];
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

      if (!playerInput.IsPressed(PlayerInput.Ability.Attack))
        StopFiring();

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
            break;
          case CharacterState.Attacking:
            Stop();
            Attack(Time.deltaTime);
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
      GetRigidbody().velocity = GetTransform().forward * currentStats.speed;
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

    private void Attack(float deltaTime) {
      if (!isFiring)
        StartFiring();
      else
        UpdateFiring(deltaTime);
    }

    private void StartFiring() {
      isFiring = true;
      accumulatedTime = 0.0f;
      FireBullet();
    }

    private void StopFiring() {
      isFiring = false;
    }

    private void UpdateBullets(float deltaTime) {
      SimulateBullets(deltaTime);
      DestroyBullets();
    }

    private void DestroyBullets() {
      bullets.RemoveAll(bullet => bullet.time >= maxLifetime);
    }

    private void SimulateBullets(float deltaTime) {
      bullets.ForEach(bullet => {
            Vector3 p0 = GetPosition(bullet);
            bullet.time += deltaTime;
            Vector3 p1 = GetPosition(bullet);
            RaycastSegment(p0, p1, bullet);
      });
    }

    private void RaycastSegment(Vector3 start, Vector3 end, Bullet bullet) {
      Vector3 direction = end - start;
      float distance = direction.magnitude;
      ray.origin = start;
      ray.direction = end - start;
      if (Physics.Raycast(ray, out hitInfo, distance)) {
        bullet.tracer.transform.position = hitInfo.point;
        bullet.time = maxLifetime;
      } else
        bullet.tracer.transform.position = end;
    }

    private void FireBullet() {
      foreach (Transform raycastOrigin in raycastOrigins) {
        Vector3 velocity = (raycastDestination.position - raycastOrigin.position).normalized * bulletSpeed;
        raycastOrigin.GetChild(0).GetComponent<ParticleSystem>().Emit(1);
        GetComponent<PlayerRPC>().InstantiateBullet(raycastOrigin.position, velocity, GetComponent<PlayerManager>().GetDirector().GetFactionLayer());
      }
    }

    private void UpdateFiring(float deltaTime) {
      accumulatedTime += deltaTime;
      float fireInterval = 1.0f / currentStats.fireRate;
      while (accumulatedTime >= 0.0f) {
        FireBullet();
        accumulatedTime -= fireInterval;
      }
    }

    Bullet CreateBullet(Vector3 position, Vector3 velocity, int layer, PhotonView source) {
      Bullet bullet = new Bullet();
      bullet.initialPosition = position;
      bullet.initialVelocity = velocity;
      bullet.time = 0;
      bullet.tracer = Instantiate(tracerEffect, position, Quaternion.identity);
      bullet.tracer.gameObject.layer = layer;
      bullet.tracer.GetComponent<PhotonViewReference>().SetPhotonView(source);
      return bullet;
    }

    public void InstantiateBullet(Vector3 position, Vector3 velocity, int layer, PhotonView source) {
      if (!GetComponent<PlayerRPC>().IsPhotonViewMine())
        return;

      bullets.Add(CreateBullet(position, velocity, layer, source));
    }

    Vector3 GetPosition(Bullet bullet) {
      // p + v*t + 0.5*g*t*t
      Vector3 gravity = Vector3.down * bulletDrop;
      return bullet.initialPosition + bullet.initialVelocity * bullet.time + 0.5f*gravity*bullet.time*bullet.time;
    }

    public void OnDodgeAnimationFinish() {
      if (characterState == CharacterState.Dodging) {
        ChangeState(CharacterState.Idle);
        readyForDoding = false;
      }
    }

    // Auto targeting
    /*public void TurnAndFireNearestTarget() {
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
    }*/

    // Take Damage
    /*public void TakeDamage(int damage, PhotonView attacker)
    {
        /*
        if (!GetComponent<PlayerRPC>().IsPhotonViewMine()) return;

        GetComponent<PlayerManager>().TakeDamage(damage, attacker);
        Player victim = GetComponent<PlayerRPC>().GetPhotonView().Owner;
        GetComponent<PlayerRPC>().CallRPC("BroadcastHealth", victim);
        */

        /*if (!photonView.IsMine) return;

        GetComponent<PlayerManager>().TakeDamage(damage, attacker);
        int VictimID = photonView.ViewID;
        //GetComponent<PlayerRPC>().CallRPC("BroadcastHealth", VictimID);
        photonView.RPC("BroadcastHealth", RpcTarget.All, VictimID);


    }*/
}
