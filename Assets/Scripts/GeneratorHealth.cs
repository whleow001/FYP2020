using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GeneratorHealth : MonoBehaviourPun
{
    private int health = 100;

    private GameDirector director;

    void Start() {
        director = GameObject.Find("Director").GetComponent<GameDirector>();
    }

    // Update is called once per frame
    void Update() {
      if (health <= 0)
        DestroyGenerator();
    }

    private void DestroyGenerator() {
      NotifyRebelTeam("Generator destroyed!", false);
      if (PhotonNetwork.IsMasterClient)
        PhotonNetwork.Destroy(gameObject);
      director.DecrementGeneratorCount();
    }

    public void TakeDamage(int damage) {
      health -= damage;

      NotifyRebelTeam("Generator Under Attack!", false);
    }

    private void NotifyRebelTeam(string message, bool ignoreCooldown) {
      director.GetPlayerManager().GetComponent<PhotonView>().RPC("NotifyRebelTeam", RpcTarget.All, message, transform.position, ignoreCooldown);
    }
}
