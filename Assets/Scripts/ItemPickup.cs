using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData itemData;

    [Header("Visual Feedback")]
    public bool showPickupPrompt = true;
    public string pickupText = "Press E to pickup";

    private bool isPlayerNearby = false;
    private InventorySystem playerInventory;

    private void Start()
    {
        // Auto-assign ItemData if missing
        if (itemData == null)
        {
            itemData = GetComponent<ItemData>();
        }

        // Find or ensure we have a trigger collider on THIS object
        SetupTriggerCollider();
    }

    private void SetupTriggerCollider()
    {
        Collider[] colliders = GetComponents<Collider>();
        bool hasTrigger = false;

        // Check if we already have a trigger collider
        foreach (Collider col in colliders)
        {
            if (col.isTrigger)
            {
                hasTrigger = true;
                Debug.Log($"Found trigger collider: {col.GetType().Name}");
                break;
            }
        }

        // If no trigger collider exists, create one
        if (!hasTrigger)
        {
            SphereCollider triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.radius = 1.5f; // Adjust size as needed
            Debug.Log("Created new trigger collider for pickup detection");
        }
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (playerInventory != null && itemData != null)
            {
                Item item = new Item(itemData);
                if (playerInventory.AddItem(item, 1))
                {
                    Debug.Log($"Picked up {itemData.itemName}");
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("Inventory full!");
                }
            }
        }
    }

    // This gets called when something enters THIS object's trigger collider
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Something entered pickup area: {other.name} with tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            playerInventory = other.GetComponent<InventorySystem>();

            if (playerInventory == null)
            {
                Debug.LogError("Player does not have InventorySystem component!");
            }

            if (showPickupPrompt && itemData != null)
            {
                Debug.Log($"Near {itemData.itemName}: {pickupText}");
            }
        }
    }

    // This gets called when something exits THIS object's trigger collider
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            playerInventory = null;
            Debug.Log("Player left pickup area");
        }
    }
}