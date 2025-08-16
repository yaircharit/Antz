public class ACO
{
    public static ACO Instace;
    public float DepositValue { get; private set; }
    public float DecayFactor { get; private set; } // How much pheromone decays per second
    public float DetectionDistance { get; private set; }
    public float ExplorationRate { get; private set; }
    public float ExplorationAngle { get; private set; }

    public ACO(float depositValue, float decayFactor, float detectionDistance, float explorationRate, float explorationAngle)
    {
        DepositValue = depositValue;
        DecayFactor = decayFactor;
        DetectionDistance = detectionDistance;
        ExplorationRate = explorationRate;
        ExplorationAngle = explorationAngle;

        Instace = this;
    }
}

