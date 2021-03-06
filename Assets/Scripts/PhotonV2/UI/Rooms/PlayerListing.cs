﻿using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerListing : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Text _text;
    [SerializeField]
    private GameObject _readyCheck;
    [SerializeField]
    private GameObject _notReadyCheck;

    public Player Player { get; private set; }
    public bool Ready = false;

    public void SetPlayerInfo(Player player)
    {
        Player = player;

        //SetPlayerText(player);
    }

    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(target, changedProps);
        if (target != null && target == Player)
        {
            if (changedProps.ContainsKey("Team"))
                SetPlayerText(target);
        }
    }
        

    public void SetPlayerText(Player player)
    {
        _text.text = player.NickName;
    }

    public void SetReadyActive()
    {
        _readyCheck.SetActive(true);
    }

    public void SetReadyInactive()
    {
        _readyCheck.SetActive(false);
    }

    public void SetNotReadyActive()
    {
        _notReadyCheck.SetActive(true);
    }

    public void SetNotReadyInactive()
    {
        _notReadyCheck.SetActive(false);
    }
}
