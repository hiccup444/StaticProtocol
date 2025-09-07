using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{

    public ItemData itemToSpawn;

    // Start is called before the first frame update
    void Start()
    {
        if (itemToSpawn != null && itemToSpawn.prefab != null)
        {
            Instantiate(itemToSpawn.prefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Missing ItemData or Prefab!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
