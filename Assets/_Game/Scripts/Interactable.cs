using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] private UnityEvent onEnterRange;
    [SerializeField] private UnityEvent onExitRange;
    
    public abstract bool Interact(GameObject source);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(this.tag))
            return;
        
        onEnterRange.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(this.tag))
            return;
        
        onExitRange.Invoke();
    }
}
