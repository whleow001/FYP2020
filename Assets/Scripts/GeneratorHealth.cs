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
          OnDestory();
    }

    private void OnDestory() {
      PhotonNetwork.Destroy(gameObject);
    }

    public void TakeDamage(int damage) {
      health -= damage;

      photonView.RPC("NotifyRebelTeam", RpcTarget.All, transform.position);
    }

    [PunRPC]
    void NotifyRebelTeam(Vector3 position) {
      GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
      foreach (GameObject player in allPlayers)
        if (player.layer == REBEL_LAYER)
          player.GetComponent<PlayerController>().Notify("Generator Under Attack!", position);
    }
}
