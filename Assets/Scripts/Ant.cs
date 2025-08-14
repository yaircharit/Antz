using UnityEngine;

public class Ant : MonoBehaviour
{
    public string Name { get; private set; } = "Ant"; // Name of the ant, can be used for identification
    public int ID { get; private set; } = -1; // Unique ID for the ant, can be used for identification
    public static int AntCount { get; private set; } = 0; // Static counter to keep track of the number of ants
    
    //[Header("Basic Settings")]
    public float CarryHeight { get; private set; } = 0.75f;
    public float CarrySpeedModifier { get; private set; } = 0.5f;
    public bool IsCarrying => CarriedObj != null;

    // 
    public float maxHealth = 100; // Maximum health of the ant
    public float currentHealth; // Current health of the ant, can be used for health management
    public float maxEnergy = 100f; // Maximum energy of the ant
    public float currentEnergy; // Current energy of the ant, can be used for energy management
    public float actualSpeed;
    public float currentPheromoneDepositValue = 0.5f; // Current pheromone deposit rate, reduced over time

    public Genome genome; // Genome of the ant, can be used for genetic algorithms or traits

    //[Header("Genome Settings")]
    public float Size => genome.traits.Size; // Size of the ant, affects speed and pheromone detection
    public float Strength => genome.traits.Strength; // Determines how strong the ant is, affects speed when carrying food (1 is normal strength)
    public float Speed => genome.traits.Speed;
    public float ViewDistance => genome.traits.ViewDistance;
    public float ViewAngle => genome.traits.ViewAngle;
    public float HungerThreshold => genome.traits.HungerThreshold; // Threshold for hunger, when the ant needs to find food
    public float PheromoneDetectionRadius => genome.traits.PheromoneDetectionRadius;
    public float PheromoneDetectionThreshold => genome.traits.PheromoneDetectionThreshold;


    ////[Header("ACO Settings")]
    public float PheromoneDepositValue => genome.traits.PheromoneDepositValue;
    public float MinPheromoneDepositValue => genome.traits.MinPheromoneDepositValue; // Minimum pheromone deposit rate to prevent pheromone from disappearing too quickly
    public float PheromoneDecayFactor => genome.traits.PheromoneDecayFactor; // How much pheromone decays per second
    public float ExplorationRate => genome.traits.ExplorationRate;
    public float ExplorationAngle => genome.traits.ExplorationAngle;

    public GameObject CarriedObj { get; private set; }
    public float CarriedMass { get; private set; } = 0f; // Mass of the carried object, used to calculate speed when carrying food

    private Transform _target;
    public Transform Target
    {
        get { return (_target) ? _target : null; } // Return null if _target is not set 
        set { _target = value; _targetPosition = Vector3.down; }// Set target and reset target position (Vector3.down indicates no target position, y >= 0)
    }

    private Vector3 _targetPosition;
    public Vector3 TargetPosition
    {
        get { return (Target) ? Target.position : _targetPosition; }
        set { Target = null; _targetPosition = value; }
    }
    public PheromoneMap PheromoneMap { get; private set; }


    public AntStateBase CurrentState;
    public Vector3 Position => transform.position;
    public Vector3 Forward => transform.forward; // Forward direction of the ant
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
        Colony = antColony;
        if (Colony == null)
        {
            Debug.LogError("No ColonyManagement found in scene!");
        }

        ID = AntCount++; // Increment the static ant count
        Name = $"Ant_{ID}"; // Set the name based on the ID

        genome = new Genome(new GenomeTraits(pheromoneDecayFactor: PheromoneMap.DecayValue * 1.2f)); // Initialize genome with default values
        genome.Mutate(); // Mutate the genome to get random traits

        maxHealth = 100f * Size; // Set maximum health based on size trait
        maxEnergy = 100f * Size; // Set maximum energy based on size trait
        transform.localScale = Vector3.one * Size; // Set the scale of the ant based on size trait

        currentHealth = maxHealth; // Initialize current health
        currentEnergy = maxEnergy;
        actualSpeed = Speed / Size; // Initialize actual speed
    }

    void Start()
    {
        ChangeState(new SeekingFoodState(this));
    }

    void FixedUpdate()
    {
        CurrentState?.Update();

        if (transform.position.y < -10f)
        {
            // Reset ant position if it falls below a certain height
            transform.position = new Vector3(Random.Range(-10f, 10f), 1f, Random.Range(-10f, 10f));
        }

        currentPheromoneDepositValue -= PheromoneDecayFactor; // Decrease pheromone deposit rate over time
        if (currentPheromoneDepositValue < MinPheromoneDepositValue)
        {
            currentPheromoneDepositValue = MinPheromoneDepositValue; // Ensure it doesn't go below the minimum
        }

        CheckVitals(); // Check the ant's vitals (health, energy, etc.) every frame
    }

    public void ChangeState(AntStateBase newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState?.Enter();
    }

    #region Movement & Direction Methods
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

        MoveInDirection(targetDirection);
        ReduceEnergy(GetEnergyCost((IsCarrying) ? "Carry" : "Move") * Time.fixedDeltaTime); // Decrease energy with each movement
    }

    public void MoveInDirection(Vector3 targetDirection, float speed)
    {
        if (targetDirection == Vector3.zero) targetDirection = Vector3.forward; // Avoid division by zero or invalid movement
        transform.forward = targetDirection;
        transform.position += speed * Time.fixedDeltaTime * targetDirection;
    }

    public Vector3 GetDirectionTo(Vector3 targetPosition)
    {
        return (targetPosition - transform.position).normalized;
    }
    public Vector3 GetRandomDirection(Vector3 direction, float angle)
    {
        float randomAngle = Random.Range(-angle, angle);
        return Quaternion.Euler(0, randomAngle, 0) * direction;
    }

    public Vector3 GetRandomDirection(float angle)
    {
        return GetRandomDirection(transform.forward, angle);
    }
    public Vector3 GetRandomDirection(Vector3 direction)
    {
        return GetRandomDirection(direction, ExplorationAngle);
    }

    public Vector3 GetRandomDirection()
    {
        return GetRandomDirection(ExplorationAngle);
    }

    public void MoveInDirection(Vector3 targetDirection)
    {
        MoveInDirection(targetDirection, actualSpeed);
    }
    #endregion

    #region Distance & Range Methods

    public Transform FindNearest(LayerMask layer)
    {
        return FindNearest(layer, ViewDistance);
    }
    public Transform FindNearest(LayerMask layer, float range)
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, range, layer);
        float closestDistance = float.MaxValue;
        Transform nearestObj = null;
        foreach (Collider col in nearbyObjects)
        {
            float distance = GetDistanceTo(col.transform);
            if (IsInView(col.transform) && distance < closestDistance)
            {
                closestDistance = distance;
                nearestObj = col.transform;
            }
        }
        return nearestObj;
    }

    public float GetDistanceTo(Transform target)
    {
        return GetDistanceTo(target.position);
    }

    public float GetDistanceTo(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition);
    }


    public bool IsInRange(Vector3 target, float range)
    {
        return GetDistanceTo(target) <= range;
    }

    public bool IsInView(Transform target)
    {
        return IsInView(target.position);
    }
    public bool IsInView(Vector3 target)
    {
        return IsInRange(target, ViewDistance) && Vector3.Angle(Forward, GetDirectionTo(target)) <= ViewAngle / 2;
    }
    #endregion

    public bool IsInNest()
    {
        return Colony != null && Colony.IsInNest(Position);
    }

    public bool Pickup(Transform obj)
    {
        if (obj != null)
        {
            CarriedObj = obj.gameObject;
            CarriedObj.transform.SetParent(gameObject.transform);
            CarriedObj.transform.position = transform.position + Vector3.up * CarryHeight;
            var rb = CarriedObj.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            CarriedMass = rb.mass;
            CarriedObj.GetComponent<Collider>().enabled = false;

            // !! SPEED ADJUSTMENT !!
            actualSpeed = Speed * ((CarrySpeedModifier / Size) * Strength / CarriedMass); // Adjust speed based on carried mass
            
            return true;
        }
        return false;
    }

    public bool Pickup()
    {
        if (Pickup(Target))
        {
            Target = null;
            return true;
        }
        return false;
    }

    public void Drop()
    {
        if (CarriedObj != null)
        {
            /*
            CarriedObj.transform.SetParent(null);
            CarriedObj.GetComponent<Rigidbody>().isKinematic = false;
            CarriedObj.GetComponent<Collider>().enabled = true;
            CarriedObj.transform.position = transform.position + transform.forward + Vector3.up * CarryHeight; // Place it infront and above the ant
            CarriedObj.transform.rotation = Quaternion.identity; // Reset rotation
            */
            CarriedObj.transform.SetParent(null);
            Destroy(CarriedObj);
            CarriedObj = null;
            CarriedMass = 0f;

            // !! SPEED ADJUSTMENT !!
            actualSpeed = Speed; // Reset speed to normal when not carrying anything
        }
    }

    #region Phermone Methods
    public Pheromone GetMinPheromone(PheromoneType type)
    {
        return GetPheromone(type, (phero1, phero2) => phero1.Value < phero2.Value);
    }
    public Pheromone GetMaxPheromone(PheromoneType type)
    {
        return GetPheromone(type, (phero, resPhero) => phero.Value > resPhero.Value);
    }

    public Pheromone GetPheromone(PheromoneType type, System.Func<Pheromone, Pheromone, bool> comperator)
    {
        Pheromone resPhero = null;
        foreach (var phero in PheromoneMap.GetPheromones(Position, PheromoneDetectionRadius, type))
        {
            if (phero.Value > PheromoneDetectionThreshold && (resPhero == null || comperator(phero, resPhero)))
            {
                resPhero = phero;
            }
        }
        return resPhero;
    }

    public void AddPheromone(PheromoneType type)
    {
        AddPheromone(type, currentPheromoneDepositValue);
    }
    public void AddPheromone(PheromoneType type, float value)
    {
        PheromoneMap.AddPheromone(transform.position, value, type);
        //TODO: ReduceEnergy
    }

    public Vector3 GetPheromoneDirections(PheromoneType type)
    {
        Vector3 res;

        var phero = GetMaxPheromone(type);

        //Debug.Log($"Ant {Name} at {Position} found pheromone: {phero}");
        if (phero != null)
        {
            // Move towards pheromone marker
            res = GetDirectionTo(phero.Position);
            if (Random.value < ExplorationRate)
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
        currentPheromoneDepositValue = PheromoneDepositValue;// * genome.traits.Size;
    }
    #endregion

    public override string ToString()
    {
        return $"Ant at {transform.position}, {CurrentState.GetType().Name}";
    }


    private void OnCollisionEnter(Collision collision)
    {
        CurrentState?.OnCollisionEnter(collision);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ViewDistance);
        if (Target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Target.position, Vector3.one);
        }
        if (TargetPosition != Vector3.down)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(TargetPosition, 1f);
        }
        Debug.Log(this);

    }

    public bool IsFull(float precentage = 0.95f)
    {
        return currentEnergy >= maxEnergy * precentage;
    }

    public void CheckVitals()
    {
        if (currentEnergy == 0)
        {
            // Set on Hunger?
            TakeDamage(10);
        }else if (currentEnergy > maxEnergy * genome.traits.HungerThreshold)
        {
            // Set on Hunger?
            Heal(1);
        }
        if (IsDead())
        {
            Kill();
        }
    }

    private void Heal(float value)
    {
        currentHealth += value;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth; // Cap health at maximum
        }
    }

    public void Kill()
    {
        // TODO: Visual indication, than die
        Debug.Log(this + " is dead!");
        Destroy(gameObject);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        //TODO: visual indicator
    }

    public bool IsDead()
    {
        return currentHealth <= 0; // Check if the ant is dead based on current health
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

    public void ReduceEnergy(float amount)
    {
        currentEnergy -= amount; // Decrease energy by the specified amount
        if (currentEnergy < 0)
        {
            currentEnergy = 0; // Ensure energy doesn't go below zero
        }
    }

    public void AddEnergy(float amount)
    {
        currentEnergy += amount; // Increase energy by the specified amount
        if (currentEnergy > maxEnergy)
        {
            currentEnergy = maxEnergy; // Cap energy at maximum
        }
    }

    public bool IsHungry()
    {
        return currentEnergy <= maxEnergy * HungerThreshold; // Check if the ant is hungry based on the hunger threshold defined in genome traits
    }


    public float GetEnergyCost(string action)
    {
        float modifiers = Size * ViewAngle / 360f * ViewDistance * PheromoneDetectionRadius;

        switch (action.ToLower())
        {
            case "move":
                return 0.001f * modifiers * actualSpeed; // Energy cost for moving
            case "carry":
                return 0.001f * modifiers * (CarriedMass/ Strength) * actualSpeed; // Energy cost for carrying food
            default:
                return 0.2f; // Default energy cost for other actions
        }
    }
}
