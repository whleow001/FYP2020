using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Objective : MonoBehaviour
{
    [SerializeField]
    protected int health = 100;

    //Game director reference
    //private GameDirector director;

    //EventsManager reference
    protected EventsManager eventsManager;

    public Slider slider;
    public Gradient gradient;
    public Image fill;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    protected void Update()
    {
        
    }

    protected void SetHealthbar(int value)
    {
        slider.value = value;
        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    protected void SetMaxHealthbar(int value)
    {
        slider.maxValue = value;
        slider.value = value;
        fill.color = gradient.Evaluate(1f);
    }

    protected abstract void DestroyObject();
    /*
    public void takeDamage(int dmg)
    {

    }*/

    public void NotifyRebelTeam(string message, bool ignoreCooldown)
    {
        //director.GetPlayerManager().GetComponent<PhotonView>().RPC("NotifyTeam", RpcTarget.All, message, transform.position, REBEL_LAYER, ignoreCooldown);
    }

    public void NotifyGovtTeam(string message, bool ignoreCooldown)
    {
        //director.GetPlayerManager().GetComponent<PhotonView>().RPC("NotifyTeam", RpcTarget.All, message, transform.position, REBEL_LAYER, ignoreCooldown);
    }

    public void NotifyBothTeam(string message, bool ignoreCooldown)
    {
        //director.GetPlayerManager().GetComponent<PhotonView>().RPC("NotifyTeam", RpcTarget.All, message, transform.position, REBEL_LAYER, ignoreCooldown);
    }
}
