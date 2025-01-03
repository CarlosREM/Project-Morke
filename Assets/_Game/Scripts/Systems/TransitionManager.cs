using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TransitionManager : MonoBehaviour
{
    private Animator _transitionAnimator;

    private static TransitionManager _instance;

    public static Action onTransitionInComplete;
    public static Action onTransitionOutComplete;
    
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
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
        if (!_instance)
            return;
        
        _instance._transitionAnimator.SetBool("Fade", true);
        _instance._transitionAnimator.SetTrigger("Transition");
    }
    
    public static void TransitionFadeOut()
    {
        if (!_instance)
            return;
        
        _instance._transitionAnimator.SetBool("Fade", false);
        _instance._transitionAnimator.SetTrigger("Transition");
    }
}
