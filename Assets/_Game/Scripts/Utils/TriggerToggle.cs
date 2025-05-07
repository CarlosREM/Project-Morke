using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class TriggerToggle : MonoBehaviour
{
    public UnityEvent<bool> onTriggerToggle;

    [SerializeField, TagSelector] private string[] tagsToCheck;
    private bool _toggleStatus = false;
    
    private void Awake()
    {
        var trigger = GetComponent<Collider2D>();
        Assert.IsNotNull(trigger, "No trigger component found in object");
        Assert.IsTrue(trigger.isTrigger, "Collider component not marked as trigger");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!tagsToCheck.Contains(collision.tag))
            return;
        
        Debug.Log("Trigger toggle activated");
        
        _toggleStatus = !_toggleStatus;
        
        onTriggerToggle.Invoke(_toggleStatus);
    }
}
