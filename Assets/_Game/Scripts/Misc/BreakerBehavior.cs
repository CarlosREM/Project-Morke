using System;
using UnityEngine;

[RequireComponent(typeof(InteractableObject))]
public class BreakerBehavior : MonoBehaviour
{
    private InteractableObject _interactableComp;
    
    private void Awake()
    {
        _interactableComp = GetComponent<InteractableObject>();
    }

    public void BreakerEnabled()
    {
        GameLoopManager.CurrentLevelManager.OnBreakerEnabled();
        _interactableComp.enabled = false;
    }
}
