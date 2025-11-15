using Assets.Scripts.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AntColony : MonoBehaviour
{
    public Ant AntPrefab; // Prefab for the ant
    public Vector3 NestPosition; // Position of the nest, can be set in the inspector or dynamically
    public float NestSize = 2f; // Size of the nest area, can be set in the inspector or dynamically
    public int InitialAntCount = 6; // Number of ants to spawn at the start, can be set in the inspector or dynamically

    public Vector3 NestPos { get; private set; } = Vector3.zero;
    public float NestRadius { get; private set; } = 2f; // Radius around the nest where ants can drop food

    private Transform AntsObject;
    public List<Ant> Ants { get; private set; } = new List<Ant>();
    public AntsListWindow antsListWindow; // Reference to the AntsListWindow UI component

    public float foodScore = 0; // Score for the amount of food collected

    private void Awake()
    {
        AntsObject = transform.Find("Ants");
        NestPos = NestPosition;
        NestRadius = NestSize;
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < InitialAntCount; i++)
        {
            SpawnAnt();
        }
        AddFood(0);
    }

    private void SpawnAnt()
    {
        Ant ant = Instantiate(AntPrefab,NestPos + Vector3.up, Quaternion.Euler(Random.insideUnitSphere), AntsObject);
        ant.Init(this);
        Ants.Add(ant);

        antsListWindow.AddAnt(ant);
    }

    public bool IsInNest(Vector3 position, float size =1)
    {
        return Vector3.Distance(position, NestPos) <= NestRadius - size/2;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(NestPos, NestRadius);
    }

    internal void AddFood(float amount)
    {
        foodScore += amount;
        Player.Instance.foodScoreText.text = $"Food: {foodScore}";
    }

    internal void RemoveFood(float amount)
    {
        foodScore -= amount;
        if (foodScore < 0) foodScore = 0; // Ensure food score doesn't go negative
        Player.Instance.foodScoreText.text = $"Food: {foodScore}";
    }

    internal bool HasFood
    {
        get { return foodScore > 0; }
    }
}
