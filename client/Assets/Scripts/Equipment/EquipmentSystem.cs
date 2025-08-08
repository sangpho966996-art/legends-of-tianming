using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class EquipmentSystem : MonoBehaviour
    {
        [Header("Equipment Slots")]
        public Dictionary<EquipmentSlot, EquipmentItem> equippedItems = new Dictionary<EquipmentSlot, EquipmentItem>();
        
        private Character character;

        private void Start()
        {
            character = GetComponent<Character>();
            InitializeEquipmentSlots();
        }

        private void InitializeEquipmentSlots()
        {
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                equippedItems[slot] = null;
            }
        }

        public bool EquipItem(EquipmentItem item)
        {
            if (!CanEquipItem(item))
            {
                Debug.LogWarning($"Cannot equip {item.name}");
                return false;
            }

            UnequipItem(item.slot);
            equippedItems[item.slot] = item;
            
            ApplyItemStats(item, true);
            
            Debug.Log($"⚔️ Equipped: {item.name}");
            return true;
        }

        public bool UnequipItem(EquipmentSlot slot)
        {
            if (equippedItems[slot] != null)
            {
                EquipmentItem item = equippedItems[slot];
                equippedItems[slot] = null;
                
                ApplyItemStats(item, false);
                
                Debug.Log($"📦 Unequipped: {item.name}");
                return true;
            }
            return false;
        }

        private bool CanEquipItem(EquipmentItem item)
        {
            if (item.levelRequirement > character.level)
            {
                Debug.LogWarning($"Level {item.levelRequirement} required to equip {item.name}");
                return false;
            }

            if (!string.IsNullOrEmpty(item.classRequirement) && item.classRequirement != character.characterClass)
            {
                Debug.LogWarning($"{item.classRequirement} class required to equip {item.name}");
                return false;
            }

            return true;
        }

        private void ApplyItemStats(EquipmentItem item, bool apply)
        {
            int multiplier = apply ? 1 : -1;
            
            character.Stats.strength += item.strengthBonus * multiplier;
            character.Stats.agility += item.agilityBonus * multiplier;
            character.Stats.intelligence += item.intelligenceBonus * multiplier;
            character.Stats.vitality += item.vitalityBonus * multiplier;
            
            character.Stats.RecalculateStats();
        }

        public EquipmentItem GetEquippedItem(EquipmentSlot slot)
        {
            return equippedItems.ContainsKey(slot) ? equippedItems[slot] : null;
        }

        public List<EquipmentItem> GetAllEquippedItems()
        {
            List<EquipmentItem> items = new List<EquipmentItem>();
            foreach (var item in equippedItems.Values)
            {
                if (item != null)
                    items.Add(item);
            }
            return items;
        }

        public int GetTotalStatBonus(StatType statType)
        {
            int total = 0;
            foreach (var item in equippedItems.Values)
            {
                if (item != null)
                {
                    switch (statType)
                    {
                        case StatType.Strength:
                            total += item.strengthBonus;
                            break;
                        case StatType.Agility:
                            total += item.agilityBonus;
                            break;
                        case StatType.Intelligence:
                            total += item.intelligenceBonus;
                            break;
                        case StatType.Vitality:
                            total += item.vitalityBonus;
                            break;
                    }
                }
            }
            return total;
        }
    }

    [System.Serializable]
    public class EquipmentItem
    {
        public string id;
        public string name;
        public string description;
        public EquipmentSlot slot;
        public ItemRarity rarity;
        public int levelRequirement = 1;
        public string classRequirement;
        
        [Header("Stat Bonuses")]
        public int strengthBonus = 0;
        public int agilityBonus = 0;
        public int intelligenceBonus = 0;
        public int vitalityBonus = 0;
        
        [Header("Visual")]
        public string iconPath;
        public GameObject modelPrefab;
    }

    public enum EquipmentSlot
    {
        Weapon,
        Helmet,
        Chest,
        Legs,
        Boots,
        Gloves,
        Ring1,
        Ring2,
        Necklace
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic
    }

    public enum StatType
    {
        Strength,
        Agility,
        Intelligence,
        Vitality
    }
}
