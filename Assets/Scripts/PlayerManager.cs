using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerManager : MonoBehaviourPun
{
    // GameDirector reference
    private GameDirector director;

    // Flags
    private bool instantiated = false;

    // Custom player properties
    private ExitGames.Client.Photon.Hashtable _myCustomProperties = new ExitGames.Client.Photon.Hashtable();

    // Start is called before the first frame update
    void Start() {
      _myCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;
      director = GameObject.Find("GameDirector").GetComponent<GameDirector>();

      Reset();
      instantiated = true;
    }

    public void Reset() {
      if (!instantiated) {
        Reset("Deaths");
        Reset("Kills");
      }
      Reset("Health");
    }

    private void ChangeValue(string key, int value) {
      _myCustomProperties[key] = value;
      PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
    }

    private void Reset(string key) {
      if (_myCustomProperties.ContainsKey(key)) {
          if (key == "Health")
            ChangeValue(key, 100);
          else
            ChangeValue(key, 0);
      }
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
    public bool TakeDamage(int damage, Player attacker) {
      ChangeValue("Health", (int)(_myCustomProperties["Health"]) - damage);

      if ((int)_myCustomProperties["Health"] <= 0) {
        Increment("Deaths");
        return true;
      }
      return false;
    }
}
