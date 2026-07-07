using System.Net.Mime;
using UnityEngine;

public class Ant : MovingEntity
{
    protected override string DefaultName => "Ant";
    public new static int Count { get; protected set; } = 0; // Static counter to keep track of the number of ants

    public static Ant SelectedAnt { get; internal set; } = null;

    public PheromoneMap PheromoneMap { get; protected set; }
    public Pheromone currentPhero = null;
    public float currentPheromoneDepositValue; // Current pheromone deposit rate, reduced over time
    public AntColony Colony { get; private set; } = null; // Reference to the colony this ant belongs to


    private void Awake()
    {
        PheromoneMap = PheromoneMap.Instance;
        if (PheromoneMap == null)
        {
            Debug.LogError("No PheromoneMap found in scene!");
        }
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        meshRenderer.material.color = baseColor; // Default color for ants

        OnSelected += Select;
        OnDeselected += Deselect;
    }

    public virtual void Init(AntColony antColony, Genome genome)
    {
        ID = Count++; // Increment the static ant count

        base.Init(genome);
        Colony = antColony;
        if (Colony == null)
        {
            Debug.LogError("No ColonyManagement found in scene!");
        }

        currentPheromoneDepositValue = genome["ACO_PheromoneDepositValue"].Value; // Initialize pheromone deposit value
    }

    void Start()
    {
        ChangeState(new ExploreState(this, PheromoneType.Home));
    }

    protected virtual void FixedUpdate()
    {
        // TODO: should it be every FixedUpdate?
        if (transform.position.y < -10f)
        {
            // Reset ant position if it falls below a certain height
            ChangeState(new ExploreState(this));
            transform.SetPositionAndRotation(Colony.NestPos + Vector3.up, Quaternion.Euler(0, 0, 0));
            ResetPheromoneDepositRate();
        }

        currentState?.Tick();
    }

    private void OnMouseDown()
    {
        RaiseOnSelected();
    }

    public void Select()
    {
        // Deselect previous ant
        if (SelectedAnt != null && SelectedAnt != this)
        {
            SelectedAnt.RaiseOnDeselected();
        }
        SelectedAnt = this;
        Highlight(true);
    }


    public void Deselect()
    {
        Highlight(false);
        SelectedAnt = null;
    }


    private float lastStuckTime = 0f; // Timer to check if the ant is stuck

    public void Move(PheromoneType type)
    {
        Vector3 targetDirection;

        if (TargetPosition != Vector3.down && IsInView(TargetPosition))
        {
            targetDirection = GetDirectionTo(TargetPosition);
            currentPhero = null;
        }
        else
        {
            targetDirection = GetPheromoneDirections(type);
        }
        MoveTowards(targetDirection, CurrentSpeed);

        if (Time.time - lastStuckTime > 1f)
        {
            isStuck = GetDistanceTo(lastPosition) < 0.01f * CurrentSpeed;
            lastStuckTime = Time.time; // Update last stuck time
            lastPosition = transform.position; // Update last position
        }
    }

    public override void MoveTowards(Vector3 direction, float speed)
    {

        // Finally move using the adjusted direction
        base.MoveTowards(direction, speed);

        // Energy consumption
        ReduceEnergy();
    }

    public bool IsInNest()
    {
        return Colony != null && Colony.IsInNest(transform.position);
    }

    public void DropPheromone(PheromoneType type)
    {
        if (type != PheromoneType.None)
        {
            PheromoneMap.AddPheromone(transform.position, currentPheromoneDepositValue, type);
            ReducePheromone();
        }
    }

    public void RemovePheromone(PheromoneType type)
    {
        if (type == PheromoneType.None) return; // No pheromone to remove

        foreach (var phero in PheromoneMap.GetPheromones(transform.position, genome["ACO_PheromoneDetectionDistance"].Value, type))
        {
            PheromoneMap.RemovePheromone(phero);
        }
        PheromoneMap.RemovePheromone(transform.position, type);
    }


    public Vector3 GetPheromoneDirections(PheromoneType type)
    {
        Vector3 res;

        currentPhero = PheromoneMap.GetMaxPheromone(type, transform.position, genome["ACO_PheromoneDetectionDistance"].Value);
        if (currentPhero != null)
        {
            // Move towards pheromone marker
            res = GetDirectionTo(currentPhero.Position);
            if (Random.value < genome["ACO_ExplorationRate"].Value)
            {
                // Randomly explore around the pheromone
                res = GetRandomDirection(res);
            }

            return res;
        }

        // No pheromone found, wander randomly
        return GetRandomDirection();
    }

    public void ResetPheromoneDepositRate()
    {
        currentPheromoneDepositValue = genome["ACO_PheromoneDepositValue"].Value;// * genome.traits.Size;
    }

    public void ReducePheromone()
    {
        currentPheromoneDepositValue -= CurrentSpeed * PheromoneMap.Instance.PheromoneDecayFactor * Time.deltaTime; // Decrease pheromone deposit rate over time
    }

    public bool LowOnPheromones()
    {
        return currentPheromoneDepositValue <= PheromoneMap.Instance.PheromoneDecayFactor * 2; // Check if pheromone deposit rate is below the minimum threshold
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
        var pheromones = PheromoneMap.GetPheromones(transform.position, genome["ACO_PheromoneDetectionDistance"].Value, type);
        return pheromones.Length > 0;
    }

    public bool FindFood()
    {
        if (IsCarrying) return true;
        // Check if the ant has found food within its view distance
        Target = FindNearest(LayerMask.GetMask("Food"), genome["ViewDistance"].Value);
        return Target != null;
    }

    public bool FoundFood()
    {
        return FindFood() || FoundPheromone(PheromoneType.Food);
    }

    internal void Wander()
    {
        Vector3 randomDirection = GetRandomDirection();
        MoveTowards(randomDirection, CurrentSpeed);
    }
}
