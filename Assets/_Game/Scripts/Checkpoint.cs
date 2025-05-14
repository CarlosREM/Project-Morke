using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private ParticleSystem onSaveParticles;
    [SerializeField] private GameObject[] visualsOn;

    private bool _isOn;
    
    public static event Action<Checkpoint> OnCheckpointActivated;
    
    private void Start()
    {
        var trigger = GetComponent<Collider2D>();
        Assert.IsNotNull(trigger, "Checkpoint requires a Collider component");
        Assert.IsTrue(trigger.isTrigger, "Checkpoint Collider is not marked as trigger");
        Assert.IsNotNull(onSaveParticles, "No particles assigned");
        
        foreach (var visual in visualsOn)
        {
            visual.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!_isOn)
        {
            _isOn = true;
            foreach (var visual in visualsOn)
            {
                visual.SetActive(true);
            }
        }

        onSaveParticles.Play();
        
        OnCheckpointActivated?.Invoke(this);
    }
}
