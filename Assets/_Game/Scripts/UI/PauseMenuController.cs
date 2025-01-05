using System;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    public PlayerHudManager HudRef { get; set; }
    private Rewired.Player _input;
    
    private void Awake()
    {
        _input = GameInputManager.MainPlayer;
    }

    private void OnEnable()
    {
        Time.timeScale = 0;
        
        GameInputManager.ChangeInputMap("UI");
    }

    private void OnDisable()
    {
        
        Time.timeScale = 1;
        
        GameInputManager.ChangeInputMap("Gameplay");
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
