using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SphereDetection : Objective
{
    // Start is called before the first frame update
    void Start()
    {
        Physics.IgnoreLayerCollision(10, 17);
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
                eventsManager.RebelNotification_S("Forcefield Down!", 2.0f);
            }
        }
    }

    protected override void DestroyObject()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    //Detect when there is a collision
    void OnCollisionStay(Collision collide)
    {
        //Output the name of the GameObject you collide with
        Debug.Log("I hit the GameObject : " + collide.gameObject.name);
    }

    public void TakeDamage(int damage)
    {
        health = health - damage;
        photonView.RPC("BroadcastHealth", RpcTarget.All, photonView.ViewID, health);
        eventsManager.RebelNotification_S("ForceField Under Attack!", 2.0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (collision.gameObject.tag == "Projectile" && collision.gameObject.layer == 9)
            {
                Debug.Log("forcefield is hit");
                TakeDamage(3);
            }
        }
    }

    //broadcast health to all clients in the server
    [PunRPC]
    void BroadcastHealth(int viewID, int health)
    {
        PhotonView PV = PhotonView.Find(viewID);
        PV.gameObject.GetComponent<SphereDetection>().SetHealthbar(health);
    }
}
