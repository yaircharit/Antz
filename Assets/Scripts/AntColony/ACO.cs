using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ACO
{
    public static ACO Instace;
    public float DepositValue { get; private set; }
    public float DepositRate { get; private set; } // Time in seconds to deposit pheromone
    public float MinDepositValue { get; private set; }  // Minimum pheromone deposit value
    public float DecayFactor { get; private set; } // How much pheromone decays per second
    public float DistanceDecayMultiplier { get; private set; } // How much pheromone decays per second
    public float DetectionDistance { get; private set; }
    public float DetectionThreshold { get; private set; }
    public float ExplorationRate { get; private set; }
    public float ExplorationAngle { get; private set; }

    public ACO(float depositValue, float depositRate, float minDepositValue, float decayFactor, float distanceDecayMultiplier, float detectionDistance, float detectionThreshold, float explorationRate, float explorationAngle)
    {
        DepositValue = depositValue;
        DepositRate = depositRate;
        MinDepositValue = minDepositValue;
        DecayFactor = decayFactor;
        DistanceDecayMultiplier = distanceDecayMultiplier;
        DetectionDistance = detectionDistance;
        DetectionThreshold = detectionThreshold;
        ExplorationRate = explorationRate;
        ExplorationAngle = explorationAngle;

        Instace = this;
    }

    public float ReducePheromone(float timeTraveled, float speed)
    {
        var currentPheromoneDepositValue = DepositValue - DecayFactor * timeTraveled * speed * DistanceDecayMultiplier; // Decrease pheromone deposit rate over time
        if (currentPheromoneDepositValue < MinDepositValue)
        {
            currentPheromoneDepositValue = MinDepositValue; // Ensure it doesn't go below the minimum
        }
        return currentPheromoneDepositValue;
    }
}

