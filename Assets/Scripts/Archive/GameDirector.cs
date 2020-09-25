using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Xml.Serialization;
using Photon.Pun.UtilityScripts;

public class GameDirector : MonoBehaviourPun
{
    [SerializeField]
    private GameObject _mainCamera;
    [SerializeField]
    private GameObject fovMask;
    [SerializeField]
    private NotificationPanelManager notificationPanel;
    [SerializeField]
    private RespawnOverlayManager respawnPanel;
    [SerializeField]
    public EndGameScreen _endGameScreen;
    [SerializeField]
    private Text fps;
    [SerializeField]
    private Text kd;
    [SerializeField]
    private Text ping;
    [SerializeField]
    private List<GameObject> spawns = new List<GameObject>();
    [SerializeField]
    private List<GameObject> prefabs = new List<GameObject>();

    private PlayerManager playerManager;


    public int matchLength = 60;
    [SerializeField]
    private Text timer;
    public int currentMatchTime;
    private Coroutine timerCoroutine;

    private float deltaTime;

    public enum EventCodes : byte
    {
        //fill in for timer
        RefreshTimer
    }

    //public const byte alloFOV = 2;


    // Team no
    private int team;

    public int generatorCount = 4;

    // Layer references
    private int GOVT_LAYER = 9;
    private int REBEL_LAYER = 10;

    // flags
    private bool maskSet = false;
    private bool forcefieldDestroyed = false;

    // Events Manager
    private EventsManager eventsManager;
    private void Awake() {
        //team = (int)PhotonNetwork.LocalPlayer.CustomProperties["_pt"];
        if ((byte)PhotonNetwork.LocalPlayer.CustomProperties["_pt"] == 0)
            team = 0;
        else
            team = 1;

      eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();
      GameObject playerClone = MasterManager.NetworkInstantiate(prefabs[team], spawns[team].transform.GetChild(Random.Range(0, 3)).transform.position, Quaternion.identity);
      playerClone.GetComponent<PlayerController>().SpawnCamera(_mainCamera);
      playerManager = playerClone.GetComponent<PlayerManager>();

        //scale player minimap icon
        EditPlayerIcon(playerClone);

        // If client is a master client
        if (PhotonNetwork.IsMasterClient) {
        // Generate random generator spawns
        List<int> randomIndexes = new List<int>();

        while (randomIndexes.Count < generatorCount) {
          int randomIndex = Random.Range(0, generatorCount);
          if (!randomIndexes.Contains(randomIndex))
            randomIndexes.Add(randomIndex);
        }

        for (int i = 0; i < randomIndexes.Count; i++)
          MasterManager.RoomObjectInstantiate(prefabs[2], spawns[2].transform.GetChild(randomIndexes[i]).transform.position, Quaternion.identity);

        for (int i = 0; i < 2; i++)
          MasterManager.RoomObjectInstantiate(prefabs[3], spawns[3].transform.GetChild(i).transform.position, spawns[3].transform.GetChild(i).transform.rotation);
      }
    }

    void Start()
    {
        InitializeTimer();

    }

    void Update() {
      if(!maskSet)
        {
            AllocateFOVMask();
            maskSet = true;
        }
      if (generatorCount == 0 && !forcefieldDestroyed && PhotonNetwork.IsMasterClient) {
            GameObject[] forcefields = GameObject.FindGameObjectsWithTag("Forcefield");
            foreach (GameObject gameObject in forcefields)
              PhotonNetwork.Destroy(gameObject);

            forcefieldDestroyed = true;
            //Debug.Log(photonView);
            //playerManager.GetComponent<PhotonView>().RPC("DisplayEndScreenRPC", RpcTarget.AllViaServer, "Government Team Win!!");
            eventsManager.DisplayEndGame_S("Government Team Wins");
            //if game ends before timer runs out
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            currentMatchTime = 0;
            RefreshTimerUI();
      }

        MarkObjective();

      // Display fps
      deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
      float fpsValue = 1.0f/deltaTime;
      fps.text = Mathf.Ceil(fpsValue).ToString();

      // Display K/D
      kd.text = PhotonNetwork.LocalPlayer.CustomProperties["Kills"] + "/" + PhotonNetwork.LocalPlayer.CustomProperties["Deaths"];

      // Display ping
      ping.text = PhotonNetwork.GetPing().ToString();

      respawnPanel.gameObject.SetActive((int)PhotonNetwork.LocalPlayer.CustomProperties["Health"] <= 0);

      // Display Respawn Overlay
      if ((int)PhotonNetwork.LocalPlayer.CustomProperties["Health"] <= 0 && !respawnPanel.IsActive)
        respawnPanel.SetTimer((int)playerManager.deathTimer);

    }

    /*private void LateUpdate()
    {
        if (!maskSet)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                playerManager.GetComponent<PhotonView>().RPC("AllocateFOV", RpcTarget.AllViaServer);
                maskSet = true;
            }
            else
            {
                AllocateFOVMask();
                maskSet = true;
            }
        }
    }*/

   // private void OnEnable() {
    //    PhotonNetwork.AddCallbackTarget(this);
    //}

    //private void OnDisable()
   // {
     //   PhotonNetwork.RemoveCallbackTarget(this);
   // }

    private void InitializeTimer()
    {
        currentMatchTime = matchLength;
        RefreshTimerUI();

        if(PhotonNetwork.IsMasterClient)
        {
            timerCoroutine = StartCoroutine(Timer());
        }
    }

    public void RefreshTimerUI()
    {
        string minutes = (currentMatchTime / 60).ToString("00");
        string seconds = (currentMatchTime % 60).ToString("00");
        timer.text = $"{minutes}:{seconds}";
    }

    //public void RefreshTimer_S()
    //{
    //    object[] package = new object[] { currentMatchTime };

    //    PhotonNetwork.RaiseEvent(
    //        (byte)EventCodes.RefreshTimer,
    //        package,
    //        new RaiseEventOptions { Receivers = ReceiverGroup.All },
    //        new SendOptions { Reliability = true }
    //    );
    //}

    //public void RefreshTimer_R(object[] data)
    //{
    //    currentMatchTime = (int)data[0];
    //    //Debug.Log(currentMatchTime.ToString());
    //    RefreshTimerUI();
    //}

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(1f);

        currentMatchTime -= 1;
        //Debug.Log(currentMatchTime.ToString());

        if (currentMatchTime <= 0)
        {
            timerCoroutine = null;
            // playerManager.GetComponent<PhotonView>().RPC("DisplayEndScreenRPC", RpcTarget.AllViaServer, "Rebel Team Win!!");
            eventsManager.DisplayEndGame_S("Rebel Team Win");
            }
        else
        {
            //Debug.Log("Call sending function now");
            eventsManager.RefreshTimer_S();
            timerCoroutine = StartCoroutine(Timer());
        }
    }

    //public void OnEvent(EventData photonEvent)
    //{
    //    if (photonEvent.Code >= 200) return;

    //    EventCodes e = (EventCodes)photonEvent.Code;
    //    object[] o = (object[])photonEvent.CustomData;

    //    switch (e)
    //    {
    //        //fill in for timer
    //        case EventCodes.RefreshTimer:
    //            //Debug.Log("Receiving event now");
    //            RefreshTimer_R(o);
    //            break;
    //    }

    //}

    private void EditPlayerIcon(GameObject playerPrefab)
    {
        playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().drawMode = SpriteDrawMode.Sliced;
        playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().size = new Vector3(1.5f, 2.0f, 1f);
        playerPrefab.transform.GetComponentInChildren<SpriteRenderer>().drawMode = SpriteDrawMode.Simple;
    }


    public void AllocateFOVMask() {
      // Get all players
      GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
      foreach (GameObject player in allPlayers)
        if (player.layer == GetFactionLayer())
            {
                if(player.transform.Find("FieldOfView(Clone)") == null)
                {
                    AddMaskAsChild(player);
                }
                /*else
                {
                    Destroy(player.transform.Find("FieldOfView").gameObject);
                    AddMaskAsChild(player);
                }*/

            }

        // Get all generators
        if (GetFactionLayer() == REBEL_LAYER) {
        GameObject[] generators = GameObject.FindGameObjectsWithTag("Generator");

        foreach (GameObject generator in generators)
                if(generator.transform.Find("FieldOfView(Clone)") == null)
                {
                    AddMaskAsChild(generator);
                }
      }
    }

    private void AddMaskAsChild(GameObject _gameObject) {
      GameObject FOVObject = Instantiate(fovMask, new Vector3(_gameObject.transform.position.x, _gameObject.transform.position.y + 0.03f, _gameObject.transform.position.z), Quaternion.identity);
      FOVObject.transform.SetParent(_gameObject.transform);
    }

    private void MarkObjective()
    {
        float fovDistance = fovMask.transform.localScale.x / 2 - 2;
        GameObject[] allGenerators = GameObject.FindGameObjectsWithTag("Generator");
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        if (allGenerators != null && allPlayers != null)
        {
            foreach (GameObject player in allPlayers)
            {
                //mark government team players' objective
                if (player.layer == GOVT_LAYER)
                {
                    foreach (GameObject generator in allGenerators)
                    {
                        float distance = Vector3.Distance(generator.transform.position, player.transform.position);
                        GameObject iconVisible = generator.transform.Find("GeneratorIcon").gameObject;

                        if (distance < fovDistance && iconVisible.activeSelf == false)
                        {
                            iconVisible.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    private int GetFactionLayer() {
      return team == 0 ? GOVT_LAYER : REBEL_LAYER;
    }

    public void AddToCombatLog(PhotonView victimID, PhotonView killerID) {
        Player victim = victimID.Owner;
        Player killer = killerID.Owner;

        //Debug.Log("Killer's health': " + photonView.GetComponent<PlayerManager>().GetProperty("Health"));
        Debug.Log(killerID + " has killed " + victimID);
    }

    public Vector3 GetSpawnLocation(int team) {
      return spawns[team-1].transform.GetChild(Random.Range(0, 3)).transform.position;
    }

    public void DecrementGeneratorCount() {
      generatorCount--;
    }

    //public void DisplayEndScreen(string message) {
    //    _endGameScreen.Show(message);
    //}

    public NotificationPanelManager GetNotificationPanel() {
      return notificationPanel;
    }

    public PlayerManager GetPlayerManager() {
      return playerManager;
    }
}
