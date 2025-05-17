using System;
using UnityEngine;
using Rewired;

public class PauseMenuController : MonoBehaviour
{
    public PlayerHudManager HudRef { get; set; }
    private Rewired.Player _input;

    private bool backInputLock = true; // this is to avoid instantly closing the menu when opening it
    private FMOD.Studio.Bus busSFX;
    
    private void Awake()
    {
        _input = GameInputManager.MainPlayer;

        busSFX = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
    }

    private void OnEnable()
    {
        Time.timeScale = 0;
        
        GameInputManager.ChangeInputMap("UI");
        _input.AddInputEventDelegate(InputBack, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "UI_Cancel");
    }

    private void OnDisable()
    {
        Time.timeScale = 1;
        
        _input.RemoveInputEventDelegate(InputBack, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "UI_Cancel");
        GameInputManager.ChangeInputMap("Gameplay");

        backInputLock = true;
    }

    
    private void InputBack(InputActionEventData obj)
    {
        if (backInputLock)
            backInputLock = false;
        else
            ButtonResume();
    }
    
    public void ButtonResume()
    {
        HudRef.HidePauseMenu();
    }
    
    public void ButtonMainMenu()
    {
        GameLoopManager.ExitGameLoop();
    }
}
