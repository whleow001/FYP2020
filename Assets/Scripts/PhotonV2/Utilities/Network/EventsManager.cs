using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Xml.Serialization;
using Photon.Pun.UtilityScripts;
using UnityEngine.Rendering;

public class EventsManager : MonoBehaviourPun, IOnEventCallback {

    private GameDirector director;
    //private EndGameScreen endGameScreen;

    //reference to game director
    private void Awake()
    {
        director = GameObject.Find("Director").GetComponent<GameDirector>();
        //endGameScreen = GameObject.Find("EndGameScreen").GetComponent<EndGameScreen>();
    }
    enum EventsCode : byte {
        RefreshTimer,
        DisplayEndGame
    }

    //On enable and disable
    private void OnEnable() {
        PhotonNetwork.AddCallbackTarget(this);
    }
    private void OnDisable() {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    //Send and receive Events RefreshTimer
    public void RefreshTimer_S()
    {
        object[] package = new object[] {director.currentMatchTime};

        PhotonNetwork.RaiseEvent(
            (byte)EventsCode.RefreshTimer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void RefreshTimer_R(object[] data)
    {
        director.currentMatchTime = (int)data[0];
        director.RefreshTimerUI();
    }

    //Send and receive event DisplayEndGame
    public void DisplayEndGame_S(string message)
    {
        object[] package = new object[] { message };

        PhotonNetwork.RaiseEvent(
            (byte)EventsCode.DisplayEndGame,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void DisplayEndGame_R(object[] data)
    {
        string WinText = data[0].ToString();
        director._endGameScreen.Show(WinText);
        }

    //Void OnEvent
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code >= 200) return;
        EventsCode e = (EventsCode)photonEvent.Code;
        object[] o = (object[])photonEvent.CustomData;
        switch (e) {
            //fill in for timer
            case EventsCode.RefreshTimer:
                RefreshTimer_R(o);
                break;
            case EventsCode.DisplayEndGame:
                DisplayEndGame_R(o);
                break;
        }
    }

    
}

