using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpriteEditor : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            transform.GetComponentInChildren<SpriteRenderer>().drawMode = SpriteDrawMode.Sliced;
            transform.GetComponentInChildren<SpriteRenderer>().size = new Vector3(1.5f, 2.0f, 1f);
            transform.GetComponentInChildren<SpriteRenderer>().drawMode = SpriteDrawMode.Simple;
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
