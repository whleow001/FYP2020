using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

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
  void BroadcastHealth(int VictimID)
  {
      //GetComponent<PlayerManager>().SetHealthBar((int)victim.CustomProperties["Health"]);
      PhotonView PV = PhotonView.Find(VictimID);
      Player victim = PV.Owner;
      Slider mainslider = PV.gameObject.GetComponentInChildren<Slider>();
      Image mainfill = PV.gameObject.transform.Find("Canvas").Find("Healthbar").Find("fill").GetComponent<Image>();
      GetComponent<PlayerManager>().SetHealthBar((int)victim.CustomProperties["Health"], mainslider, mainfill);
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
