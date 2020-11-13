using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonViewReference : MonoBehaviour
{
    private PhotonView photonView;
    private Bot bot;

    public void SetPhotonView(PhotonView _photonView) {
      photonView = _photonView;
    }

    public PhotonView GetPhotonView() {
      return photonView;
    }

    public void SetBot(Bot _bot) {
      bot = _bot;
    }

    public Bot GetBot() {
      return bot;
    }
}
