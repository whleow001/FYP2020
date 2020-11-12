using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Texts_B : BaseTexts
{
  public const int objective = 6;
  public const int points = 7;
}

public class Spawns_B : BaseSpawns
{
  public const int ControlPoint = 2;
}

public class Prefabs_B : BasePrefabs
{
  public const int ControlPoint = 4;
}

public class RebelHQ_B : GameDirector {
  private List<ControlPoint> controlPoints = new List<ControlPoint>();
  private Coroutine cpCoroutine;

  [Header("Control Points")]
  [SerializeField]
  private List<Image> controlPointsImg = new List<Image>();

  [SerializeField]
  private int pointsPerCpPerSecond;
  private int totalGovtPoints = 0;

  // Update scene specific objectives
  protected override void UpdateObjectives() {
    // In case of non-master client - populate cp list
    if (!controlPoints.Any()) {
      GameObject[] cpArray = GameObject.FindGameObjectsWithTag("ControlPoint");

      for (int i = 0; i < 3; i++) {
        controlPoints.Add(cpArray[i].GetComponent<ControlPoint>());
        controlPoints[i].SetIndex(i);
        controlPoints[i].SetDirector(this);
      }
    }

    // Calculate points
    if (PhotonNetwork.IsMasterClient && cpCoroutine == null)
      cpCoroutine = StartCoroutine(IncrementCP());
  }

  // Initialize scene specific game objects
  protected override void InitializeGameObjects() {
    if (GetTeamIndex() == 0)
      GetUIText(Texts_B.objective).SetText("Capture Control Points to accumulate 500 points to win!");
    else
      GetUIText(Texts_B.objective).SetText("Defend the Control Points and run down the clock before the other team reaches 500 points!");

    // If client is a master client
    if (PhotonNetwork.IsMasterClient) {
      // Spawn Control Points
      for (int i = 0; i < 3; i++) {
        ControlPoint cp = MasterManager.RoomObjectInstantiate(GetPrefab(Prefabs_B.ControlPoint), GetSpawn(Spawns_B.ControlPoint).transform.GetChild(i).transform.position + new Vector3(0, 0.1f, 0), Quaternion.Euler(-90, 0, 0)).GetComponent<ControlPoint>();
        cp.SetIndex(i);
        cp.SetDirector(this);

        controlPoints.Add(cp);
      }
    }
  }

    protected override void RespawnCreep()
    {

    }

    // Updates scene specific texts
    protected override void UpdateUITexts() {
    if (controlPoints.Any()) {
      for (int i = 0; i < 3; i++) {
        switch (controlPoints[i].GetState()) {
          case ControlPoint.State.Rebel:
            controlPointsImg[i].color = new Color32(0, 0, 255, 100);
            break;
          case ControlPoint.State.RebelTransit:
            controlPointsImg[i].color = new Color32(0, 0, 255, 50);
            break;
          case ControlPoint.State.Neutral:
            controlPointsImg[i].color = new Color32(255, 255, 255, 50);
            break;
          case ControlPoint.State.GovtTransit:
            controlPointsImg[i].color = new Color32(255, 0, 0, 50);
            break;
          case ControlPoint.State.Government:
            controlPointsImg[i].color = new Color32(255, 0, 0, 100);
            break;
          default:
            break;
        }
      }
    }

    GetUIText(Texts_B.points).SetText(totalGovtPoints + " / 500");
  }

  private IEnumerator IncrementCP() {
    yield return new WaitForSeconds(1.0f);

    totalGovtPoints += pointsPerCpPerSecond * controlPoints.Count(cp => cp.GetState() == ControlPoint.State.Government);
    if (totalGovtPoints>=500)
        {
            eventsManager.DisplayEndGame_S("Government Team Win");
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine);
                StopCoroutine(creepCoroutine);
                StopCoroutine(cpCoroutine);
            }
            currentMatchTime = 0;
            RefreshTimerUI();
        }
    if (currentMatchTime <= 0)
      cpCoroutine = null;
    else {
      eventsManager.RefreshPoints_S();
      cpCoroutine = StartCoroutine(IncrementCP());
    }


  }

  public void ChangePCState(ControlPoint.State state, int index) {
    controlPoints[index].ChangeState(state);
  }

  public int GetGovtPoints() {
    return totalGovtPoints;
  }

  public void SetGovtPoints(int points) {
    totalGovtPoints = points;
  }
}
