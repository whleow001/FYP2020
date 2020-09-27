using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RebelHQ_A : GameDirector {
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


[SerializeField]
private int generatorCount;

protected override void Update()
{
        base.Update();
        
        if (generatorCount == 0 && !forcefieldDestroyed && PhotonNetwork.IsMasterClient)
        {
            GameObject[] forcefields = GameObject.FindGameObjectsWithTag("Forcefield");
            foreach (GameObject gameObject in forcefields)
                PhotonNetwork.Destroy(gameObject);

            forcefieldDestroyed = true;
            //Debug.Log(photonView);
            //playerManager.GetComponent<PhotonView>().RPC("DisplayEndScreenRPC", RpcTarget.AllViaServer, "Government Team Win!!");
            eventsManager.DisplayEndGame_S("Government Team Wins");
            //if game ends before timer runs out
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);
            currentMatchTime = 0;
            RefreshTimerUI();
        }
        
 }

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
    MasterManager.RoomObjectInstantiate(prefabs[0], spawns[0].transform.GetChild(randomIndexes[i]).transform.position, Quaternion.identity);

  // Spawn forcefields
  for (int i = 0; i < 2; i++)
    MasterManager.RoomObjectInstantiate(prefabs[1], spawns[1].transform.GetChild(i).transform.position, spawns[1].transform.GetChild(i).transform.rotation);
}
}

// Updates scene specific texts
protected override void UpdateUITexts() {

}

// Decrement generator count
public void DecrementGeneratorCount() {
        generatorCount--;
        //notfication panel index in the UITexts List
        //UITexts[3].SetText("Generator is destroyed", 2.0f);
    }

}

