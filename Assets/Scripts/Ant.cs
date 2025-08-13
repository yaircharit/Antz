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


    //[Header("Genome Settings")]
    public float Strength { get; private set; } = 1.4f; // Determines how strong the ant is, affects speed when carrying food (1 is normal strength)
    public float Speed { get; private set; } = 8f;
    public float ViewRadius { get; private set; } = 10f;
    public float ViewAngle { get; private set; } = 120; // Angle in degrees
    public float PheromoneDetectionRadius { get; private set; } = 10f;
    public float PheromoneDetectionThreshold { get; private set; } = 0.1f;

    public float ActualSpeed => Speed * (IsCarrying ? (CarrySpeedModifier * Strength / CarriedMass) : 1f);


    //[Header("ACO Settings")]
    public float PheromoneDepositRate { get; private set; } = 0.5f;
    public float CurrentPheromoneDepositRate { get; private set; } = 0.5f; // Current deposit rate, reduced over time
    public float MinPheromoneDepositRate { get; private set; } = 0.2f; // Minimum pheromone deposit rate to prevent pheromone from disappearing too quickly
    public float PheromoneDecayFactor { get; private set; } // How much pheromone decays per second
    public float ExplorationRate { get; private set; } = 0.4f;
    public float ExplorationAngle { get; private set; } = 10f;

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
        PheromoneDecayFactor = PheromoneMap.DecayValue * 1.2f; // Set the pheromone decay factor based on the pheromone map's decay value
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

        if (CurrentPheromoneDepositRate < MinPheromoneDepositRate)
        {
            CurrentPheromoneDepositRate = MinPheromoneDepositRate; // Ensure it doesn't go below the minimum
        }
        else
        {
            CurrentPheromoneDepositRate -= PheromoneDecayFactor ; // Decrease pheromone deposit rate over time
        }
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
    }


    public void MoveInDirection(Vector3 targetDirection, float speed)
    {
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
        MoveInDirection(targetDirection, ActualSpeed);
    }
    #endregion

    #region Distance & Range Methods

    public Transform FindNearest(LayerMask layer)
    {
        return FindNearest(layer, ViewRadius);
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
        return IsInRange(target, ViewRadius) && Vector3.Angle(Forward, GetDirectionTo(target)) <= ViewAngle / 2;
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
        AddPheromone(type, CurrentPheromoneDepositRate);
    }
    public void AddPheromone(PheromoneType type, float value)
    {
        PheromoneMap.AddPheromone(transform.position, value, type);
    }

    public Vector3 GetPheromoneDirections(PheromoneType type)
    {
        Vector3 res;

        var phero = GetMaxPheromone(type);

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
        CurrentPheromoneDepositRate = PheromoneDepositRate;
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
        Gizmos.DrawWireSphere(transform.position, ViewRadius);
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
    }
}
