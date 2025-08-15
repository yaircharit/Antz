using UnityEngine;

public class Ant : MovingEntity
{
    public override string Name { get; protected set; } = "Ant"; // Name of the ant, can be used for identification
    public new static int Count { get; protected set; } = 0; // Static counter to keep track of the number of ants


    public PheromoneMap PheromoneMap { get; protected set; }
    public float currentPheromoneDepositValue = 0.5f; // Current pheromone deposit rate, reduced over time
    public AntColony Colony { get; private set; } = null; // Reference to the colony this ant belongs to


    private void Awake()
    {
        PheromoneMap = PheromoneMap.Instance;
        if (PheromoneMap == null)
        {
            Debug.LogError("No PheromoneMap found in scene!");
        }
    }

    public void Init(AntColony antColony)
    {
        base.Init();
        Colony = antColony;
        if (Colony == null)
        {
            Debug.LogError("No ColonyManagement found in scene!");
        }

        ID = Count++; // Increment the static ant count
        Name = $"{Name}_{ID}"; // Set the name based on the ID
    }

    void Start()
    {
        ChangeState(new SeekingFoodState(this));
    }

    void FixedUpdate()
    {
        timeTraveled += Time.fixedDeltaTime;
        if (transform.position.y < -10f)
        {
            // Reset ant position if it falls below a certain height
            transform.position = Colony.NestPos;
            ResetPheromoneDepositRate();
        }

        currentState?.Update();

        CheckVitals(); // Check the ant's vitals (health, energy, etc.) every frame
        currentPheromoneDepositValue = ACO.Instace.ReducePheromone(timeTraveled,actualSpeed);
    }

    public void Move(PheromoneType type)
    {
        Vector3 targetDirection;
        if (TargetPosition != Vector3.down && IsInView(TargetPosition))
        {
            targetDirection = GetDirectionTo(TargetPosition);
        }
        else
        {
            targetDirection = GetPheromoneDirections(type);
        }

        MoveInDirection(targetDirection, actualSpeed);
        ReduceEnergy(GetEnergyCost((IsCarrying) ? "Carry" : "Move") * Time.fixedDeltaTime); // Decrease energy with each movement
    }

    public bool IsInNest()
    {
        return Colony != null && Colony.IsInNest(transform.position);
    }

    public void AddPheromone(PheromoneType type)
    {
        AddPheromone(type, currentPheromoneDepositValue);
    }

    public void AddPheromone(PheromoneType type, float value)
    {
        PheromoneMap.AddPheromone(transform.position, value, type);
        //TODO: ReduceEnergy?
    }

    public Pheromone tempPhero = null;
    public Vector3 GetPheromoneDirections(PheromoneType type)
    {
        Vector3 res;

        var phero = PheromoneMap.GetMaxPheromone(type, transform.position);
        tempPhero = phero; // Store the position of the pheromone for debugging

        if (phero != null)
        {
            // Move towards pheromone marker
            res = GetDirectionTo(phero.Position);
            if (Random.value < ACO.Instace.ExplorationRate)
            {
                // Randomly explore around the pheromone
                res = GetRandomDirection(res);
            }
        }
        else
        {
            // No pheromone found, wander randomly
            res = GetRandomDirection();
        }

        res.y = 0; // Ensure movement is horizontal
        return res;
    }

    public void ResetPheromoneDepositRate()
    {
        timeTraveled = 0; // Reset distance traveled
        currentPheromoneDepositValue = PheromoneMap.Instance.ACOConfig.DepositValue;// * genome.traits.Size;
    }
    public void Eat(float amount = 1, float foodValue = 10)
    {
        if (IsCarrying)
        {
            AddEnergy(CarriedMass * foodValue); // Increase energy by the specified amount
            Drop();
            return;
        }

        if (Colony != null && IsInNest() && Colony.foodScore >= amount)
        {
            AddEnergy(amount * foodValue); // Increase energy by the specified amount

            Colony.RemoveFood(amount); // Remove food from the colony's food score
        }
    }




    void OnDrawGizmosSelected()
    {
        if (tempPhero != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(tempPhero.Position, 0.9f);
            Debug.Log($"Pheromone: {tempPhero} at {tempPhero.Position} with value {tempPhero.Value}");
        }
    }
}
