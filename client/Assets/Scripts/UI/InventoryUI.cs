using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Inventory Grid")]
        public GridLayoutGroup inventoryGrid;
        public GameObject inventorySlotPrefab;
        public int slotsPerRow = 10;
        public int totalRows = 5;
        
        [Header("Item Info")]
        public GameObject itemInfoPanel;
        public Text itemNameText;
        public Text itemDescriptionText;
        public Text itemValueText;
        public Text itemTypeText;
        
        private InventorySystem inventorySystem;
        private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
        private Character playerCharacter;

        private void Start()
        {
            InitializeInventoryUI();
            FindPlayerCharacter();
        }

        private void InitializeInventoryUI()
        {
            CreateInventorySlots();
            
            if (itemInfoPanel != null)
                itemInfoPanel.SetActive(false);
        }

        private void CreateInventorySlots()
        {
            if (inventoryGrid == null || inventorySlotPrefab == null) return;
            
            int totalSlots = slotsPerRow * totalRows;
            
            for (int i = 0; i < totalSlots; i++)
            {
                GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryGrid.transform);
                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                
                if (slotUI == null)
                    slotUI = slotObj.AddComponent<InventorySlotUI>();
                
                slotUI.Initialize(i, this);
                slotUIs.Add(slotUI);
            }
        }

        private void FindPlayerCharacter()
        {
            var playerManager = GameManager.Instance?.GetPlayerManager();
            if (playerManager != null && playerManager.LocalPlayer != null)
            {
                playerCharacter = playerManager.LocalPlayer.GetComponent<Character>();
                if (playerCharacter != null)
                {
                    inventorySystem = playerCharacter.GetComponent<InventorySystem>();
                    SubscribeToEvents();
                    UpdateAllSlots();
                }
            }
        }

        private void SubscribeToEvents()
        {
            if (inventorySystem != null)
            {
                inventorySystem.OnSlotChanged += UpdateSlot;
                inventorySystem.OnInventoryChanged += UpdateAllSlots;
            }
        }

        private void UpdateSlot(int slotIndex, InventorySlot slot)
        {
            if (slotIndex >= 0 && slotIndex < slotUIs.Count)
            {
                slotUIs[slotIndex].UpdateSlot(slot);
            }
        }

        private void UpdateAllSlots()
        {
            if (inventorySystem == null) return;
            
            for (int i = 0; i < slotUIs.Count; i++)
            {
                var slot = inventorySystem.GetSlot(i);
                slotUIs[i].UpdateSlot(slot);
            }
        }

        public void OnSlotClicked(int slotIndex)
        {
            if (inventorySystem == null) return;
            
            var slot = inventorySystem.GetSlot(slotIndex);
            if (slot != null && slot.item != null)
            {
                ShowItemInfo(slot.item);
            }
            else
            {
                HideItemInfo();
            }
        }

        public void OnSlotDoubleClicked(int slotIndex)
        {
            if (inventorySystem == null) return;
            
            inventorySystem.UseItem(slotIndex);
        }

        public void OnSlotDragStart(int slotIndex)
        {
            Debug.Log($"🎒 Started dragging from slot {slotIndex}");
        }

        public void OnSlotDragEnd(int fromSlot, int toSlot)
        {
            if (inventorySystem == null) return;
            
            if (fromSlot != toSlot)
            {
                inventorySystem.MoveItem(fromSlot, toSlot);
                Debug.Log($"🎒 Moved item from slot {fromSlot} to slot {toSlot}");
            }
        }

        private void ShowItemInfo(ItemData item)
        {
            if (itemInfoPanel == null) return;
            
            itemInfoPanel.SetActive(true);
            
            if (itemNameText != null)
                itemNameText.text = item.name;
                
            if (itemDescriptionText != null)
                itemDescriptionText.text = item.description;
                
            if (itemValueText != null)
                itemValueText.text = $"Value: {item.value} gold";
                
            if (itemTypeText != null)
                itemTypeText.text = $"Type: {item.itemType}";
        }

        private void HideItemInfo()
        {
            if (itemInfoPanel != null)
                itemInfoPanel.SetActive(false);
        }

        public void SetPlayerCharacter(Character character)
        {
            playerCharacter = character;
            inventorySystem = character?.GetComponent<InventorySystem>();
            
            if (inventorySystem != null)
            {
                SubscribeToEvents();
                UpdateAllSlots();
            }
        }
    }

    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [Header("Slot Components")]
        public Image itemIcon;
        public Text quantityText;
        public Image backgroundImage;
        
        private int slotIndex;
        private InventoryUI inventoryUI;
        private InventorySlot currentSlot;
        private bool isDragging = false;
        private int dragStartSlot = -1;

        public void Initialize(int index, InventoryUI ui)
        {
            slotIndex = index;
            inventoryUI = ui;
            
            if (itemIcon == null)
                itemIcon = GetComponentInChildren<Image>();
                
            if (quantityText == null)
                quantityText = GetComponentInChildren<Text>();
                
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
        }

        public void UpdateSlot(InventorySlot slot)
        {
            currentSlot = slot;
            
            if (slot == null || slot.item == null)
            {
                if (itemIcon != null)
                    itemIcon.enabled = false;
                    
                if (quantityText != null)
                    quantityText.text = "";
                    
                if (backgroundImage != null)
                    backgroundImage.color = Color.gray;
            }
            else
            {
                if (itemIcon != null)
                {
                    itemIcon.enabled = true;
                    itemIcon.color = GetRarityColor(slot.item.rarity);
                }
                
                if (quantityText != null)
                {
                    quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
                }
                
                if (backgroundImage != null)
                    backgroundImage.color = Color.white;
            }
        }

        private Color GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return Color.white;
                case ItemRarity.Uncommon: return Color.green;
                case ItemRarity.Rare: return Color.blue;
                case ItemRarity.Epic: return Color.magenta;
                case ItemRarity.Legendary: return Color.yellow;
                default: return Color.white;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.clickCount == 1)
            {
                inventoryUI.OnSlotClicked(slotIndex);
            }
            else if (eventData.clickCount == 2)
            {
                inventoryUI.OnSlotDoubleClicked(slotIndex);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (currentSlot == null || currentSlot.item == null) return;
            
            isDragging = true;
            dragStartSlot = slotIndex;
            inventoryUI.OnSlotDragStart(slotIndex);
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (isDragging && dragStartSlot != -1)
            {
                inventoryUI.OnSlotDragEnd(dragStartSlot, slotIndex);
                dragStartSlot = -1;
            }
        }
    }
}
