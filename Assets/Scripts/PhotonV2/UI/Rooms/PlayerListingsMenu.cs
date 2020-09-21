using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;

public class PlayerListingsMenu : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    [SerializeField]
    private Transform _content;
    private PhotonTeamsManager _teamManager;

    private int teamOne = 0;
    private int teamTwo = 0;

    [SerializeField]
    private PlayerListing _playerListing;

    private RoomsCanvases _roomCanvases;

    public override void OnEnable()
    {
        base.OnEnable();
        _teamManager = GameObject.Find("TeamManager").GetComponent<PhotonTeamsManager>();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        if(PhotonNetwork.LocalPlayer.JoinTeam("Government"))
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + " has joined team 0");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        if(teamOne <= teamTwo)
        {
            if(newPlayer.JoinTeam("Government"))
            {
                Debug.Log(newPlayer.NickName + " has joined team 0");
            }
            
        }
        else
        {
            if(newPlayer.JoinTeam("Rebels"))
            {
                Debug.Log(newPlayer.NickName + " has joined team 1");
            }
            
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        if (targetPlayer != null)
        {
            if (changedProps.ContainsKey("_pt")) { }
                //Debug.Log(targetPlayer.CustomProperties["_pt"].ToString());
        }
        Debug.Log("Govt team: " + _teamManager.GetTeamMembersCount(0));
        teamOne = _teamManager.GetTeamMembersCount("Government");
        Debug.Log("Rebel team: " + _teamManager.GetTeamMembersCount(1));
        teamTwo = _teamManager.GetTeamMembersCount("Rebels");
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
    }

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomCanvases = canvases;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        _roomCanvases.CurrentRoomCanvas.LeaveRoomMenu.OnClick_LeaveRoom();
    }
}
