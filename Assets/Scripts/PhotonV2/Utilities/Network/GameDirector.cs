﻿using System.Collections;
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
    private List<GameObject> spawns = new List<GameObject>();
    [SerializeField]
    private List<GameObject> prefabs = new List<GameObject>();

    private void Awake() {
      int index = (int)PhotonNetwork.LocalPlayer.CustomProperties["Team"] - 1;

      GameObject playerClone = MasterManager.NetworkInstantiate(prefabs[index], spawns[index].transform.GetChild(Random.Range(0, 3)).transform.position, Quaternion.identity);
      playerClone.GetComponent<PlayerController>().SpawnCamera(_mainCamera);
    }
}
