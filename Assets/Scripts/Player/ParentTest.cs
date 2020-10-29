using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentTest : MonoBehaviourPun
{

    private void Awake()
    {
        photonView.RPC("SetParent", RpcTarget.All, photonView.ViewID);

        
    }

    [PunRPC]
    public void SetParent(int photonViewID)
    {
        PhotonView PV = PhotonView.Find(photonViewID);
        GameObject[] containers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject parent in containers)
        {
            if (parent.GetComponent<PhotonView>().Owner == PV.Owner)
            {
                PV.gameObject.transform.parent = parent.transform;
                PV.gameObject.transform.localPosition = new Vector3(0, 0, 0);
                PV.gameObject.transform.localRotation = Quaternion.identity;
            }
        }
    }

}
