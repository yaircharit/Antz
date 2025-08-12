using UnityEngine;
using System.Collections.Generic;
using System;

public enum PheromoneType { Food, Home }

public class Pheromone
{
    public PheromoneType Type;
    public float Value;
    public Vector3Int Position;

    private Color color;
    public Color Color => new(color.r, color.g, color.b, Value);

    public Pheromone(PheromoneType type, float value, Vector3Int position)
    {
        Type = type;
        Value = value;
        Position = position;

        color = type switch
        {
            PheromoneType.Food => new Color(1f, 0f, 0f, 0f),// Red for food
            PheromoneType.Home => new Color(0f, 0f, 1f, 0f),// Blue for home
            _ => Color.white,// Default color
        };
    }

    public float Decay(float decayFactor)
    {
        return Value -= decayFactor;
    }

    public override string ToString()
    {
        return $"Pheromone Type: {Type}, Value: {Value}, Position: {Position}";
    }
}

public class PheromoneMap : MonoBehaviour
{
    public Vector3 center = Vector3.zero;

    public float cellSize = 1f;
    public float decayValue = 0.001f;

    private Dictionary<PheromoneType, Dictionary<Vector3Int, Pheromone>> pheromoneGrids;

    void Awake()
    {
        pheromoneGrids = new Dictionary<PheromoneType, Dictionary<Vector3Int, Pheromone>>();
        foreach (PheromoneType type in System.Enum.GetValues(typeof(PheromoneType)))
        {
            pheromoneGrids[type] = new Dictionary<Vector3Int, Pheromone>();
        }
    }

    public Vector3Int Floor(Vector3 worldPos)
    {
        return Vector3Int.FloorToInt(worldPos);
    }

    public void AddPheromone(Vector3 worldPos, float amount, PheromoneType type)
    {
        if (pheromoneGrids[type].TryGetValue(Floor(worldPos), out Pheromone pheromone))
            pheromone.Value = amount;
        else
            pheromoneGrids[type][Floor(worldPos)] = new Pheromone(type, amount, Floor(worldPos));
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
                if (phero.Decay(decayValue) <= 0)
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

    void Update()
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
                Gizmos.DrawCube(phero.Position + new Vector3(0, 0.1f, 0), new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));
            }
        }
    }
}
