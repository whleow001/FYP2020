using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GeneratorHealth : MonoBehaviourPun
{
    private int health = 100;
    private int REBEL_LAYER = 10;

    private GameDirector director;

    void Start()
    {
        director = GameObject.Find("Director").GetComponent<GameDirector>();    
    }

    // Update is called once per frame 
    void Update() {
      if (PhotonNetwork.IsMasterClient)
        if (health <= 0)
          DestroyGenerator();
    }
    
    private void DestroyGenerator() {
      photonView.RPC("NotifyRebelTeam", RpcTarget.All, "A Generator has been destroyed!", transform.position);
      PhotonNetwork.Destroy(gameObject);
        director.generatorCount--;
    }

    public void TakeDamage(int damage) {
      health -= damage;

      photonView.RPC("NotifyRebelTeam", RpcTarget.All, "Generator Under Attack!", transform.position);
        ////test
        //if (PhotonNetwork.IsMasterClient)
        //    if (health <= 0)
        //        DestroyGenerator();
    }

    [PunRPC]
    void NotifyRebelTeam(string message, Vector3 position) {
      GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
      foreach (GameObject player in allPlayers)
        if (player.layer == REBEL_LAYER)
          player.GetComponent<PlayerController>().Notify(message, position);
    }
}
