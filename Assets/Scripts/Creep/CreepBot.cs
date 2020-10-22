using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CreepBot : MonoBehaviour
{
    /*
        obj     = enemy spawn
        creeps  = array of all creep gameobjects
        players = array of all player gameobjects
        targets = array of all creep and player gameobjects
    */

    private GameObject obj;
    private GameObject[] creeps, players, targets;

    /*
    public float normalSpeed = 0.015f;
    private float speed;
    private float immobile = 0.0f;*/

    private NavMeshAgent agent;
    private Animator creepAnimator;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] objectives = GameObject.FindGameObjectsWithTag("Respawn");
        foreach (GameObject objective in objectives)
        {
            if(objective.layer != this.gameObject.layer)
            {
                obj = objective;
            }
        }
//        obj = GameObject.Find("GovtSpawn");
        creepAnimator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

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
                if (targetDirection.magnitude <= 1.1f)
                {
                    creepAnimator.SetBool("isAttacking", true);
                    this.gameObject.GetComponent<NavMeshAgent>().enabled = false;
                    //transform.rotation = Quaternion.LookRotation(targetDirection);
                }
                else
                {
                    creepAnimator.SetBool("isAttacking", false);
                    this.gameObject.GetComponent<NavMeshAgent>().enabled = true;
                    agent.SetDestination(closestTarget.transform.position);
                }

            }

            //if no enemy target nearby, creep will move toward objective
            else
            {
                //transform.rotation = Quaternion.LookRotation(agent.velocity.normalized);
                this.gameObject.GetComponent<NavMeshAgent>().enabled = true;
                agent.SetDestination(obj.transform.position);
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

}
