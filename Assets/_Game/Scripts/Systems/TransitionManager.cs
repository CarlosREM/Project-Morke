using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TransitionManager : MonoBehaviour
{
    private Animator _transitionAnimator;

    public static TransitionManager Instance { get; private set; }

    public static event Action OnTransitionInComplete;
    public static event Action OnTransitionOutComplete;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        
        _transitionAnimator = GetComponent<Animator>();
        Debug.Log("<color=white>[Transition Manager]</color> <color=green>Ready</color>", this);
    }

    private void TransitionInComplete()
    {
        OnTransitionInComplete?.Invoke();
    }

    private void TransitionOutComplete()
    {
        PlayerHudManager.CanPause = true;
        OnTransitionOutComplete?.Invoke();
    }
    
    public static void TransitionFadeIn()
    {
        if (!Instance)
            return;
        
        Instance._transitionAnimator.SetBool("Fade", true);
        Instance._transitionAnimator.SetTrigger("Transition");
        PlayerHudManager.CanPause = false;
    }
    
    public static void TransitionFadeOut()
    {
        if (!Instance)
            return;
        
        Instance._transitionAnimator.SetBool("Fade", false);
        Instance._transitionAnimator.SetTrigger("Transition");
    }
}
