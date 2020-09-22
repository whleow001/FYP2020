using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun.UtilityScripts;

public class LeaveRoomMenu : MonoBehaviour
{
    private RoomsCanvases _roomsCanvases;

    private PlayerListingsMenu _playerListingsMenu;

    public void OnEnable()
    {
        _playerListingsMenu = GameObject.Find("PlayerListingsMenu").GetComponent<PlayerListingsMenu>();
    }

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomsCanvases = canvases;
    }

    public void OnClick_LeaveRoom()
    {
        _playerListingsMenu.RemovePlayerListing(PhotonNetwork.LocalPlayer);
        if (PhotonNetwork.LocalPlayer.LeaveCurrentTeam())
        {
            Debug.Log(PhotonNetwork.LocalPlayer.NickName + "has left his/her team");
        }
        else
        {
            Debug.Log("function not called");
        }
        PhotonNetwork.LeaveRoom(true);
        _roomsCanvases.CurrentRoomCanvas.Hide();
    }


}
