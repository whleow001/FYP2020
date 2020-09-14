using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class InstructionCanvas : MonoBehaviour
{
    [SerializeField]
    private InstructionMenu _instructionMenu;

    private RoomsCanvases _roomsCanvases;

    public void FirstInitialize(RoomsCanvases canvases)
    {
        _roomsCanvases = canvases;
        _instructionMenu.FirstInitialize(canvases);
        }
}
