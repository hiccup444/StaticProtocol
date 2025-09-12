using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Properties")]
    public int id;
    public string itemName;
    [TextArea(3, 5)]
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
    [Tooltip("Amount of oxygen this item restores when used")]
    public float oxygenRestore = 0f;

    [Header("Economy")]
    public int sellValue = 0; // how much money it's worth
}