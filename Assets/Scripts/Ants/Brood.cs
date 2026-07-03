using System.Collections;
using UnityEngine;

public class Brood : Ant
{
    public float IncubationTime { get; private set; } = 30f; // seconds
    public float Age { get; private set; } = 0f;
    public int RequiredFeedCount { get; private set; } = 1;
    public int FedCount { get; private set; } = 0;

    public override void Init(AntColony colony, Genome genome)
    {
        base.Init(colony, genome);

        baseColor = Color.Lerp(Color.white, Color.yellow, 0.3f); // Brood is a pale yellow color
        IncubationTime = genome["BroodHatchTime"].Value * 60; // corrected the line to properly retrieve the size
        //RequiredFeedCount = (int)(genome.Traits["Size"].Value * genome.Traits["Strength"].Value); // fixed the syntax error
        Age = 0f;
        FedCount = 0;
    }

    protected override void FixedUpdate()
    {
        Age += Time.fixedDeltaTime; // changed from Time.deltaTime to Time.fixedDeltaTime
        if (Age >= IncubationTime)
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
        if (Colony != null && genome != null)
        {
            var a = Colony.SpawnAnt(genome);
            Debug.Log($"{a} was born!");
        }
        Destroy(this);
    }
}
