using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TransitionManager : MonoBehaviour
{
    private Animator _transitionAnimator;

    public static TransitionManager Instance { get; private set; }

    public static Action onTransitionInComplete;
    public static Action onTransitionOutComplete;
    
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
    }

    private void TransitionInComplete()
    {
        onTransitionInComplete?.Invoke();
    }

    private void TransitionOutComplete()
    {
        onTransitionOutComplete?.Invoke();
    }
    
    public static void TransitionFadeIn()
    {
        if (!Instance)
            return;
        
        Instance._transitionAnimator.SetBool("Fade", true);
        Instance._transitionAnimator.SetTrigger("Transition");
    }
    
    public static void TransitionFadeOut()
    {
        if (!Instance)
            return;
        
        Instance._transitionAnimator.SetBool("Fade", false);
        Instance._transitionAnimator.SetTrigger("Transition");
    }
}
