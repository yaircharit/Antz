using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodGenerator : MonoBehaviour
{
    public static FoodGenerator Instance { get; private set; }

    [Header("Apple Block Settings")]
    [SerializeField] private FoodBlock appleBlockPrefab; // Assign a cube or apple-like prefab in the inspector
    [SerializeField] private int appleSize = 7; // Size of the apple grid (odd number recommended)
    [SerializeField] private float blockSpacing = 1.0f;

    [Header("Random Spawn Area")]
    private Vector3 spawnCenter; // Center point for spawn area
    [SerializeField] private float spawnRadius = 50f; // Distance from center to edge of spawn area
    [SerializeField] private float spawnDeadzone = 10f; // Height offset for spawning food blocks

    private GameObject foodParent; // Parent for all spawned food blocks

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
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
        if (FoodBlock.count == 0 || Input.GetKeyDown(KeyCode.RightControl))
        {
            SpawnAppleShape();
        }
        if (Input.GetMouseButtonDown(0))
        {
            SpawnAppleAtMouseClick();
        }
    }

    public static void SetCenter(Vector3 center)
    {
        Instance.spawnCenter = center;
    }
    private void SpawnAppleAtMouseClick()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            Vector3 spawnPos = hit.point;
            spawnPos.y = Chunk.floorHeight;
            SpawnAppleShape(spawnPos);
        }
    }

    public static void SpawnAppleShape()
    {
        // Get random position around spawnCenter within annulus [spawnDeadzone, spawnRadius]
        // Use polar coordinates so spawnCenter is respected regardless of its value
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float dist = Random.Range(Instance.spawnDeadzone, Instance.spawnRadius);
        float randX = Instance.spawnCenter.x + Mathf.Cos(angle) * dist;
        float randZ = Instance.spawnCenter.z + Mathf.Sin(angle) * dist;
        Vector3 spawnPos = new Vector3(randX, Chunk.floorHeight, randZ); //TODO: Adjust spawn height based on terrain height if needed
        SpawnAppleShape(spawnPos);
    }

    public static void SpawnAppleShape(Vector3 spawnPos)
    {
        spawnPos.y += Instance.appleSize / 2f * Instance.blockSpacing; // Adjust spawn position to center the apple shape
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
