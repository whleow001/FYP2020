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
      _myCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;
      director = GameObject.Find("Director").GetComponent<GameDirector>();

      Reset();
      instantiated = true;

      hp = GameObject.Find("Scoreboard").GetComponent<Text>();
    }

    void Update() {
      hp.text = _myCustomProperties["Health"].ToString() + "\n" + _myCustomProperties["Kills"].ToString() + " - " + _myCustomProperties["Deaths"].ToString();
    }

    public void Reset() {
      if (!instantiated) {
        ResetProperty("Deaths");
        ResetProperty("Kills");
      }
      ResetProperty("Health");
    }

    private void ChangeValue(string key, int value) {
      _myCustomProperties[key] = value;
      PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
    }

    private void ResetProperty(string key) {
      if (key == "Health")
        ChangeValue(key, 100);
      else
        ChangeValue(key, 0);
    }

    public void Increment(string key) {
      if (_myCustomProperties.ContainsKey(key))
        ChangeValue(key, (int)(_myCustomProperties[key]) + 1);
    }

    public int GetProperty(string key) {
      if (_myCustomProperties.ContainsKey(key))
        return (int)_myCustomProperties[key];
      return 0;
    }

    // Take Damage
    public bool TakeDamage(int damage) {
      if (!photonView.IsMine) return false;

      ChangeValue("Health", (int)(_myCustomProperties["Health"]) - damage);

      if ((int)_myCustomProperties["Health"] <= 0) {
        Increment("Deaths");
        ChangeValue("Health", 100);
        return true;
      }
      return false;
    }
}
