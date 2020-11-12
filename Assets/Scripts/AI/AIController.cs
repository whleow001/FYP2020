using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
    private enum AIState {
      Objective = 0,
      Engage = 1
    };

    // Layer references
    private int GOVT_LAYER = 9;
    private int REBEL_LAYER = 10;

    private Transform raycastDestination;
    private Transform raycastOrigins;

    private AIState aiState = AIState.Objective;
    [HideInInspector]
    public PlayerController.CharacterState characterState = PlayerController.CharacterState.Idle;

    private AIDirector aiDirector;

    private float sightRange = 7.0f;
    private float attackRange = 5.0f;
    private float bulletSpeed = 70.0f;

    private Vector3 objectiveOfInterest;
    private GameObject enemyOfInterest;
    private List<Vector3> objectivesLocation = new List<Vector3>();

    private UnityEngine.AI.NavMeshAgent agent;
    private Animator aiAnimator;

    private bool readyForFiring = false;

    public void AddObjective(Vector3 objective) {
      objectivesLocation.Add(objective);
    }

    public void RemoveObjective(Vector3 objective) {
      if (objectivesLocation.Contains(objective))
        objectivesLocation.Remove(objective);

      if (objectiveOfInterest == objective)
        SearchClosestObjective();
    }

    public void ClearObjectives() {
      objectivesLocation = new List<Vector3>();
    }

    public void SetObjectiveOfInterest(Vector3 objective) {
      if (objectivesLocation.Contains(objective))
        objectiveOfInterest = objective;
    }

    // Start is called before the first frame update
    void Start() {
      agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    // Update is called once per frame
    void Update() {
      if (objectiveOfInterest == Vector3.zero)
        SearchClosestObjective();

      if (aiState == AIState.Objective) {
        ChangeCharacterState(PlayerController.CharacterState.Running);

        if (Physics.CheckSphere(transform.position, sightRange)) {
          Collider[] collidersInSightRange = Physics.OverlapSphere(transform.position, sightRange);
          foreach (Collider colliderInSightRange in collidersInSightRange) {
            if (colliderInSightRange.gameObject.layer == ((aiDirector.GetTeam() == 0) ? REBEL_LAYER : GOVT_LAYER)) {
              enemyOfInterest = colliderInSightRange.gameObject;

              print("AI: Moving towards enemy: " + enemyOfInterest);
              ChangeAIState(AIState.Engage);
            }
          }
        }
        else {
          print("AI: Moving towards objective");

          agent.SetDestination(objectiveOfInterest);
        }
      }
      else if (aiState == AIState.Engage) {
        if (!enemyOfInterest) {
          ChangeAIState(AIState.Objective);
          ChangeCharacterState(PlayerController.CharacterState.Running);
        }

        else {
          float distance = Vector3.Distance(enemyOfInterest.transform.position, transform.position);
          // Lost sight of target
          if (distance > sightRange)
            enemyOfInterest = null;
          // Target not in attack range
          else if (distance > attackRange) {
            agent.SetDestination(enemyOfInterest.transform.position);
            ChangeCharacterState(PlayerController.CharacterState.Running);
          }
          // Target in attack range
          else {
            print("AI: Attacking " + enemyOfInterest);

            agent.isStopped = true;
            agent.ResetPath();
            transform.LookAt(enemyOfInterest.transform, Vector3.up);

            ChangeCharacterState(PlayerController.CharacterState.Attacking);
          }
        }
      }

      if (readyForFiring) {
        FireBullet();
      }
    }

    private void FireBullet() {
      foreach (Transform raycastOrigin in raycastOrigins) {
        Vector3 velocity = (raycastDestination.position - raycastOrigin.position).normalized * bulletSpeed;
        raycastOrigin.GetChild(0).GetComponent<ParticleSystem>().Emit(1);
        aiDirector.GetDirector().GetPlayerManager().GetComponent<PlayerRPC>().InstantiateBullet(raycastOrigin.position, velocity, aiDirector.GetTeam() == 0 ? GOVT_LAYER : REBEL_LAYER, 0);
      }
    }

    private void SearchClosestObjective() {
      float closestDistance = Mathf.Infinity;

      foreach (Vector3 objectiveLocation in objectivesLocation) {
        float distance = Vector3.Distance(objectiveLocation, transform.position);

        if (distance < closestDistance) {
          objectiveOfInterest = objectiveLocation;
          closestDistance = distance;
        }
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

    public void SetAIDirector(AIDirector _aiDirector) {
      aiDirector = _aiDirector;
    }

    public AIDirector GetAIDirector() {
      return aiDirector;
    }

    public void TakeDamage(Damage dmg) {
      aiDirector.TakeDamage(dmg);
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
}
