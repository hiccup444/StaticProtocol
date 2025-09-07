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
            isEquippable = this.isEquippable
        };
    }

    public void Use()
    {
        if (isConsumable)
        {
            switch (itemName)
            {
                case "Oxygen Canister":
                    
                    break;
                
            }
        }
    }
}