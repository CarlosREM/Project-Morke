using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterHealth))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyStaticBehavior : MonoBehaviour
{
    private CharacterHealth _health;
    private Rigidbody2D _rb;
    
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem onHurtParticles;
    [SerializeField] private Transform eyeTransform;
    [SerializeField] private Transform currentEyePosition;
    [SerializeField] private List<Transform> repositionPoints;
    
    [Header("Parameters")] 
    [SerializeField] private float activateDelay = 1f;
    
    [SerializeField] private Vector2 blinkDelayRange;
    private bool _canBlink;
    private float _currentBlinkDelay;
    
    private bool _isFlapping;
    [SerializeField, Min(0)] private int moveSpeedOnFlap;

    [Header("SFX")] 
    [SerializeField] private FMODUnity.StudioEventEmitter sfxOnFlap;
    [SerializeField] private FMODUnity.StudioEventEmitter sfxOnDamaged;
    [SerializeField] private FMODUnity.StudioEventEmitter sfxOnDead;
    
    private void Awake()
    {
        _health = GetComponent<CharacterHealth>();
        _rb = GetComponent<Rigidbody2D>();
        _health.IsInvincible = true;
    }

    private void OnEnable()
    {
        if (!_health)
            return;
        
        _health.FullHeal();
        
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
        //Destroy(this.gameObject);
        
        animator.SetTrigger("Reset");
        animator.ResetTrigger("Reveal");
    }

    private void OnSpawn()
    {
        _health.IsInvincible = false;
    }

    private void OnHurt(int damage)
    {
        onHurtParticles.Play();
        animator.SetTrigger("Blink");
        animator.SetFloat("Health", _health.CurrentHealth);

        // start
        _currentBlinkDelay = Random.Range(blinkDelayRange.x, blinkDelayRange.y);
        
        sfxOnDamaged?.Play();
    }

    private void OnDeath()
    {
        onHurtParticles.Play();
        animator.SetFloat("Health", _health.CurrentHealth);
        
        sfxOnDead?.Play();
    }

    public void OnBlink()
    {
        // change weakspot position everytime it blinks
        int posIdx = Random.Range(0, repositionPoints.Count);
        var newPos = repositionPoints[posIdx];
        
        // this is so we can remove it from the next random
        repositionPoints.Remove(newPos);
        repositionPoints.Add(currentEyePosition); // adds back previous position
        currentEyePosition = newPos;
        eyeTransform.position = currentEyePosition.position;
    }

    public void AfterBlink()
    {
        _currentBlinkDelay = Random.Range(blinkDelayRange.x, blinkDelayRange.y);
        _canBlink = true;
    }

    public void OnFlap()
    {
        var playerPos = GameLoopManager.Instance.PlayerRef.transform.position;
        
        var towardsPlayer = playerPos - transform.position;
        
        Vector2 moveDirection = Vector2.zero;
        moveDirection.x = (towardsPlayer.x < 0) ? -1 : 1;
        
        _rb.AddForce(moveDirection * moveSpeedOnFlap, ForceMode2D.Impulse);
        
        sfxOnFlap?.Play();
    }
    
    private void Update()
    {
        if (_health.IsDead)
            return;
        
        _currentBlinkDelay -= (_currentBlinkDelay > 0) ? Time.deltaTime : 0;
        if (_currentBlinkDelay <= 0 && _canBlink)
        {
            animator.SetTrigger("Blink");
            _canBlink = false;
            // on ending blink, it will reposition weakspot
        }
    }
}
