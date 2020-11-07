﻿using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CreepBot : MonoBehaviourPun
{
    /*
        obj     = enemy spawn
        creeps  = array of all creep gameobjects
        players = array of all player gameobjects
        targets = array of all creep and player gameobjects
    */

    private GameObject obj, gate, forcefield;
    private GameObject[] creeps, players, crypts, targets;
    private float objRadius;

    private NavMeshAgent agent;
    private Animator creepAnimator;

    // to determine whether creep has dealt damage or not in one attack animation
    private bool attack = false;

    // for creep attack range
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
            if (objective.layer != this.gameObject.layer)
            {
                obj = objective;
            }
        }

        // get radius of enemy spawn
        objRadius = obj.GetComponent<Renderer>().bounds.extents.magnitude / 2;

        //get game object gate
        gate = GameObject.FindGameObjectWithTag("Gate");

        //get game object forcefield
        forcefield = GameObject.FindGameObjectWithTag("Forcefield");

        creepAnimator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        SetMaxHealthBar(health);

        // range of creep attack based on creep size
        creepRange = transform.localScale.x/2 + 0.1f;

        //combining players, creeps, and crypts as targets
        creeps = GameObject.FindGameObjectsWithTag("Creep");
        players = GameObject.FindGameObjectsWithTag("Player");
        crypts = GameObject.FindGameObjectsWithTag("Crypt");
        targets = players.Concat(creeps).ToArray();
        targets = targets.Concat(crypts).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        //combining players, creeps, crypts, forcefield and gate as targets
        creeps = GameObject.FindGameObjectsWithTag("Creep");
        players = GameObject.FindGameObjectsWithTag("Player");
        crypts = GameObject.FindGameObjectsWithTag("Crypt");
        targets = players.Concat(creeps).ToArray();
        targets = targets.Concat(crypts).ToArray();
        if(forcefield!= null && this.gameObject.layer==9 && forcefield.layer==17)
        {
            Array.Resize(ref targets, targets.Length + 1);
            targets[targets.Length - 1] = forcefield;
        }

        if (gate != null)
        {
            Array.Resize(ref targets, targets.Length + 1);
            targets[targets.Length - 1] = gate;
        }

        //if can't find objective, creep will become idle
        if (obj == null)
        {
            //Debug.Log("Obj not found");
            this.gameObject.GetComponent<NavMeshAgent>().enabled = false;
            creepAnimator.SetBool("isIdle", true);
            //return;
        }
        else
        {
            creepAnimator.SetBool("isIdle", false);
        }

        //counting position of objective according to creep position
        Vector3 objDirection = obj.transform.position - transform.position;
        //calculating closest edge of the objective according to creep position
        Vector3 objEdge = obj.transform.position - (obj.transform.position - this.transform.position).normalized * objRadius;

        GameObject closestTarget = FindClosestTarget(targets);
        Vector3 targetDirection = closestTarget.transform.position - transform.position;

        // change range according to target's tag (creep will have longer range against building as centre of the building is the target)
        if (closestTarget.tag == "Player" || closestTarget.tag == "Creep")
        {
            creepRange = transform.localScale.x / 2 + 0.5f;
        }
       /* else if (closestTarget.tag == "Forcefield")
        {
            //creepRange = closestTarget.GetComponent<SphereCollider>().bounds.size.x / 2 + 0.2f;
            creepRange = objRadius +1;
        }*/
        else
        {
            creepRange = closestTarget.GetComponent<Collider>().bounds.size.x / 2 + 0.2f;
        }

        // create a path
        NavMeshPath path = new NavMeshPath();

        if (health > 0)
        {

            //if target closeby is not on the same team, move towards them
            if (targetDirection.magnitude <= 13 && closestTarget.layer != this.gameObject.layer)
            {
                creepAnimator.SetBool("isIdle", false);

// attack
                //if close enough to attack, stop moving and attack closest player enemy
                if (targetDirection.magnitude <= creepRange)
                {
                    creepAnimator.SetBool("isAttacking", true);
                    this.gameObject.GetComponent<NavMeshAgent>().enabled = false;
                    transform.rotation = Quaternion.LookRotation(targetDirection);

                    // if currently is in attacking animation
                    if (this.creepAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
                    {
                        if (creepAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.5f && attack == false)
                        {
                            //closestTarget.GetComponent<CreepBot>().TakeDamage(20);
                            closestTarget.SendMessage("TakeDamage", 20);
                            attack = true;
                        }
                    }
                    else
                    {
                        attack = false;
                    }
                }

// chase
                // chase target instead if target is still in creep field of view
                else
                {
                    creepAnimator.SetBool("isAttacking", false);
                    this.gameObject.GetComponent<NavMeshAgent>().enabled = true;
                    if (PhotonNetwork.IsMasterClient)
                    {
                        // calculate path to target
                        agent.CalculatePath(closestTarget.transform.position, path);
                        // if no path can be found, go to obj instead
                        if (path.status == NavMeshPathStatus.PathPartial)
                        {
                            agent.SetDestination(objEdge);
                        }
                        else
                        {
                            agent.SetDestination(closestTarget.transform.position);
                        }
                        transform.rotation = Quaternion.LookRotation(agent.velocity.normalized);
                    }
                    attack = false;

                }

            }

            //if no enemy target nearby, creep will move toward objective's edge
            else
            {
// idle when objective reached
                creepAnimator.SetBool("isAttacking", false);
                if (objDirection.magnitude <= objRadius)
                {
                    creepAnimator.SetBool("isIdle", true);
                    this.gameObject.GetComponent<NavMeshAgent>().enabled = false;
                    transform.rotation = Quaternion.LookRotation(objDirection);
                }

// walk to objective if not reached yet
                else
                {
                    creepAnimator.SetBool("isIdle", false);
                    this.gameObject.GetComponent<NavMeshAgent>().enabled = true;
                    if (PhotonNetwork.IsMasterClient)
                    {
                        // calculate path to target
                        agent.CalculatePath(objEdge, path);
                        Debug.Log(path.status);
                        // if no path can be found, go to obj instead
                        if (path.status == NavMeshPathStatus.PathPartial)
                        {
                            agent.SetDestination(obj.transform.position);
                        }
                        else if(path.status == NavMeshPathStatus.PathComplete)
                        {
                            agent.SetDestination(objEdge);
                        }
                        transform.rotation = Quaternion.LookRotation(agent.velocity.normalized);
                    }
                }
                attack = false;

            }

        }

// creep destroyed when health <= 0
        else
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }


    // function to find closest target
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
        if (gameObject.layer == 9)
        {
            fill.color = Color.red;
        }
        else if (gameObject.layer == 10)
        {
            fill.color = Color.blue;
        }
        slider.value = value;
        //fill.color = gradient.Evaluate(slider.normalizedValue);

    }

    public void SetMaxHealthBar(int value)
    {
        if (gameObject.layer == 9)
        {
            fill.color = Color.red;
        }
        else if (gameObject.layer == 10)
        {
            fill.color = Color.blue;
        }

        slider.maxValue = 100;
        slider.value = 100;
        //fill.color = gradient.Evaluate(1f);

    }
    
    private void OnCollisionEnter(Collision other)
    {

        if (other.gameObject.tag == "Projectile" && other.gameObject.layer != this.gameObject.layer)
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
    
}
