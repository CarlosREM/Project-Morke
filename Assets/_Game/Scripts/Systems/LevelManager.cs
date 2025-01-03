using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameLoopManager gameLoopManagerPrefab;
     
    [SerializeField] List<Transform> levelCheckpoints;

    private IEnumerator Start()
    {
        GameLoopManager.CurrentLevelManager = this;
        if (!GameLoopManager.Instance)
        {
            Instantiate(gameLoopManagerPrefab);
            // initialize is called on game loop manager start
        }
        else
        {
            GameLoopManager.InitializeLevel();
        }
        
        yield return null;
        
        GameInputManager.ChangeInputMap("Gameplay");
        
        yield return null;
        
        TransitionManager.TransitionFadeOut();
    }

    public Transform GetLevelCheckpoint(int index)
    {
        Assert.IsTrue(index >= 0 && index < levelCheckpoints.Count, "Invalid level checkpoint index");
        return levelCheckpoints[index];
    }

    public void OnCheckpointReached(int checkpointIndex)
    {
        GameLoopManager.CheckpointIndex = checkpointIndex;
    }
}
