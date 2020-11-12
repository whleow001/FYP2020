using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldDetection : MonoBehaviour
{

    private void Awake()
    {
        gameObject.layer = gameObject.transform.parent.gameObject.layer;
    }
    // Start is called before the first frame update
    void Start()
    {
        Physics.IgnoreLayerCollision(9, gameObject.layer);
    }
}
