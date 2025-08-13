using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AntColony : MonoBehaviour
{
    public Ant AntPrefab; // Prefab for the ant

    public Vector3 NestPos { get; private set; } = Vector3.zero;
    public float NestRadius { get; private set; } = 2f; // Radius around the nest where ants can drop food

    private Transform AntsObject;
    public int InitialAntCount { get; private set; } = 3; // Number of ants to spawn at the start
    public List<Ant> Ants { get; private set; } = new List<Ant>();

    public float foodScore = 0; // Score for the amount of food collected

    private void Awake()
    {
        AntsObject = transform.Find("Ants");
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
        Ant ant = Instantiate(AntPrefab, NestPos +Vector3.up + Random.insideUnitSphere * NestRadius, Quaternion.Euler(Random.insideUnitSphere), AntsObject);
        ant.Init(this);
        Ants.Add(ant);
    }

    public bool IsInNest(Vector3 position)
    {
        return Vector3.Distance(position, NestPos) <= NestRadius;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(NestPos, NestRadius);
    }

    internal void AddFood(float carriedMass)
    {
        foodScore += carriedMass;
        Player.Instance.foodScoreText.text = $"Food: {foodScore}";
    }
}
