using System;
using System.Collections;
using Unity.Behavior;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyControl : MonoBehaviour
{
    private BehaviorGraphAgent _agent;
    private CharacterHealth _health;

    [Header("Components")] 
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem onHurtParticles;
    [SerializeField] private FMODUnity.EventReference sfxIdle;
    [SerializeField] private FMODUnity.StudioEventEmitter sfxHurt, sfxDead, sfxRespawn;

    [Header("Parameters")] 
    [SerializeField] private bool canRespawn;
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
        
        _health.OnHurt += OnHurt;
        _health.OnDeath += OnDeath;
        Invoke(nameof(OnSpawn), activateDelay);
    }

    private void OnDisable()
    {
        if (!_health)
            return;
        
        _health.OnHurt -= OnHurt;
        _health.OnDeath -= OnDeath;
        
        canRespawn = false;
        OnDeath();
    }

    private void OnSpawn()
    {
        _agent.enabled = true;
        _health.IsInvincible = false;
        //StartCoroutine(IdleNoises());
    }
    
    private void OnHurt(int obj)
    {
        sfxHurt.Play();
        onHurtParticles.Play();
    }
    
    private void OnDeath()
    {
        sfxDead.Play();
        onHurtParticles.Play();
        animator.SetTrigger("Dead");
        _agent.enabled = false;
        _health.enabled = false;
        _health.IsInvincible = true;
        
        if (canRespawn)
            Invoke(nameof(OnRespawn), respawnDelay);
    }

    private void OnRespawn()
    {
        _health.enabled = true;
        animator.SetTrigger("Reset");
        sfxRespawn.Play();
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
