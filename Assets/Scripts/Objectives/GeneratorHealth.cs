using System.Collections;
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
    //public Slider slider;
    //public Gradient gradient;
    //public Image fill;

    // Layer references
    private int GOVT_LAYER = 9;
    private int REBEL_LAYER = 10;

    //EventsManager reference
    //protected EventsManager eventsManager;

    void Start() {
        Adirector = GameObject.Find("Director").GetComponent<RebelHQ_A>();
        eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();
        SetMaxHealthbar(health);
    }

    // Update is called once per frame
    void Update() {
        if (health <= 0)
        {
            DestroyObject();
            eventsManager.RebelNotification_S("Generator Destroyed!", 2.0f);
        }

      if (takeDamage) {
        TakeDamage(20);
            takeDamage = false;
      }
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
    protected override void DestroyObject() {
        Adirector.DecrementGeneratorCount();

      if (PhotonNetwork.IsMasterClient)
        PhotonNetwork.Destroy(gameObject);
    }

    public void TakeDamage(int damage) {
      health -= damage;
      SetHealthbar(health);
      eventsManager.RebelNotification_S("Generator Under Attack!", 2.0f);
    }
}
