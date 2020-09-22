using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

//public class RebelHQ_A : GameDirector {
  /*
  Scene specific UITexts
  ======================

  Scene specific Spawns
  =====================
  2 - GeneratorSpawn
  3 - ForcefieldSpawn

  Scene specific Prefabs
  =======================
  2 - Generator
  3 - Forcefield
  */

/*
[SerializeField]
private int generatorCount;

// Initialize scene specific game objects
protected override void InitializeGameObjects() {
// If client is a master client
if (PhotonNetwork.IsMasterClient) {
  // Generate random generator spawns
  List<int> randomIndexes = new List<int>();

  while (randomIndexes.Count < generatorCount) {
    int randomIndex = Random.Range(0, generatorCount);
    if (!randomIndexes.Contains(randomIndex))
      randomIndexes.Add(randomIndex);
  }

  // Spawn generators
  for (int i = 0; i < randomIndexes.Count; i++)
    MasterManager.RoomObjectInstantiate(prefabs[2], spawns[2].transform.GetChild(randomIndexes[i]).transform.position, Quaternion.identity);

  // Spawn forcefields
  for (int i = 0; i < 2; i++)
    MasterManager.RoomObjectInstantiate(prefabs[3], spawns[3].transform.GetChild(i).transform.position, spawns[3].transform.GetChild(i).transform.rotation);
}
}

// Updates scene specific texts
protected override void UpdateUITexts() {

}

// Decrement generator count
public void DecrementGeneratorCount() {
generatorCount--;
}
}
*/