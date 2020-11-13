using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldDetection : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        //gameObject.layer = gameObject.transform.parent.gameObject.layer;
        Physics.IgnoreLayerCollision(17, gameObject.transform.parent.gameObject.layer);
    }
}
