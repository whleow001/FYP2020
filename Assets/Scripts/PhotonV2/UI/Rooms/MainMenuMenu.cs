using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MainMenuMenu : MonoBehaviour
{
    private RoomsCanvases _roomsCanvases;

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomsCanvases = canvases;
    }

    public void OnClick_CreateRoomMenu()
    {
        _roomsCanvases.CreateOrJoinRoomCanvas.Show();
    }

    public void OnClick_InstructionsPage()
    {
        _roomsCanvases.InstructionCanvas.Show();
    }
}
