using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace LegendsOfTianming.Core
{
    public class InventorySystem : MonoBehaviour
    {
        [Header("Inventory Settings")]
        public int maxSlots = 50;
        public int maxStackSize = 99;
        
        private Dictionary<int, InventorySlot> inventory = new Dictionary<int, InventorySlot>();
        private Character character;

        public event System.Action<int, InventorySlot> OnSlotChanged;
        public event System.Action OnInventoryChanged;

        private void Start()
        {
            character = GetComponent<Character>();
            InitializeInventory();
        }

        private void InitializeInventory()
        {
            for (int i = 0; i < maxSlots; i++)
            {
                inventory[i] = new InventorySlot();
            }
        }

        public bool AddItem(ItemData item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            if (item.stackable)
            {
                foreach (var slot in inventory.Values)
                {
                    if (slot.item != null && slot.item.id == item.id && slot.quantity < maxStackSize)
                    {
                        int spaceAvailable = maxStackSize - slot.quantity;
                        int amountToAdd = Mathf.Min(quantity, spaceAvailable);
                        
                        slot.quantity += amountToAdd;
                        quantity -= amountToAdd;
                        
                        OnSlotChanged?.Invoke(GetSlotIndex(slot), slot);
                        
                        if (quantity <= 0)
                        {
                            OnInventoryChanged?.Invoke();
                            return true;
                        }
                    }
                }
            }

            while (quantity > 0)
            {
                int emptySlot = FindEmptySlot();
                if (emptySlot == -1)
                {
                    Debug.LogWarning("💼 Inventory full! Cannot add more items.");
                    return false;
                }

                int amountToAdd = item.stackable ? Mathf.Min(quantity, maxStackSize) : 1;
                
                inventory[emptySlot].item = item;
                inventory[emptySlot].quantity = amountToAdd;
                quantity -= amountToAdd;
                
                OnSlotChanged?.Invoke(emptySlot, inventory[emptySlot]);
            }

            OnInventoryChanged?.Invoke();
            Debug.Log($"📦 Added {item.name} to inventory");
            return true;
        }

        public bool RemoveItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) return false;

            int remainingToRemove = quantity;
            List<int> slotsToUpdate = new List<int>();

            foreach (var kvp in inventory)
            {
                if (kvp.Value.item != null && kvp.Value.item.id == itemId)
                {
                    int amountToRemove = Mathf.Min(remainingToRemove, kvp.Value.quantity);
                    kvp.Value.quantity -= amountToRemove;
                    remainingToRemove -= amountToRemove;
                    
                    if (kvp.Value.quantity <= 0)
                    {
                        kvp.Value.item = null;
                        kvp.Value.quantity = 0;
                    }
                    
                    slotsToUpdate.Add(kvp.Key);
                    
                    if (remainingToRemove <= 0) break;
                }
            }

            foreach (int slotIndex in slotsToUpdate)
            {
                OnSlotChanged?.Invoke(slotIndex, inventory[slotIndex]);
            }

            if (remainingToRemove < quantity)
            {
                OnInventoryChanged?.Invoke();
                Debug.Log($"📦 Removed {quantity - remainingToRemove} {itemId} from inventory");
                return remainingToRemove == 0;
            }

            return false;
        }

        public bool HasItem(string itemId, int quantity = 1)
        {
            int totalCount = 0;
            foreach (var slot in inventory.Values)
            {
                if (slot.item != null && slot.item.id == itemId)
                {
                    totalCount += slot.quantity;
                    if (totalCount >= quantity) return true;
                }
            }
            return false;
        }

        public int GetItemCount(string itemId)
        {
            int totalCount = 0;
            foreach (var slot in inventory.Values)
            {
                if (slot.item != null && slot.item.id == itemId)
                {
                    totalCount += slot.quantity;
                }
            }
            return totalCount;
        }

        public bool UseItem(int slotIndex)
        {
            if (!inventory.ContainsKey(slotIndex) || inventory[slotIndex].item == null)
                return false;

            var slot = inventory[slotIndex];
            var item = slot.item;

            if (!item.usable) return false;

            switch (item.itemType)
            {
                case ItemType.Consumable:
                    UseConsumable(item);
                    break;
                case ItemType.Equipment:
                    EquipItem(item);
                    break;
                default:
                    Debug.LogWarning($"Cannot use item type: {item.itemType}");
                    return false;
            }

            if (item.itemType == ItemType.Consumable)
            {
                slot.quantity--;
                if (slot.quantity <= 0)
                {
                    slot.item = null;
                    slot.quantity = 0;
                }
                OnSlotChanged?.Invoke(slotIndex, slot);
                OnInventoryChanged?.Invoke();
            }

            return true;
        }

        private void UseConsumable(ItemData item)
        {
            switch (item.id)
            {
                case "health_potion_small":
                    character.Stats.Heal(50);
                    break;
                case "health_potion_medium":
                    character.Stats.Heal(100);
                    break;
                case "health_potion_large":
                    character.Stats.Heal(200);
                    break;
                case "mana_potion_small":
                    character.Stats.RestoreMana(50);
                    break;
                case "mana_potion_medium":
                    character.Stats.RestoreMana(100);
                    break;
                case "mana_potion_large":
                    character.Stats.RestoreMana(200);
                    break;
                default:
                    Debug.Log($"🧪 Used consumable: {item.name}");
                    break;
            }
        }

        private void EquipItem(ItemData item)
        {
            var equipmentSystem = character.GetComponent<EquipmentSystem>();
            if (equipmentSystem != null && item is EquipmentItem equipItem)
            {
                equipmentSystem.EquipItem(equipItem);
            }
        }

        public bool MoveItem(int fromSlot, int toSlot)
        {
            if (!inventory.ContainsKey(fromSlot) || !inventory.ContainsKey(toSlot))
                return false;

            var fromItem = inventory[fromSlot];
            var toItem = inventory[toSlot];

            inventory[fromSlot] = toItem;
            inventory[toSlot] = fromItem;

            OnSlotChanged?.Invoke(fromSlot, inventory[fromSlot]);
            OnSlotChanged?.Invoke(toSlot, inventory[toSlot]);
            OnInventoryChanged?.Invoke();

            return true;
        }

        private int FindEmptySlot()
        {
            for (int i = 0; i < maxSlots; i++)
            {
                if (inventory[i].item == null)
                    return i;
            }
            return -1;
        }

        private int GetSlotIndex(InventorySlot slot)
        {
            foreach (var kvp in inventory)
            {
                if (kvp.Value == slot)
                    return kvp.Key;
            }
            return -1;
        }

        public InventorySlot GetSlot(int index)
        {
            return inventory.ContainsKey(index) ? inventory[index] : null;
        }

        public List<InventorySlot> GetAllItems()
        {
            return inventory.Values.Where(slot => slot.item != null).ToList();
        }

        public int GetEmptySlotCount()
        {
            return inventory.Values.Count(slot => slot.item == null);
        }

        public bool IsInventoryFull()
        {
            return GetEmptySlotCount() == 0;
        }
    }

    [System.Serializable]
    public class InventorySlot
    {
        public ItemData item;
        public int quantity;

        public bool IsEmpty => item == null || quantity <= 0;
    }

    [System.Serializable]
    public class ItemData
    {
        public string id;
        public string name;
        public string description;
        public ItemType itemType;
        public ItemRarity rarity;
        public bool stackable = true;
        public bool usable = true;
        public int value = 0;
        public string iconPath;
    }

    public enum ItemType
    {
        Consumable,
        Equipment,
        Material,
        Quest,
        Misc
    }
}
