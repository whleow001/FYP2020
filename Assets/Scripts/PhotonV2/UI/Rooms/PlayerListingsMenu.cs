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
    private Transform _contentTeamOne;
    [SerializeField]
    private Transform _contentTeamTwo;

    private PhotonTeamsManager _teamManager;

    private int teamOne = 0;
    private int teamTwo = 0;

    [SerializeField]
    private PlayerListing _playerListing;

    private RoomsCanvases _roomCanvases;

    private bool _ready = false;

    [SerializeField]
    private Text _readyUpText;

    private List<PlayerListing> _listingsOne = new List<PlayerListing>();
    private List<PlayerListing> _listingsTwo = new List<PlayerListing>();

    GameObject StartButton;
    GameObject ReadyUpButton;

    //==============================================================================================

    //override methods

    public override void OnEnable()
    {
        base.OnEnable();
        StartButton = GameObject.Find("StartGame");
        ReadyUpButton = GameObject.Find("ReadyUp");

        _teamManager = GameObject.Find("TeamManager").GetComponent<PhotonTeamsManager>();
        if (PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(true);
            ReadyUpButton.SetActive(false);
        }
        else if (!PhotonNetwork.IsMasterClient)
        {
            StartButton.SetActive(false);
            ReadyUpButton.SetActive(true);
        }
        
        SetReadyUp(false);
        GetCurrentRoomPlayers();

        //if(!PhotonNetwork.IsMasterClient)
        //{
        //    StartButton.SetActive(false);
        //    ReadyUpButton.SetActive(true);
        //}
        //else
        //{
        //    StartButton.SetActive(true);
        //    ReadyUpButton.SetActive(false);
        //}
    }

    public override void OnDisable()
    {
        base.OnDisable();
        for (int i = 0; i < _listingsOne.Count; i++)
            Destroy(_listingsOne[i].gameObject);

        for (int i = 0; i < _listingsTwo.Count; i++)
            Destroy(_listingsTwo[i].gameObject);

        _listingsOne.Clear();
        _listingsTwo.Clear();
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

        if (targetPlayer.CustomProperties.ContainsKey("_pt"))
        {
            if ((byte)targetPlayer.CustomProperties["_pt"] == 0)
                AddPlayerListingOne(targetPlayer);
            else
                AddPlayerListingTwo(targetPlayer);
        }
    }

    //public override void OnLeftRoom()
    //{
    //    base.OnLeftRoom();
    //    //PhotonNetwork.LocalPlayer.CustomProperties["_pt"] = null;
    //    if(PhotonNetwork.LocalPlayer.LeaveCurrentTeam())
    //    {
    //        Debug.Log(PhotonNetwork.LocalPlayer.NickName + "has left his/her team");
    //    }
    //    else
    //    {
    //        Debug.Log("function not called");
    //    }
    //}

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int index = _listingsOne.FindIndex(x => x.Player == otherPlayer);
        if (index != -1)
        {
            Destroy(_listingsOne[index].gameObject);
            _listingsOne.RemoveAt(index);
        }
        else
        {
            index = _listingsTwo.FindIndex(x => x.Player == otherPlayer);
            if (index != -1)
            {
                Destroy(_listingsTwo[index].gameObject);
                _listingsTwo.RemoveAt(index);
            }
        }
    }

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomCanvases = canvases;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        _roomCanvases.CurrentRoomCanvas.LeaveRoomMenu.OnClick_LeaveRoom();
    }

    //==============================================================================================

    //Ready up methods
    private void SetReadyUp(bool state)
    {
        _ready = state;
        if (_ready)
            _readyUpText.text = "Not Ready?";
        else
            _readyUpText.text = "Ready?";
    }

    public void OnClick_ReadyUp()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            SetReadyUp(!_ready);
            base.photonView.RPC("RPC_ChangeReadyState", RpcTarget.All, PhotonNetwork.LocalPlayer, _ready);
        }
    }

    [PunRPC]
    private void RPC_ChangeReadyState(Player player, bool ready)
    {
        if (ready)
        {
            if ((byte)player.CustomProperties["_pt"] == 0)
            {
                int index = _listingsOne.FindIndex(x => x.Player == player);
                if (index != -1)
                {
                    _listingsOne[index].Ready = ready;
                    _listingsOne[index].SetNotReadyInactive();
                    _listingsOne[index].SetReadyActive();
                }
            }
            else
            {
                int index = _listingsTwo.FindIndex(x => x.Player == player);
                if (index != -1)
                {
                    _listingsTwo[index].Ready = ready;
                    _listingsTwo[index].SetNotReadyInactive();
                    _listingsTwo[index].SetReadyActive();
                }
            }
        }
        else if (!ready)
        {
            if ((byte)player.CustomProperties["_pt"] == 0)
            {
                int index = _listingsOne.FindIndex(x => x.Player == player);
                if (index != -1)
                {
                    _listingsOne[index].Ready = ready;
                    _listingsOne[index].SetNotReadyActive();
                    _listingsOne[index].SetReadyInactive();
                }
            }
            else
            {
                int index = _listingsTwo.FindIndex(x => x.Player == player);
                if (index != -1)
                {
                    _listingsTwo[index].Ready = ready;
                    _listingsTwo[index].SetNotReadyActive();
                    _listingsTwo[index].SetReadyInactive();
                }
            }
        }
    }

    //==============================================================================================

    //Start game methods
    public void OnClick_StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < _listingsOne.Count; i++)
            {
                if (_listingsOne[i].Player != PhotonNetwork.LocalPlayer)
                {
                    if (!_listingsOne[i].Ready)
                        return;
                }
            }

            for (int i = 0; i < _listingsTwo.Count; i++)
            {
                if (_listingsTwo[i].Player != PhotonNetwork.LocalPlayer)
                {
                    if (!_listingsTwo[i].Ready)
                        return;
                }
            }

            PhotonNetwork.CurrentRoom.IsOpen = false; // players cannot join after game start
            PhotonNetwork.CurrentRoom.IsVisible = false; // removes room from room listing after game start
            PhotonNetwork.LoadLevel(1);
            //PhotonNetwork.LoadLevel(2);

        }
    }

    //==============================================================================================

    //Setting player listing and methods involved
    //Getting the current room players listings
    private void GetCurrentRoomPlayers()
    {
        //if (!photonView.IsMine) return;

        if (!PhotonNetwork.IsConnected)
            return;
        if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null)
            return;


        foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            if (playerInfo.Value != PhotonNetwork.LocalPlayer)
            {
                if ((byte)playerInfo.Value.CustomProperties["_pt"] == 0)
                    AddPlayerListingOne(playerInfo.Value);
                else
                    AddPlayerListingTwo(playerInfo.Value);
            }
        }

    }

    //Adding player to current room listings
    private void AddPlayerListingOne(Player player)
    {
        int index = _listingsOne.FindIndex(x => x.Player == player);
        if (index != -1)
        {
            _listingsOne[index].SetPlayerInfo(player);
            if(player.IsMasterClient)
            {
                _listingsOne[index].SetReadyActive();
                _listingsOne[index].SetNotReadyInactive();
            }
        }
        else
        {
            PlayerListing listing = Instantiate(_playerListing, _contentTeamOne);
            if (listing != null)
            {
                listing.SetPlayerInfo(player);
                _listingsOne.Add(listing);
                listing.SetPlayerText(player);
                Debug.Log(player.NickName);

                if(player.IsMasterClient)
                {
                    listing.SetReadyActive();
                    listing.SetNotReadyInactive();
                }
            }
        }
    }

    private void AddPlayerListingTwo(Player player)
    {
        int index = _listingsTwo.FindIndex(x => x.Player == player);
        if (index != -1)
        {
            _listingsTwo[index].SetPlayerInfo(player);
        }
        else
        {
            PlayerListing listing = Instantiate(_playerListing, _contentTeamTwo);
            if (listing != null)
            {
                listing.SetPlayerInfo(player);
                _listingsTwo.Add(listing);
                listing.SetPlayerText(player);
                Debug.Log(player.NickName);
            }

            if (player.IsMasterClient)
            {
                listing.SetReadyActive();
                listing.SetNotReadyInactive();
            }
        }
    }

    //Remove player listings
    [PunRPC]
    public void RemovePlayerListing(Player player)
    {
        if ((byte)player.CustomProperties["_pt"] == 0)
        {
            int index = _listingsOne.FindIndex(x => x.Player == player);
            if (index != -1)
            {
                Destroy(_listingsOne[index].gameObject);
                _listingsOne.RemoveAt(index);
            }
        }
        else
        {
            int index = _listingsTwo.FindIndex(x => x.Player == player);
            if (index != -1)
            {
                Destroy(_listingsTwo[index].gameObject);
                _listingsTwo.RemoveAt(index);
            }
        }
    }

    //Switch teams
    public void OnClick_ButtonSwitchTeams()
    {
        base.photonView.RPC("RemovePlayerListing", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer);
        if ((byte)PhotonNetwork.LocalPlayer.CustomProperties["_pt"] == 0)
        {
            //RemovePlayerListing(PhotonNetwork.LocalPlayer);
            if (PhotonNetwork.LocalPlayer.SwitchTeam(1))
            {
                AddPlayerListingTwo(PhotonNetwork.LocalPlayer);
            }
        }
        else
        {
            //RemovePlayerListing(PhotonNetwork.LocalPlayer);
            if (PhotonNetwork.LocalPlayer.SwitchTeam(0))
            {
                AddPlayerListingOne(PhotonNetwork.LocalPlayer);
            }
        }
    }
}
