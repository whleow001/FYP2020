using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerManager : MonoBehaviourPun/*, IPunObservable*/
{
    /*public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            // we own this player, send data to others
            stream.SendNext((int)_myCustomProperties["Health"]);
        }
        else
        {
            // network player, to receive data
            int value = (int)stream.ReceiveNext();
            //_myCustomProperties["Health"] = value;
            this._myCustomProperties["Health"] = value;
        }
    }*/

    // GameDirector reference
    private GameDirector director;
    private Rigidbody rb;

    // Hp Reference

    public Healthbar healthbar;

    private Text hp;
    //public Healthbar healthbar;
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    // Notification Panel Reference
    private NotificationPanelManager notificationPanel;
    private float panelTime = 3.0f;
    private float panelElapsedTime;
    private bool showPanel = false;

    // Layer references
    private int GOVT_LAYER = 9;
    private int REBEL_LAYER = 10;

    public bool kill = false;

    // Flags
    private bool instantiated = false;

    // Death timer
    private float deathTimer = 3;

    // Custom player properties
    private ExitGames.Client.Photon.Hashtable _myCustomProperties;

    // Start is called before the first frame update
    void Start() {
      GetProperties();
      rb = GetComponent<Rigidbody>();
      director = GameObject.Find("Director").GetComponent<GameDirector>();

      Reset();
      instantiated = true;

      notificationPanel = director.GetNotificationPanel();
    }

    void Update() {
      if (kill) {
        TakeDamage(100, photonView);
        kill = false;
      }

      notificationPanel.SetActiveState(showPanel);

      if (showPanel) {
        panelElapsedTime += Time.deltaTime;

        if (panelElapsedTime >= panelTime)
          showPanel = false;
      }
    }


    private void GetProperties() {
      _myCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;
    }

    public void Reset() {
      if (!instantiated) {
        ResetProperty("Deaths");
        ResetProperty("Kills");
      }

      ResetProperty("Health");
      GetComponent<PlayerInput>().enabled = true;
      GetComponent<Collider>().enabled = true;
      ChangeAlpha(1.0f);
      rb.useGravity = true;
    }

    public void SetHealthBar(int value)
    {
        slider.value = value;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void SetMaxHealthBar(int value)
    {
        slider.maxValue = 100;
        slider.value = 100;
        fill.color = gradient.Evaluate(1f);
    }

    private void ChangeValue(string key, int value) {
      GetProperties();
      _myCustomProperties[key] = value;
      PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
      if(key == "Health")
        {
            SetHealthBar(value);
        }
    }

    private void ResetProperty(string key) {
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

    IEnumerator Respawn() {
      rb.useGravity = false;
      GetComponent<Collider>().enabled = false;
      GetComponent<PlayerInput>().enabled = false;

      // Fade
      for (float i = deathTimer; i > 0; i -= 0.1f) {
        Notify("Respawn in 0:0" + (int)i, 0.1f, true);

        if (i > deathTimer - 1)
          ChangeAlpha(-0.1f, true);

        yield return new WaitForSeconds(.1f);
      }

      Reset();
      gameObject.transform.position = director.GetSpawnLocation(GetProperty("Team"));
      photonView.RPC("BroadcastHealth", RpcTarget.All, photonView.Owner);
    }

    private void ChangeAlpha(float value, bool incrementOrDecrement = false) {
      Color c = transform.Find("Player_Base_Cube.005").GetComponent<Renderer>().material.color;
      if (incrementOrDecrement)
        c.a += value;
      else
        c.a = value;
      transform.Find("Player_Base_Cube.005").GetComponent<Renderer>().material.color = c;
    }

    public void Increment(string key) {
      if (_myCustomProperties.ContainsKey(key))
        ChangeValue(key, GetProperty(key) + 1);
    }

    public int GetProperty(string key) {
      GetProperties();
      if (_myCustomProperties.ContainsKey(key))
        return (int)_myCustomProperties[key];
      return 0;
    }

    // Take Damage
    public void TakeDamage(int damage, PhotonView attacker) {
      if (!photonView.IsMine) return;

      ChangeValue("Health", (int)(_myCustomProperties["Health"]) - damage);
      //healthbar.SetHealth((int)_myCustomProperties["Health"]);
      Player victim = photonView.Owner;
      photonView.RPC("BroadcastHealth", RpcTarget.All, victim);

      if (GetProperty("Health") <= 0) {
        Increment("Deaths");
        Player killer = attacker.Owner;
        CreditKill(killer);
        StartCoroutine("Respawn");

        director.AddToCombatLog(photonView, attacker);
      }
    }

    // Credit kill
    public void CreditKill(Player killer) {
      if (!photonView.IsMine) return;

      _myCustomProperties = killer.CustomProperties;
      _myCustomProperties["Kills"] = (int)(_myCustomProperties["Kills"]) + 1;
      killer.SetCustomProperties(_myCustomProperties);
    }

    public void DisplayEndScreen() {
        //if (!photonView.IsMine) return;

        director.DisplayEndScreen();
    }

    public void WinText()
    {
        //if (!photonView.IsMine) return;

        director.WinText();
    }

    [PunRPC]
    void WinTextRPC()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player == photonView.Owner)
            {
                GetComponent<PlayerManager>().WinText();
            }
        }
    }

    public void Notify(string message, float seconds, bool ignoreCooldown = false, int layer = -1, Vector3 position = default(Vector3)) {
      if (gameObject.layer == layer || layer == -1) {
        if (!showPanel || (showPanel && ignoreCooldown)) {
          notificationPanel.SetText(message);
          panelElapsedTime = 0;
          panelTime = seconds;
        }
        //Debug.Log(position);

        showPanel = true;
      }
    }

    [PunRPC]
    void DisplayEndScreenRPC() {
        foreach(Player player in PhotonNetwork.PlayerList)
          if (player == photonView.Owner)
            GetComponent<PlayerManager>().DisplayEndScreen();
    }

    [PunRPC]
    void NotifyTeam(string message, Vector3 position, int layer, bool ignoreCooldown) {
      foreach (Player player in PhotonNetwork.PlayerList)
        if (player == photonView.Owner)
          GetComponent<PlayerManager>().Notify(message, 3, ignoreCooldown, layer, position);
    }
    
    [PunRPC]
    void BroadcastHealth(Player victim)
    {
        SetHealthBar((int)victim.CustomProperties["Health"]);
        Debug.Log("healthbar value: " + slider.value);
    }
    
}
