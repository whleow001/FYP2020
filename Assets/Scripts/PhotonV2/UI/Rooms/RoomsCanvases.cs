using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsCanvases : MonoBehaviour
{
    [SerializeField]
    private CreateOrJoinRoomCanvas _createOrJoinRoomCanvas;
    public CreateOrJoinRoomCanvas CreateOrJoinRoomCanvas { get { return _createOrJoinRoomCanvas; } }

    [SerializeField]
    private CurrentRoomCanvas _currentRoomCanvas;
    public CurrentRoomCanvas CurrentRoomCanvas { get { return _currentRoomCanvas; } }

    [SerializeField]
    private MainMenuCanvas _mainMenuCanvas;
    public MainMenuCanvas MainMenuCanvas { get { return _mainMenuCanvas; } }

    [SerializeField]
    private InstructionCanvas _instructionCanvas;
    public InstructionCanvas InstructionCanvas { get { return _instructionCanvas; } }

    //[SerializeField]
    //private LoginAndRegCanvas _loginAndRegCanvas;
    //public LoginAndRegCanvas LoginAndRegCanvas { get { return _loginAndRegCanvas; } }

    private void Awake()
    {
        FirstInitialize();
    }

    private void FirstInitialize()
    {
        MainMenuCanvas.FirstInitialize(this);
        CreateOrJoinRoomCanvas.FirstInitialize(this);
        CurrentRoomCanvas.FirstInitialize(this);
        InstructionCanvas.FirstInitialize(this);
        //LoginAndRegCanvas.FirstInitialize(this);
    }
}
