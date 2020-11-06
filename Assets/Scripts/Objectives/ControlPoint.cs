using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class ControlPoint : MonoBehaviourPun
{
    public enum State : int {
      Government = 0,
      GovtTransit = 1,
      Neutral = 2,
      RebelTransit= 3,
      Rebel = 4
    };

    // State
    [SerializeField]
    private State state = State.Rebel;
    private float stateFloat = -2.5f;

    // Materials
    [SerializeField]
    private List<Material> materials = new List<Material>();
    /*
      0 - Govt
      1 - GovtTransit
      2 - StandStill
      3 - RebelTransit
      4 - Rebel
    */

    // index
    private int index;

    // Layer references
    private int GOVT_LAYER = 9;
    private int REBEL_LAYER = 10;

    // References
    private RebelHQ_B director;
    private EventsManager eventsManager;

    // Start is called before the first frame update
    private void Start() {
      eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();
      ChangeMaterial();
    }

    // Update is called once per frame
    private void Update() {
      if (PhotonNetwork.IsMasterClient) {
        List<GameObject>[] players = GetPlayers();

        // If more govt players than rebel players on cp
        if (players[0].Count > players[1].Count && stateFloat < 2.5f)
          stateFloat += Time.deltaTime;

        // If more rebel players than govt players on cp
        else if (players[0].Count < players[1].Count && stateFloat > -2.5f )
          stateFloat -= Time.deltaTime;

        // Both sides have no players on
        else if (players[0].Count == 0 && players[1].Count == 0) {
          // stateFloat leans towards govt
          if (stateFloat > 1.0f && stateFloat < 2.5f)
            stateFloat += (Time.deltaTime/2);
          // stateFloat leans towards rebels
          else if (stateFloat > -2.5f && stateFloat < -1.0f)
            stateFloat -= (Time.deltaTime/2);
        }

        // Change state based on stateFloat
        if (stateFloat <= -2.5f)
          MasterClientChangeState(State.Rebel);
        else if (stateFloat >= 2.5f)
          MasterClientChangeState(State.Government);
        else if (stateFloat > -2.5f && stateFloat < -1.0f)
          MasterClientChangeState(State.RebelTransit);
        else if (stateFloat < 2.5f && stateFloat > 1.0f)
          MasterClientChangeState(State.GovtTransit);
        else
          MasterClientChangeState(State.Neutral);
      }
    }

    private void ChangeMaterial() {
      Material[] _materials = gameObject.GetComponent<MeshRenderer>().materials;
      _materials[0] = materials[(int)state];

      gameObject.GetComponent<MeshRenderer>().materials = _materials;
    }

    private List<GameObject>[] GetPlayers() {
      Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, 2.0f);

      List<GameObject> players = new List<GameObject>();
      foreach(var hitCollider in hitColliders)
        if (hitCollider.tag == "Player")
          players.Add(hitCollider.gameObject);

      List<GameObject> govt = new List<GameObject>();
      List<GameObject> rebels = new List<GameObject>();

      foreach (GameObject player in players) {
        if (player.layer == GOVT_LAYER)
          govt.Add(player);
        else if (player.layer == REBEL_LAYER)
          rebels.Add(player);
      }

      return new List<GameObject>[] { govt, rebels };
    }

    private void MasterClientChangeState(State _state) {
      if (state == _state)
        return;

      photonView.RPC("ChangeCPState", RpcTarget.All, _state, index);
    }

    public void ChangeState(State _state) {
      State prevState = state;
      state = _state;
      ChangeMaterial();

      if (!PhotonNetwork.IsMasterClient)
        return;

      if (state == State.Rebel) {
        eventsManager.RebelNotification_S("Control Point Captured!", 2.0f);
        eventsManager.GovtNotification_S("Control Point Lost!", 2.0f);
      }

      else if (state == State.Government) {
        eventsManager.GovtNotification_S("Control Point Captured!", 2.0f);
        eventsManager.RebelNotification_S("Control Point Lost!", 2.0f);
      }

      else if (prevState == State.Rebel) {
        eventsManager.RebelNotification_S("Losing Control Point!", 2.0f);
        eventsManager.GovtNotification_S("Capturing Control Point!", 2.0f);
      }

      else if (prevState == State.Government) {
        eventsManager.GovtNotification_S("Losing Control Point!", 2.0f);
        eventsManager.RebelNotification_S("Capturing Control Point!", 2.0f);
      }
    }

    public void SetDirector(RebelHQ_B _director) {
      director = _director;
    }

    public void SetIndex(int i) {
      index = i;
    }

    public int GetIndex() {
      return index;
    }

    public State GetState() {
      return state;
    }

    [PunRPC]
    void ChangeCPState(ControlPoint.State _state, int _index) {
      director.ChangePCState(_state, _index);
    }
}
