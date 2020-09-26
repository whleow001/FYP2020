using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public abstract class GameDirector : MonoBehaviourPun {
  // Team Number
  private int teamIndex;

  // Layer references
  private int GOVT_LAYER = 9;
  private int REBEL_LAYER = 10;

  // fps tracker
  private float deltaTime;

  // UI References
  [Header("UI Texts")]
  [SerializeField]
  protected List<UIText> UITexts = new List<UIText>();

  // Spawns
  [Header("Spawns")]
  [SerializeField]
  protected List<GameObject> spawns = new List<GameObject>();

  // Prefabs
  [Header("Prefabs")]
  [SerializeField]
  protected List<GameObject> prefabs = new List<GameObject>();

  [Header("Misc References")]
  [SerializeField]
  protected PlayerManager playerManager;
  public EndGameScreen _endGameScreen;

    //fov mask variable for child class to access
    [SerializeField]
    protected GameObject fovMask;

    // flags
    private bool maskSet = false;
    protected bool forcefieldDestroyed = false;

    //ported from old game director regarding codes with event manager
    public int matchLength = 60;
    [SerializeField]
    private Text timer;
    public int currentMatchTime;
    protected Coroutine timerCoroutine;

    // Events Manager
    protected EventsManager eventsManager;


    //Common UITexts
    //==============
    //0 - k/d
    //1 - fps
    //2 - ping

    //Common Spawns
    //=============
    //0 - GovtSpawn
    //1 - RebelSpawn


    private void Awake() {
    //teamIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
    if ((byte)PhotonNetwork.LocalPlayer.CustomProperties["_pt"] == 0)
        teamIndex = 0;
    else
        teamIndex = 1;

    eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();

    //GameObject notificationPanel = GameObject.Find("NotificaltionPanel");
    //notificationText = notificationPanel.transform.GetChild(0).GetComponent<Text>();

    //playerManager.InstantiatePrefab(prefabs[teamIndex], spawns[teamIndex]);

    // Initialize scene specific objects
    InitializeGameObjects();
  }

    private void Start()
    {
        InitializeTimer();
    }

    protected virtual void Update() {
    if (!maskSet)
    {
        AllocateFOVMask();
        maskSet = true;
    }

    // Update K/D
    UITexts[0].SetText(PhotonNetwork.LocalPlayer.CustomProperties["Kills"] + "/" + PhotonNetwork.LocalPlayer.CustomProperties["Deaths"]);

    // Update fps
    deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    float fpsValue = 1.0f/deltaTime;
    UITexts[1].SetText(Mathf.Ceil(fpsValue).ToString());

    // Update ping
    UITexts[2].SetText(PhotonNetwork.GetPing().ToString());

    // Update scene specific texts
    UpdateUITexts();
  }

  // Abstract function to be overridden
  protected abstract void InitializeGameObjects();
  protected abstract void UpdateUITexts();

  // Gets own faction layer
  public int GetFactionLayer() {
    return teamIndex == 0 ? GOVT_LAYER : REBEL_LAYER;
  }

  // Gets the opponent's faction layer
  public int GetOtherFactionLayer() {
    return teamIndex == 1 ? REBEL_LAYER : GOVT_LAYER;
  }

  public int GetTeamIndex()
    {
        return teamIndex;
    }

  public int GetOtherTeamIndex()
    {
        return teamIndex;
    }

    public void AllocateFOVMask()
    {
        // Get all players
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in allPlayers)
            if (player.layer == GetFactionLayer())
            {
                if (player.transform.Find("FieldOfView(Clone)") == null)
                {
                    AddMaskAsChild(player);
                }
            }

        // Get all generators
        if (GetFactionLayer() == REBEL_LAYER)
        {
            GameObject[] generators = GameObject.FindGameObjectsWithTag("Generator");

            foreach (GameObject generator in generators)
                if (generator.transform.Find("FieldOfView(Clone)") == null)
                {
                    AddMaskAsChild(generator);
                }
        }
    }

    private void AddMaskAsChild(GameObject _gameObject)
    {
        GameObject FOVObject = Instantiate(fovMask, new Vector3(_gameObject.transform.position.x, _gameObject.transform.position.y + 0.03f, _gameObject.transform.position.z), Quaternion.identity);
        FOVObject.transform.SetParent(_gameObject.transform);
    }

    private void InitializeTimer()
    {
        currentMatchTime = matchLength;
        RefreshTimerUI();

        if (PhotonNetwork.IsMasterClient)
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

    public UIText GetNotificationPanel()
    {
        return UITexts[3];
    }

    public void DisplayEndGameScreen()
    {
        string message = "hello";
        _endGameScreen.Show(message);
    }

    /* this function might not be needed, if we credit killer from playerManager
    public void CreditKiller()
    {
        //playerManager.CreditKill(PhotonView);
    }
    */

    protected void ChangeCharacter()
    {

    }


}
