using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class TeamTwoListing : MonoBehaviour
{
    [SerializeField]
    private Text _teamTwoText;

    public Player Player { get; private set; }

    public void SetTeamTwoPlayerInfo(Player player)
    {
        Player = player;
    }

    public void SetPlayerText(Player player)
    {
        if (player.CustomProperties.ContainsKey("Kills") && player.CustomProperties.ContainsKey("Deaths"))
        {
            int Kills = (int)player.CustomProperties["Kills"];
            int Deaths = (int)player.CustomProperties["Deaths"];
            _teamTwoText.text = player.NickName + "\t\t\t" + Kills.ToString() + " / " + Deaths.ToString();
        }
    }
}
