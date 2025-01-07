using System;
using Unity.Behavior;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    private BehaviorGraphAgent _agent;
    private CharacterHealth _health;

    [Header("Components")] 
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem onHurtParticles;

    [Header("Parameters")] 
    [SerializeField] private float activateDelay = 1f;
    [SerializeField] private float respawnDelay = 6f;
    [SerializeField] private float flashlightDmgDelay;
    private float _currentLightTime;
    private bool _isInsideLight;

    private void Awake()
    {
        _agent = GetComponent<BehaviorGraphAgent>();
        _health = GetComponent<CharacterHealth>();
        _health.IsInvincible = true;
    }

    private void OnEnable()
    {
        if (!_health)
            return;
        
        _health.OnDeath += OnDeath;
        Invoke(nameof(OnSpawn), activateDelay);
    }

    private void OnDisable()
    {
        if (!_health)
            return;
        
        _health.OnDeath -= OnDeath;
    }

    private void OnSpawn()
    {
        _agent.enabled = true;
        _health.IsInvincible = false;
    }
    
    private void OnDeath()
    {
        onHurtParticles.Play();
        animator.SetTrigger("Dead");
        _agent.enabled = false;
        _health.enabled = false;
        _health.IsInvincible = true;
        
        Invoke(nameof(OnRespawn), respawnDelay);
    }

    private void OnRespawn()
    {
        _health.enabled = true;
        animator.SetTrigger("Reset");
        Invoke(nameof(OnSpawn), activateDelay);
    }

    private void Update()
    {
        if (_isInsideLight && !_health.IsInvincible)
        {
            _currentLightTime += Time.deltaTime;
            if (_currentLightTime >= flashlightDmgDelay)
            {
                _health.Hurt(1);
                _currentLightTime = 0;
                onHurtParticles.Play();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        if (other.attachedRigidbody.GetComponent<PlayerFlashlight>())
        {
            _isInsideLight = true;
            _currentLightTime = 0;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        if (other.attachedRigidbody.GetComponent<PlayerFlashlight>())
        {
            _isInsideLight = false;
            _currentLightTime = 0;
        }
    }
}
