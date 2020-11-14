using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class AIController : MonoBehaviourPun
{
    private enum AIState {
      Objective = 0,
      Engage = 1
    };

    // Layer references
    private int GOVT_LAYER = 9;
    private int REBEL_LAYER = 10;
    private int ENVIRONMENT_LAYER = 13;
    private int REBELSPHERE_LAYER = 17;

    private Transform raycastDestination;
    private Transform raycastOrigins;

    private AIState aiState = AIState.Objective;
    [HideInInspector]
    public PlayerController.CharacterState characterState = PlayerController.CharacterState.Idle;

    private AIDirector aiDirector;

    private float sightRange = 6.0f;
    private float attackRange = 6.0f;
    private float bulletSpeed = 70.0f;

    private Vector3 objectiveOfInterest;
    private int objectivesIndex = 0;
    private Transform enemyOfInterest;
    private List<Vector3> objectivesLocation = new List<Vector3>();

    private UnityEngine.AI.NavMeshAgent agent;
    private Animator aiAnimator;

    // Flags
    private bool readyForFiring = false;
    private bool fovInstantiated = false;

    public void AddObjective(Vector3 objective) {
      objectivesLocation.Add(objective);
    }

    public void ClearObjectives() {
      objectivesLocation = new List<Vector3>();
    }

    private void NextObjective() {
      if (objectiveOfInterest == Vector3.zero)
        objectiveOfInterest = objectivesLocation[0];
      else {
        objectivesIndex++;

        if (objectivesIndex == objectivesLocation.Count)
          objectivesIndex = 0;
        objectiveOfInterest = objectivesLocation[objectivesIndex];
      }

      print("Next Objective: " + objectiveOfInterest);
    }

    // Update is called once per frame
    void Update() {
      if (!fovInstantiated) {
        photonView.RPC("AllocateFOV", RpcTarget.All);
        fovInstantiated = true;
      }

      if (objectiveOfInterest == Vector3.zero)
        NextObjective();

      transform.rotation = Quaternion.LookRotation(agent.velocity.normalized);

      if (aiDirector.GetHealth() <= 0) {
        ChangeCharacterState(PlayerController.CharacterState.Dead);
        ChangeAIState(AIState.Objective);
      }

      else if (aiState == AIState.Objective) {
        ChangeCharacterState(PlayerController.CharacterState.Running);

        if (Physics.CheckSphere(transform.position, sightRange)) {
          Collider[] collidersInSightRange = Physics.OverlapSphere(transform.position, sightRange);
          foreach (Collider colliderInSightRange in collidersInSightRange) {
            int team = aiDirector.GetTeam();

            if ((team == 0 && colliderInSightRange.gameObject.layer == REBEL_LAYER) ||
                (team == 0 && colliderInSightRange.gameObject.layer == REBELSPHERE_LAYER) ||
                (team == 1 && colliderInSightRange.gameObject.layer == GOVT_LAYER)) {
              if (Physics.Linecast(transform.position, colliderInSightRange.gameObject.transform.position)) {
                enemyOfInterest = colliderInSightRange.gameObject.transform;
                ChangeAIState(AIState.Engage);
                break;
              }
            }
          }
        }

        if (aiState == AIState.Objective) {
          agent.SetDestination(objectiveOfInterest);

          if ((objectiveOfInterest - transform.position).magnitude <= agent.stoppingDistance)
            NextObjective();
        }
      }
      else if (aiState == AIState.Engage) {
        if (!enemyOfInterest)
          ChangeAIState(AIState.Objective);

        else {
          float distance = Vector3.Distance(enemyOfInterest.position, transform.position);
          // Lost sight of target
          if (distance > sightRange)
            enemyOfInterest = null;
          // Target not in attack range
          // else if (distance > attackRange) {
          //   agent.SetDestination(enemyOfInterest.position);
          //   ChangeCharacterState(PlayerController.CharacterState.Running);
          // }
          // Target in attack range
          else {
            agent.ResetPath();
            agent.velocity = Vector3.zero;

            ChangeCharacterState(PlayerController.CharacterState.Attacking);
            transform.LookAt(enemyOfInterest, Vector3.up);
          }
        }
      }

      if (readyForFiring)
        FireBullet();
    }

    private void FireBullet() {
      foreach (Transform raycastOrigin in raycastOrigins) {
        Vector3 velocity = (raycastDestination.position - raycastOrigin.position).normalized * bulletSpeed;
        raycastOrigin.GetChild(0).GetComponent<ParticleSystem>().Emit(1);
        aiDirector.GetDirector().GetPlayerManager().GetComponent<PlayerRPC>().InstantiateBullet(raycastOrigin.position, velocity, aiDirector.GetTeam() == 0 ? GOVT_LAYER : REBEL_LAYER, 0, aiDirector.GetBotDetails());
      }
    }

    private void ChangeAIState(AIState _aiState) {
      if (aiState == _aiState)
        return;

      aiState = _aiState;
    }

    private void ChangeCharacterState(PlayerController.CharacterState _characterState) {
      if (characterState == _characterState)
        return;

      characterState = _characterState;
    }

    public void SetAgent(UnityEngine.AI.NavMeshAgent _agent) {
      agent = _agent;

      agent.updateRotation = false;
      agent.speed = 5;
      agent.acceleration = 2;
      agent.angularSpeed = 900;
      agent.stoppingDistance = 10;
    }

    public void SetAIDirector(AIDirector _aiDirector) {
      aiDirector = _aiDirector;
    }

    public AIDirector GetAIDirector() {
      return aiDirector;
    }

    public void TakeDamage(Damage dmg) {
      aiDirector.TakeDamage(dmg);
    }

    private void OnCollisionEnter(Collision other) {
      if (photonView.IsMine)
      {
          if (other.gameObject.tag == "Projectile" && other.gameObject.layer == ((aiDirector.GetTeam() == 0) ? REBEL_LAYER : GOVT_LAYER))
          {
              aiDirector.TakeDamage(new Damage(20, other.gameObject.transform.position), other.gameObject.GetComponent<PhotonViewReference>().GetPhotonView().ViewID, other.gameObject.GetComponent<PhotonViewReference>().GetBot());
          }
      }
    }

    public bool IsDead() {
      return aiDirector.GetHealth() <= 0;
    }

    public void ReadyForFiring(bool ready) {
      readyForFiring = ready;
    }

    public void ReinitializeGunpoints(GameObject aiClone) {
      raycastOrigins = aiClone.transform.Find("RaycastOrigins").transform;
      raycastDestination = aiClone.transform.Find("RaycastDestination").transform;
    }

    //broadcast health to all clients in the server
    [PunRPC]
    void BroadcastHealth(int victimID)
    {
        PhotonView PV = PhotonView.Find(victimID);
        Player victim = PV.Owner;
        Slider mainslider = PV.gameObject.GetComponentInChildren<Slider>();
        Image mainfill = PV.gameObject.transform.Find("Canvas").Find("Healthbar").Find("fill").GetComponent<Image>();
        aiDirector.SetHealthBar(aiDirector.GetHealth(), mainslider, mainfill);
    }

    [PunRPC]
    void ChangeIcons(int viewID, int team, int classIndex, int botPosition, string botName)
    {
        PhotonView PV = PhotonView.Find(viewID);

        //broadcast nameplate to all client
        Text name = PV.gameObject.transform.Find("Canvas").Find("Text").GetComponent<Text>();
        name.text = botName;
        name.color = Color.white;
        Image healthColor = PV.gameObject.transform.Find("Canvas").Find("Healthbar").Find("fill").GetComponent<Image>();

        if (team == 0) {
          healthColor.color = Color.red;
          aiDirector.GetDirector().GetPlayerManager().GovtIcons.transform.GetChild(botPosition).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UISprites/number" + classIndex);
        }
        else {
          healthColor.color = Color.blue;
          aiDirector.GetDirector().GetPlayerManager().RebelIcons.transform.GetChild(botPosition).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>("UISprites/number" + classIndex);
        }
    }

    [PunRPC]
    void ChangeMaterial(int ParentViewID, int ModelViewID, int team, int selectedLayer)
    {
        PhotonView ParentPV = PhotonView.Find(ParentViewID);
        PhotonView ModelPV = PhotonView.Find(ModelViewID);
        ParentPV.gameObject.layer = selectedLayer;
        Material[] materials = ModelPV.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().materials;
        materials[1] = aiDirector.GetMaterial(team);
        ModelPV.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().materials = materials;
        ModelPV.gameObject.layer = selectedLayer;
    }

    [PunRPC]
    void AllocateFOV() {
      aiDirector.GetDirector().AllocateFOVMask();
    }
}
