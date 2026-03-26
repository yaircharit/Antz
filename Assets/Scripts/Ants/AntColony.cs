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

    public Vector3 NestPos { get; private set; } = Vector3.zero;
    public float NestRadius { get; private set; } = 2f; // Radius around the nest where ants can drop food

    [SerializeField] private Transform AntsContainer;

    public QueenAnt queen { get; private set; }
    public List<Ant> Ants { get; private set; } = new List<Ant>();
    public AntsListWindow antsListWindow; // Reference to the AntsListWindow UI component
    public TMP_Text foodScoreText;

    public float foodScore = 0; // Score for the amount of food collected

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

    public bool IsInNest(Vector3 position, float size =1)
    {
        return Vector3.Distance(position, NestPos) <= NestRadius - size/2;
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
