using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

public class AIDirector : MonoBehaviour
{
  [SerializeField]
  private GameObject aiContainer;
  private GameObject AvatarParent;
  private GameObject spawnPoint;
  private GameObject aiClone;
  private GameObject selectedCharacter;

  // References
  private GameDirector director;
  private AIController aiController;
  private AIAnimation aiAnimation;
  private EventsManager eventsManager;

  // Flags
  private bool instantiated = false;
  private bool attributesSet = false;

  private Player player;

  // Layer references
  private int GOVT_LAYER = 9;
  private int REBEL_LAYER = 10;

  // Attributes
  private int team;
  private int position;
  private int kills;
  private int deaths;
  private int health;
  private int classIndex;
  private string name;

  // Respawn
  private int respawnTime = 3;
  public int respawnTimer;
  private Coroutine respawnCoroutine;
  private bool reinitializing = false;

  private void Start() {
    eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();
  }

  private void Update() {
    if (!attributesSet && player != null) {
      team = (byte)player.CustomProperties["_pt"];
      kills = (int)player.CustomProperties["Kills"];
      deaths = (int)player.CustomProperties["Deaths"];
      classIndex = (int)player.CustomProperties["Class"];
      name = player.NickName + " (AI)";

      HashSet<Player> teamPlayers;
      PhotonTeamsManager photonTeamManager = GameObject.Find("TeamManager").GetComponent<PhotonTeamsManager>();
      photonTeamManager.playersPerTeam.TryGetValue((byte)team, out teamPlayers);

      int i = 0;
      foreach (var _player in teamPlayers) {
        if (_player == player)
          position = i;
        i++;
      }

      attributesSet = true;
    }

    if (!instantiated && director) {
      selectedCharacter = director.GetPrefab((int)player.CustomProperties["Class"]);
      spawnPoint = director.GetSpawn(team);
      InitializeCharacter();

      instantiated = true;
    }
  }

  private void InitializeCharacter() {
    if (!AvatarParent) {
      AvatarParent = MasterManager.NetworkInstantiate(aiContainer, spawnPoint.transform.GetChild(UnityEngine.Random.Range(0, 3)).transform.position, Quaternion.identity);
      // AvatarParent = MasterManager.NetworkInstantiate(aiContainer, Vector3.zero, Quaternion.identity);
      Destroy(AvatarParent.GetComponent<PlayerContainer>());
      UnityEngine.AI.NavMeshAgent agent = AvatarParent.AddComponent<UnityEngine.AI.NavMeshAgent>();

      aiAnimation = AvatarParent.AddComponent<AIAnimation>();

      aiController = AvatarParent.AddComponent<AIController>();
      aiController.SetAgent(agent);
      aiController.SetAIDirector(this);

      AvatarParent.layer = team == 0 ? GOVT_LAYER : REBEL_LAYER;
    }

    AvatarParent.transform.rotation = Quaternion.identity;

    aiClone = MasterManager.NetworkInstantiate(selectedCharacter, AvatarParent.transform.position, Quaternion.identity);
    AvatarParent.GetComponent<AIAnimation>().ReinitializeAnimator(aiClone);
    aiController.ClearObjectives();
    aiController.ReinitializeGunpoints(aiClone);

    if (director is RebelHQ_A) {
      Transform generatorSpawns = director.GetSpawn(Spawns_A.Generator).transform;

      foreach (Transform generator in generatorSpawns)
        aiController.AddObjective(generator.position);

      Transform forcefieldSpawns = director.GetSpawn(Spawns_A.ForcefieldSpawn).transform;

      foreach (Transform forcefield in forcefieldSpawns)
        aiController.AddObjective(forcefield.position);

      aiController.AddObjective(director.GetSpawn(Spawns_A.ForcefieldSphere).transform.position);
    }

    else if (director is RebelHQ_B) {
      Transform cpSpawn = director.GetSpawn(Spawns_B.ControlPoint).transform;

      foreach (Transform cp in cpSpawn)
        aiController.AddObjective(cp.position);
    }

    health = 100;
  }

  private ParticleSystem GetHitEffect() {
    return AvatarParent.transform.Find("BloodEffect").GetComponent<ParticleSystem>();
  }

  public void SetPlayer(Player _player) {
    player = _player;
  }

  public void SetDirector(GameDirector _director) {
    director = _director;
  }

  public GameDirector GetDirector() {
    return director;
  }

  public void TakeDamage(Damage dmg, int attackerViewID = -1) {
    if (dmg.sourcePosition != Vector3.zero && GetHitEffect())
    {
      GetHitEffect().transform.position = dmg.sourcePosition;
      GetHitEffect().transform.forward = (new Vector3(gameObject.transform.position.x, dmg.sourcePosition.y, dmg.sourcePosition.z) - dmg.sourcePosition).normalized;
      GetHitEffect().Emit(1);
    }

    health -= dmg.damage;

    if (health <= 0) {
      deaths += 1;
      PhotonView attacker = PhotonView.Find(attackerViewID);

      if (attacker != null) {
        Player killer = attacker.Owner;
        CreditKiller(killer);

        // Notification for "player" killed "player"
        eventsManager.GeneralNotification_S(killer.NickName + " has killed " + name, 2.0f, "CombatLog");
      }

      respawnTimer = respawnTime;

      respawnCoroutine = StartCoroutine(RespawnTimer());
    }
  }

  private IEnumerator RespawnTimer() {
    eventsManager.DeathTimer_S(team, position, respawnTimer);

    if (respawnTimer < 0) {
      PhotonNetwork.Destroy(aiClone);
      AvatarParent.transform.position = spawnPoint.transform.GetChild(UnityEngine.Random.Range(0, 3)).transform.position;
      health = 100;
      InitializeCharacter();

      respawnCoroutine = null;
    }
    else {
      yield return new WaitForSeconds(1.0f);
      respawnTimer--;
      respawnCoroutine = StartCoroutine(RespawnTimer());
    }
  }

  private void CreditKiller(Player killer) {
    ExitGames.Client.Photon.Hashtable killerProperties = killer.CustomProperties;
    killerProperties["Kills"] = (int)(killerProperties["Kills"]) + 1;
    killer.SetCustomProperties(killerProperties);
  }

  private String GetTimerText(int _seconds) {
    string minutes = (_seconds / 60).ToString("00");
    string seconds = (_seconds % 60).ToString("00");
    return $"{minutes}:{seconds}";
  }

  public int GetHealth() {
    return health;
  }

  public int GetTeam() {
    return team;
  }

  public int GetClassIndex() {
    return classIndex;
  }

  public GameObject GetAIClone() {
    return aiClone;
  }
}
