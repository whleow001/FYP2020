using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject _mainCamera;

    [Header("Materials")]
    [SerializeField]
    private List<Material> teamMaterials = new List<Material>();

    [Header("References")]
    [SerializeField]
    private GameDirector director;
    [SerializeField]
    private EventsManager eventsManager;
    [SerializeField]
    public GameObject GovtIcons;
    [SerializeField]
    public GameObject RebelIcons;

    [SerializeField]
    private GameObject playerContainer;

    private ParticleSystem hitEffect;

    //currently using int, can change back to GameObject type if we are using same character models for both teams.
    private GameObject selectedCharacter;
    private int selectedCharacterIndex;

    private GameObject spawnPoint;

    private bool instantiated = false;
    private GameObject playerClone;
    private GameObject AvatarParent;
    public int team;   // team number;
    public int position;  // position in team;

    private Scene currentScene;

    public Slider slider;
    public Gradient gradient;
    public Image fill;

    private int respawnTime = 3;
    public int respawnTimer;
    private Coroutine respawnCoroutine;
    private bool reinitializing = false;

    public bool kill = false;

    // Custom player properties
    private ExitGames.Client.Photon.Hashtable properties;

    void Awake() {
      ChangeCharacter(1);
    }

    // Start is called before the first frame update
    void Start()
    {
        team = director.GetTeamIndex();
        Debug.Log("team number is " + team);
        spawnPoint = director.GetSpawn(team);
        // ChangeCharacter(1);

        currentScene = SceneManager.GetActiveScene();

        InitializeCharacter();

        Reset();

    }

    // Update is called once per frame
    void Update()
    {
      //if (kill) {
      // TakeDamage(new Damage(100, Vector3.zero));

      // kill = false;
      //}
    }

    public GameDirector GetDirector() {
      return director;
    }

    public Material GetTeamMaterials(int material)
    {
        return teamMaterials[material];
    }

    //this function is will be used if we using same model for both teams, changing their material and layer
    //do not think spawn needs to be a parameter here, should be layer instead, however currently not working as intended
    public void SetProperties(int selectedMaterial)
    {
        int ModelViewID = playerClone.GetComponent<PhotonView>().ViewID;
        int ParentViewID = AvatarParent.GetComponent<PhotonView>().ViewID;
        int selectedLayer = director.GetFactionLayer();
        GetComponent<PlayerRPC>().ChangeMaterial(ParentViewID, ModelViewID, selectedMaterial, selectedLayer);
    }

    public void SetPosition(int _position) {
      position = _position;
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

    public void ChangeCharacter(int index)
    {
        print("New char: " + index);
        selectedCharacterIndex = index;
        ChangeValue("Class", selectedCharacterIndex);

        selectedCharacter = director.GetPrefab(selectedCharacterIndex);

        Debug.Log(properties["Class"]);
    }

    private void InitializeCharacter()
    {
        if (AvatarParent == null)
        {
            AvatarParent = MasterManager.NetworkInstantiate(playerContainer, spawnPoint.transform.GetChild(Random.Range(0, 3)).transform.position, Quaternion.identity);
        }
        //selectedCharacter = (int)(properties["Class"]);
        AvatarParent.transform.rotation = Quaternion.identity;
        playerClone = MasterManager.NetworkInstantiate(selectedCharacter, AvatarParent.transform.position, Quaternion.identity);

        //Text NameText = playerClone.transform.Find("Canvas").Find("Text").GetComponent<Text>();
        slider = playerClone.GetComponentInChildren<Slider>();
        fill = playerClone.transform.Find("Canvas").Find("Healthbar").Find("fill").GetComponent<Image>();

        //changing material and layer not working yet
        SetProperties(team);
        GetComponent<PlayerRPC>().ChangeIcons();

        //scale player minimap icon
        EditPlayerIcon(playerClone);

        AvatarParent.GetComponent<PlayerContainer>().SpawnCamera(_mainCamera, playerClone);
        //AvatarParent.GetComponent<PlayerContainer>().SetPlayerManager(this);
        //playerClone.transform.SetParent(gameObject.transform);
    }

    private void EditPlayerIcon(GameObject playerPrefab)
    {
        playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().drawMode = SpriteDrawMode.Sliced;
        playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().size = new Vector3(1.5f, 2.0f, 1f);
        playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().drawMode = SpriteDrawMode.Simple;
        // changing material and layer not working yet
        if(team == 0)
        {
            playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().color = Color.blue;
        }
        else
        {
            playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().color = Color.red;
        }

    }

    public GameObject GetPlayerClone() {
      return playerClone;
    }

    public GameObject GetPlayerAvatar() {
      return AvatarParent;
    }

    public ParticleSystem GetHitEffect() {
      return AvatarParent.transform.Find("BloodEffect").GetComponent<ParticleSystem>();
    }

    private void GetProperties()
    {
        properties = PhotonNetwork.LocalPlayer.CustomProperties;
    }

    public void Reset()
    {
        if (!instantiated && currentScene.name=="Rebel HQ")
        {
            ResetProperty("Deaths");
            ResetProperty("Kills");

            instantiated = true;
        }

        ResetProperty("Health");
    }



    private void ChangeValue(string key, int value)
    {
        GetProperties();

        properties[key] = value;
        PhotonNetwork.SetPlayerCustomProperties(properties);
        if(key == "Health")
        {
            SetHealthBar(value);
        }
    }

    private void ResetProperty(string key)
    {
        if (key == "Health")
        {
            ChangeValue(key, 100);
            SetMaxHealthBar(100);
        }
        else
        {
            ChangeValue(key, 0);
        }
    }

    public void Increment(string key)
    {
        if (properties.ContainsKey(key))
            ChangeValue(key, GetProperty(key) + 1);
    }

    public int GetProperty(string key)
    {
        GetProperties();
        if (properties.ContainsKey(key))
            return (int)properties[key];
        return 0;
    }

    public void TakeDamage(Damage dmg, int attackerViewID = -1)
    {
        print("AI Under attack, but im getting attacked too");
        Debug.Log("viewID of attacker: " + attackerViewID);
        if (GetProperty("Health") > 0)
        {
            if (dmg.sourcePosition != Vector3.zero)
            {
                GetHitEffect().transform.position = dmg.sourcePosition;
                GetHitEffect().transform.forward = (new Vector3(gameObject.transform.position.x, dmg.sourcePosition.y, dmg.sourcePosition.z) - dmg.sourcePosition).normalized;
                GetHitEffect().Emit(1);
            }

            ChangeValue("Health", (int)(properties["Health"]) - dmg.damage);
            GetComponent<PlayerRPC>().CallRPC("BroadcastHealth", playerClone.GetComponent<PhotonView>().ViewID);

            if (GetProperty("Health") <= 0)
            {
                Increment("Deaths");
                PhotonView attacker = PhotonView.Find(attackerViewID);
                Debug.Log("PV of attacker: " + attacker);

                if (attacker != null)
                {
                    Player killer = attacker.Owner;
                    Debug.Log(killer);
                    Debug.Log("Player: " + killer.NickName);
                    CreditKiller(killer);

                    //Notification for "player" killed "player"
                    eventsManager.GeneralNotification_S(killer.NickName + " has killed " + PhotonNetwork.LocalPlayer.NickName, 2.0f, "CombatLog");
                }

                respawnTimer = respawnTime;
                reinitializing = true;

                //respawn timer overlay
                // director.GetUIText(4).SetText("", 3.0f, true);
                // Respawn();
                //director.AddToCombatLog(photonView, attacker);
                respawnCoroutine = StartCoroutine(RespawnTimer());
            }
        }
    }

    private IEnumerator RespawnTimer() {
      eventsManager.DeathTimer_S(team, position, respawnTimer);

      if (respawnTimer < 0) {
        PhotonNetwork.Destroy(playerClone);
        AvatarParent.transform.position = spawnPoint.transform.GetChild(Random.Range(0, 3)).transform.position;
        Reset();
        InitializeCharacter();

        GetComponent<PlayerController>().ReinitializeGunpoints();
        GetComponent<PlayerController>().SetStatsOnRespawn();
        GetComponent<PlayerAnimation>().ReinitializeAnimator();

        reinitializing = false;

        respawnCoroutine = null;
      }

      else {
        director.GetUIText(4).SetText(GetTimerText(respawnTimer), 1.0f);

        yield return new WaitForSeconds(1.0f);
        respawnTimer--;
        respawnCoroutine = StartCoroutine(RespawnTimer());
      }
    }

    private string GetTimerText(int _seconds) {
      string minutes = (_seconds / 60).ToString("00");
      string seconds = (_seconds % 60).ToString("00");
      return $"{minutes}:{seconds}";
    }

    // Credit kill
    public void CreditKiller(Player killer)
    {
        //if (!photonView.IsMine) return;
        Debug.Log(killer);
        ExitGames.Client.Photon.Hashtable killerProperties;
        killerProperties = killer.CustomProperties;
        killerProperties["Kills"] = (int)(killerProperties["Kills"]) + 1;
        killer.SetCustomProperties(killerProperties);
    }

    // private void Respawn()
    // {
    //
    //    // PhotonNetwork.Destroy(playerClone);
    //    // AvatarParent.transform.position = spawnPoint.transform.GetChild(Random.Range(0, 3)).transform.position;
    //    // InitializeCharacter();
    //    // Reset();
    //
    //    GetComponent<PlayerController>().ReinitializeGunpoints();
    //    GetComponent<PlayerController>().SetStatsOnRespawn();
    //    GetComponent<PlayerAnimation>().ReinitializeAnimator();
    //    //GetComponent<PlayerRPC>().CallRPC("BroadcastHealth", GetComponent<PlayerRPC>().GetPhotonView().ViewID);
    //    //playerClone.GetComponent<PhotonView>().RPC("BroadcastHealth", RpcTarget.All, playerClone.GetComponent<PhotonView>().Owner);
    // }

   //Player Disconnect PHOTON Room script
   public override void OnPlayerLeftRoom (Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log(otherPlayer.NickName + " Has Left the game");
        eventsManager.GeneralNotification_S(otherPlayer.NickName + " Has Left the game", 2.0f, "PlayerDisconnect");

        if (GetComponent<PlayerRPC>().IsMasterClient()) {
          StartCoroutine(director.SpawnAI(otherPlayer));
        }
    }

    //Master client leave room
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Has Disconnected");
        eventsManager.GeneralNotification_S(PhotonNetwork.LocalPlayer.NickName + " Has Disconnected", 2.0f, "PlayerDisconnect");
    }

    public int getSelectedCharacterIndex() {
      return selectedCharacterIndex;
    }

    public bool IsDead() {
      return GetProperty("Health") <= 0;
    }

    public bool IsReinitializing() {
      return reinitializing;
    }

    //Player disconnect under game setup script then add button for disconnect under char select
    //public void DisconnectPlayer()
    //{
    //    StartCoroutine(DisconnectAndLoad());
    //}

    //IEnumerator DisonnectAndLoad()
    //{
    //    PhotonNetwork.Disconnect();
    //    while (PhotonNetwork.IsConnected)
    //        yield return null;
    //    SceneManager.LoadScene(MultiplayerSetting.multiplayerSetting.menuScene);
    //}
}
