using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldDetection : MonoBehaviour
{

    private void Awake()
    {
        //gameObject.layer = transform.parent.gameObject.layer;
        Physics.IgnoreLayerCollision(18, gameObject.transform.parent.gameObject.layer);
        Physics.IgnoreLayerCollision(18, 17);
    }

    private void OnEnable()
    {
        Debug.Log(gameObject.transform.parent.gameObject.layer);
        Debug.Log(gameObject.layer);
        //Physics.IgnoreLayerCollision(18, gameObject.transform.parent.gameObject.layer);
        //Physics.IgnoreLayerCollision(18, 17);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.layer);
        if (transform.parent.gameObject.GetComponent<PhotonView>().IsMine)
        {
            if (collision.gameObject.tag == "Projectile" && collision.gameObject.layer == transform.parent.gameObject.GetComponent<PlayerContainer>().GetPlayerManager().GetDirector().GetOtherFactionLayer())
            {
                PhotonNetwork.Destroy(collision.gameObject);
            }
        }
    }
}
