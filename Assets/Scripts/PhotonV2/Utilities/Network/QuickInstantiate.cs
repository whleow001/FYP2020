using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class QuickInstantiate : MonoBehaviourPun
{
    [SerializeField]
    private GameObject _mainCamera;
    [SerializeField]
    private List<GameObject> spawns = new List<GameObject>();
    [SerializeField]
    private List<GameObject> prefabs = new List<GameObject>();
    //[SerializeField]
    //private List<Transform> generatorSpawn = new List<Transform>();
    //[SerializeField]
    //private GameObject generator;

    private void Awake() {
      if (!photonView.IsMine) return;

      foreach (Player player in PhotonNetwork.PlayerList) {
          int index = (int)player.CustomProperties["Team"] - 1;
          //Vector3 spawnLocation = Random.insideUnitSphere * spawns[index].renderer.bounds.extents.magnitude;

          Debug.Log(prefabs[index]);
          GameObject playerClone = MasterManager.NetworkInstantiate(prefabs[index], spawns[index].transform.position, Quaternion.identity);
          playerClone.GetComponent<PlayerController>().SpawnCamera(_mainCamera);

          //cameraClone.GetComponent<CameraMotor>().SetPlayer(playerClone);
      }
    }
}
