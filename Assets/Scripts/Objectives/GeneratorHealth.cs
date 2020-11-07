﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class GeneratorHealth : Objective
{
    //test notif panel
    //public int health = 100;
    //private int health = 100;

    private RebelHQ_A Adirector;

    public bool takeDamage = false;

    public GameObject healthPanel;
    public Slider PanelSlider;
    public Image PanelFill;
    

    // Layer references
    private int GOVT_LAYER = 9;
    private int REBEL_LAYER = 10;

    //EventsManager reference
    //protected EventsManager eventsManager;

    void Start()
    {
        Adirector = GameObject.Find("Director").GetComponent<RebelHQ_A>();
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
                eventsManager.RebelNotification_S("Generator Destroyed!", 2.0f);
            }
        }


        /*
        if(takeDamage)
        {
            TakeDamage(20);
            takeDamage = false;
        }
        */
    }
    /*
    public void SetHealthbar(int value)
    {
        slider.value = value;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void SetMaxHealthbar(int value)
    {
        slider.maxValue = value;
        slider.value = value;
        fill.color = gradient.Evaluate(1f);
    }
    */
    protected override void DestroyObject()
    {
        Adirector.DecrementGeneratorCount();
        healthPanel.SetActive(false);
        PhotonNetwork.Destroy(gameObject);
    }

    public void TakeDamage(int damage)
    {
        health = health - damage;
        photonView.RPC("BroadcastHealth", RpcTarget.All, photonView.ViewID, health);
        //SetHealthbar(health);
        eventsManager.RebelNotification_S("Generator Under Attack!", 2.0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (collision.gameObject.tag == "Projectile" && collision.gameObject.layer == 9)
            {
                Debug.Log("generator is hit");
                TakeDamage(20);
            }
        }

    }

    public void SetHealthPanel(GameObject healthbar)
    {
        healthPanel = healthbar;
        PanelSlider = healthbar.GetComponentInChildren<Slider>();
        PanelFill = healthbar.transform.Find("fill").GetComponent<Image>();
    }

    //broadcast health to all clients in the server
    [PunRPC]
    void BroadcastHealth(int viewID, int health)
    {
        PhotonView PV = PhotonView.Find(viewID);
        PV.gameObject.GetComponent<GeneratorHealth>().SetHealthbar(health);
        PanelSlider.value = health;
        PanelFill.color = gradient.Evaluate(PanelSlider.normalizedValue);
        //SetHealthbar(health);
    }
}