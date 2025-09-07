using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData itemData;

    [Header("Visual Feedback")]
    public bool showPickupPrompt = true;
    public string pickupText = "Press E to pickup";

    private InventorySystem playerInventory;

    private static ItemPickup activePickup;
    private static Camera playerCamera;

    // Settings
    public static float pickupRange = 5f;       // How far player can pick up
    public static float pickupRadius = 1.0f;    // Radius around crosshair

    private void Start()
    {
        if (itemData == null)
        {
            itemData = GetComponent<ItemData>();
        }

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerInventory == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerInventory = player.GetComponent<InventorySystem>();
        }
    }

    private void Update()
    {
        // Only show/pick if this is the current target
        if (this == activePickup)
        {
            if (showPickupPrompt && itemData != null)
                Debug.Log($"Looking near {itemData.itemName}: {pickupText}");

            if (Input.GetKeyDown(KeyCode.E))
                TryPickup();
        }
    }

    private void TryPickup()
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

    // ----------------- Detection -----------------
    private void LateUpdate()
    {
        if (playerCamera == null) return;

        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        // Collect all colliders in a sphere in front of camera
        Collider[] hits = Physics.OverlapSphere(origin + direction * pickupRange * 0.5f, pickupRadius);

        ItemPickup closestPickup = null;
        float closestDot = -1f; // dot product (higher = closer to crosshair)

        foreach (var hit in hits)
        {
            ItemPickup pickup = hit.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                Vector3 toItem = (pickup.transform.position - origin).normalized;
                float dot = Vector3.Dot(direction, toItem); // how aligned with crosshair

                if (dot > closestDot)
                {
                    closestDot = dot;
                    closestPickup = pickup;
                }
            }
        }

        activePickup = closestPickup;
    }
}
