using System;
using UnityEngine;
using UnityEngine.Events;

public class CharacterHealth : MonoBehaviour
{
   [SerializeField] private int maxHealth;
   public int MaxHealth => maxHealth;
   public int CurrentHealth { get; private set; }
   
   [SerializeField] private float invincibleDelayAfterDamage;
   private float _invincibleDelay;

   [SerializeField] private UnityEvent<int> onHurtEvent;
   [SerializeField] private UnityEvent<int> onHealEvent;
   [SerializeField] private UnityEvent onDeathEvent;
   
   public Action<int> OnHurt;
   public Action<int> OnHeal;
   public Action OnDeath;

   public bool IsDead => CurrentHealth <= 0;
   public bool IsFull => CurrentHealth >= maxHealth;

   public bool IsInvincible { get; set; }
   private bool _invincibleAfterDamage;

   private void OnEnable()
   {
      CurrentHealth = maxHealth;
   }

   private void Update()
   {
      if (IsInvincible && !_invincibleAfterDamage)
         return;
    
      // reduce delay only if invincibility is due to receiving damage
      _invincibleDelay -= (_invincibleDelay > 0) ? Time.deltaTime : 0f;
      if (_invincibleDelay <= 0 && _invincibleAfterDamage)
      {
         _invincibleAfterDamage = false;
         IsInvincible = false;
      }
   }

   public void Hurt(int amount)
   {
      if (IsInvincible || IsDead || !enabled)
         return;
      
      CurrentHealth -= amount;
      
      if (CurrentHealth <= 0)
      {
         CurrentHealth = 0;
         OnDeath?.Invoke();
         onDeathEvent?.Invoke();
      }
      else
      {
         _invincibleDelay = invincibleDelayAfterDamage;
         _invincibleAfterDamage = true;
         IsInvincible = true;
         OnHurt?.Invoke(amount);
         onHurtEvent?.Invoke(amount);
      }
   }

   public void Heal(int amount)
   {
      if (IsFull || !enabled)
         return;
      
      // limits amount to never exceed max health threshold
      amount = Mathf.Min(amount, maxHealth - CurrentHealth);
      CurrentHealth += amount;
      OnHeal?.Invoke(amount);
      onHealEvent?.Invoke(amount);
   }

   public void FullHeal() => Heal(maxHealth);
}
