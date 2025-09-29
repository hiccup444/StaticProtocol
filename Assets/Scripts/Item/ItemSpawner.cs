using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public ItemData itemToSpawn;

    void Start()
    {
        if (itemToSpawn != null && itemToSpawn.prefab != null)
        {
            GameObject spawnedItem = Instantiate(itemToSpawn.prefab, transform.position, Quaternion.identity);
            
            // Add ItemPickup component if it doesn't exist
            ItemPickup pickup = spawnedItem.GetComponent<ItemPickup>();
            if (pickup == null)
            {
                pickup = spawnedItem.AddComponent<ItemPickup>();
            }
            pickup.itemData = itemToSpawn;
            
            // Setup ambient sound for spawned item
            if (itemToSpawn.ambientSoundWhenNotHeld != null)
            {
                AudioSource ambientSource = ItemAudioManager.Instance.SetupAmbientSource(
                    spawnedItem, 
                    itemToSpawn.ambientSoundWhenNotHeld
                );
                ItemAudioManager.Instance.PlayAmbient(ambientSource);
            }
        }
        else
        {
            Debug.LogWarning("Missing ItemData or Prefab!");
        }
    }
}