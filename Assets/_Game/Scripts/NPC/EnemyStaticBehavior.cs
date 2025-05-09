using UnityEngine;

[RequireComponent(typeof(CharacterHealth))]
public class EnemyStaticBehavior : MonoBehaviour
{
    private CharacterHealth _health;
    
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem onHurtParticles;
    
    [Header("Parameters")] 
    [SerializeField] private float activateDelay = 1f;
    [SerializeField] private float flashlightDmgDelay;
    private float _currentLightTime;
    private bool _isInsideLight;
    
    private void Awake()
    {
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
        
        // TODO: maybe try pooling the enemies?
        Destroy(this.gameObject);
    }

    private void OnSpawn()
    {
        _health.IsInvincible = false;
    }

    private void OnHurt(int damage)
    {
        onHurtParticles.Play();
        animator.SetTrigger("Blink");
    }

    private void OnDeath()
    {
        onHurtParticles.Play();
        animator.SetTrigger("Dead");
        _health.enabled = false;
        _health.IsInvincible = true;
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
