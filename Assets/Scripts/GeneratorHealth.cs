using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GeneratorHealth : MonoBehaviourPun
{
    private int health = 100;
    private int REBEL_LAYER = 10;

    // Update is called once per frame
    void Update() {
      if (PhotonNetwork.IsMasterClient)
        if (health <= 0)
          DestroyGenerator();
    }

    private void DestroyGenerator() {
      PhotonNetwork.Destroy(gameObject);

      photonView.RPC("NotifyRebelTeam", RpcTarget.All, "A Generator has been destroyed!", transform.position);
      photonView.RPC("NotifyGeneratorDown", RpcTarget.All);
    }

    public void TakeDamage(int damage) {
      health -= damage;

      photonView.RPC("NotifyRebelTeam", RpcTarget.All, "Generator Under Attack!", transform.position);
    }

    [PunRPC]
    void NotifyRebelTeam(string message, Vector3 position) {
      GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
      foreach (GameObject player in allPlayers)
        if (player.layer == REBEL_LAYER)
          player.GetComponent<PlayerController>().Notify(message, position);
    }

    [PunRPC]
    void NotifyGeneratorDown() {
      GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
      foreach (GameObject player in allPlayers)
        player.GetComponent<PlayerController>().DecrementGeneratorCount();
    }
}
