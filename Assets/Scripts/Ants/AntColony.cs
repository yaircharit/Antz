using Assets.Scripts.Interface;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Assets.Scripts;

public class AntColony : MonoBehaviour
{
    [SerializeField] private Ant AntPrefab; // Prefab for the ant
    [SerializeField] private QueenAnt QueenAntPrefab; // Prefab for the ant

    public float NestSize = 2f; // Size of the nest area, can be set in the inspector or dynamically

<<<<<<< HEAD
    public Vector3 NestPos { get; private set; }
=======
    public Vector3Int NestPos
    {
        get { return Vector3Int.RoundToInt(transform.position); }
        set { transform.position = value; }
    }
>>>>>>> f215773 (Stable 2D version)
    public float NestRadius { get; private set; } = 2f; // Radius around the nest where ants can drop food

    [SerializeField] private Transform _AntsContainer;
    public Transform AntsContainer { get { return _AntsContainer; } }

    public QueenAnt queen { get; private set; }
    public List<Ant> Ants { get; private set; } = new List<Ant>();
    public AntsListWindow antsListWindow; // Reference to the AntsListWindow UI component
    public TMP_Text foodScoreText;

    public float foodScore = 0; // Score for the amount of food collected

    private void Awake()
    {
        NestPos = Vector3.up * Chunk.floorHeight; //TODO: should be more dynamic, maybe based on terrain height or a specific point in the world
        NestRadius = NestSize / 2f;
        transform.position = NestPos; // Position the colony at the nest position
    }


    public Ant SpawnAnt(Genome genome)
    {
        Ant ant = Instantiate(AntPrefab, NestPos + Vector3.up, Quaternion.Euler(Random.insideUnitSphere), AntsContainer);
        ant.Init(this, genome);
        AddAnt(ant);

        return ant;
    }

    public QueenAnt SpawnQueen(Genome genome)
    {
        QueenAnt qAnt = Instantiate(QueenAntPrefab, NestPos + Vector3.up, Quaternion.Euler(Random.insideUnitSphere), AntsContainer);
        queen = qAnt;
        queen.Init(this, genome);
        AddAnt(queen);
        return queen;
    }


    public void AddAnt(Ant ant)
    {
        Ants.Add(ant);
        antsListWindow.AddAnt(ant);
    }

    public bool IsInNest(Vector3 position)
    {
        return Vector3.Distance(position, NestPos) <= NestRadius;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = UnityEngine.Color.blue;
        Gizmos.DrawWireSphere(NestPos, NestRadius);
    }

    internal void AddFood(float amount)
    {
        foodScore += amount;
        foodScoreText.text = $"Food: {foodScore}";
    }

    internal void RemoveFood(float amount)
    {
        foodScore -= amount;
        if (foodScore < 0) foodScore = 0; // Ensure food score doesn't go negative
        foodScoreText.text = $"Food: {foodScore}";
    }

    internal bool HasFood
    {
        get { return foodScore > 0; }
    }
}
