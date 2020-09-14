using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Xml.Serialization;

public class GameDirector : MonoBehaviourPun, IOnEventCallback
{
    [SerializeField]
    private GameObject _mainCamera;
    [SerializeField]
    private GameObject fovMask;
    [SerializeField]
    private GameObject notificationPanel;
    [SerializeField]
    private EndGameScreen _endGameScreen;
    [SerializeField]
    private List<GameObject> spawns = new List<GameObject>();
    [SerializeField]
    private List<GameObject> prefabs = new List<GameObject>();

    private PlayerManager playerManager;


    public int matchLength = 180;
    [SerializeField]
    private Text timer; 
    private int currentMatchTime;
    private Coroutine timerCoroutine;

    public enum EventCodes : byte
    {
        //fill in for timer
        RefreshTimer
    }


    // Team no
    private int team;

    public int generatorCount = 4;

    // Layer references
    private int GOVT_LAYER = 9;
    private int REBEL_LAYER = 10;

    // flags
    private bool maskSet = false;
    private bool forcefieldDestroyed = false;

    private void Awake() {
      team = (int)PhotonNetwork.LocalPlayer.CustomProperties["Team"] - 1;

      GameObject playerClone = MasterManager.NetworkInstantiate(prefabs[team], spawns[team].transform.GetChild(Random.Range(0, 3)).transform.position, Quaternion.identity);
      playerClone.GetComponent<PlayerController>().SpawnCamera(_mainCamera);
        playerManager = playerClone.GetComponent<PlayerManager>();

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
      if (!maskSet)
        AllocateFOVMask();

      if (generatorCount == 0 && !forcefieldDestroyed && PhotonNetwork.IsMasterClient) {
            GameObject[] forcefields = GameObject.FindGameObjectsWithTag("Forcefield");
            foreach (GameObject gameObject in forcefields)
              PhotonNetwork.Destroy(gameObject);

            forcefieldDestroyed = true;
            //Debug.Log(photonView);
            playerManager.GetComponent<PhotonView>().RPC("WinTextRPC", RpcTarget.AllViaServer);
            playerManager.GetComponent<PhotonView>().RPC("DisplayEndScreenRPC", RpcTarget.AllViaServer);
            //if game ends before timer runs out 
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            currentMatchTime = 0;
            RefreshTimerUI();
        }
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void InitializeTimer()
    {
        currentMatchTime = matchLength;
        RefreshTimerUI();

        if(PhotonNetwork.IsMasterClient)
        {
            timerCoroutine = StartCoroutine(Timer());
        }
    }

    private void RefreshTimerUI()
    {
        string minutes = (currentMatchTime / 60).ToString("00");
        string seconds = (currentMatchTime % 60).ToString("00");
        timer.text = $"{minutes}:{seconds}";
    }

    public void RefreshTimer_S()
    {
        object[] package = new object[] { currentMatchTime };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.RefreshTimer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void RefreshTimer_R(object[] data)
    {
        currentMatchTime = (int)data[0];
        //Debug.Log(currentMatchTime.ToString());
        RefreshTimerUI();
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(1f);

        currentMatchTime -= 1;
        //Debug.Log(currentMatchTime.ToString());

        if (currentMatchTime <= 0)
        {
            timerCoroutine = null;
            playerManager.GetComponent<PhotonView>().RPC("WinTextRPC", RpcTarget.AllViaServer);
            playerManager.GetComponent<PhotonView>().RPC("DisplayEndScreenRPC", RpcTarget.AllViaServer);
        }
        else
        {
            //Debug.Log("Call sending function now");
            RefreshTimer_S();
            timerCoroutine = StartCoroutine(Timer());
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code >= 200) return;

        EventCodes e = (EventCodes)photonEvent.Code;
        object[] o = (object[])photonEvent.CustomData;

        switch (e)
        {
            //fill in for timer
            case EventCodes.RefreshTimer:
                //Debug.Log("Receiving event now");
                RefreshTimer_R(o);
                break;
        }

    }

    private void AllocateFOVMask() {
      // Get all players
      GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
      foreach (GameObject player in allPlayers)
        if (player.layer == GetFactionLayer())
            AddMaskAsChild(player);

      // Get all generators
      if (GetFactionLayer() == REBEL_LAYER) {
        GameObject[] generators = GameObject.FindGameObjectsWithTag("Generator");

        foreach (GameObject generator in generators)
          AddMaskAsChild(generator);
      }

      maskSet = true;
    }

    private void AddMaskAsChild(GameObject _gameObject) {
      GameObject FOVObject = Instantiate(fovMask, new Vector3(_gameObject.transform.position.x, _gameObject.transform.position.y + 0.03f, _gameObject.transform.position.z), Quaternion.identity);
      FOVObject.transform.SetParent(_gameObject.transform);
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

    public void DisplayEndScreen() {
        _endGameScreen.Show();
        //_endGameScreen.SetWinText();
    }

    public void WinText()
    {
        _endGameScreen.SetWinText();
    }

    public GameObject GetNotificationPanel() {
      return notificationPanel;
    }

    public PlayerManager GetPlayerManager() {
      return playerManager;
    }
}
