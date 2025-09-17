using UnityEngine;
using UnityEngine.Events;
using System;

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 4;
    [SerializeField] private int selectedSlot = 0;
    
    [Header("Events")]
    public UnityEvent<int> OnSlotChanged;
    public UnityEvent<int, Item> OnItemAdded;
    public UnityEvent<int, Item> OnItemRemoved;
    public UnityEvent<int, Item> OnItemUsed;
    
    // Inventory data
    private Item[] inventory;
    private int[] itemCounts;
    private Oxygen playerOxygen;
    HelmetHandler helmetHandler;

    // Properties
    public int InventorySize => inventorySize;
    public int SelectedSlot => selectedSlot;
    public Item SelectedItem => inventory[selectedSlot];
    public Item[] Inventory => inventory;
    public int[] ItemCounts => itemCounts;
    
    private void Awake()
    {
        InitializeInventory();

        playerOxygen = GetComponent<Oxygen>();
        helmetHandler = GetComponentInChildren<HelmetHandler>();
    }

    
    private void InitializeInventory()
    {
        inventory = new Item[inventorySize];
        itemCounts = new int[inventorySize];
        
        // Initialize all slots as empty
        for (int i = 0; i < inventorySize; i++)
        {
            inventory[i] = new Item();
            itemCounts[i] = 0;
        }
    }
    
    public bool AddItem(Item item, int count = 1)
    {
        // Try to stack with existing items first
        for (int i = 0; i < inventorySize; i++)
        {
            if (!inventory[i].IsEmpty() && 
                inventory[i].id == item.id && 
                itemCounts[i] < inventory[i].maxStackSize)
            {
                int spaceAvailable = inventory[i].maxStackSize - itemCounts[i];
                int amountToAdd = Mathf.Min(count, spaceAvailable);
                
                itemCounts[i] += amountToAdd;
                count -= amountToAdd;
                
                OnItemAdded?.Invoke(i, inventory[i]);
                
                if (count <= 0)
                    return true;
            }
        }
        
        // Add to empty slots
        for (int i = 0; i < inventorySize; i++)
        {
            if (inventory[i].IsEmpty())
            {
                inventory[i] = item.Clone();
                itemCounts[i] = Mathf.Min(count, item.maxStackSize);
                count -= itemCounts[i];
                
                OnItemAdded?.Invoke(i, inventory[i]);
                
                if (count <= 0)
                    return true;
            }
        }
        
        return count <= 0; // Return true if all items were added
    }
    
    public bool RemoveItem(int slotIndex, int count = 1)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize)
            return false;
            
        if (inventory[slotIndex].IsEmpty())
            return false;
            
        int availableCount = itemCounts[slotIndex];
        int amountToRemove = Mathf.Min(count, availableCount);
        
        itemCounts[slotIndex] -= amountToRemove;
        
        if (itemCounts[slotIndex] <= 0)
        {
            inventory[slotIndex] = new Item();
            itemCounts[slotIndex] = 0;
        }
        
        OnItemRemoved?.Invoke(slotIndex, inventory[slotIndex]);
        return true;
    }

    public bool UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize)
            return false;

        Item item = inventory[slotIndex];


        if (item.IsEmpty())
            return false;

        if (!item.isConsumable)
        {
            Debug.Log($"{item.itemName} is not consumable!");
            return false;
        }

        switch (item.id)
        {
            case 0: // Debug Item
                Debug.Log("Debug Item was used.");
                break;

            case 1: // Oxygen Tank
                playerOxygen.currentOxygen = playerOxygen.currentOxygen + 25;
                break;

            case 2: //Glass Sealant
                if (playerOxygen != null)
                {
                    helmetHandler.RepairHelmet(1);
                    Debug.Log("Glass Sealant used helmet repaired!");
                }
                break;

            default:
                Debug.Log($"Used {item.itemName}, but no special behavior defined.");
                break;
        }

        // Trigger the event
        OnItemUsed?.Invoke(slotIndex, item);

        // Remove one from stack
        RemoveItem(slotIndex, 1);

        Debug.Log($"Successfully used {item.itemName}");
        return true;
    }

    // Example helper methods
    private void HealPlayer(int amount)
    {
        // Replace this with your player's health system
        Debug.Log($"Healing player for {amount} HP!");
    }

    private void EquipWeapon(Item weapon)
    {
        Debug.Log($"Equipping {weapon.itemName}!");
    }

    private void CraftWoodItem()
    {
        Debug.Log("Crafting with wood!");
    }

    public void SelectSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize)
            return;
            
        selectedSlot = slotIndex;
        OnSlotChanged?.Invoke(selectedSlot);
    }
    
    public void NextSlot()
    {
        selectedSlot = (selectedSlot + 1) % inventorySize;
        OnSlotChanged?.Invoke(selectedSlot);
    }
    
    public void PreviousSlot()
    {
        selectedSlot = (selectedSlot - 1 + inventorySize) % inventorySize;
        OnSlotChanged?.Invoke(selectedSlot);
    }
    
    public Item GetItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize)
            return new Item();
            
        return inventory[slotIndex];
    }
    
    public int GetItemCount(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize)
            return 0;
            
        return itemCounts[slotIndex];
    }
    
    public bool HasItem(int itemId)
    {
        for (int i = 0; i < inventorySize; i++)
        {
            if (!inventory[i].IsEmpty() && inventory[i].id == itemId)
                return true;
        }
        return false;
    }
    
    public int GetTotalItemCount(int itemId)
    {
        int totalCount = 0;
        for (int i = 0; i < inventorySize; i++)
        {
            if (!inventory[i].IsEmpty() && inventory[i].id == itemId)
            {
                totalCount += itemCounts[i];
            }
        }
        return totalCount;
    }
    
    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySize)
            return;
            
        inventory[slotIndex] = new Item();
        itemCounts[slotIndex] = 0;
        OnItemRemoved?.Invoke(slotIndex, inventory[slotIndex]);
    }
    
    public void ClearInventory()
    {
        for (int i = 0; i < inventorySize; i++)
        {
            inventory[i] = new Item();
            itemCounts[i] = 0;
        }
        OnSlotChanged?.Invoke(selectedSlot);
    }
    
    // Debug method to add test items
    [ContextMenu("Add Test Items")]
    public void AddTestItems()
    {
        // Create some test items
        Item testItem1 = new Item
        {
            id = 1,
            itemName = "Health Potion",
            description = "Restores health",
            itemType = ItemType.Consumable,
            maxStackSize = 5,
            isConsumable = true
        };
        
        Item testItem2 = new Item
        {
            id = 2,
            itemName = "Sword",
            description = "A sharp blade",
            itemType = ItemType.Weapon,
            maxStackSize = 1,
            isEquippable = true
        };
        
        Item testItem3 = new Item
        {
            id = 3,
            itemName = "Wood",
            description = "Basic building material",
            itemType = ItemType.Material,
            maxStackSize = 99
        };
        
        AddItem(testItem1, 3);
        AddItem(testItem2, 1);
        AddItem(testItem3, 10);
    }
}
