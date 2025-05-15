using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class ColliderToggle : MonoBehaviour
{
    [Serializable]
    enum ToggleActivationMode
    {
        OnTouch,
        OnStay,
        RightLeft,
        LeftRight,
        UpDown,
        DownUp,
    }
    
    [SerializeField] private ToggleActivationMode toggleActivationMode = ToggleActivationMode.OnTouch;
    [SerializeField, TagSelector] private string[] tagsToCheck;
    
    public UnityEvent<bool> onToggle;

    private bool _toggleStatus;
    private bool _isTrigger;
    
    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        Assert.IsNotNull(col, "No collider component found in object");

        _isTrigger = col.isTrigger;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (_isTrigger)
            return;
        
        if (col.rigidbody == null)
            return;
        
        if (!tagsToCheck.Contains(col.rigidbody.tag))
            return;

        UpdateToggle(col.rigidbody);
        
        Debug.Log($"Collision toggle activated: {_toggleStatus}");
        
        onToggle.Invoke(_toggleStatus);
    }
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!_isTrigger)
            return;
        
        if (col.attachedRigidbody == null)
            return;
        
        if (!tagsToCheck.Contains(col.attachedRigidbody.tag))
            return;
        
        UpdateToggle(col.attachedRigidbody);
        
        Debug.Log($"Trigger toggle activated: {_toggleStatus}");
        
        onToggle.Invoke(_toggleStatus);
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (!_isTrigger)
            return;
        
        if (col.attachedRigidbody == null)
            return;
        
        if (!tagsToCheck.Contains(col.attachedRigidbody.tag))
            return;
        
        if (toggleActivationMode == ToggleActivationMode.OnStay)
            UpdateToggle(col.attachedRigidbody);
        
        Debug.Log($"Trigger toggle activated: {_toggleStatus}");
        
        onToggle.Invoke(_toggleStatus);
    }

    private void UpdateToggle(Rigidbody2D otherRb)
    {
        switch (toggleActivationMode)
        {
            default:
                return;
            
            case ToggleActivationMode.OnTouch:
            case ToggleActivationMode.OnStay:
                _toggleStatus = !_toggleStatus;
                break;
            
            case ToggleActivationMode.RightLeft:
                // on if right
                // off if left
                _toggleStatus = otherRb.position.x >= transform.position.x;
                break;            
            
            case ToggleActivationMode.LeftRight:
                // on if right
                // off if left
                _toggleStatus = otherRb.position.x <= transform.position.x;
                break;
            
            case ToggleActivationMode.UpDown:
                // on is up
                // off is down
                _toggleStatus = otherRb.position.y >= transform.position.y;
                break;
            
            case ToggleActivationMode.DownUp:
                // on is up
                // off is down
                _toggleStatus = otherRb.position.y <= transform.position.y;
                break;
        }
    }
}
