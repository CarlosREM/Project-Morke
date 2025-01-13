using System;
using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
public class BreakerBehavior : MonoBehaviour
{
    private InteractableObject _interactableComp;
    [SerializeField] private FMODUnity.StudioEventEmitter sfx;
    [SerializeField] private int objectiveIdx;
    
    private void Awake()
    {
        _interactableComp = GetComponent<InteractableObject>();
    }

    public void BreakerEnabled()
    {
        GameLoopManager.CurrentLevelManager.OnBreakerEnabled(objectiveIdx);
        sfx.Play();
        _interactableComp.enabled = false;
    }
}
