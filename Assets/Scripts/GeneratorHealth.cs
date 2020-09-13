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

    void Start() {
        director = GameObject.Find("Director").GetComponent<GameDirector>();
    }

    // Update is called once per frame
    void Update() {
      if (PhotonNetwork.IsMasterClient)
        if (health <= 0)
          DestroyGenerator();
    }

    private void DestroyGenerator() {
      photonView.RPC("NotifyRebelTeam", RpcTarget.All, "Generator destroyed!", transform.position, true);
      PhotonNetwork.Destroy(gameObject);
      director.DecrementGeneratorCount();
    }

    public void TakeDamage(int damage) {
      health -= damage;

      photonView.RPC("NotifyRebelTeam", RpcTarget.All, "Generator Under Attack!", transform.position, false);
    }

    [PunRPC]
    void NotifyRebelTeam(string message, Vector3 position, bool ignoreCooldown) {
      foreach (Player player in PhotonNetwork.PlayerList)
        if (player == photonView.Owner)
          if (photonView.gameObject.layer == REBEL_LAYER)
            GetComponent<PlayerManager>().Notify(message, 3, ignoreCooldown, position);
    }
}
