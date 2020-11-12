using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class InstructionMenu : MonoBehaviour
{
    private RoomsCanvases _roomsCanvases;

    [SerializeField]
    public GameObject InstructionsPage;
    [SerializeField]
    public GameObject ControlsPage;
    [SerializeField]
    public GameObject ObjectivesPage;
    [SerializeField]
    public GameObject CharactersPage;

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomsCanvases = canvases;
    }

    public void OnClick_BackButton()
    {
        _roomsCanvases.InstructionCanvas.Hide();
    }

    public void OnClick_ControlsButton()
    {
        ControlsPage.SetActive(true);
        InstructionsPage.SetActive(false);
    }

    public void OnClick_ObjectivesButton()
    {
        ObjectivesPage.SetActive(true);
        InstructionsPage.SetActive(false);
    }

    public void OnClick_CharactersButton()
    {
        CharactersPage.SetActive(true);
        InstructionsPage.SetActive(false);
    }

    public void OnClick_ControlsBackButton()
    {
        ControlsPage.SetActive(false);
        InstructionsPage.SetActive(true);
    }

    public void OnClick_ObjectivesBackButton()
    {
        ObjectivesPage.SetActive(false);
        InstructionsPage.SetActive(true);
    }

    public void OnClick_CharactersBackButton()
    {
        CharactersPage.SetActive(false);
        InstructionsPage.SetActive(true);
    }
}
