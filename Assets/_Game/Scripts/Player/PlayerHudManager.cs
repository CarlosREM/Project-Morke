using System;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerHudManager : MonoBehaviour
{
    [SerializeField] private CharacterHealth playerHealth;
    private Animator _hudAnimator;

    [SerializeField] private float speedUpPerMissingHealth = 1.5f;
    
    private void Awake()
    {
        _hudAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (!playerHealth)
            return;
        
        playerHealth.OnHurt += OnPlayerHurt;
        playerHealth.OnHeal += OnPlayerHeal;
        playerHealth.OnDeath += OnPlayerDeath;
        
        _hudAnimator.SetTrigger("Reset");
        _hudAnimator.SetFloat("HP Speed", 1);
    }

    private void OnDisable()
    {
        if (!playerHealth)
            return;
        
        playerHealth.OnHurt -= OnPlayerHurt;
        playerHealth.OnHeal -= OnPlayerHeal;
        playerHealth.OnDeath -= OnPlayerDeath;
    }


    public void Initialize(CharacterHealth pPlayerHealth)
    {
        this.playerHealth = pPlayerHealth;
        this.OnEnable();
    }
    
    private void OnPlayerHurt(int hurtAmount)
    {
        float animSpeed = _hudAnimator.GetFloat("HP Speed");
        
        animSpeed += speedUpPerMissingHealth * hurtAmount;
        
        _hudAnimator.SetFloat("HP Speed", animSpeed);
    }
    
    private void OnPlayerHeal(int healAmount)
    {
        float animSpeed = _hudAnimator.GetFloat("HP Speed");
        
        animSpeed -= speedUpPerMissingHealth * healAmount;
        
        _hudAnimator.SetFloat("HP Speed", animSpeed);
    }

    private void OnPlayerDeath()
    {
        _hudAnimator.SetFloat("HP Speed", 1);
        _hudAnimator.SetTrigger("Dead");
    }


}
