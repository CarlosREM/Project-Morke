using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class InteractableObject : MonoBehaviour
{
    [SerializeField] private bool disableOnStart;
    [SerializeField] private UnityEvent onEnterRange;
    [SerializeField] private UnityEvent onExitRange;
    [SerializeField] private UnityEvent onInteracted;
    [SerializeField] private float delayAfterInteracted = 0.5f;
    [SerializeField] private bool disableAfterInteracted;
    private float _delayTimer;

    [SerializeField] private Light2D lightMarker;
    private Collider2D _collider;

    public void Awake()
    {
        _collider = GetComponent<Collider2D>();

        if (disableOnStart)
        {
            enabled = false;
        }
    }

    private void OnEnable()
    {
        _collider.enabled = true;
        lightMarker.SetActive(true);
    }

    private void OnDisable()
    {
        onExitRange?.Invoke();
        _collider.enabled = false;
        lightMarker.SetActive(false);
    }

    private void Update()
    {
        _delayTimer -= (_delayTimer > 0) ? Time.deltaTime : 0;
        if (_delayTimer <= 0)
        {
            _collider.enabled = true;
        }
    }

    public virtual void Interact(GameObject source)
    {
        if (enabled)
        {
            onInteracted?.Invoke();

            if (disableAfterInteracted)
                gameObject.SetActive(false);
            
            else if (delayAfterInteracted > 0)
            {
                _delayTimer = delayAfterInteracted;
                _collider.enabled = false;
                onExitRange?.Invoke();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(this.tag))
            return;
        
        onEnterRange.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(this.tag))
            return;
        
        onExitRange.Invoke();
    }
}
