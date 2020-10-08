using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class PlayerRPC : MonoBehaviourPun
{
  public bool IsPhotonViewMine()
  {
    return photonView.IsMine;
  }

  public bool IsMasterClient()
  {
    return PhotonNetwork.IsMasterClient;
  }

  public void CallRPC(string rpcCall, Player target = null)
  {
    if (target != null)
      photonView.RPC(rpcCall, RpcTarget.All, target);
    else
      photonView.RPC(rpcCall, RpcTarget.All);
  }

  public PhotonView GetPhotonView()
  {
    return photonView;
  }

  //broadcast health to all clients in the server
  [PunRPC]
  void BroadcastHealth(Player victim)
  {
      GetComponent<PlayerController>().SetHealthBar((int)victim.CustomProperties["Health"]);
  }

  [PunRPC]
  void AllocateFOV()
  {
      GetComponent<PlayerManager>().GetDirector().AllocateFOVMask();
  }

  [PunRPC]
  void Fire()
  {
    /*ray.origin = raycastOrigin.transform.position;
    ray.direction = raycastOrigin.forward;

    if (Physics.Raycast(ray, out hitInfo, range)) {
      if (hitInfo.collider.gameObject.layer != gameObject.layer) {
              if (hitInfo.collider.gameObject.tag == "Player")
                  hitInfo.transform.gameObject.GetComponent<PlayerController>().TakeDamage(20, photonView);
              else if (hitInfo.collider.gameObject.tag == "Generator")
              {
                  hitInfo.transform.gameObject.GetComponent<GeneratorHealth>().TakeDamage(20);
              }
      }
    }

    Debug.DrawRay(ray.origin, transform.TransformDirection(Vector3.forward) * range, Color.red, 0.5f);*/
  }
}
