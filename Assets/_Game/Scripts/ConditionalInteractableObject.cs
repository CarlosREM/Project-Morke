using UnityEngine;
using UnityEngine.Events;
public class ConditionalInteractableObject : InteractableObject
{
    [SerializeField] private UnityEvent onFailedInteracted;

    [SerializeField] private Conditional[] conditionals;

    public override void Interact(GameObject source)
    {
        foreach (var condition in conditionals)
        {
            if (!condition.TestCondition())
            {
                onFailedInteracted?.Invoke();
                return;
            }
        }
        
        base.Interact(source);
    }
}