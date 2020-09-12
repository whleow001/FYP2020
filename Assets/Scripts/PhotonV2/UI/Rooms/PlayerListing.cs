using ExitGames.Client.Photon;
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
        if (player.CustomProperties.ContainsKey("Team"))
        {
            int result = (int)player.CustomProperties["Team"];
            _text.text = result.ToString() + ", " + player.NickName;
        }
            



        //_text.text = player.CustomProperties["Team"].ToString() + ", " + player.NickName;
    }
}
