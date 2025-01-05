using System;
using Unity.Behavior;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    private BehaviorGraphAgent _agent;
    private CharacterHealth _health;

    [Header("Components")] 
    [SerializeField] private ParticleSystem onHurtParticles;
    
    [Header("Parameters")]
    [SerializeField] private float flashlightDmgDelay;
    private float _currentLightTime;
    private bool _isInsideLight;

    private void Awake()
    {
        _agent = GetComponent<BehaviorGraphAgent>();
        _health = GetComponent<CharacterHealth>();
    }

    private void OnEnable()
    {
        if (!_health)
            return;
        
        _health.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        if (!_health)
            return;
        
        _health.OnDeath -= OnDeath;
    }
    
    private void OnDeath()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_isInsideLight)
        {
            _currentLightTime += Time.deltaTime;
            if (_currentLightTime >= flashlightDmgDelay)
            {
                _health.Hurt(1);
                _currentLightTime = 0;
                onHurtParticles.gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody.GetComponent<PlayerFlashlight>())
        {
            _isInsideLight = true;
            _currentLightTime = 0;
            Debug.Log("OH NO NOT THE LIGHT");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.attachedRigidbody.GetComponent<PlayerFlashlight>())
        {
            _isInsideLight = false;
            _currentLightTime = 0;
            Debug.Log("Phew, im out of that now");
        }
    }
}
