using UnityEngine;
using System.Collections.Generic;


public class PheromoneMap : MonoBehaviour
{
    public static PheromoneMap Instance { get; private set; }

    public Vector3 Center { get; private set; } = Vector3.zero; 
    public float DecayValue { get; private set; } = 0.00015f;

    public float cellSize = 1f;

    private Dictionary<PheromoneType, Dictionary<Vector3Int, Pheromone>> pheromoneGrids;

    void Awake()
    {
        Instance ??= this;

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
            pheromone.Value = amount; // Update existing pheromone value
        else
            pheromoneGrids[type][pos] = new Pheromone(type, amount, pos);
    }

    public Pheromone GetPheromone(Vector3 worldPos, PheromoneType type)
    {
        return pheromoneGrids[type].TryGetValue(Floor(worldPos), out Pheromone pheromone) ? pheromone : null;
    }

    public float GetPheromoneValue(Vector3 worldPos, PheromoneType type)
    {
        Pheromone phero = GetPheromone(worldPos, type);
        return phero != null ? phero.Value : 0f;
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
                if (phero.Decay(DecayValue) <= 0)
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
        foreach (var phero in pheromoneGrids[type].Values)
        {
            float distance = Vector3.Distance(phero.Position, position);
            // TODO: optimize this
            if (distance <= range)
            {
                res.Add(phero);
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
