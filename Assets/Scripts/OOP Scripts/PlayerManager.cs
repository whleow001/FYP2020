using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    //reference current scene gamedirector
    [SerializeField]
    private GameDirector director;

    [SerializeField]
    private GameObject _mainCamera;

    // Spawns
    [Header("Spawns")]
    [SerializeField]
    protected List<GameObject> PlayerSpawns = new List<GameObject>();

    // Prefabs
    [Header("Prefabs")]
    [SerializeField]
    protected List<GameObject> playerPrefabs = new List<GameObject>();

    // Custom player properties
    private ExitGames.Client.Photon.Hashtable properties;

    private GameObject selectedCharacter;

    private Material selectedMaterial;

    private Vector3 spawnPoint;

    private bool instantiated = false;
    private GameObject playerClone;
    private int team;   // team number;

    // Events Manager
    protected EventsManager eventsManager;

    // Start is called before the first frame update
    void Start()
    {
        GetProperties();
        director = GameObject.Find("Director").GetComponent<GameDirector>();

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

    public void SetProperties(Material material, Vector3 spawn)
    {

    }

    public void ChangeCharacter(GameObject selectedCharacter)
    {

    }

    private void InitializeCharacter()
    {
        team = director.GetTeamIndex();
        playerClone = MasterManager.NetworkInstantiate(playerPrefabs[team], PlayerSpawns[team].transform.GetChild(Random.Range(0, 3)).transform.position, Quaternion.identity);
        playerClone.GetComponent<PlayerController>().SpawnCamera(_mainCamera);
    }

    private void EditPlayerIcon(GameObject playerPrefab)
    {
        playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().drawMode = SpriteDrawMode.Sliced;
        playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().size = new Vector3(1.5f, 2.0f, 1f);
        playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().drawMode = SpriteDrawMode.Simple;
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
            eventsManager.GeneralNotification_S(killer.NickName + " has killed "  +PhotonNetwork.LocalPlayer.NickName, 2.0f);
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
       playerClone.transform.position = PlayerSpawns[team].transform.GetChild(Random.Range(0, 3)).transform.position;
       playerClone.GetComponent<PhotonView>().RPC("BroadcastHealth", RpcTarget.All, playerClone.GetComponent<PhotonView>().Owner);
    }
}
