using System;
using UnityEngine;

public class EnemyWeakspot : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterHealth health;
    
    [Header("Parameters")] 
    [SerializeField] private float flashlightDmgDelay;
    private float _currentLightTime;
    private bool _isInsideLight;

    private void Update()
    {
        if (_isInsideLight && !health.IsInvincible)
        {
            _currentLightTime += Time.deltaTime;
            if (_currentLightTime >= flashlightDmgDelay)
            {
                health.Hurt(1);
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
