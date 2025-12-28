using System.Collections;
using UnityEngine;

public class Brood : MonoBehaviour
{
    public Genome Genome { get; private set; }
    public AntColony Colony { get; private set; }

    public float IncubationTime { get; private set; } = 30f; // seconds
    public float Age { get; private set; } = 0f;
    public int RequiredFeedCount { get; private set; } = 1;
    public int FedCount { get; private set; } = 0;

    public void Init(AntColony colony, Genome genome)
    {
        Colony = colony;
        Genome = genome;
        IncubationTime = genome["BroodHatchTime"].Value * 60; // corrected the line to properly retrieve the size
        //RequiredFeedCount = (int)(genome.Traits["Size"].Value * genome.Traits["Strength"].Value); // fixed the syntax error
        Age = 0f;
        FedCount = 0;
        transform.localScale *= genome["Size"].Value;
    }

    private void FixedUpdate()
    {
        Age += Time.fixedDeltaTime; // changed from Time.deltaTime to Time.fixedDeltaTime
        if (Age >= IncubationTime )
        {
            Hatch();
        }
    }

    public void Feed(int count = 1)
    {
        FedCount += count;
    }

    private void Hatch()
    {
        // Spawn the child ant via colony
        if (Colony != null && Genome != null)
        {
            Colony.SpawnAnt(Genome);
        }
        Destroy(gameObject);
    }
}
