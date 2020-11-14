using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SphereDetection : Objective
{
    string objectName;
    // Start is called before the first frame update
    void Start()
    {
        Physics.IgnoreLayerCollision(10, 17);
        eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();
        SetMaxHealthbar(health);
        objectName = this.gameObject.name;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (health <= 0)
            {
                DestroyObject();
                eventsManager.RebelNotification_S(objectName+" Down!", 2.0f);
            }
        }
    }

    protected override void DestroyObject()
    {
        GetComponent<DestroyManager>().CueSound();
        PhotonNetwork.Destroy(gameObject);
    }

    public int GetHealth()
    {
        return health;
    }

    //Detect when there is a collision
    void OnCollisionStay(Collision collide)
    {
        //Output the name of the GameObject you collide with
        Debug.Log("I hit the GameObject : " + collide.gameObject.name);
    }

    public void TakeDamage(Damage dmg)
    {
        hitEffect.transform.position = dmg.sourcePosition;
        hitEffect.transform.forward = (gameObject.transform.position - dmg.sourcePosition).normalized;
        hitEffect.Emit(1);

        health = health - dmg.damage;
        photonView.RPC("BroadcastHealth", RpcTarget.All, photonView.ViewID, health);
        eventsManager.RebelNotification_S(objectName+" Under Attack!", 2.0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (collision.gameObject.tag == "Projectile" && collision.gameObject.layer == 9)
            {
                Debug.Log(objectName+" is hit");
                TakeDamage(new Damage(3, collision.gameObject.transform.position));

                if (health > 3)
                  GetComponent<DamageManager>().CueSound();
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
