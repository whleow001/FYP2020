using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonViewReference : MonoBehaviour
{
    private PhotonView photonView;


    public void SetPhotonView(PhotonView _photonView) {
      photonView = _photonView;
    }

    public PhotonView GetPhotonView() {
      return photonView;
    }
}
