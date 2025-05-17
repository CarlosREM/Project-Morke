using System;
using UnityEngine;
using UnityEngine.Assertions;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private float cooldown = 2;
    [SerializeField] private ParticleSystem onSaveParticles;
    [SerializeField] private GameObject[] visualsOn;
    [SerializeField] private FMODUnity.EventReference sfxReference;

    private bool _isOn;
    private float _currentCooldown;
    
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

    private void Update()
    {
        _currentCooldown.UpdateTimer();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // don't activate if not player
        if (!other.CompareTag("Player"))
            return;

        // don't activate if on cooldown
        if (_currentCooldown > 0)
            return;
        
        // don't activate if respawning
        if (other.attachedRigidbody.GetComponent<CharacterHealth>().IsDead)
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
        other.attachedRigidbody.GetComponent<CharacterHealth>().FullHeal();
        _currentCooldown = cooldown;
        
        FMODUnity.RuntimeManager.PlayOneShot(sfxReference, transform.position);
        
        OnCheckpointActivated?.Invoke(this);
    }
}
