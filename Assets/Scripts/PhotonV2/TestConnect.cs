using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;

public class TestConnect : MonoBehaviourPunCallbacks
{
    public GameObject[] DisableOnConnect;
    public GameObject[] EnableOnConnect;

    public static void ConnectToPhoton()
    {
        Debug.Log("Connecting to server.");
        PhotonNetwork.AutomaticallySyncScene = true;

        GetAccountInfoRequest request = new GetAccountInfoRequest();
        PlayFabClientAPI.GetAccountInfo(request, OnGetUsernameResult, OnGetUsernameError);
        PhotonNetwork.GameVersion = MasterManager.GameSettings.GameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public static void OnGetUsernameResult(GetAccountInfoResult result)
    {
        PhotonNetwork.NickName = result.AccountInfo.Username;
    }

    public static void OnGetUsernameError(PlayFabError error)
    {
        Debug.Log("Error");
    }

    public override void OnConnectedToMaster()
    { 
        Debug.Log("Connected to server.", this);
        print(PhotonNetwork.LocalPlayer.NickName);
        if(!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();

        foreach(GameObject g in DisableOnConnect)
        {
            g.SetActive(false);
        }
        foreach(GameObject g in EnableOnConnect)
        {
            g.SetActive(true);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from server for reason " + cause.ToString(), this);
    }
}
