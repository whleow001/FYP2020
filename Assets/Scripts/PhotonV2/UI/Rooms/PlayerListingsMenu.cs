﻿using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerListingsMenu : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Transform _content;
    //private Transform _contentTeamOne;

    //[SerializeField]
    //private Transform _contentTeamTwo;

    [SerializeField]
    private PlayerListing _playerListing;

    [SerializeField]
    private Text _readyUpText;

    //For team selection

    private List<PlayerListing> _listings = new List<PlayerListing>();
    private RoomsCanvases _roomCanvases;
    private bool _ready = false;
    private int teamOneNo = 0;
    private int teamTwoNo = 0;
    //private bool _nextTeam = false;

    public override void OnEnable()
    {
        base.OnEnable();
        SetReadyUp(false);
        GetCurrentRoomPlayers();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        for (int i = 0; i < _listings.Count; i++)
            Destroy(_listings[i].gameObject);

        _listings.Clear();
    }

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomCanvases = canvases;
    }

    private void SetReadyUp(bool state)
    {
        _ready = state;
        if (_ready)
            _readyUpText.text = "R";
        else
            _readyUpText.text = "N";
    }

    private void GetCurrentRoomPlayers()
    {
        //if (!photonView.IsMine) return;

        if (!PhotonNetwork.IsConnected)
            return;
        if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null)
            return;

        
        foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            SetTeamOne();
            AddPlayerListing(playerInfo.Value);
            teamOneNo++;
        }
        
    }

    private void AddPlayerListing(Player player)
    {
        int index = _listings.FindIndex(x => x.Player == player);
        if (index != -1)
        {
            _listings[index].SetPlayerInfo(player);
        }
        else
        {
            PlayerListing listing = Instantiate(_playerListing, _content);
            if (listing != null)
            {
                listing.SetPlayerInfo(player);
                _listings.Add(listing);
                listing.SetPlayerText(player);
                Debug.Log(player.NickName);
            }
        }
        
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        _roomCanvases.CurrentRoomCanvas.LeaveRoomMenu.OnClick_LeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //if (!photonView.IsMine) return;

        /*
        if (teamOneNo < teamTwoNo || teamOneNo == teamTwoNo)
        {
            SetTeamOne();
            AddPlayerListing(newPlayer);
            teamOneNo++;
        }
        else
        {
            SetTeamTwo();
            AddPlayerListing(newPlayer);
            teamTwoNo++;
        }*/

        AddPlayerListing(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int index = _listings.FindIndex(x => x.Player == otherPlayer);
        if (index != -1)
        {
            Destroy(_listings[index].gameObject);
            _listings.RemoveAt(index);
        }
    }

    public void OnClick_StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < _listings.Count; i++)
            {
                if (_listings[i].Player != PhotonNetwork.LocalPlayer)
                {
                    if (!_listings[i].Ready)
                        return;
                }
            }

            PhotonNetwork.CurrentRoom.IsOpen = false; // players cannot join after game start
            PhotonNetwork.CurrentRoom.IsVisible = false; // removes room from room listing after game start
            PhotonNetwork.LoadLevel(1);

        }
    }

    public void OnClick_ReadyUp()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            SetReadyUp(!_ready);
            base.photonView.RPC("RPC_ChangeReadyState", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer, _ready);
        }
    }

    [PunRPC]
    private void RPC_ChangeReadyState(Player player, bool ready)
    {
        int index = _listings.FindIndex(x => x.Player == player);
        if (index != -1)
            _listings[index].Ready = ready;
    }

    //For team selection
    private void SetTeamOne()
    {
        ExitGames.Client.Photon.Hashtable _myCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        _myCustomProperties["Team"] = 1;
        PhotonNetwork.SetPlayerCustomProperties(_myCustomProperties);
    }

    private void SetTeamTwo()
    {
        ExitGames.Client.Photon.Hashtable _myCustomProperties = PhotonNetwork.LocalPlayer.CustomProperties;
        _myCustomProperties["Team"] = 2;
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
