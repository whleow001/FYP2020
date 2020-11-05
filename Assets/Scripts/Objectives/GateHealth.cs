using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GateHealth : Objective
{
    // Start is called before the first frame update
    void Start()
    {
        SetMaxHealthbar(health);
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            DestroyObject();
            //eventsManager.RebelNotification_S("Generator Destroyed!", 2.0f);
        }
    }

    protected override void DestroyObject()
    {
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        health = health - damage;
        SetHealthbar(health);
        //eventsManager.RebelNotification_S("Generator Under Attack!", 2.0f);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Projectile" && collision.gameObject.layer == 9)
        {
            Debug.Log("Gate is hit");
            TakeDamage(5);
        }
    }
}
