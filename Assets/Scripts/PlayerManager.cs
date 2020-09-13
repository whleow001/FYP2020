using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerManager : MonoBehaviourPun
{
    // GameDirector reference
    private GameDirector director;

    // Hp Reference
    private Text hp;

    // Flags
    private bool instantiated = false;

    // Custom player properties
    private ExitGames.Client.Photon.Hashtable _myCustomProperties;

    // Start is called before the first frame update
    void Start() {
      GetProperties();
      director = GameObject.Find("Director").GetComponent<GameDirector>();

      Reset();
      instantiated = true;

      hp = GameObject.Find("Scoreboard").GetComponent<Text>();

      //DontDestroyOnLoad(gameObject);
    }

    void Update() {
      hp.text = _myCustomProperties["Health"].ToString() + "\n" + _myCustomProperties["Kills"].ToString() + " - " + _myCustomProperties["Deaths"].ToString();
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
    }

    private void ChangeValue(string key, int value) {
      GetProperties();
      _myCustomProperties[key] = value;
      PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
    }

    private void ResetProperty(string key) {
      if (key == "Health")
        ChangeValue(key, 100);
      else
        ChangeValue(key, 0);
    }

    private void Respawn() {
      Reset();
      gameObject.transform.position = director.GetSpawnLocation(GetProperty("Team"));
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

      if (GetProperty("Health") <= 0) {
        Increment("Deaths");
        Player killer = attacker.Owner;
        CreditKill(killer);
        Respawn();

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

    public PhotonView GetPhotonView()
    {
        return photonView;
    }

    public void DisplayEndScreen()
    {
        director.DisplayEndScreen();
    }

    [PunRPC]
    void DisplayEndScreenRPC()
    {
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            if (player == photonView.Owner)
            {
                GetComponent<PlayerManager>().DisplayEndScreen();
            }
        }
    }
}
