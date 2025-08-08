using UnityEngine;
using System;

namespace LegendsOfTianming.Core
{
    public class CharacterStats : MonoBehaviour
    {
        [Header("Base Stats")]
        public int strength = 10;
        public int agility = 10;
        public int intelligence = 10;
        public int vitality = 10;

        [Header("Combat Stats")]
        public int currentHealth = 100;
        public int maxHealth = 100;
        public int currentMana = 100;
        public int maxMana = 100;

        [Header("Currency")]
        public int gold = 1000;
        public int silver = 0;

        public event Action<int, int> OnHealthChanged;
        public event Action<int, int> OnManaChanged;
        public event Action OnStatsChanged;

        public int Strength => strength;
        public int Agility => agility;
        public int Intelligence => intelligence;
        public int Vitality => vitality;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int CurrentMana => currentMana;
        public int MaxMana => maxMana;
        public int Gold => gold;
        public int Silver => silver;

        private void Start()
        {
            RecalculateStats();
        }

        public void Initialize(CharacterStatsData baseStats)
        {
            strength = baseStats.strength;
            agility = baseStats.agility;
            intelligence = baseStats.intelligence;
            vitality = baseStats.vitality;
            gold = baseStats.gold;
            silver = baseStats.silver;
            
            RecalculateStats();
            
            currentHealth = maxHealth;
            currentMana = maxMana;
        }

        public void RecalculateStats()
        {
            int oldMaxHealth = maxHealth;
            int oldMaxMana = maxMana;
            
            maxHealth = 100 + (vitality * 10);
            maxMana = 100 + (intelligence * 8);
            
            if (oldMaxHealth != maxHealth)
            {
                float healthRatio = (float)currentHealth / oldMaxHealth;
                currentHealth = Mathf.RoundToInt(maxHealth * healthRatio);
            }
            
            if (oldMaxMana != maxMana)
            {
                float manaRatio = (float)currentMana / oldMaxMana;
                currentMana = Mathf.RoundToInt(maxMana * manaRatio);
            }
            
            OnStatsChanged?.Invoke();
        }

        public void TakeDamage(int damage)
        {
            int actualDamage = Mathf.Max(1, damage - GetDefense());
            currentHealth = Mathf.Max(0, currentHealth - actualDamage);
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (currentHealth <= 0)
            {
                HandleDeath();
            }
            
            Debug.Log($"💔 {gameObject.name} took {actualDamage} damage ({currentHealth}/{maxHealth})");
        }

        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            Debug.Log($"💚 {gameObject.name} healed for {amount} ({currentHealth}/{maxHealth})");
        }

        public bool ConsumeMana(int amount)
        {
            if (currentMana >= amount)
            {
                currentMana -= amount;
                OnManaChanged?.Invoke(currentMana, maxMana);
                return true;
            }
            return false;
        }

        public void RestoreMana(int amount)
        {
            currentMana = Mathf.Min(maxMana, currentMana + amount);
            OnManaChanged?.Invoke(currentMana, maxMana);
        }

        public void AddGold(int amount)
        {
            gold += amount;
            Debug.Log($"💰 Gained {amount} gold (Total: {gold})");
        }

        public bool SpendGold(int amount)
        {
            if (gold >= amount)
            {
                gold -= amount;
                Debug.Log($"💸 Spent {amount} gold (Remaining: {gold})");
                return true;
            }
            return false;
        }

        public void AddSilver(int amount)
        {
            silver += amount;
            Debug.Log($"🥈 Gained {amount} silver (Total: {silver})");
        }

        public void LevelUp()
        {
            strength += 2;
            agility += 2;
            intelligence += 2;
            vitality += 2;
            
            RecalculateStats();
            
            currentHealth = maxHealth;
            currentMana = maxMana;
            
            Debug.Log($"🎉 Level up! Stats increased");
        }

        public int GetAttackPower()
        {
            return strength * 2 + agility;
        }

        public int GetDefense()
        {
            return vitality + (strength / 2);
        }

        public float GetCriticalChance()
        {
            return Mathf.Min(0.5f, agility * 0.01f);
        }

        public float GetAttackSpeed()
        {
            return 1f + (agility * 0.02f);
        }

        public float GetMovementSpeed()
        {
            return 5f + (agility * 0.1f);
        }

        public int GetMagicPower()
        {
            return intelligence * 2;
        }

        private void HandleDeath()
        {
            Debug.Log($"💀 {gameObject.name} has died!");
            
            var character = GetComponent<Character>();
            if (character != null)
            {
            }
        }

        public bool IsAlive()
        {
            return currentHealth > 0;
        }

        public float GetHealthPercentage()
        {
            return maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        }

        public float GetManaPercentage()
        {
            return maxMana > 0 ? (float)currentMana / maxMana : 0f;
        }

        public void FullRestore()
        {
            currentHealth = maxHealth;
            currentMana = maxMana;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnManaChanged?.Invoke(currentMana, maxMana);
        }
    }
}
