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
    [SerializeField]
    private TrailRenderer lightningTrail;
    private Transform raycastDestination;
    private Transform raycastOrigins;
    private bool isFiring = false;
    private float bulletSpeed = 70.0f;
    private float bulletDrop = 0.0f;
    private bool readyForFiring = false;

    Ray ray;
    RaycastHit hitInfo;
    float accumulatedTime;
    List<Bullet> bullets = new List<Bullet>();
    float maxLifetime = 2.0f;

    private PlayerInput playerInput;

    // Movement variables
    private float angle;
    private bool readyForDoding = false;
    private bool gameOver = false;

    // skill variables
    public bool SkillActive = false;
    public bool sniperActive = false;
    //private Coroutine usedSkillCoroutine;
    //private bool readyForSkill = true;

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
      new Stats(10, 10, 1),       // Gunslinger
      new Stats(8, 10, 1),        // Sniper
      new Stats(5, 7, 1)         // Juggernaut
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
        if (!raycastOrigins)
          ReinitializeGunpoints();

        if (characterState == CharacterState.Dodging)
          return;

        if (playerInput.IsPressed(PlayerInput.Ability.Skill1) && !SkillActive && characterState!=CharacterState.Dead)
        {
            if((int)GetComponent<PlayerManager>().GetProperty("Class") == 2)
            {
                sniperActive = true;
                ChangeState(CharacterState.Attacking);

                GetComponent<PlayerManager>().GetPlayerAvatar().GetComponent<SniperSkillManager>().CueSound();
            }
            else
            {
                if ((int)GetComponent<PlayerManager>().GetProperty("Class") == 1)
                  GetComponent<PlayerManager>().GetPlayerAvatar().GetComponent<GunslingerSkillManager>().CueSound();
                else if ((int)GetComponent<PlayerManager>().GetProperty("Class") == 3)
                  GetComponent<PlayerManager>().GetPlayerAvatar().GetComponent<JuggernautSkillManager>().CueSound();
                GetComponent<PlayerRPC>().ShowSkillEffect(GetComponent<PlayerManager>().GetPlayerAvatar().GetPhotonView().ViewID, (int)GetComponent<PlayerManager>().GetProperty("Class"));
            }

            //classSkill();
        }

        if (!gameOver)
          UpdateState();
        UpdateBullets(Time.deltaTime);
        Rotate();

        // if (readyForFiring && !isFiring) {
        //   accumulatedTime = 0.0f;
        //   isFiring = true;
        //   FireBullet();
        // }
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
      GameObject playerClone = GetComponent<PlayerManager>().GetPlayerClone();
      GameObject avatarParent = GetComponent<PlayerManager>().GetPlayerAvatar();

      if (playerClone) {
        raycastOrigins = GetComponent<PlayerManager>().GetPlayerClone().transform.Find("RaycastOrigins").transform;
        raycastDestination = GetComponent<PlayerManager>().GetPlayerClone().transform.Find("RaycastDestination").transform;
      }
    }

    public void SetStatsOnRespawn()
    {
        currentStats = stats[GetComponent<PlayerManager>().getSelectedCharacterIndex()-1];
        ChangeState(CharacterState.Idle);
    }

    public void MoveSpdUp()
    {
        if(SkillActive == true)
        {
            currentStats = new Stats(20, 10, 1);
        }
        else
        {
            currentStats = stats[GetComponent<PlayerManager>().GetProperty("Class")-1];
        }

    }

    private void UpdateState() {
        if (GetComponent<PlayerManager>().IsDead())
            ChangeState(CharacterState.Dead);
        else if (!playerInput.IsJoystickMoving() && !playerInput.IsPressed(PlayerInput.Ability.Attack) && !playerInput.IsPressed(PlayerInput.Ability.Dodge) && !playerInput.IsPressed(PlayerInput.Ability.Skill1))
            ChangeState(CharacterState.Idle);
        else if (playerInput.IsPressed(PlayerInput.Ability.Dodge))
            ChangeState(CharacterState.Dodging);
        else if (playerInput.IsPressed(PlayerInput.Ability.Attack))
            ChangeState(CharacterState.Attacking);
        else if (playerInput.IsJoystickMoving())
            ChangeState(CharacterState.Running);

      if (!playerInput.IsPressed(PlayerInput.Ability.Attack))
        StopFiring();

      GetComponent<PlayerManager>().GetPlayerAvatar().GetComponent<FootstepsManager>().SetPlayFootsteps(characterState == CharacterState.Running);

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
            if(sniperActive)
                    {
                        FireBullet(2);
                        sniperActive = false;
                    }
            else
                    {
                        Attack(Time.deltaTime);
                    }

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
      if (readyForFiring) {
        FireBullet(0);
        GetComponent<PlayerManager>().GetPlayerAvatar().GetComponent<FiringManager>().CueSound();
      }
    }

    private void StopFiring() {
      isFiring = false;
      readyForFiring = false;
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
      }
      else
        {
            bullet.tracer.transform.position = end;
        }

    }

    private void FireBullet(int skill) {
      foreach (Transform raycastOrigin in raycastOrigins) {
        Vector3 velocity = (raycastDestination.position - raycastOrigin.position).normalized * bulletSpeed;
        raycastOrigin.GetChild(0).GetComponent<ParticleSystem>().Emit(1);
        GetComponent<PlayerRPC>().InstantiateBullet(raycastOrigin.position, velocity, GetComponent<PlayerManager>().GetDirector().GetFactionLayer(), skill);
      }
    }

    Bullet CreateBullet(Vector3 position, Vector3 velocity, int layer, int skill, int botPosition, string botName, PhotonView source) {
      Bullet bullet = new Bullet();
      bullet.initialPosition = position;
      bullet.initialVelocity = velocity;
      bullet.time = 0;
      if(skill == 2)
        {
            bullet.tracer = Instantiate(lightningTrail, position, Quaternion.identity);
        }
      else
        {
            bullet.tracer = Instantiate(tracerEffect, position, Quaternion.identity);
        }
      bullet.tracer.gameObject.layer = layer;
      bullet.tracer.GetComponent<PhotonViewReference>().SetPhotonView(source);
      bullet.tracer.GetComponent<PhotonViewReference>().SetBot(new Bot(botPosition, botName));
      return bullet;
    }

    public void InstantiateBullet(Vector3 position, Vector3 velocity, int layer, int skill, int botPosition, string botName, PhotonView source) {
      if (!GetComponent<PlayerRPC>().IsPhotonViewMine())
        return;

      bullets.Add(CreateBullet(position, velocity, layer, skill, botPosition, botName, source));
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

    public void SetGovtWin() {
      gameOver = true;
      if (GetComponent<PlayerManager>().team == 0)
        ChangeState(CharacterState.Victory);
      else
        ChangeState(CharacterState.Defeat);
    }

    public void ReadyForFiring(bool ready) {
      readyForFiring = ready;
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
