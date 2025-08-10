using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodGenerator : MonoBehaviour
{
    [Header("Apple Block Settings")]
    [SerializeField] private GameObject appleBlockPrefab; // Assign a cube or apple-like prefab in the inspector
    [SerializeField] private int appleSize = 7; // Size of the apple grid (odd number recommended)
    [SerializeField] private float blockSpacing = 1.0f;
    [SerializeField] private float spawnHeight = 10f;
    [SerializeField] private float missingBlockChance = 0.15f; // 0.0 = no missing, 1.0 = all missing
    [SerializeField] private LayerMask groundMask = ~0; // Default to everything
    [Header("Random Spawn Area")]
    [SerializeField] private Vector2 randomAreaMin = new Vector2(-10, -10);
    [SerializeField] private Vector2 randomAreaMax = new Vector2(10, 10);

    void Start()
    {
        // Pick a random XZ position in the defined area
        float randX = Random.Range(randomAreaMin.x, randomAreaMax.x);
        float randZ = Random.Range(randomAreaMin.y, randomAreaMax.y);
        Vector3 groundPos = new Vector3(randX, 0, randZ);
        Vector3 spawnPos = groundPos + Vector3.up * spawnHeight;
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
                        // Randomly skip some blocks for the "missing" effect
                        if (Random.value < missingBlockChance) continue;
                        Vector3 pos = center + new Vector3(x * blockSpacing, y * blockSpacing, z * blockSpacing);
                        GameObject appleBlock = Instantiate(appleBlockPrefab, pos, Quaternion.identity);
                        Rigidbody rb = appleBlock.GetComponent<Rigidbody>();
                        if (rb == null)
                        {
                            rb = appleBlock.AddComponent<Rigidbody>();
                        }
                        rb.useGravity = true;
                        rb.mass = 1f;
                    }
                }
            }
        }
    }
}
