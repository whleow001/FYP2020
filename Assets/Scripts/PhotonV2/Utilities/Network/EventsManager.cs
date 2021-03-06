﻿using System.Collections;
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

    [SerializeField]
    private EndGameScreen endGameScreen;

    enum EventsCode : byte
    {
        RefreshTimer,
        DeathTimer,
        RefreshPoints,
        DisplayEndGame,
        RebelNotification,
        GovtNotification,
        GeneralNotification,
        CreditBotKill,
        AddAIListing,
        PlayFootsteps,
        PlayNormalAttack,
        PlayGunslingerSkill,
        PlayJuggernautSkill,
        PlaySniperSkill,
        PlayExplosion,
        PlayOnHit,
        ShowCutscene
    }

    //reference to game director & Player controller
    private void Awake()
    {
        director = GameObject.Find("Director").GetComponent<GameDirector>();
    }

    //Void OnEvent
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code >= 200) return;
        EventsCode e = (EventsCode)photonEvent.Code;
        object[] o = (object[])photonEvent.CustomData;
        switch (e)
        {
            //fill in for timer
            case EventsCode.RefreshTimer:
                RefreshTimer_R(o);
                break;
            case EventsCode.DeathTimer:
                DeathTimer_R(o);
                break;
            case EventsCode.RefreshPoints:
                RefreshPoints_R(o);
                break;
            case EventsCode.DisplayEndGame:
                DisplayEndGame_R(o);
                break;
            case EventsCode.RebelNotification:
                RebelNotification_R(o);
                break;
            case EventsCode.GovtNotification:
                GovtNotification_R(o);
                break;
            case EventsCode.GeneralNotification:
                GeneralNotification_R(o);
                break;
            case EventsCode.CreditBotKill:
                CreditBotKill_R(o);
                break;
            case EventsCode.AddAIListing:
                AddAIListing_R(o);
                break;
            case EventsCode.PlayFootsteps:
                PlayFootsteps_R(o);
                break;
            case EventsCode.PlayNormalAttack:
                PlayNormalAttack_R(o);
                break;
            case EventsCode.PlayGunslingerSkill:
                PlayGunslingerSkill_R(o);
                break;
            case EventsCode.PlayJuggernautSkill:
                PlayJuggernautSkill_R(o);
                break;
            case EventsCode.PlaySniperSkill:
                PlaySniperSkill_R(o);
                break;
            case EventsCode.PlayExplosion:
                PlayExplosion_R(o);
                break;
            case EventsCode.PlayOnHit:
                PlayOnHit_R(o);
                break;
            case EventsCode.ShowCutscene:
                StartCutscene_R(o);
                break;

        }
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

    public void DeathTimer_S(int team, int position, int respawnTimer)
    {
      object[] package = new object[] { team, position, respawnTimer };

      PhotonNetwork.RaiseEvent(
        (byte)EventsCode.DeathTimer,
        package,
        new RaiseEventOptions { Receivers = ReceiverGroup.All },
        new SendOptions { Reliability = true }
      );
    }

    public void DeathTimer_R(object[] data)
    {
      director.deathTimers[(int)data[0]][(int)data[1]] = (int)data[2];
      director.UpdateDeathTimers();
    }

    //Send and recieve Events RefreshPoints
    public void RefreshPoints_S()
    {
      if (director is RebelHQ_B) {
        RebelHQ_B Bdirector = (RebelHQ_B)director;
        object[] package = new object[] {Bdirector.GetGovtPoints()};

        PhotonNetwork.RaiseEvent(
          (byte)EventsCode.RefreshPoints,
          package,
          new RaiseEventOptions { Receivers = ReceiverGroup.All },
          new SendOptions { Reliability = true }
        );
      }

    }

    public void RefreshPoints_R(object[] data)
    {
      if (director is RebelHQ_B) {
        RebelHQ_B Bdirector = (RebelHQ_B)director;
        Bdirector.SetGovtPoints((int)data[0]);
      }
    }

    //Send and receive event DisplayEndGame
    public void DisplayEndGame_S(string message)
    {
        object[] package = new object[] { message };

        PhotonNetwork.RaiseEvent(
            (byte)EventsCode.DisplayEndGame,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = false }
            );
    }

    public void DisplayEndGame_R(object[] data)
    {
        string WinText = data[0].ToString();
        director.GetEndGameScreen().Show(WinText);
    }

    //Send and receive event DisplayEndGame
    public void StartCutscene_S(bool win)
    {
        object[] package = new object[] { win };

        PhotonNetwork.RaiseEvent(
            (byte)EventsCode.ShowCutscene,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = false }
            );
    }

    public void StartCutscene_R(object[] data)
    {
        bool GovtWin = (bool)data[0];
        if(GovtWin)
        {
            director.GetPlayerManager().GetComponent<PlayerController>().SetGovtWin();
            StartCoroutine(director.CutsceneTime());
        }
        //director.GetEndGameScreen().Show(WinText);
    }

    public void RebelNotification_S(string message, float durationSeconds)
    {
        object[] package = new object[2];

        package[0] = message;
        package[1] = durationSeconds;
        //package[2] = timer;

        foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            if ((byte)playerInfo.Value.CustomProperties["_pt"] == 1)
            {
                PhotonNetwork.RaiseEvent(
                    (byte)EventsCode.RebelNotification,
                    package,
                    new RaiseEventOptions { TargetActors = new[] { playerInfo.Value.ActorNumber } },
                    new SendOptions { Reliability = true }
                );
            }
        }
    }

    public void RebelNotification_R(object[] data)
    {
        string NotifText = data[0].ToString();
        float duration = (float)data[1];
        //bool timerState = (bool)data[2];

        director.GetUIText(BaseTexts.notify).SetText(NotifText, duration);
        //director.UITexts[3].SetActiveState(true);
    }

    public void GovtNotification_S(string message, float durationSeconds)
    {
        object[] package = new object[2];

        package[0] = message;
        package[1] = durationSeconds;
        //package[2] = timer;

        foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            if ((byte)playerInfo.Value.CustomProperties["_pt"] == 0)
            {
                PhotonNetwork.RaiseEvent(
                    (byte)EventsCode.RebelNotification,
                    package,
                    new RaiseEventOptions { TargetActors = new[] { playerInfo.Value.ActorNumber } },
                    new SendOptions { Reliability = true }
                );
            }
        }
    }

    public void GovtNotification_R(object[] data)
    {
        string NotifText = data[0].ToString();
        float duration = (float)data[1];
        //bool timerState = (bool)data[2];

        director.GetUIText(BaseTexts.notify).SetText(NotifText, duration);
        //director.UITexts[3].SetActiveState(true);
    }

    public void CreditBotKill_S(int botPosition) {
      object[] package = new object[1];
      package[0] = botPosition;

      PhotonNetwork.RaiseEvent(
        (byte)EventsCode.CreditBotKill,
        package,
        new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
        new SendOptions { Reliability = true }
      );
    }

    public void CreditBotKill_R(object[] data) {
      int botPosition = (int)data[0];

      director.CreditBotKill(botPosition);
    }

    public void AddAIListing_S(int team, string botName, int kills, int deaths)
    {
      object[] package = new object[4];

      package[0] = team;
      package[1] = botName;
      package[2] = kills;
      package[3] = deaths;

      PhotonNetwork.RaiseEvent(
        (byte)EventsCode.AddAIListing,
        package,
        new RaiseEventOptions { Receivers = ReceiverGroup.All },
        new SendOptions { Reliability = true }
      );
    }

    public void AddAIListing_R(object[] data)
    {
      int team = (int)data[0];
      string botName = data[1].ToString();
      int kills = (int)data[2];
      int deaths = (int)data[3];

      if (team == 0)
        endGameScreen.AddAIListingOne(botName, kills, deaths);
      else
        endGameScreen.AddAIListingTwo(botName, kills, deaths);
    }

    public void PlayFootsteps_S(int viewID)
    {
      object[] package = new object[1];

      package[0] = viewID;

      PhotonNetwork.RaiseEvent(
        (byte)EventsCode.PlayFootsteps,
        package,
        new RaiseEventOptions { Receivers = ReceiverGroup.All },
        new SendOptions { Reliability = true }
      );
    }

    public void PlayFootsteps_R(object[] data)
    {
      int viewID = (int)data[0];

      PhotonView PV = PhotonView.Find(viewID);

      if (PV.gameObject)
        PV.gameObject.GetComponent<FootstepsManager>().PlaySource();
    }

    public void PlayNormalAttack_S(int viewID)
    {
      object[] package = new object[1];

      package[0] = viewID;

      PhotonNetwork.RaiseEvent(
        (byte)EventsCode.PlayNormalAttack,
        package,
        new RaiseEventOptions { Receivers = ReceiverGroup.All },
        new SendOptions { Reliability = true }
      );
    }

    public void PlayNormalAttack_R(object[] data)
    {
      int viewID = (int)data[0];

      PhotonView PV = PhotonView.Find(viewID);

      if (PV.gameObject)
        PV.gameObject.GetComponent<FiringManager>().PlaySource();
    }

    public void PlayGunslingerSkill_S(int viewID)
    {
      object[] package = new object[1];

      package[0] = viewID;

      PhotonNetwork.RaiseEvent(
        (byte)EventsCode.PlayGunslingerSkill,
        package,
        new RaiseEventOptions { Receivers = ReceiverGroup.All },
        new SendOptions { Reliability = true }
      );
    }

    public void PlayGunslingerSkill_R(object[] data)
    {
      int viewID = (int)data[0];

      PhotonView PV = PhotonView.Find(viewID);

      if (PV.gameObject)
        PV.gameObject.GetComponent<GunslingerSkillManager>().PlaySource();
    }

    public void PlayJuggernautSkill_S(int viewID)
    {
      object[] package = new object[1];

      package[0] = viewID;

      PhotonNetwork.RaiseEvent(
        (byte)EventsCode.PlayJuggernautSkill,
        package,
        new RaiseEventOptions { Receivers = ReceiverGroup.All },
        new SendOptions { Reliability = true }
      );
    }

    public void PlayJuggernautSkill_R(object[] data)
    {
      int viewID = (int)data[0];

      PhotonView PV = PhotonView.Find(viewID);

      if (PV.gameObject)
        PV.gameObject.GetComponent<JuggernautSkillManager>().PlaySource();
    }

    public void PlaySniperSkill_S(int viewID)
    {
      object[] package = new object[1];

      package[0] = viewID;

      PhotonNetwork.RaiseEvent(
        (byte)EventsCode.PlaySniperSkill,
        package,
        new RaiseEventOptions { Receivers = ReceiverGroup.All },
        new SendOptions { Reliability = true }
      );
    }

    public void PlaySniperSkill_R(object[] data)
    {
      int viewID = (int)data[0];

      PhotonView PV = PhotonView.Find(viewID);

      if (PV.gameObject)
        PV.gameObject.GetComponent<SniperSkillManager>().PlaySource();
    }

    public void PlayExplosion_S(int viewID)
    {
      object[] package = new object[1];

      package[0] = viewID;

      PhotonNetwork.RaiseEvent(
        (byte)EventsCode.PlayExplosion,
        package,
        new RaiseEventOptions { Receivers = ReceiverGroup.All },
        new SendOptions { Reliability = true }
      );
    }

    public void PlayExplosion_R(object[] data)
    {
      int viewID = (int)data[0];

      PhotonView PV = PhotonView.Find(viewID);

      if (PV.gameObject)
        PV.gameObject.GetComponent<DestroyManager>().PlaySource();
    }

    public void PlayOnHit_S(int viewID)
    {
      object[] package = new object[1];

      package[0] = viewID;

      PhotonNetwork.RaiseEvent(
        (byte)EventsCode.PlayOnHit,
        package,
        new RaiseEventOptions { Receivers = ReceiverGroup.All },
        new SendOptions { Reliability = true }
      );
    }

    public void PlayOnHit_R(object[] data)
    {
      int viewID = (int)data[0];

      PhotonView PV = PhotonView.Find(viewID);

      if (PV.gameObject)
        PV.gameObject.GetComponent<DamageManager>().PlaySource();
    }

    public void GeneralNotification_S(string message, float durationSeconds, string purpose)
    {
        object[] package = new object[3];

        package[0] = message;
        package[1] = durationSeconds;
        package[2] = purpose;

        PhotonNetwork.RaiseEvent(
            (byte)EventsCode.RebelNotification,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
        );
    }

    public void GeneralNotification_R(object[] data)
    {
        string NotifText = data[0].ToString();
        float duration = (float)data[1];
        //bool timerState = (bool)data[2];
        string purpose = data[2].ToString();
        if (purpose == "CombatLog")
        {
            director.GetUIText(BaseTexts.notify).SetText(NotifText, duration);
        }
        else if (purpose == "PlayerDisconnect")
        {
            director.GetUIText(BaseTexts.disconnect).SetText(NotifText, duration);
        }
    }
}
