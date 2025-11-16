using UnityEngine;

public enum PheromoneType { None, Food, Home, Nest, Danger, Panic } //TODO: Implement Nest & Danger pheromones logic

public class Pheromone
{
    public PheromoneType Type;
    public float Value;
    public Vector3Int Position;

    private Color color;
    public Color Color => new(color.r, color.g, color.b, Value/2);

    public Pheromone(PheromoneType type, float value, Vector3Int position)
    {
        Type = type;
        Value = value;
        Position = position;

        color = PheromoneMap.Instance.PheromoneColors[(int)Type];

        if (Position.y < 0)
        {
            Value = 0;
        }
    }

    public float Decay(float decayFactor)
    {
        return Value -= decayFactor * Time.fixedDeltaTime;
    }

    public override string ToString()
    {
        return $"Pheromone<{Type}>@{Position} = {Value}";
    }
}
