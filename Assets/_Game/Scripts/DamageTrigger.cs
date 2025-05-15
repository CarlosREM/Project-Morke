using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class DamageTrigger : MonoBehaviour
{ 
    [SerializeField, Min(0)] private int damage;
    
    [TagSelector] public string[] DamageTagFilter = new string[] { };

    [SerializeField] private UnityEvent onDamageDealt;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.attachedRigidbody)
            return;
        
        // ignore object if it doesn't have a health component
        if (!other.attachedRigidbody.TryGetComponent<CharacterHealth>(out var otherHealth))
            return;

        if (DamageTagFilter.Contains(otherHealth.tag) && !otherHealth.IsDead)
        {
            otherHealth.Hurt(damage);
            onDamageDealt.Invoke();
        }
    }
}
