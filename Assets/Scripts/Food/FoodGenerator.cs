using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodGenerator : MonoBehaviour
{
    private static FoodGenerator Instance { get; set; }

    [Header("Apple Block Settings")]
    [SerializeField] private FoodBlock appleBlockPrefab; // Assign a cube or apple-like prefab in the inspector
    [SerializeField] private int appleSize = 7; // Size of the apple grid (odd number recommended)
    [SerializeField] private float blockSpacing = 1.0f;
    [SerializeField] private float spawnHeight = 7f;

    [Header("Random Spawn Area")]
    [SerializeField] private Vector2 spawnAreaCenter = Vector2.zero; // Center point for spawn area
    [SerializeField] private float spawnRadius = 40f; // Distance from center to edge of spawn area

    private GameObject foodParent; // Parent for all spawned food blocks

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Create a parent GameObject for all food blocks
        foodParent = new GameObject("Apple");
        foodParent.transform.SetParent(transform);
        FoodBlock.count = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightControl) || FoodBlock.count == 0)
        {
            SpawnAppleShape();
        }

        // Spawn aaple where player right clicked
        if (Input.GetMouseButtonDown(1))
        {
            // TODO: get mouse position on mesh
            SpawnAppleShape();
        }
    }

    public static void SpawnAppleShape()
    {
        // Get random position within square defined by center and radius
        float randX = Random.Range(Instance.spawnAreaCenter.x - Instance.spawnRadius, Instance.spawnAreaCenter.x + Instance.spawnRadius);
        float randZ = Random.Range(Instance.spawnAreaCenter.y - Instance.spawnRadius, Instance.spawnAreaCenter.y + Instance.spawnRadius);
        Vector3 spawnPos = new(randX, Instance.spawnHeight, randZ);
        SpawnAppleShape(spawnPos);
    }
    public static void SpawnAppleShape(Vector3 spawnPos)
    {

        int r = Instance.appleSize / 2;
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
                        Vector3 pos = spawnPos + new Vector3(x * Instance.blockSpacing, y * Instance.blockSpacing, z * Instance.blockSpacing);
                        Instantiate(Instance.appleBlockPrefab, pos, Quaternion.identity, Instance.foodParent.transform);
                    }
                }
            }
        }
    }
}
