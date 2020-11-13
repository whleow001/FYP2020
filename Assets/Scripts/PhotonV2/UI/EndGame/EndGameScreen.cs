using System;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
public class EndGameScreen : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Transform _contentTeamOne;
    [SerializeField]
    private Transform _contentTeamTwo;

    [SerializeField]
    private Text _winText;

    [SerializeField]
    private TeamOneListing _teamOneListing;
    [SerializeField]
    private TeamTwoListing _teamTwoListing;

    //Team List
    private List<TeamOneListing> _listingsOne = new List<TeamOneListing>();
    private List<TeamTwoListing> _listingsTwo = new List<TeamTwoListing>();

    private GameDirector director;
    private EventsManager eventsManager;
    private int i;

    private bool shown = false;

    public void Show(string message)
    {
        director = GameObject.Find("Director").GetComponent<GameDirector>();
        eventsManager = GameObject.Find("EventsManager").GetComponent<EventsManager>();
        //i = GetGeneratorCount(director);
        if (PhotonNetwork.IsMasterClient && !shown) {
          List<AIDirector> aiDirectors = director.GetAIDirectorList();

          foreach (AIDirector aiDirector in aiDirectors)
            eventsManager.AddAIListing_S(aiDirector.GetTeam(), aiDirector.GetBotDetails().botName, aiDirector.GetKills(), aiDirector.GetDeaths());

          shown = true;
        }

        SetWinText(message);
        gameObject.SetActive(true);
    }


    public override void OnEnable()
    {
        base.OnEnable();
        GetCurrentRoomPlayers();
        Debug.Log(PhotonNetwork.InRoom);
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
            if ((byte)playerInfo.Value.CustomProperties["_pt"] == 0)
                AddPlayerListingOne(playerInfo.Value);
            else if ((byte)playerInfo.Value.CustomProperties["_pt"] == 1)
                AddPlayerListingTwo(playerInfo.Value);
        }
    }

    private void AddPlayerListingOne(Player player)
    {
        int index = _listingsOne.FindIndex(x => x.Player == player);
        if (index != -1)
        {
            _listingsOne[index].SetTeamOnePlayerInfo(player);
        }
        else
        {
            if(_teamOneListing != null)
            {
                TeamOneListing listingOne = Instantiate(_teamOneListing, _contentTeamOne);
                if (listingOne != null)
                {
                    listingOne.SetTeamOnePlayerInfo(player);
                    _listingsOne.Add(listingOne);
                    listingOne.SetPlayerText(player);
                    //Debug.Log(player.NickName);
                }
            }

        }
    }

    public void AddAIListingOne(String botName, int kills, int deaths) {
      if (_teamOneListing != null) {
        TeamOneListing listingOne = Instantiate(_teamOneListing, _contentTeamOne);
        if (listingOne != null) {
          _listingsOne.Add(listingOne);
          listingOne.SetAIText(botName + "\t\t\t" + kills + " / " + deaths);
        }
      }
    }

    private void AddPlayerListingTwo(Player player)
    {
        int index = _listingsOne.FindIndex(x => x.Player == player);
        if (index != -1)
        {
            _listingsTwo[index].SetTeamTwoPlayerInfo(player);
        }
        else
        {
            if(_teamTwoListing != null)
            {
                TeamTwoListing listingTwo = Instantiate(_teamTwoListing, _contentTeamTwo);
                if (listingTwo != null)
                {
                    listingTwo.SetTeamTwoPlayerInfo(player);
                    _listingsTwo.Add(listingTwo);
                    listingTwo.SetPlayerText(player);
                    //Debug.Log(player.NickName);
                }
            }

        }
    }

    public void AddAIListingTwo(String botName, int kills, int deaths) {
      if (_teamTwoListing != null) {
        TeamTwoListing listingTwo = Instantiate(_teamTwoListing, _contentTeamTwo);
        if (listingTwo != null) {
          _listingsTwo.Add(listingTwo);
          listingTwo.SetAIText(botName + "\t\t\t" + kills + " / " + deaths);
        }
      }
    }

    public override void OnPlayerLeftRoom (Player otherPlayer)
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
        //test
        //PhotonNetwork.LoadLevel(0);

    }
    /*
    public int GetGeneratorCount(GameDirector director)
    {
        return director.generatorCount;
    }
    */

    public void SetWinText(string message)
    {
        _winText.text = message;
    }

    public void OnClick_LeaveButton()
    {
        //if (!photonView.IsMine) return;
        //Debug.Log(PhotonNetwork.InRoom);
        //PhotonNetwork.AutomaticallySyncScene = true;
        if(PhotonNetwork.InRoom == true)
        {
            if (PhotonNetwork.LocalPlayer.LeaveCurrentTeam())
            {
                Debug.Log(PhotonNetwork.LocalPlayer.NickName + "has left his/her team");
            }
            else
            {
                Debug.Log("function not called");
            }

            PhotonNetwork.LeaveRoom();
            //PhotonNetwork.LoadLevel(0);
        }
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel(0);
    }
}
