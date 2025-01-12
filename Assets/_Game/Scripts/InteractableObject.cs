using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class InteractableObject : MonoBehaviour
{
    [SerializeField] private UnityEvent onEnterRange;
    [SerializeField] private UnityEvent onExitRange;
    [SerializeField] private UnityEvent onInteracted;
    
    private Collider2D _collider;

    public void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void OnDisable()
    {
        onExitRange?.Invoke();
        _collider.enabled = false;
    }

    public void Interact(GameObject source)
    {
        if (enabled)
            onInteracted?.Invoke();
    }

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
