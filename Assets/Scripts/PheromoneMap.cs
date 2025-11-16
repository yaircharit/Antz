using System.Collections.Generic;
using UnityEngine;


public class PheromoneMap : MonoBehaviour
{
    public static PheromoneMap Instance { get; private set; }
    public Vector3 Center { get; private set; } = Vector3.zero;
    public float cellSize = 1f;

    private Dictionary<PheromoneType, Dictionary<Vector3Int, Pheromone>> pheromoneGrids;

    [Header("ACO Configuration")]
    [SerializeField] private float PheromoneDepositValue = 0.5f;
    [SerializeField] private float PheromoneDecayFactor = 0.0001f; // How much pheromone decays per second
    [SerializeField] private float PheromoneDetectionDistance = 2;
    [SerializeField] private float ExplorationRate = 0.3f;
    [SerializeField] private float ExplorationAngle = 20f;

    [Header("Pheromone Colors")]
    [SerializeField] public Color[] PheromoneColors = {
        Color.clear,
        Color.red,   // Food
        Color.blue,  // Home
    };

    public ACO ACOConfig { get; private set; }


    void Awake()
    {
        Instance = Instance != null ? Instance : this;

        ACOConfig ??= new(PheromoneDepositValue, PheromoneDecayFactor, PheromoneDetectionDistance, ExplorationRate, ExplorationAngle);

        if (pheromoneGrids == null)
        {
            pheromoneGrids = new Dictionary<PheromoneType, Dictionary<Vector3Int, Pheromone>>();
            foreach (PheromoneType type in System.Enum.GetValues(typeof(PheromoneType)))
            {
                pheromoneGrids[type] = new Dictionary<Vector3Int, Pheromone>();
            }
        }
    }

    public Vector3Int Round(Vector3 worldPos)
    {
        return Vector3Int.RoundToInt(worldPos);
    }

    public void AddPheromone(Vector3 worldPos, float amount, PheromoneType type)
    {
        var pos = Round(worldPos);
        if (pheromoneGrids[type].TryGetValue(pos, out Pheromone pheromone))
        {
            pheromone.Value = Mathf.Max(amount, pheromone.Value); // Update existing pheromone value
        }
        else
            pheromoneGrids[type][pos] = new Pheromone(type, amount, pos);
    }

    public void RemovePheromone(Pheromone pheromone)
    {
        RemovePheromone(pheromone.Position, pheromone.Type);
    }
    public void RemovePheromone(Vector3 worldPos, PheromoneType type)
    {
        Vector3Int gridPos = Round(worldPos);
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
                if (phero.Decay(ACOConfig.DecayFactor) <= 0)
                {
                    toRemove.Add(phero);
                }
            }
        }

        foreach (var pheromone in toRemove)
        {
            RemovePheromone(pheromone);
        }
    }

    public Pheromone[] GetPheromones(Vector3 position, float range, PheromoneType type)
    {
        List<Pheromone> res = new();

        Vector3Int gridPos = Round(position);

        for (int i = -(int)range; i <= range; i++)
        {
            for (int j = -(int)range; j <= range; j++)
            {
                for (int k = -1; k <= 1; k++)
                {

                    Vector3Int checkPos = new(gridPos.x + i, gridPos.y + k, gridPos.z + j);

                    if (pheromoneGrids[type].TryGetValue(checkPos, out Pheromone phero)
                        && Vector3.Distance(checkPos, position) <= range)
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
        foreach (var phero in GetPheromones(center, (range > 0) ? range : PheromoneDetectionDistance, type))
        {
            if ((resPhero == null || comperator(phero, resPhero)))
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
                Gizmos.DrawCube(phero.Position, new Vector3(cellSize , 0.01f, cellSize));
            }
        }
    }
}
