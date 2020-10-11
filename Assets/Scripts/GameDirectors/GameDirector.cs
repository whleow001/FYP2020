using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class BaseTexts
{
  public const int kd = 0;
  public const int fps = 1;
  public const int ping = 2;
  public const int notify = 3;
  public const int respawn = 4;
  public const int disconnect = 5;
}

public class BaseSpawns
{
  public const int Government = 0;
  public const int Rebel = 1;
}

public class BasePrefabs
{
  public const int fovMask = 0;
  public const int Gunslinger = 1;
  public const int Sniper = 2;
  public const int Juggernaut = 3;
}

public abstract class GameDirector : MonoBehaviourPun {
    // Team Number
    private int teamIndex;

    // Layer references
    protected int GOVT_LAYER = 9;
    protected int REBEL_LAYER = 10;

    // fps tracker
    private float deltaTime;

    // UI References
    // had to change protection level for eventsmanager access
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

    [Header("References")]
    [SerializeField]
    protected PlayerManager playerManager;
    [SerializeField]
    protected EventsManager eventsManager;

    [Header("Overlays")]
    protected EndGameScreen _endGameScreen;

    // flags
    private bool maskSet = false;

    //ported from old game director regarding codes with event manager
    public int matchLength = 60;
    [SerializeField]
    private Text timer;
    public int currentMatchTime;
    protected Coroutine timerCoroutine;

    private GameObject charPanel;
    private int charIndex;

    private Button char1Button;
    private Button char2Button;
    private Button char3Button;

    private Text char1text;
    private Text char2text;
    private Text char3text;

    private string selectedText = "Selected";
    private string unselectedText = "Select";


    private void Awake() {
      teamIndex = (byte)PhotonNetwork.LocalPlayer.CustomProperties["_pt"];

      charPanel = GameObject.Find("CharacterSelectionOverlay");
      char1Button = GameObject.Find("Char1").GetComponent<Button>();
      char2Button = GameObject.Find("Char2").GetComponent<Button>();
      char3Button = GameObject.Find("Char3").GetComponent<Button>();

      char1text = char1Button.GetComponentInChildren<Text>();
      char2text = char2Button.GetComponentInChildren<Text>();
      char3text = char3Button.GetComponentInChildren<Text>();
      charPanel.SetActive(false);

      // Initialize scene specific objects
      InitializeGameObjects();
    }

    private void Start()
    {
        InitializeTimer();
    }

    private void Update()
    {
      if (!maskSet)
      {
          AllocateFOVMask();
          maskSet = true;
      }

      //Debug.Log("Update");

      // Update K/D
      GetUIText(Texts.kd).SetText(PhotonNetwork.LocalPlayer.CustomProperties["Kills"] + "/" + PhotonNetwork.LocalPlayer.CustomProperties["Deaths"]);

      // Update fps
      deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
      float fpsValue = 1.0f/deltaTime;
      GetUIText(Texts.fps).SetText(Mathf.Ceil(fpsValue).ToString());

      // Update ping
      GetUIText(Texts.ping).SetText(PhotonNetwork.GetPing().ToString());

      // Update scene specific texts
      UpdateUITexts();

      // Update scene specific objectives
      UpdateObjectives();
    }

    public UIText GetUIText(int index)
    {
      return UITexts[index];
    }

    public GameObject GetSpawn(int index)
    {
      return spawns[index];
    }

    public GameObject GetPrefab(int index)
    {
      return prefabs[index];
    }

    // Abstract functions to be overridden
    protected abstract void InitializeGameObjects();
    protected abstract void UpdateUITexts();
    protected abstract void UpdateObjectives();

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
        GameObject FOVObject = Instantiate(GetPrefab((int)Prefabs.fovMask), new Vector3(_gameObject.transform.position.x, _gameObject.transform.position.y + 0.03f, _gameObject.transform.position.z), Quaternion.identity);
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

    public EndGameScreen GetEndGameScreen() {
      return _endGameScreen;
    }

    //public void DisplayEndGameScreen()
    //{
    //    string message = "hello";
    //    _endGameScreen.Show(message);
    //}

    /* this function might not be needed, if we credit killer from playerManager
    public void CreditKiller()
    {
        //playerManager.CreditKill(PhotonView);
    }
    */

    public void ChangeCharacter()
    {
        charPanel.SetActive(true);
    }

    public void char1ButtonPress()
    {
        charIndex = 0;
        playerManager.ChangeCharacter(charIndex);
        char1text.text = selectedText;
        char2text.text = unselectedText;
        char3text.text = unselectedText;
        char1Button.interactable = false;
        char2Button.interactable = true;
        char3Button.interactable = true;
        charPanel.SetActive(false);
    }

    public void char2ButtonPress()
    {
        charIndex = 1;
        playerManager.ChangeCharacter(charIndex);
        char1text.text = unselectedText;
        char2text.text = selectedText;
        char3text.text = unselectedText;
        char1Button.interactable = true;
        char2Button.interactable = false;
        char3Button.interactable = true;
        charPanel.SetActive(false);
    }

    public void char3ButtonPress()
    {
        charIndex = 2;
        playerManager.ChangeCharacter(charIndex);
        char1text.text = unselectedText;
        char2text.text = unselectedText;
        char3text.text = selectedText;
        char1Button.interactable = true;
        char2Button.interactable = true;
        char3Button.interactable = false;
        charPanel.SetActive(false);
    }
}
