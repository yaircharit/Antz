using UnityEngine;
using System.Collections.Generic;

public class Pheromone
{
    public enum PheromoneType { Food, Home }
    public PheromoneType Type;
    public float Value;
    public Pheromone(PheromoneType type, float value)
    {
        Type = type;
        Value = value;
    }
}

public class PheromoneMap : MonoBehaviour
{
    public int width = 100;
    public int height = 100;
    public float cellSize = 1f;
    public float evaporationRate = 0.001f;
    public Vector3 center = Vector3.zero;
    public float evaporationInterval = 0.2f; // Time in seconds between pheromone evaporation updates


    private Dictionary<Pheromone.PheromoneType, float[,]> pheromoneGrids;
    private float evaporationTimer = 0f;

    void Awake()
    {
        pheromoneGrids = new Dictionary<Pheromone.PheromoneType, float[,]>();
        foreach (Pheromone.PheromoneType type in System.Enum.GetValues(typeof(Pheromone.PheromoneType)))
        {
            pheromoneGrids[type] = new float[width, height];
        }
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        // Offset so (0,0) is at the center of the grid
        Vector3 offset = worldPos - center;
        int x = Mathf.Clamp(Mathf.FloorToInt(offset.x / cellSize) + width / 2, 0, width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(offset.z / cellSize) + height / 2, 0, height - 1);
        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(int x, int y)
    {
        // Convert grid coordinates back to world position
        float wx = (x - width / 2 + 0.5f) * cellSize + center.x;
        float wz = (y - height / 2 + 0.5f) * cellSize + center.z;
        return new Vector3(wx, center.y, wz);
    }

    public void AddPheromone(Vector3 worldPos, float amount, Pheromone.PheromoneType type)
    {
        Vector2Int grid = WorldToGrid(worldPos);
        pheromoneGrids[type][grid.x, grid.y] += amount;
    }

    public float GetPheromone(Vector3 worldPos, Pheromone.PheromoneType type)
    {
        Vector2Int grid = WorldToGrid(worldPos);
        return pheromoneGrids[type][grid.x, grid.y];
    }

    public void Evaporate()
    {
        foreach (var grid in pheromoneGrids.Values)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y] *= (1f - evaporationRate);
                }
            }
        }
    }

    void Update()
    {
        evaporationTimer += Time.deltaTime;
        if (evaporationTimer >= 1f)
        {
            Evaporate();
            evaporationTimer = 0f;
        }
    }

    void OnDrawGizmos()
    {
        if (pheromoneGrids == null) return;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float foodLevel = pheromoneGrids[Pheromone.PheromoneType.Food][x, y];
                float homeLevel = pheromoneGrids[Pheromone.PheromoneType.Home][x, y];
                Vector3 worldPos = GridToWorld(x, y);
                if (foodLevel > 0)
                {
                    Color color = new Color(1f, 0f, 0f, Mathf.Clamp01(foodLevel)); // Red for food
                    Gizmos.color = color;
                    Gizmos.DrawCube(worldPos + new Vector3(0, 0.05f, 0), new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));
                }
                if (homeLevel > 0)
                {
                    Color color = new Color(0f, 0f, 1f, Mathf.Clamp01(homeLevel)); // Blue for home
                    Gizmos.color = color;
                    Gizmos.DrawCube(worldPos + new Vector3(0, 0.15f, 0), new Vector3(cellSize * 0.7f, 0.1f, cellSize * 0.7f));
                }
            }
        }
    }
}
