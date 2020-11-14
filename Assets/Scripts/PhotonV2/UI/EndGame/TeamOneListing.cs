using System;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class TeamOneListing : MonoBehaviour
{
    [SerializeField]
    private Text _teamOneText;

    public Player Player { get; private set; }

    public void SetTeamOnePlayerInfo(Player player)
    {
        Player = player;
    }

    public void SetPlayerText(Player player)
    {
        if (player.CustomProperties.ContainsKey("Kills") && player.CustomProperties.ContainsKey("Deaths"))
        {
            int Kills = (int)player.CustomProperties["Kills"];
            int Deaths = (int)player.CustomProperties["Deaths"];
            _teamOneText.text = player.NickName + "\t\t\t" + Kills.ToString() + " / " + Deaths.ToString();
        }
    }

    public void SetAIText(String text) {
      _teamOneText.text = text;
    }
}
