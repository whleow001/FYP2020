﻿using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    //reference current scene gamedirector
    [SerializeField]
    private GameDirector director;

    [SerializeField]
    private GameObject _mainCamera;

    // Layer references
    protected int GOVT_LAYER = 9;
    protected int REBEL_LAYER = 10;

    // Spawns
    [Header("Spawns")]
    [SerializeField]
    protected List<GameObject> PlayerSpawns = new List<GameObject>();

    // Prefabs
    [Header("GovtPrefabs")]
    [SerializeField]
    protected List<GameObject> playerPrefabs = new List<GameObject>();

    [Header("rebelPrefabs")]
    [SerializeField]
    protected List<GameObject> rebelPrefabs = new List<GameObject>();

    [Header("Materials")]
    [SerializeField]
    protected List<Material> TeamMaterials = new List<Material>();

    [SerializeField]
    protected GameObject playerContainer;

    // Custom player properties
    private ExitGames.Client.Photon.Hashtable properties;

    //currently using int, can change back to GameObject type if we are using same character models for both teams.
    private GameObject selectedCharacter;

    //private Material selectedMaterial;

    //private Vector3 spawnPoint;

    private bool instantiated = false;
    private GameObject playerClone;
    private GameObject AvatarParent;
    private int team;   // team number;


    // Events Manager
    protected EventsManager eventsManager;


    private int charIndex; //character index
    // Start is called before the first frame update
    void Start()
    {
        GetProperties();
        director = GameObject.Find("Director").GetComponent<GameDirector>();
        //manager = GameObject.Find("Manager").GetComponent<EventsManager>();

        team = director.GetTeamIndex();
        AvatarParent = MasterManager.NetworkInstantiate(playerContainer, PlayerSpawns[team].transform.GetChild(Random.Range(0, 3)).transform.position, Quaternion.identity);
        //ChangeValue("Class", 0);
        ChangeCharacter(0);

        InitializeCharacter();

        Reset();
        instantiated = true;

        //scale player minimap icon
        EditPlayerIcon(playerClone);

        eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    //this function is will be used if we using same model for both teams, changing their material and layer 
    //do not think spawn needs to be a parameter here, should be layer instead, however currently not working as intended
    public void SetProperties(Material selectedMaterial, int selectedLayer)
    {
        playerClone.GetComponentInChildren<SkinnedMeshRenderer>().material = selectedMaterial;
        playerClone.layer = selectedLayer;
    }

    public void ChangeCharacter(int selectedCharacterIndex)
    {
        ChangeValue("Class", selectedCharacterIndex);
        
        if(team == 0)
        {
            selectedCharacter = playerPrefabs[(int)(properties["Class"])];
        }
        else
        {
            selectedCharacter = rebelPrefabs[(int)(properties["Class"])];
        }
        Debug.Log(properties["Class"]);
    }

    private void InitializeCharacter()
    {
        //selectedCharacter = (int)(properties["Class"]);
        playerClone = MasterManager.NetworkInstantiate(selectedCharacter, AvatarParent.transform.position, Quaternion.identity);
        //changing material and layer not working yet
        /*if (team == 0)
        {
            SetProperties(TeamMaterials[0], GOVT_LAYER);
        }
        else
        {
            SetProperties(TeamMaterials[1], REBEL_LAYER);
        }*/
        AvatarParent.GetComponent<PlayerContainer>().SpawnCamera(_mainCamera, playerClone);
        //playerClone.transform.SetParent(AvatarParent.transform);
    }

    private void EditPlayerIcon(GameObject playerPrefab)
    {
        playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().drawMode = SpriteDrawMode.Sliced;
        playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().size = new Vector3(1.5f, 2.0f, 1f);
        playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().drawMode = SpriteDrawMode.Simple;
        /*  // changing material and layer not working yet
        if(team == 0)
        {
            playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().color = Color.blue;
        }
        else
        {
            playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().color = Color.red;
        }*/
        
    }

    private void GetProperties()
    {
        properties = PhotonNetwork.LocalPlayer.CustomProperties;
    }

    public void Reset()
    {
        if (!instantiated)
        {
            ResetProperty("Deaths");
            ResetProperty("Kills");
        }

        ResetProperty("Health");
        //GetComponent<PlayerInput>().enabled = true;
        //GetComponent<Collider>().enabled = true;
        //rb.useGravity = true;
    }

    

    private void ChangeValue(string key, int value)
    {
        GetProperties();
        properties[key] = value;
        PhotonNetwork.SetPlayerCustomProperties(properties);
        if(key == "Health")
        {
            playerClone.GetComponent<PlayerController>().SetHealthBar(value);
        }
    }

    private void ResetProperty(string key)
    {
        if (key == "Health")
        {
            ChangeValue(key, 100);
            playerClone.GetComponent<PlayerController>().SetMaxHealthBar(100);
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

    public void TakeDamage(int dmg, PhotonView attacker)
    {
        ChangeValue("Health", (int)(properties["Health"]) - dmg);

        if (GetProperty("Health") <= 0)
        {
            Increment("Deaths");
            Player killer = attacker.Owner;
            CreditKiller(killer);
            //Debug.Log(director.UITexts[4]);
            director.UITexts[4].SetText("", 3.0f, true);
            //Notification for "player" killed "player"
            eventsManager.GeneralNotification_S(killer.NickName + " has killed "  + PhotonNetwork.LocalPlayer.NickName, 2.0f, "CombatLog");
            Respawn();
            //director.AddToCombatLog(photonView, attacker);
        }
    }

    // Credit kill
    public void CreditKiller(Player killer)
    {
        //if (!photonView.IsMine) return;

        properties = killer.CustomProperties;
        properties["Kills"] = (int)(properties["Kills"]) + 1;
        killer.SetCustomProperties(properties);
    }

    private void Respawn()
    {
       Reset();
       PhotonNetwork.Destroy(playerClone);
       AvatarParent.transform.position = PlayerSpawns[team].transform.GetChild(Random.Range(0, 3)).transform.position;
       InitializeCharacter();
       playerClone.GetComponent<PhotonView>().RPC("BroadcastHealth", RpcTarget.All, playerClone.GetComponent<PhotonView>().Owner);
    }

   //Player Disconnect PHOTON Room script
   public override void OnPlayerLeftRoom (Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log(otherPlayer.NickName + " Has Left the game");
        eventsManager.GeneralNotification_S(otherPlayer.NickName + " Has Left the game", 2.0f, "PlayerDisconnect");
    }

    //Master client leave room
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " Has Disconnected");
        eventsManager.GeneralNotification_S(PhotonNetwork.LocalPlayer.NickName + " Has Disconnected", 2.0f, "PlayerDisconnect");
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
