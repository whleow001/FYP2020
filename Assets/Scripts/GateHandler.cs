using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateHandler : MonoBehaviour
{
    Animator GateAnim;
    [SerializeField]
    GameObject gateCollider;

    bool gateOpen;

    // Start is called before the first frame update
    void Start()
    {
        gateOpen = false;
        GateAnim = GetComponent<Animator>();
        //Physics.IgnoreCollision(GetComponent<Collider>(), bullet.GetComponent<Collider>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == 10)
        {
            gateOpen = true;
            GateAnim.SetTrigger("open");
            //gateCollider.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (gateOpen == true)
        {
            gateOpen = false;
            GateAnim.SetTrigger("close");
            //gateCollider.SetActive(true);
        }
    }
}
