using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private ParticleSystem onSaveParticles;

    private bool _isOn;
    
    public static event Action<Checkpoint> OnCheckpointActivated;
    
    private void Start()
    {
        var trigger = GetComponent<Collider2D>();
        Assert.IsNotNull(trigger, "Checkpoint requires a Collider component");
        Assert.IsTrue(trigger.isTrigger, "Checkpoint Collider is not marked as trigger");
        Assert.IsNotNull(onSaveParticles, "No particles assigned");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!_isOn)
            _isOn = true;
        
        onSaveParticles.Play();
        
        OnCheckpointActivated?.Invoke(this);
    }
}
