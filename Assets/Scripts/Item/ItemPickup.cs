using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData itemData;

    [Header("Visual Feedback")]
    public bool showPickupPrompt = true;
    public string pickupText = "Press E to pickup";

    private InventorySystem playerInventory;
    private AudioSource ambientSource;

    private static ItemPickup activePickup;
    private static Camera playerCamera;

    public static float pickupRange = 5f;
    public static float pickupRadius = 1.0f;

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

        // Setup ambient sound for items in the world
        if (itemData != null && itemData.ambientSoundWhenNotHeld != null)
        {
            ambientSource = ItemAudioManager.Instance.SetupAmbientSource(
                gameObject, 
                itemData.ambientSoundWhenNotHeld
            );
            ItemAudioManager.Instance.PlayAmbient(ambientSource);
        }
    }

    private void Update()
    {
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
            
            // Play pickup sound at item's position
            ItemAudioManager.Instance.PlayPickupSound(item.pickupSound, transform.position);
            
            if (playerInventory.AddItem(item, 1))
            {
                Debug.Log($"Picked up {itemData.itemName}");
                
                // Stop ambient sound before destroying
                if (ambientSource != null)
                {
                    ItemAudioManager.Instance.StopAmbient(ambientSource);
                }
                
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Inventory full!");
            }
        }
    }

    private void LateUpdate()
    {
        if (playerCamera == null) return;

        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        Collider[] hits = Physics.OverlapSphere(origin + direction * pickupRange * 0.5f, pickupRadius);

        ItemPickup closestPickup = null;
        float closestDot = -1f;

        foreach (var hit in hits)
        {
            ItemPickup pickup = hit.GetComponent<ItemPickup>();
            if (pickup != null)
            {
                Vector3 toItem = (pickup.transform.position - origin).normalized;
                float dot = Vector3.Dot(direction, toItem);

                if (dot > closestDot)
                {
                    closestDot = dot;
                    closestPickup = pickup;
                }
            }
        }

        activePickup = closestPickup;
    }

    public ItemData GetItemData()
    {
        return itemData;
    }

    private void OnDestroy()
    {
        // Clean up ambient sound
        if (ambientSource != null)
        {
            ItemAudioManager.Instance.StopAmbient(ambientSource);
        }
    }
}