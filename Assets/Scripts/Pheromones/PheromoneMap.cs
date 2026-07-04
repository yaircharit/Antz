using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class PheromoneMap : MonoBehaviour
{
    public static PheromoneMap Instance { get; private set; }
    private Dictionary<PheromoneType, Dictionary<Vector3Int, Pheromone>> pheromoneGrids;

    [Header("Map Components")]
    [SerializeField] private PheromoneMapMeshRenderer meshRenderer;

    [Header("ACO Configuration")]
    [SerializeField] public float PheromoneDecayFactor = 0.0001f; // How much pheromone decays per second

    [Header("Pheromone Colors")]
    [SerializeField]
    public Color[] PheromoneColors = {
        Color.clear,
        Color.red,   // Food
        Color.blue,  // Home
    };

    void Awake()
    {
        Instance = Instance != null ? Instance : this;

        if (meshRenderer == null)
            meshRenderer = GetComponent<PheromoneMapMeshRenderer>();

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
        worldPos.y = Mathf.Floor(worldPos.y); // Round down to the nearest whole number for y-coordinate
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
                if (phero.Decay(PheromoneDecayFactor) <= 0)
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

    public Pheromone GetMinPheromone(PheromoneType type, Vector3 center, float distance)
    {
        return GetPheromone(type, center, distance, (phero1, phero2) => phero1.Value < phero2.Value);
    }
    public Pheromone GetMaxPheromone(PheromoneType type, Vector3 center, float distance)
    {
        return GetPheromone(type, center,distance, (phero, resPhero) => phero.Value > resPhero.Value);
    }

    public Pheromone GetPheromone(PheromoneType type, Vector3 center, float distance, System.Func<Pheromone, Pheromone, bool> comperator, float range = 0)
    {
        Pheromone resPhero = null;
        foreach (var phero in GetPheromones(center, (range > 0) ? range : distance, type))
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
        meshRenderer.BuildMesh(pheromoneGrids);
    }
}
