using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class TeamSelection : MonoBehaviour
{

    //[SerializeField]
    //private Text _text;

    private ExitGames.Client.Photon.Hashtable _myCustomProperties = new ExitGames.Client.Photon.Hashtable();

    private void SetTeamOne()
    {
        //System.Random rnd = new System.Random();
        //int result = rnd.Next(0, 99);

        //_text.text = result.ToString();

        _myCustomProperties["Team"] = 1;
        //_text.text = "Team 1";
        PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
        //PhotonNetwork.LocalPlayer.CustomProperties = _myCustomProperties;
    }

    private void SetTeamTwo()
    {
        _myCustomProperties["Team"] = 2;
        //_text.text = "Team 2";
        PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
    }

    public void OnClick_ButtonTeamOne()
    {
        SetTeamOne();
    }

    public void OnClick_ButtonTeamTwo()
    {
        SetTeamTwo();
    }
}
