using System;
using UnityEngine;
using UnityEngine.Events;

public class ProgressGate : MonoBehaviour
{
    [SerializeField] private int objectiveIndex;
    [SerializeField] private int minProgress;

    [SerializeField] private UnityEvent onCompleteProgress;

    private void OnEnable()
    {
        Debug.Log($"<color=white>[Progress Gate]</color> Tracking Objective {objectiveIndex} (Min Progress: {minProgress})", this);

        LevelManager.OnObjectiveProgress += TrackObjectiveProgress;
    }

    private void TrackObjectiveProgress(int index, LevelManager.LevelObjective objective)
    {
        if (index < objectiveIndex)
            return;

        if (index == objectiveIndex && objective.currentProgress < minProgress)
            return;
        
        Debug.Log("<color=white>[Progress Gate]</color> Progress gate unlocked", this);
        onCompleteProgress?.Invoke();
        enabled = false;
        
        LevelManager.OnObjectiveProgress -= TrackObjectiveProgress;
    }
}
