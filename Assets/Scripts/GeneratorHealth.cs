using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GeneratorHealth : MonoBehaviourPun
{
    private int health = 100;

    private GameDirector director;

    public bool takeDamage = false;

    // Layer references
    private int GOVT_LAYER = 9;
    private int REBEL_LAYER = 10;

    void Start() {
        director = GameObject.Find("Director").GetComponent<GameDirector>();
    }

    // Update is called once per frame
    void Update() {
      if (health <= 0)
        DestroyGenerator();

      if (takeDamage) {
        TakeDamage(20);
        takeDamage = false;
      }
    }

    private void DestroyGenerator() {
      director.DecrementGeneratorCount();
      NotifyRebelTeam("Generator destroyed!", true);

      if (PhotonNetwork.IsMasterClient)
        PhotonNetwork.Destroy(gameObject);
    }

    public void TakeDamage(int damage) {
      health -= damage;

      NotifyRebelTeam("Generator Under Attack!", false);
    }

    private void NotifyRebelTeam(string message, bool ignoreCooldown) {
      director.GetPlayerManager().GetComponent<PhotonView>().RPC("NotifyTeam", RpcTarget.All, message, transform.position, REBEL_LAYER, ignoreCooldown);
    }
}
