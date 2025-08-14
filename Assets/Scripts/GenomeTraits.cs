using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GenomeTraits
{
    // Genome properties that define the characteristics of the ant
    public float Size { get; private set; } // Size of the ant, affects speed and pheromone detection
    public float Strength { get; private set; }  // Determines how strong the ant is, affects speed when carrying food (1 is normal strength)
    public float Speed { get; private set; } 
    public float ViewDistance { get; private set; }
    public float ViewAngle { get; private set; } // Angle in degrees
    public float HungerThreshold { get; private set; } // Threshold for hunger, when the ant needs to find food

    // Pheromone properties (ACO configuration)
    public float PheromoneDetectionRadius { get; private set; }
    public float PheromoneDetectionThreshold { get; private set; } 
    public float PheromoneDepositValue { get; private set; }
    public float MinPheromoneDepositValue { get; private set; }  // Minimum pheromone deposit rate to prevent pheromone from disappearing too quickly
    public float PheromoneDecayFactor { get; private set; } // How much pheromone decays per second
    public float ExplorationRate { get; private set; }
    public float ExplorationAngle { get; private set; }

    // Maybe devide to different classes for pheromone types? 1 class to inherit/contain from all types?
    // Maybe will be useful for new mutations

    // Constructor to initialize the genome traits with default values or specified values
    public GenomeTraits(float size = 1, float strength = 1.4f, float speed = 8, float viewDistance = 10, float viewAngle = 120, float hungerThreshold = 0.4f,
                        float pheromoneDetectionRadius = 10f, float pheromoneDetectionThreshold = 0.001f, 
                        float pheromoneDepositValue = 0.5f, 
                        float minPheromoneDepositValue = 0.2f, float pheromoneDecayFactor = 0.0001f, 
                        float explorationRate = 0.4f, float explorationAngle = 10f)
    {
        Size = size;
        Strength = strength;
        Speed = speed;
        ViewDistance = viewDistance;
        ViewAngle = viewAngle;
        HungerThreshold = hungerThreshold;
        PheromoneDetectionRadius = pheromoneDetectionRadius;
        PheromoneDetectionThreshold = pheromoneDetectionThreshold;
        PheromoneDepositValue = pheromoneDepositValue;
        MinPheromoneDepositValue = minPheromoneDepositValue;
        PheromoneDecayFactor = pheromoneDecayFactor;
        ExplorationRate = explorationRate;
        ExplorationAngle = explorationAngle;
    }


    public GenomeTraits Clone()
    {
        return new GenomeTraits(Size, Strength, Speed, ViewDistance, ViewAngle, HungerThreshold,
                                PheromoneDetectionRadius, PheromoneDetectionThreshold, 
                                PheromoneDepositValue, 
                                MinPheromoneDepositValue, PheromoneDecayFactor, 
                                ExplorationRate, ExplorationAngle);
    }

    public void Mutate()
    {
        Random rand = new();
        Size += (float)(rand.NextDouble() - 0.5);
        Strength += (float)(rand.NextDouble() - 0.5);
        Speed += (float)(rand.NextDouble() - 0.5) ;
    }

}

