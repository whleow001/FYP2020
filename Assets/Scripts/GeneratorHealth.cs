using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GeneratorHealth : MonoBehaviourPun
{
    private int health = 100;

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
    }
}
