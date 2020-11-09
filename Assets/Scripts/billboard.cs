using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class billboard : MonoBehaviour
{
    public Transform cam;

    void LateUpdate()
    {

        transform.LookAt(transform.position + cam.forward);
        //if (cam.gameObject.activeInHierarchy == false)
        //{
        //    Transform newCam = GameObject.Find("CutsceneCamera").gameObject.transform;
        //    transform.LookAt(transform.position + newCam.forward);
        //}
        //else
        //{
        //    transform.LookAt(transform.position + cam.forward);
        //}
        
    }
}
