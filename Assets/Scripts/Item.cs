using UnityEngine;

[System.Serializable]
public class Item
{
    [Header("Item Properties")]
    public int id;
    public string itemName;
    public string description;
    public Sprite icon;
    public GameObject prefab;
    public ItemType itemType;
    public int maxStackSize = 1;

    [Header("Item Stats")]
    public float weight = 1f;
    public int value = 1;
    public bool isConsumable = false;
    public bool isEquippable = false;

    [Header("Consumable Effects")]
    public float oxygenRestore = 0f;

    public Item()
    {
        id = -1;
        itemName = "Empty";
        description = "";
        icon = null;
        prefab = null;
        itemType = ItemType.Misc;
        maxStackSize = 1;
        weight = 1f;
        value = 1;
        isConsumable = false;
        isEquippable = false;
        oxygenRestore = 0f;
    }

    public Item(ItemData data)
    {
        id = data.id;
        itemName = data.itemName;
        description = data.description;
        icon = data.icon;
        prefab = data.prefab;
        itemType = data.itemType;
        maxStackSize = data.maxStackSize;
        weight = data.weight;
        value = data.value;
        isConsumable = data.isConsumable;
        isEquippable = data.isEquippable;
        oxygenRestore = data.oxygenRestore;
    }

    public bool IsEmpty()
    {
        return id == -1 || itemName == "Empty";
    }

    public Item Clone()
    {
        return new Item
        {
            id = this.id,
            itemName = this.itemName,
            description = this.description,
            icon = this.icon,
            prefab = this.prefab,
            itemType = this.itemType,
            maxStackSize = this.maxStackSize,
            weight = this.weight,
            value = this.value,
            isConsumable = this.isConsumable,
            isEquippable = this.isEquippable,
            oxygenRestore = this.oxygenRestore
        };
    }

    public bool Use(GameObject player)
    {
        if (!isConsumable)
        {
            Debug.Log($"{itemName} is not consumable!");
            return false;
        }

        Debug.Log($"Using {itemName}");

        // Get player oxygen component
        Oxygen playerOxygen = player.GetComponent<Oxygen>();

        bool wasUsed = false;

        // Generic item effects based on item properties
        if (oxygenRestore > 0 && playerOxygen != null)
        {
            playerOxygen.RefillOxygen(oxygenRestore);
            Debug.Log($"Restored {oxygenRestore} oxygen");
            wasUsed = true;
        }

        // Specific item behaviors
        switch (itemName.ToLower())
        {
            case "oxygen canister":
                if (playerOxygen != null)
                {
                    playerOxygen.RefillOxygen(50f);
                    Debug.Log("Oxygen canister used - 50 oxygen restored!");
                    wasUsed = true;
                }
                else
                {
                    Debug.LogError("No Oxygen component found on player!");
                }
                break;

            case "small oxygen canister":
                if (playerOxygen != null)
                {
                    playerOxygen.RefillOxygen(25f);
                    Debug.Log("Small oxygen canister used - 25 oxygen restored!");
                    wasUsed = true;
                }
                break;

            case "large oxygen canister":
                if (playerOxygen != null)
                {
                    playerOxygen.RefillOxygen(100f);
                    Debug.Log("Large oxygen canister used - 100 oxygen restored!");
                    wasUsed = true;
                }
                break;

         

            default:
                Debug.Log($"No specific use behavior for {itemName}");
                // Still return true if it had generic effects from oxygenRestore
                break;
        }

        return wasUsed;
    }
}