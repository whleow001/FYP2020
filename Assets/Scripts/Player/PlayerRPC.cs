using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerRPC : MonoBehaviour
{
    public bool IsPhotonViewMine()
    {
        return GetPhotonView().IsMine;
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public void CallRPC(string rpcCall, Player target = null)
    {
        if (target != null)
            GetPhotonView().RPC(rpcCall, RpcTarget.All, target);
        else
            GetPhotonView().RPC(rpcCall, RpcTarget.All);
    }

    public PhotonView GetPhotonView()
    {
        return GetComponent<PlayerManager>().GetPlayerAvatar().GetComponent<PhotonView>();
    }

    public void ChangeMaterial(int ParentViewID, int ModelViewID, int selectedMaterial, int selectedLayer)
    {
        GetPhotonView().RPC("ChangeMaterials", RpcTarget.All, ParentViewID, ModelViewID, selectedMaterial, selectedLayer);
    }


    public void InstantiateBullet(Vector3 position, Vector3 velocity, int layer)
    {
        GetPhotonView().RPC("InstantiateBullet", RpcTarget.All, position, velocity, layer);
    }

    public void ChangeIcons()
    {
        GetPhotonView().RPC("ChangeIcons", RpcTarget.All, GetPhotonView().ViewID);

    }
}
