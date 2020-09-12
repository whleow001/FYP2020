using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MainMenuCanvas : MonoBehaviour
{
    [SerializeField]
    private MainMenuMenu _mainMenuMenu;

    private RoomsCanvases _roomsCanvases;

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomsCanvases = canvases;
        _mainMenuMenu.FirstInitialize(canvases);

    }
}
