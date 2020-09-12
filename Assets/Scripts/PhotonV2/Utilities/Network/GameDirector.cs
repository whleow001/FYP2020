using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class GameDirector : MonoBehaviourPun
{
    [SerializeField]
    private GameObject _mainCamera;
    [SerializeField]
    private GameObject fovMask;
    [SerializeField]
    private List<GameObject> spawns = new List<GameObject>();
    [SerializeField]
    private List<GameObject> prefabs = new List<GameObject>();

    // Team no
    private int team;

    // Layer references
    private int GOVT_LAYER = 9;
    private int REBEL_LAYER = 10;

    // flags
    public bool maskSet = false;

    private void Awake() {
      team = (int)PhotonNetwork.LocalPlayer.CustomProperties["Team"] - 1;

      GameObject playerClone = MasterManager.NetworkInstantiate(prefabs[team], spawns[team].transform.GetChild(Random.Range(0, 3)).transform.position, Quaternion.identity);
      playerClone.GetComponent<PlayerController>().SpawnCamera(_mainCamera);

      // If client is a master client
      if (PhotonNetwork.IsMasterClient) {
        // Generate random generator spawns
        List<int> randomIndexes = new List<int>();

        while (randomIndexes.Count < 4) {
          int randomIndex = Random.Range(0, 4);
          if (!randomIndexes.Contains(randomIndex))
            randomIndexes.Add(randomIndex);
        }

        for (int i = 0; i < randomIndexes.Count; i++)
          MasterManager.RoomObjectInstantiate(prefabs[2], spawns[2].transform.GetChild(randomIndexes[i]).transform.position, Quaternion.identity);
      }
    }

    void Update() {
      if (!maskSet)
        AllocateFOVMask();
    }

    private void AllocateFOVMask() {
      // Get all players
      GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
      foreach (GameObject player in allPlayers)
        if (player.layer == GetFactionLayer())
            AddMaskAsChild(player);

      // Get all generators
      if (team == 1) {
        GameObject[] generators = GameObject.FindGameObjectsWithTag("Generator");

        foreach (GameObject generator in generators) {
          AddMaskAsChild(generator);
        }
      }



      maskSet = true;
    }

    private void AddMaskAsChild(GameObject _gameObject) {
      Debug.Log(_gameObject);
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

      //killerID.GetComponent<PlayerManager>().CreditKill();
    }

    public Vector3 GetSpawnLocation(int team) {
      return spawns[team-1].transform.GetChild(Random.Range(0, 3)).transform.position;
    }
}
