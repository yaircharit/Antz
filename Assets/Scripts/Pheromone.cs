using UnityEngine;

public enum PheromoneType { None, Food, Home, Nest, Danger, Panic } //TODO: Implement Nest & Danger pheromones logic

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
