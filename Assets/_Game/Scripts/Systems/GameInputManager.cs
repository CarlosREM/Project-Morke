using System;
using Rewired;
using UnityEngine;

[RequireComponent(typeof(Rewired.InputManager))]
public class GameInputManager : MonoBehaviour
{
    public static Rewired.InputManager InputManager { get; private set; }
    public static GameInputManager Instance { get; private set; }

    public static Player MainPlayer => ReInput.players.SystemPlayer;

    
    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
            return;
        }
        
        Instance = this;
        InputManager = GetComponent<Rewired.InputManager>();
    }

    public static void ChangeInputMap(string mapName)
    {
        if (!ReInput.isReady)
            return;

        var mapEnabler = MainPlayer.controllers.maps.mapEnabler;
        var playerMapRulesets = mapEnabler.ruleSets;

        // This works as follows:
        // assuming that every map category has a corresponding map ruleset by the SAME name
        // this ruleset will be used to activate that specific map and deactivate the rest.
        // "map name" should match with any of the existing map categories
        // otherwise, the map switch wont occur

        var mapNames = InputManager.userData.GetMapCategoryNames();

        if (System.Array.FindIndex(mapNames, item => item == mapName) < 0)
        {
            Debug.LogError($"ChangePlayerMap: No map by name {mapName}");
            return;
        }

        for (int i = 0; i < playerMapRulesets.Count; ++i)
        {
            playerMapRulesets[i].enabled = mapName == mapNames[i];
        }

        mapEnabler.Apply();

        Cursor.visible = mapName.Equals("UI");
    }
    
}
