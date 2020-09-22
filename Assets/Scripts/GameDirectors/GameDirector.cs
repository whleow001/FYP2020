using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

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
  protected List<UIText> UITexts = new List<UIText>();

  // Spawns
  [Header("Spawns")]
  protected List<GameObject> spawns = new List<GameObject>();

  // Prefabs
  [Header("Prefabs")]
  protected List<GameObject> prefabs = new List<GameObject>();

  [Header("Misc References")]
  protected PlayerManager playerManager;

  /*
  Common UITexts
  ==============
  0 - k/d
  1 - fps
  2 - ping

  Common Spawns
  =============
  0 - GovtSpawn
  1 - RebelSpawn
  */

  private void Awake() {
    teamIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["Team"];

    //playerManager.InstantiatePrefab(prefabs[teamIndex], spawns[teamIndex]);

    // Initialize scene specific objects
    InitializeGameObjects();
  }

  private void Update() {
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
}
