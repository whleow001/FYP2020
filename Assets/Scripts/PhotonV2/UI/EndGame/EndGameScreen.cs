using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
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

    public void Show()
    {
        director = GameObject.Find("Director").GetComponent<GameDirector>();
        gameObject.SetActive(true);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        GetCurrentRoomPlayers();
        SetWinText();
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
            if ((int)playerInfo.Value.CustomProperties["Team"] == 1)
                AddPlayerListingOne(playerInfo.Value);
            else if ((int)playerInfo.Value.CustomProperties["Team"] == 2)
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

    private void AddPlayerListingTwo(Player player)
    {
        int index = _listingsOne.FindIndex(x => x.Player == player);
        if (index != -1)
        {
            _listingsTwo[index].SetTeamTwoPlayerInfo(player);
        }
        else
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

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if ((int)otherPlayer.CustomProperties["Team"] == 1)
        {
            int index = _listingsOne.FindIndex(x => x.Player == otherPlayer);
            if (index != -1)
            {
                Destroy(_listingsOne[index].gameObject);
                _listingsOne.RemoveAt(index);
            }
        }
        else if((int)otherPlayer.CustomProperties["Team"] == 2)
        {
            int index = _listingsTwo.FindIndex(x => x.Player == otherPlayer);
            if (index != -1)
            {
                Destroy(_listingsTwo[index].gameObject);
                _listingsTwo.RemoveAt(index);
            }
        }
    }

    public void SetWinText()
    {
        if (director.generatorCount == 0)
        {
            _winText.text = "Government Team Win!!";
        }
    }

    public void OnClick_LeaveButton()
    {
        PhotonNetwork.LoadLevel(0);
    }
}
