using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Texts : BaseTexts
{

}

public class Spawns : BaseSpawns
{
  public const int Generator = 2;
  public const int Forcefield = 3;
}

public class Prefabs : BasePrefabs
{
  public const int Generator = 4;
  public const int Forcefield = 5;
}

public class RebelHQ_A : GameDirector {
  // Win/Loss condition
  [SerializeField]
  private int generatorCount;

  // Flags
  private bool forcefieldDestroyed = false;

  // Update scene specific objectives
  protected override void UpdateObjectives() {
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

    MarkObjective();
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
        MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs.Generator), GetSpawn(Spawns.Generator).transform.GetChild(randomIndexes[i]).transform.position, Quaternion.identity);

      // Spawn forcefields
      for (int i = 0; i < 2; i++)
        MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs.Forcefield), GetSpawn(Spawns.Forcefield).transform.GetChild(i).transform.position, GetSpawn(Spawns.Forcefield).transform.GetChild(i).transform.rotation);

      //// spawn govt creep
      for(int i = 0; i < 3; i++)
        MasterManager.RoomObjectInstantiate(prefabs[6], spawns[0].transform.GetChild(i+3).transform.position, Quaternion.identity);

      //// spawn rebel creep
      for (int i = 0; i < 3; i++)
       MasterManager.RoomObjectInstantiate(prefabs[7], spawns[1].transform.GetChild(i+3).transform.position, Quaternion.identity);
    }
  }

  private void MarkObjective()
  {
      float fovDistance = prefabs[0].transform.localScale.x / 2 - 2;
      GameObject[] allGenerators = GameObject.FindGameObjectsWithTag("Generator");
      GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

      if (allGenerators != null && allPlayers != null)
      {
          foreach (GameObject player in allPlayers)
          {
              //mark government team players' objective
              if (player.layer == GOVT_LAYER)
              {
                  foreach (GameObject generator in allGenerators)
                  {
                      float distance = Vector3.Distance(generator.transform.position, player.transform.position);
                      GameObject iconVisible = generator.transform.Find("GeneratorIcon").gameObject;

                      if (distance < fovDistance && iconVisible.activeSelf == false)
                      {
                          iconVisible.SetActive(true);
                      }
                  }
              }
          }
      }
  }



  // Updates scene specific texts
  protected override void UpdateUITexts()
  {

  }

  // Decrement generator count
  public void DecrementGeneratorCount()
  {
      generatorCount--;
      //notfication panel index in the UITexts List
      //UITexts[3].SetText("Generator is destroyed", 2.0f);
  }
}
