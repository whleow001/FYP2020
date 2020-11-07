using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Texts_A : BaseTexts
{
  public const int objective = 6;
}

public class Spawns_A : BaseSpawns
{
  public const int Generator = 2;
  public const int Forcefield = 3;
  public const int ForcefieldSphere = 4;
  public const int CryptSpawn = 5;
  public const int GateSpawn = 6;
}

public class Prefabs_A : BasePrefabs
{
  public const int Generator = 4;
  public const int Forcefield = 5;
  public const int ForcefieldSphere = 8;
  public const int Crypt = 9;
  public const int Gate = 10;
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
    print("set text");
    if (GetTeamIndex() == 0)
      GetUIText(Texts_A.objective).SetText("Destroy all generators or the forcefield to advance into the Rebel's base!");
    else
      GetUIText(Texts_A.objective).SetText("Defend the generators or the forcefield and run down the clock to tire the Government!");

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
        MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.Generator), GetSpawn(Spawns_A.Generator).transform.GetChild(randomIndexes[i]).transform.position, Quaternion.identity);

      // Spawn forcefields
     // for (int i = 0; i < 2; i++)
     //   MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.Forcefield), GetSpawn(Spawns_A.Forcefield).transform.GetChild(i).transform.position, GetSpawn(Spawns_A.Forcefield).transform.GetChild(i).transform.rotation);

      // Spawn crypt
      for (int i = 0; i < 3; i++)
        MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.Crypt), GetSpawn(Spawns_A.CryptSpawn).transform.GetChild(i).transform.position, GetSpawn(Spawns_A.CryptSpawn).transform.GetChild(i).transform.rotation);

      //forcefield sphere spawn on rebel side
      MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.ForcefieldSphere), GetSpawn(Spawns_A.ForcefieldSphere).transform.position, GetSpawn(Spawns_A.ForcefieldSphere).transform.rotation);
      //sphere.transform.localScale = new Vector3(2, 2, 2);

      //Gate spawn
      MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.Gate), GetSpawn(Spawns_A.GateSpawn).transform.position, GetSpawn(Spawns_A.GateSpawn).transform.rotation);

      //// spawn govt creep
      for(int i = 0; i < 3; i++)
       MasterManager.RoomObjectInstantiate(prefabs[6], spawns[0].transform.GetChild(i+3).transform.position, Quaternion.identity);

      //      MasterManager.RoomObjectInstantiate(prefabs[6], spawns[0].transform.GetChild(4).transform.position, Quaternion.identity);

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
