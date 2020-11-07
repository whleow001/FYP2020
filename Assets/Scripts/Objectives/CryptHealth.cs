using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CryptHealth : Objective
{

    private RebelHQ_A Adirector;

    public bool takeDamage = false;

    // Start is called before the first frame update
    void Start()
    {
        //Adirector = GameObject.Find("Director").GetComponent<RebelHQ_A>();
        eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();
        SetMaxHealthbar(health);
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (health <= 0)
            {
                DestroyObject();
                eventsManager.RebelNotification_S("Crypt Destroyed!", 2.0f);
            }
        }
    }

    protected override void DestroyObject()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        health = health - damage;
        photonView.RPC("BroadcastHealth", RpcTarget.All, photonView.ViewID, health);
        eventsManager.RebelNotification_S("Crypt Under Attack!", 2.0f);
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (PhotonNetwork.IsMasterClient)
        {
            if (collision.gameObject.tag == "Projectile" && collision.gameObject.layer == 9)
            {
                Debug.Log("crypt is hit");
                TakeDamage(5);
            }
        }
    }

    //broadcast health to all clients in the server
    [PunRPC]
    void BroadcastHealth(int viewID, int health)
    {
        PhotonView PV = PhotonView.Find(viewID);
        PV.gameObject.GetComponent<CryptHealth>().SetHealthbar(health);
    }
}
