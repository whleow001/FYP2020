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
  public const int GovtSpawn = 0;
  public const int RebelSpawn = 1;
  public const int Generator = 2;
  public const int ForcefieldSphere = 3;
  public const int CryptSpawn = 4;
  public const int ForcefieldSpawn = 5;
}

public class Prefabs_A : BasePrefabs
{
  public const int Generator = 4;
  public const int ForcefieldSphere = 5;
  public const int GovtCreep = 6;
  public const int RebelCreep = 7;
  public const int Crypt = 8;
  public const int MidForcefield = 9;
  public const int TopForcefield = 10;
  public const int BottomForcefield = 11;
}

public class RebelHQ_A : GameDirector {
  // Win/Loss condition
  [SerializeField]
  private int generatorCount;

  [SerializeField]
  public GameObject ObjectivePanel;

  //temp storage array for gen gameobject
  private GameObject[] temp;

  private GameObject forcefieldTemp;

  // Flags
  private bool forcefieldDestroyed = false;

  // Update scene specific objectives
  protected override void UpdateObjectives() {
    if (PhotonNetwork.IsMasterClient && !forcefieldDestroyed)
    {
        if(generatorCount == 0 || forcefieldTemp.GetComponent<SphereDetection>().GetHealth() <= 0)
            {
                PhotonNetwork.Destroy(forcefieldTemp);
                forcefieldDestroyed = true;
                //eventsManager.DisplayEndGame_S("Government Team Wins");
                eventsManager.StartCutscene_S(true);
                //PhotonNetwork.LoadLevel(2);

                //if game ends before timer runs out
                if (timerCoroutine != null) StopCoroutine(timerCoroutine);
                currentMatchTime = 0;
                RefreshTimerUI();
            }
    }

    MarkObjective();
  }

  // Initialize scene specific game objects
  protected override void InitializeGameObjects() {
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
            {
                MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.Generator), GetSpawn(Spawns_A.Generator).transform.GetChild(randomIndexes[i]).transform.position, Quaternion.identity);
            }

      // Spawn crypt
      for (int i = 0; i < 3; i++)
        MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.Crypt), GetSpawn(Spawns_A.CryptSpawn).transform.GetChild(i).transform.position, GetSpawn(Spawns_A.CryptSpawn).transform.GetChild(i).transform.rotation);

      //forcefield sphere spawn on rebel side
      forcefieldTemp = MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.ForcefieldSphere), GetSpawn(Spawns_A.ForcefieldSphere).transform.position, GetSpawn(Spawns_A.ForcefieldSphere).transform.rotation);

      //Gate spawn
      //MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.Gate), GetSpawn(Spawns_A.GateSpawn).transform.position, GetSpawn(Spawns_A.GateSpawn).transform.rotation);

      // mid forcefield spawn
      MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.MidForcefield), GetSpawn(Spawns_A.ForcefieldSpawn).transform.GetChild(0).transform.position, GetSpawn(Spawns_A.ForcefieldSpawn).transform.GetChild(0).transform.rotation);

      // top forcefield spawn
      MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.TopForcefield), GetSpawn(Spawns_A.ForcefieldSpawn).transform.GetChild(1).transform.position, GetSpawn(Spawns_A.ForcefieldSpawn).transform.GetChild(1).transform.rotation);

      // bottom forcefield spawn
      MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.BottomForcefield), GetSpawn(Spawns_A.ForcefieldSpawn).transform.GetChild(2).transform.position, GetSpawn(Spawns_A.ForcefieldSpawn).transform.GetChild(2).transform.rotation);

      // spawn govt creep
      for (int i = 0; i < 3; i++)
       MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.GovtCreep), GetSpawn(Spawns_A.GovtSpawn).transform.GetChild(i+3).transform.position, GetSpawn(Spawns_A.GovtSpawn).transform.GetChild(i+3).transform.rotation);

    // for testing creep
    //   MasterManager.RoomObjectInstantiate(prefabs[6], spawns[0].transform.GetChild(4).transform.position, Quaternion.identity);

      // spawn rebel creep
      for (int i = 0; i < 3; i++)
       MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.RebelCreep), GetSpawn(Spawns_A.RebelSpawn).transform.GetChild(i+3).transform.position, GetSpawn(Spawns_A.RebelSpawn).transform.GetChild(i + 3).transform.rotation);
    }

    temp = GameObject.FindGameObjectsWithTag("Generator");
    for (int i = 0; i < temp.Length; i++)
        {
            temp[i].GetComponent<GeneratorHealth>().SetHealthPanel(ObjectivePanel.transform.GetChild(i).gameObject);
        }
  }

    protected override void RespawnCreep()
    {
        // respawn govt creep
        for (int i = 0; i < 3; i++)
            MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.GovtCreep), GetSpawn(Spawns_A.GovtSpawn).transform.GetChild(i + 3).transform.position, GetSpawn(Spawns_A.GovtSpawn).transform.GetChild(i + 3).transform.rotation);

        // respawn rebel creep
        GameObject[] crypts = GameObject.FindGameObjectsWithTag("Crypt");
        foreach (GameObject crypt in crypts)
        {
            MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_A.RebelCreep), crypt.transform.GetChild(1).transform.position, crypt.transform.GetChild(1).transform.rotation);
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
