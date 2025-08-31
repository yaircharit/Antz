using UnityEngine;

public class Ant : MovingEntity
{
    public override string Name { get; protected set; } = "Ant"; // Name of the ant, can be used for identification
    public new static int Count { get; protected set; } = 0; // Static counter to keep track of the number of ants

    public static Ant SelectedAnt { get; internal set; } = null;
    private PopupWindow statsUIInstance => Player.Instance.statsInfoInstance; // Assign in inspector to always-present window

    public PheromoneMap PheromoneMap { get; protected set; }
    public Pheromone currentPhero = null;
    public float currentPheromoneDepositValue; // Current pheromone deposit rate, reduced over time
    public AntColony Colony { get; private set; } = null; // Reference to the colony this ant belongs to

    public bool isStuck = false; // Flag to indicate if the ant is stuck

    private void Awake()
    {
        PheromoneMap = PheromoneMap.Instance;
        if (PheromoneMap == null)
        {
            Debug.LogError("No PheromoneMap found in scene!");
        }
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        meshRenderer.material.color = baseColor;
    }

    public void Init(AntColony antColony)
    {
        ID = Count++; // Increment the static ant count

        base.Init();
        Colony = antColony;
        if (Colony == null)
        {
            Debug.LogError("No ColonyManagement found in scene!");
        }

        currentPheromoneDepositValue = PheromoneMap.Instance.ACOConfig.DepositValue; // Initialize pheromone deposit value
    }

    void Start()
    {
        ChangeState(new SeekingFoodState(this, PheromoneType.Home));
    }

    public float updateInterval = 0.5f; // Interval in seconds to update the ant's state
    private float timeSinceLastUpdate = 0f;
    void FixedUpdate()
    {
        if (transform.position.y < -10f)
        {
            // Reset ant position if it falls below a certain height
            transform.position = Colony.NestPos + Vector3.up * 3;
            ResetPheromoneDepositRate();
        }

        currentState?.Update();

        // Check if it's time to update the ant's vitals
        if (Time.time - timeSinceLastUpdate < updateInterval) return;
        timeSinceLastUpdate = Time.time;

        CheckVitals(); // Check the ant's vitals (health, energy, etc.) every frame
    }

    private void OnMouseDown()
    {
        // Deselect previous ant
        if (SelectedAnt != null && SelectedAnt != this)
        {
            SelectedAnt.Deselect();
        }
        SelectedAnt = this;
        UpdateStatsUI();
        Highlight(true);
        statsUIInstance.Active = true;
    }

    private void UpdateStatsUI()
    {
        if (statsUIInstance == null) return;
        statsUIInstance.Header = Name;
        statsUIInstance.Body = GetStatsString();
    }

    private void Highlight(bool enable)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = enable ? highlightColor : baseColor;
        }
    }

    public void Deselect()
    {
        Highlight(false);
        if (SelectedAnt == this)
        {
            SelectedAnt = null;
            statsUIInstance.Active = false;
        }
    }

    private void OnDestroy()
    {
        Deselect();
    }

    void Update()
    {
        if (SelectedAnt == this)
        {
            UpdateStatsUIAndLine();
        }
    }

    private void UpdateStatsUIAndLine()
    {
        if (statsUIInstance != null && SelectedAnt == this)
        {
            statsUIInstance.Header = Name;
            statsUIInstance.Body = GetStatsString();
        }
    }


    private float lastStuckTime = 0f; // Timer to check if the ant is stuck

    public void Move(PheromoneType type)
    {
        Vector3 targetDirection = transform.forward;

        if (TargetPosition != Vector3.down && IsInView(TargetPosition))
        {
            targetDirection = GetDirectionTo(TargetPosition);
            currentPhero = null;
        }
        else
        {
            targetDirection = GetPheromoneDirections(type);
        }

        targetDirection.y = transform.forward.y;
        MoveInDirection(targetDirection, actualSpeed);
        ReduceEnergy(GetEnergyCost((IsCarrying) ? "Carry" : "Move") * Time.fixedDeltaTime); // Decrease energy with each movement

        if (Time.time - lastStuckTime > 1f)
        {
            isStuck = GetDistanceTo(lastPosition) < 0.1f * actualSpeed;
            lastStuckTime = Time.time; // Update last stuck time
            lastPosition = transform.position; // Update last position
        }
    }

    public bool IsInNest()
    {
        return Colony != null && Colony.IsInNest(transform.position, genome.Size);
    }

    public void AddPheromone(PheromoneType type)
    {
        AddPheromone(type, currentPheromoneDepositValue);
    }

    public void AddPheromone(PheromoneType type, float value)
    {
        if (type != PheromoneType.None)
        {
            PheromoneMap.AddPheromone(transform.position, value, type);
            ReducePheromone();
            //TODO: ReduceEnergy?
        }
    }

    public void RemovePheromone(PheromoneType type)
    {
        if (type == PheromoneType.None) return; // No pheromone to remove

        foreach (var phero in PheromoneMap.GetPheromones(transform.position, ACO.Instance.DetectionDistance, type))
        {
            PheromoneMap.RemovePheromone(phero);
        }
        PheromoneMap.RemovePheromone(transform.position, type);
    }


    public Vector3 GetPheromoneDirections(PheromoneType type)
    {
        Vector3 res;

        currentPhero = PheromoneMap.GetMaxPheromone(type, transform.position);
        if (currentPhero != null)
        {
            // Move towards pheromone marker
            res = GetDirectionTo(currentPhero.Position);
            if (Random.value < ACO.Instance.ExplorationRate)
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

        return res;
    }

    public void ResetPheromoneDepositRate()
    {
        currentPheromoneDepositValue = PheromoneMap.Instance.ACOConfig.DepositValue;// * genome.traits.Size;
    }

    public void ReducePheromone()
    {
        currentPheromoneDepositValue -= actualSpeed * ACO.Instance.DecayFactor * Time.deltaTime; // Decrease pheromone deposit rate over time
        currentPheromoneDepositValue = Mathf.Max(currentPheromoneDepositValue, 0); // Ensure it doesn't go below a minimum threshold
    }

    public bool LowOnPheromones()
    {
        return currentPheromoneDepositValue <= ACO.Instance.DecayFactor * 2; // Check if pheromone deposit rate is below the minimum threshold
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

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        if (currentPhero != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentPhero.Position, 0.9f);
        }

        Debug.Log($"{this}, {currentPhero}");

    }

    public bool FoundPheromone(PheromoneType type)
    {
        // Check if the ant has found a pheromone of the specified type within its view distance
        var pheromones = PheromoneMap.GetPheromones(transform.position, ACO.Instance.DetectionDistance, type);
        return pheromones.Length > 0;
    }

    public void FindFood()
    {
        // Check if the ant has found food within its view distance
        Target = FindNearest(LayerMask.GetMask("Food"), genome.ViewDistance);
    }

    public bool FoundFood()
    {
        FindFood();
        return Target != null || FoundPheromone(PheromoneType.Food);
    }
}
