using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class GameDirector : MonoBehaviourPun
{
    // Custom Room properties
    private ExitGames.Client.Photon.Hashtable _roomCustomProperties;

    // Generators
    GameObject[] generators;

    [SerializeField]
    private GameObject _mainCamera;
    [SerializeField]
    private List<GameObject> spawns = new List<GameObject>();
    [SerializeField]
    private List<GameObject> prefabs = new List<GameObject>();

    private void Awake() {
      int index = (int)PhotonNetwork.LocalPlayer.CustomProperties["Team"] - 1;

      GameObject playerClone = MasterManager.NetworkInstantiate(prefabs[index], spawns[index].transform.GetChild(Random.Range(0, 3)).transform.position, Quaternion.identity);
      playerClone.GetComponent<PlayerController>().SpawnCamera(_mainCamera);

      GetRoomProperties();

      // Generate random generator spawns
      List<int> randomIndexes = new List<int>();

      while (randomIndexes.Count < 4) {
        int randomIndex = Random.Range(0, 4);
        if (!randomIndexes.Contains(randomIndex))
          randomIndexes.Add(randomIndex);
      }

      // If client is a master client
      if (PhotonNetwork.IsMasterClient) {
        for (int i = 0; i < randomIndexes.Count; i++) {
          MasterManager.RoomObjectInstantiate(prefabs[2], spawns[2].transform.GetChild(randomIndexes[i]).transform.position, Quaternion.identity);
          ChangeValue("Generator" + i, 100);
        }
      }

      generators = GameObject.FindGameObjectsWithTag("Generator");
    }

    private void Update() {
      if (PhotonNetwork.IsMasterClient)
        for (int i = 0; i < generators.Length; i++)
          if (GetProperty("Generator" + i) <= 0 && generators[i])
            PhotonNetwork.Destroy(generators[i]);
    }

    private void GetRoomProperties() {
      _roomCustomProperties = PhotonNetwork.CurrentRoom.CustomProperties;
    }

    private void ChangeValue(string key, int value) {
      GetRoomProperties();
      _roomCustomProperties[key] = value;
      PhotonNetwork.CurrentRoom.SetCustomProperties(_roomCustomProperties);
    }

    public int GetProperty(string key) {
      GetRoomProperties();
      if (_roomCustomProperties.ContainsKey(key))
        return (int)_roomCustomProperties[key];
      return 0;
    }

    public void AddToCombatLog(PhotonView victimID, PhotonView killerID) {
      //Player victim = Player.Find(victimID.ViewID);
      //Player killer = Player.Find(killerID.ViewID);

      //Debug.Log("Killer's health': " + photonView.GetComponent<PlayerManager>().GetProperty("Health"));

      killerID.GetComponent<PlayerManager>().CreditKill();
    }

    public Vector3 GetSpawnLocation(int team) {
      return spawns[team-1].transform.GetChild(Random.Range(0, 3)).transform.position;
    }

    public void DamageGenerator(GameObject generator, int damage) {
      for (int i = 0; i < generators.Length; i++)
        if (generators[i] == generator)
          ChangeValue("Generator" + i, GetProperty("Generator" + i) - 20);
    }
}
