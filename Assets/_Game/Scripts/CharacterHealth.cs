using UnityEngine;
using System;

public class CharacterHealth : MonoBehaviour
{
   [SerializeField] private int maxHealth;
   public int MaxHealth => maxHealth;
   
   public int CurrentHealth { get; private set; }
   
   public Action<int> OnHurt;
   public Action<int> OnHeal;
   public Action OnDeath;

   public bool IsDead => CurrentHealth <= 0;
   public bool IsFull => CurrentHealth >= maxHealth;

   public bool IsInvincible { get; set; }

   private void OnEnable()
   {
      CurrentHealth = maxHealth;
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
      }
      else
      {
         OnHurt?.Invoke(amount);
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
   }
}
