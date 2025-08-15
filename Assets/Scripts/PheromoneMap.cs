using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class PheromoneMap : MonoBehaviour
{
    public static PheromoneMap Instance { get; private set; }
    public Vector3 Center { get; private set; } = Vector3.zero;
    public float cellSize = 1f;

    private Dictionary<PheromoneType, Dictionary<Vector3Int, Pheromone>> pheromoneGrids;

    [Header("ACO Configuration")]
    
    [SerializeField] private float PheromoneDepositValue = 0.5f;
    [SerializeField] private float PheromoneDepositRate; // Time in seconds to deposit pheromone
    [SerializeField] private float MinPheromoneDepositValue = 0.2f;  // Minimum pheromone deposit value
    [SerializeField] private float PheromoneDecayFactor = 0.0001f; // How much pheromone decays per second
    [SerializeField] private float PheromoneDistanceMultiplier = 20; // How much pheromone decays per distance traveled
    [SerializeField] private float PheromoneDetectionDistance = 2;
    [SerializeField] private float PheromoneDetectionThreshold = 0.1f;
    [SerializeField] private float ExplorationRate = 0.3f;
    [SerializeField] private float ExplorationAngle = 20f;
    public ACO ACOConfig { get; private set; }


    void Awake()
    {
        Instance ??= this;

        ACOConfig ??= new(PheromoneDepositValue, PheromoneDepositRate, MinPheromoneDepositValue,PheromoneDecayFactor, PheromoneDistanceMultiplier ,PheromoneDetectionDistance, PheromoneDetectionThreshold, ExplorationRate, ExplorationAngle);

        if (pheromoneGrids == null)
        {
            pheromoneGrids = new Dictionary<PheromoneType, Dictionary<Vector3Int, Pheromone>>();
            foreach (PheromoneType type in System.Enum.GetValues(typeof(PheromoneType)))
            {
                pheromoneGrids[type] = new Dictionary<Vector3Int, Pheromone>();
            }
        }
    }

    public Vector3Int Floor(Vector3 worldPos)
    {
        return Vector3Int.FloorToInt(worldPos);
    }

    public void AddPheromone(Vector3 worldPos, float amount, PheromoneType type)
    {
        var pos = Floor(worldPos);
        if (pheromoneGrids[type].TryGetValue(pos, out Pheromone pheromone))
        {
            pheromone.Value = amount; // Update existing pheromone value
            if (pheromone.Value > 1f)
            {
                pheromone.Value = 1f; // Cap the value to prevent overflow
            }
        }
        else
            pheromoneGrids[type][pos] = new Pheromone(type, amount, pos);
    }

    public void RemovePhermone(Pheromone pheromone)
    {
        RemovePhermone(pheromone.Position, pheromone.Type);
    }
    public void RemovePhermone(Vector3 worldPos, PheromoneType type)
    {
        Vector3Int gridPos = Floor(worldPos);
        if (pheromoneGrids[type].ContainsKey(gridPos))
        {
            pheromoneGrids[type].Remove(gridPos);
        }
    }

    public void Evaporate()
    {
        List<Pheromone> toRemove = new();
        foreach (var grid in pheromoneGrids.Values)
        {
            foreach (var phero in grid.Values)
            {
                if (phero.Decay(ACOConfig.DecayFactor) <= 0.001f)
                {
                    toRemove.Add(phero);
                }
            }
        }

        foreach (var pheromone in toRemove)
        {
            RemovePhermone(pheromone);
        }
    }

    public Pheromone[] GetPheromones(Vector3 position, float range, PheromoneType type)
    {
        List<Pheromone> res = new();

        Vector3Int gridPos = Floor(position);

        for (int i = -(int)range; i < range; i++)
        {
            for (int j = -(int)range; j < range; j++) // TODO: optimize this, check only within range not in a square
            {
                Vector3Int checkPos = new(gridPos.x + i, gridPos.y, gridPos.z + j);
                if (pheromoneGrids[type].TryGetValue(checkPos, out Pheromone phero))
                {
                    float distance = Vector3.Distance(phero.Position, position);
                    if (distance <= range)
                    {
                        res.Add(phero);
                    }
                }
            }
        }
        return res.ToArray();
    }

    public Dictionary<PheromoneType, Pheromone[]> GetPheromones(Vector3 position, float range)
    {
        Dictionary<PheromoneType, Pheromone[]> res = new();
        foreach (var type in pheromoneGrids.Keys)
        {
            res.Add(type, GetPheromones(position, range, type));
        }
        return res;
    }

    public Pheromone GetMinPheromone(PheromoneType type, Vector3 center)
    {
        return GetPheromone(type, center, (phero1, phero2) => phero1.Value < phero2.Value);
    }
    public Pheromone GetMaxPheromone(PheromoneType type, Vector3 center)
    {
        return GetPheromone(type, center, (phero, resPhero) => phero.Value > resPhero.Value);
    }

    public Pheromone GetPheromone(PheromoneType type, Vector3 center, System.Func<Pheromone, Pheromone, bool> comperator, float range = 0)
    {
        Pheromone resPhero = null;
        foreach (var phero in GetPheromones(center, (range > 0)? range: PheromoneDetectionDistance, type))
        {
            if (phero.Value > PheromoneDetectionThreshold && (resPhero == null || comperator(phero, resPhero)))
            {
                resPhero = phero;
            }
        }
        return resPhero;
    }


    void FixedUpdate()
    {
        Evaporate();
    }

    void OnDrawGizmos()
    {
        if (pheromoneGrids == null) return;

        foreach (var grid in pheromoneGrids.Values)
        {
            foreach (var phero in grid.Values)
            {
                Gizmos.color = phero.Color;
                Gizmos.DrawCube(phero.Position + new Vector3(0, 0.01f, 0), new Vector3(cellSize * 0.9f, 0.01f, cellSize * 0.9f));
            }
        }
    }
}
