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

    //Detect when there is a collision
    void OnCollisionStay(Collision collide)
    {
        //Output the name of the GameObject you collide with
        Debug.Log("I hit the GameObject : " + collide.gameObject.name);
    }

    public void TakeDamage(int damage)
    {
        health = health - damage;
        SetHealthbar(health);
        //eventsManager.RebelNotification_S("Generator Under Attack!", 2.0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("i am here inside");
        if (collision.gameObject.tag == "Projectile" && collision.gameObject.layer == 9)
        {
            Debug.Log("sphere is hit");
            TakeDamage(3);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("i am here inside");
    }
}
