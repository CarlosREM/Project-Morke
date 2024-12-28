using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LevelManager : MonoBehaviour
{
    [SerializeField] List<Transform> levelCheckpoints;
    
    public Transform GetLevelCheckpoint(int index)
    {
        Assert.IsTrue(index >= 0 && index < levelCheckpoints.Count, "Invalid level checkpoint index");
        return levelCheckpoints[index];
    }

    public void OnCheckpointReached(int checkpointIndex)
    {
        GameLoopManager.SetCheckpointIndex(checkpointIndex);
    }
}
