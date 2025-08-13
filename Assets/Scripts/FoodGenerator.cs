using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodGenerator : MonoBehaviour
{
    [Header("Apple Block Settings")]
    [SerializeField] private GameObject appleBlockPrefab; // Assign a cube or apple-like prefab in the inspector
    [SerializeField] private int appleSize = 7; // Size of the apple grid (odd number recommended)
    [SerializeField] private float blockSpacing = 1.0f;
    [SerializeField] private float spawnHeight = 7f;
    [SerializeField] private float mass = 5f; // Mass of each apple block

    [Header("Random Spawn Area")]
    [SerializeField] private Vector2 spawnAreaCenter = Vector2.zero; // Center point for spawn area
    [SerializeField] private float spawnRadius = 10f; // Distance from center to edge of spawn area

    private GameObject foodParent; // Parent for all spawned food blocks

    void Start()
    {
        // Create a parent GameObject for all food blocks
        foodParent = new GameObject("FoodParent");
        foodParent.transform.SetParent(transform);

        // Get random position within square defined by center and radius
        float randX = Random.Range(spawnAreaCenter.x - spawnRadius, spawnAreaCenter.x + spawnRadius);
        float randZ = Random.Range(spawnAreaCenter.y - spawnRadius, spawnAreaCenter.y + spawnRadius);
        Vector3 spawnPos = new(randX, spawnHeight, randZ);
        SpawnAppleShape(spawnPos);
    }

    void SpawnAppleShape(Vector3 center)
    {
        int r = appleSize / 2;
        // Apple shape: ellipsoid with a slight flattening and a "dimple" for the apple top
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                for (int z = -r; z <= r; z++)
                {
                    // Ellipsoid formula (apple-like):
                    float normX = x / (float)r;
                    float normY = y / (float)r;
                    float normZ = z / (float)r;
                    float ellipsoid = normX * normX + normY * normY * 1.2f + normZ * normZ;
                    // Add a dimple at the top (y > 0)
                    if (y > 0) ellipsoid += 0.15f * normY;
                    // Only spawn blocks inside the apple shape
                    if (ellipsoid <= 1.0f)
                    {
                        Vector3 pos = center + new Vector3(x * blockSpacing, y * blockSpacing, z * blockSpacing);
                        GameObject appleBlock = Instantiate(appleBlockPrefab, pos, Quaternion.identity);
                        appleBlock.transform.SetParent(foodParent.transform);
                        Rigidbody rb = appleBlock.GetComponent<Rigidbody>();
                        if (rb == null)
                        {
                            rb = appleBlock.AddComponent<Rigidbody>();
                        }
                        rb.useGravity = true;
                        rb.mass = mass;
                    }
                }
            }
        }
    }
}
