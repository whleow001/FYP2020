using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using UnityEngine.UI;

public class CreepBot : MonoBehaviourPun
{
    /*
        obj     = enemy spawn
        creeps  = array of all creep gameobjects
        players = array of all player gameobjects
        targets = array of all creep and player gameobjects
    */

    private GameObject obj;
    private GameObject[] creeps, players, targets;

    private NavMeshAgent agent;
    private Animator creepAnimator;


    private bool attack = false;
    private float creepRange;

    private int health = 100;
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    public PlayerManager playerManager;


    // Start is called before the first frame update
    void Start()
    {
        playerManager = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        GameObject[] objectives = GameObject.FindGameObjectsWithTag("Respawn");
        foreach (GameObject objective in objectives)
        {
            if(objective.layer != this.gameObject.layer)
            {
                obj = objective;
            }
        }

        creepAnimator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        // range of creep attack based on creep size
        creepRange = transform.localScale.x + 0.1f;

        //combining players and creeps as targets
        creeps = GameObject.FindGameObjectsWithTag("Creep");
        players = GameObject.FindGameObjectsWithTag("Player");
        targets = players.Concat(creeps).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        //combining players and creeps as targets
        creeps = GameObject.FindGameObjectsWithTag("Creep");
        players = GameObject.FindGameObjectsWithTag("Player");
        targets = players.Concat(creeps).ToArray();


        //if can't find objective, creep will become idle
        if (obj==null)
        {
            //Debug.Log("Obj not found");
            this.gameObject.GetComponent<NavMeshAgent>().enabled = false;
            creepAnimator.SetBool("isIdle",true);
            //return;
        }
        else
        {
            creepAnimator.SetBool("isIdle", false);
        }

        Vector3 objDirection = obj.transform.position - transform.position;

        GameObject closestTarget = FindClosestTarget(targets);
        //Debug.Log(closestTarget);
        Vector3 targetDirection = closestTarget.transform.position - transform.position;

        

        //if target closeby is not on the same team, move towards them
        if (targetDirection.magnitude <= 13 && closestTarget.layer != this.gameObject.layer)
        {
            //transform.rotation = Quaternion.LookRotation(agent.velocity.normalized);
            //if close enough to attack, stop moving and attack closest player enemy
            if (targetDirection.magnitude <= creepRange)
            {
                creepAnimator.SetBool("isAttacking", true);
                this.gameObject.GetComponent<NavMeshAgent>().enabled = false;
                transform.rotation = Quaternion.LookRotation(targetDirection);

                if (this.creepAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
                {
                    if (creepAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f && attack == false)
                    {
                        Debug.Log("attack");
                        attack = true;
                    }
                }
                else
                {
                    attack = false;
                }
            }
            else
            {
                creepAnimator.SetBool("isAttacking", false);
                if (PhotonNetwork.IsMasterClient)
                {
                    transform.rotation = Quaternion.LookRotation(agent.velocity.normalized);
                    agent.SetDestination(closestTarget.transform.position);
                }
                this.gameObject.GetComponent<NavMeshAgent>().enabled = true;
                attack = false;
                
            }

        }

        //if no enemy target nearby, creep will move toward objective
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                transform.rotation = Quaternion.LookRotation(agent.velocity.normalized);
                agent.SetDestination(obj.transform.position);
            }
            this.gameObject.GetComponent<NavMeshAgent>().enabled = true;
            attack = false;
        }


    }

    private GameObject FindClosestTarget(GameObject[] targets)
    {
        float minDistance = (targets[0].transform.position - transform.position).magnitude;
        GameObject closestTarget = targets[0];

        foreach (GameObject target in targets)
        {
            float distance = (target.transform.position - transform.position).magnitude;

            if (distance < minDistance && target.layer != this.gameObject.layer)
            {
                minDistance = distance;
                closestTarget = target;
            }
        }

        return closestTarget;
    }

    public void SetHealthBar(int value)
    {
        slider.value = value;
        fill.color = gradient.Evaluate(slider.normalizedValue);

    }

    public void SetMaxHealthBar(int value)
    {
        slider.maxValue = 100;
        slider.value = 100;
        fill.color = gradient.Evaluate(1f);

    }
    /*
    private void OnCollisionEnter(Collision other)
    {

        if (other.gameObject.tag == "Projectile" && other.gameObject.layer == playerManager.GetDirector().GetOtherFactionLayer())
        {
            TakeDamage(20);
        }
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        photonView.RPC("BroadcastHealth", RpcTarget.All, photonView.ViewID);
        
    }

    //broadcast health to all clients in the server
    [PunRPC]
    void BroadcastHealth(int victimID)
    {
        PhotonView PV = PhotonView.Find(victimID);
        SetHealthBar(health);
        //GetComponent<PlayerManager>().SetHealthBar((int)victim.CustomProperties["Health"], mainslider, mainfill);
    }
    */
}
