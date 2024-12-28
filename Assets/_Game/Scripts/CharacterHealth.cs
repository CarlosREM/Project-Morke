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
   
   private void OnEnable()
   {
      CurrentHealth = maxHealth;
   }

   public void Hurt(int amount)
   { 
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
      if (CurrentHealth >= maxHealth)
         return;
      
      // limits amount to never exceed max health threshold
      amount = Mathf.Min(amount, maxHealth - CurrentHealth);
      CurrentHealth += amount;
      OnHeal?.Invoke(amount);
   }
}
