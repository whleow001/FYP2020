using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
  private int kills;
  private int deaths;
  private int health;
  private int classIndex;
  private Bot bot = new Bot();

  // Respawn
  private int respawnTime = 3;
  public int respawnTimer;
  private Coroutine respawnCoroutine;
  private bool reinitializing = false;

  // Healthbar
  public Slider slider;
  public Gradient gradient;
  public Image fill;

  // Material
  [SerializeField]
  private List<Material> teamMaterials = new List<Material>();

  private void Awake() {
    DontDestroyOnLoad(gameObject);
  }

  private void Start() {
    eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();
  }

  private void Update() {
    if (SceneManager.GetActiveScene().name == "RoomsV2")
      Destroy(gameObject);

    if (!attributesSet && player != null) {
      team = (byte)player.CustomProperties["_pt"];
      kills = (int)player.CustomProperties["Kills"];
      deaths = (int)player.CustomProperties["Deaths"];
      classIndex = (int)player.CustomProperties["Class"];
      selectedCharacter = director.GetPrefab(classIndex);
      bot.botName = player.NickName + " (AI)";

      HashSet<Player> teamPlayers;
      PhotonTeamsManager photonTeamManager = GameObject.Find("TeamManager").GetComponent<PhotonTeamsManager>();
      photonTeamManager.playersPerTeam.TryGetValue((byte)team, out teamPlayers);

      int i = 0;
      foreach (var _player in teamPlayers) {
        if (_player == player)
          bot.botPosition = i;
        i++;
      }

      attributesSet = true;
    }

    if (!instantiated && director) {
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

    slider = aiClone.GetComponentInChildren<Slider>();
    fill = aiClone.transform.Find("Canvas").Find("Healthbar").Find("fill").GetComponent<Image>();

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

    SetProperties();
    aiController.GetComponent<PhotonView>().RPC("ChangeIcons", RpcTarget.All, aiClone.GetComponent<PhotonView>().ViewID, team, classIndex, bot.botPosition, bot.botName);
  }

  private ParticleSystem GetHitEffect() {
    return AvatarParent.transform.Find("BloodEffect").GetComponent<ParticleSystem>();
  }

  private void SetProperties() {
    int ModelViewID = aiClone.GetComponent<PhotonView>().ViewID;
    int ParentViewID = AvatarParent.GetComponent<PhotonView>().ViewID;
    int selectedLayer = (team == 0) ? GOVT_LAYER : REBEL_LAYER;
    aiController.GetComponent<PhotonView>().RPC("ChangeMaterial", RpcTarget.All, ParentViewID, ModelViewID, team, selectedLayer);
  }

  public Material GetMaterial(int _team) {
    return teamMaterials[_team];
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

  public void TakeDamage(Damage dmg, int attackerViewID = -1, Bot bot = null) {
    if (health > 0) {
      if (dmg.sourcePosition != Vector3.zero && GetHitEffect())
      {
        GetHitEffect().transform.position = dmg.sourcePosition;
        GetHitEffect().transform.forward = (new Vector3(gameObject.transform.position.x, dmg.sourcePosition.y, dmg.sourcePosition.z) - dmg.sourcePosition).normalized;
        GetHitEffect().Emit(1);
      }

      health -= dmg.damage;
      aiController.GetComponent<PhotonView>().RPC("BroadcastHealth", RpcTarget.All, aiClone.GetComponent<PhotonView>().ViewID);

      if (health <= 0) {
        deaths += 1;
        PhotonView attacker = PhotonView.Find(attackerViewID);

        if (attacker != null) {
          Player killer = attacker.Owner;
          String killerName = "";

          if (killer.IsMasterClient && bot.botPosition != -1) {
            eventsManager.CreditBotKill_S(bot.botPosition);
            killerName = bot.botName;
          }
          else {
            CreditKiller(killer);
            killerName = killer.NickName;
          }

          eventsManager.GeneralNotification_S(killerName + " has killed " + name, 2.0f, "CombatLog");
          respawnTimer = respawnTime;

          respawnCoroutine = StartCoroutine(RespawnTimer());
        }
      }
    }
  }

  private IEnumerator RespawnTimer() {
    eventsManager.DeathTimer_S(team, bot.botPosition, respawnTimer);

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

  public void CreditBotKill() {
    kills += 1;
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

  public Bot GetBotDetails() {
    return bot;
  }

  public int GetKills() {
    return kills;
  }

  public int GetDeaths() {
    return deaths;
  }

  public void ResetOnNewScene() {
    instantiated = false;
    AvatarParent = null;
  }

  public void SetHealthBar(int value, Slider mainslider = null, Image mainfill = null)
  {

      if(mainslider != null && mainfill != null)
      {
          mainslider.value = value;
          //mainfill.color = gradient.Evaluate(mainslider.normalizedValue);
      }
      else
      {
          slider.value = value;
          //fill.color = gradient.Evaluate(slider.normalizedValue);
      }

  }

  public void SetMaxHealthBar(int value, Slider mainslider = null, Image mainfill = null)
  {

      if (mainslider != null && mainfill != null)
      {
          mainslider.maxValue = 100;
          mainslider.value = 100;
          //mainfill.color = gradient.Evaluate(1f);
      }
      else
      {
          slider.maxValue = 100;
          slider.value = 100;
          //fill.color = gradient.Evaluate(1f);
      }

  }
}
