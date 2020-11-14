using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldDetection : MonoBehaviour
{

    private void Awake()
    {
        gameObject.layer = transform.parent.gameObject.layer;
    }

    private void OnEnable()
    {
        Debug.Log(gameObject.transform.parent.gameObject.layer);
        Debug.Log(gameObject.layer);
        //Physics.IgnoreLayerCollision(18, gameObject.transform.parent.gameObject.layer);
        //Physics.IgnoreLayerCollision(18, 17);
    }
}
