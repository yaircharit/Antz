using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Ant : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] public GameObject nestObj;
    [SerializeField] public float carryHeight = 0.75f;
    [SerializeField] public float carrySpeedModifier = 0.5f;

    public bool IsCarrying => carriedObj != null;


    [Header("Genome Settings")]
    [SerializeField] public float strength = 1.4f; // Determines how strong the ant is, affects speed when carrying food (1 is normal strength)
    [SerializeField] public float speed = 8f;
    [SerializeField] public float viewRadius = 10f;
    [SerializeField] public float viewAngle = 240f; // Angle in degrees
    [SerializeField] public float pheromoneDetectionRadius = 10f;
    [SerializeField] public float pheromoneDetectionThreshold = 0.1f;

    public float ActualSpeed => speed * (IsCarrying ? (carrySpeedModifier * strength) : 1f);


    [Header("ACO Settings")]
    [SerializeField] public float pheromoneDepositRate = 0.5f;
    [SerializeField, Range(0, 1)] public float explorationRate = 0.2f;
    [SerializeField, Range(0, 180)] public float explorationAngle = 10f;

    [HideInInspector] public GameObject carriedObj;
    [HideInInspector] public Transform target;
    [HideInInspector] public PheromoneMap pheromoneMap;


    private AntStateBase currentState;
    private Vector3 Position => transform.position;
    public AntStateBase CurrentState => currentState;

    void Start()
    {
        pheromoneMap = FindObjectOfType<PheromoneMap>();
        if (pheromoneMap == null)
        {
            Debug.LogError("No PheromoneMap found in scene!");
        }
        ChangeState(new SeekingFoodState(this));
    }

    void FixedUpdate()
    {
        currentState?.Update();

        if (transform.position.y < -10f)
        {
            // Reset ant position if it falls below a certain height
            transform.position = new Vector3(Random.Range(-10f, 10f), 1f, Random.Range(-10f, 10f));
        }
    }

    public void ChangeState(AntStateBase newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    #region Movement & Direction Methods
    public void MoveTowards(Vector3 targetPosition, float speed)
    {
        MoveInDirection((targetPosition - transform.position).normalized, speed);
    }

    public void MoveInDirection(Vector3 targetDirection, float speed)
    {
        transform.forward = targetDirection;
        transform.position += targetDirection * speed * Time.deltaTime;
    }
    public Vector3 GetDirectionTo(GameObject target)
    {
        return GetDirectionTo(target.transform);
    }
    public Vector3 GetDirectionTo(Transform target)
    {
        return GetDirectionTo(target.position);
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
        return GetRandomDirection(direction, explorationAngle);
    }

    public Vector3 GetRandomDirection()
    {
        return GetRandomDirection(explorationAngle);
    }

    public void MoveTowards(Vector3 targetPosition)
    {
        MoveTowards(targetPosition, ActualSpeed);
    }
    public void MoveInDirection(Vector3 targetDirection)
    {
        MoveInDirection(targetDirection, ActualSpeed);
    }
    #endregion

    #region Disnce & Range Methods
    public Transform FindNearest(LayerMask layer)
    {
        return FindNearest(layer, viewRadius, viewAngle);
    }
    public Transform FindNearest(LayerMask layer, float range, float angle)
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, range, layer);
        float closestDistance = float.MaxValue;
        Transform nearestObj = null;
        foreach (Collider col in nearbyObjects)
        {
            float distance = GetDistanceTo(col.transform);
            if (IsInView(col.transform))
            {
                closestDistance = distance;
                nearestObj = col.transform;
            }
        }
        return nearestObj;
    }
    public float GetDistanceTo(GameObject target)
    {
        return GetDistanceTo(target.transform);
    }

    public float GetDistanceTo(Transform target)
    {
        return GetDistanceTo(target.position);
    }

    public float GetDistanceTo(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition);
    }

    public bool IsInRange(GameObject target, float range)
    {
        return target != null && IsInRange(target.transform, range);
    }
    public bool IsInRange(GameObject target)
    {
        return target != null && IsInRange(target, pheromoneDetectionRadius);
    }

    public bool IsInRange(Transform target, float range)
    {
        return target != null && IsInRange(target.position, range);
    }
    public bool IsInRange(Transform target)
    {
        return target != null && IsInRange(target, pheromoneDetectionRadius);
    }

    public bool IsInRange(Vector3 target, float range)
    {
        return GetDistanceTo(target) <= range;
    }
    public bool IsInRange(Vector3 target)
    {
        return IsInRange(target, pheromoneDetectionRadius);
    }

    public bool IsInView(GameObject target)
    {
        return IsInView(target.transform);
    }
    public bool IsInView(Transform target)
    {
        return IsInView(target.position);
    }
    public bool IsInView(Vector3 target)
    {
        return IsInRange(target, viewRadius) && Vector3.Angle(Position, GetDirectionTo(target)) <= viewAngle / 2;
    }
    #endregion

    public bool Pickup(Transform obj)
    {
        if (obj != null)
        {
            carriedObj = obj.gameObject;
            carriedObj.transform.SetParent(gameObject.transform);
            carriedObj.transform.position = transform.position + Vector3.up * carryHeight;
            carriedObj.GetComponent<Rigidbody>().isKinematic = true;
            carriedObj.GetComponent<Collider>().enabled = false;

            return true;
        }
        return false;
    }

    public bool Pickup()
    {
        if (Pickup(target))
        {
            target = null;
            return true;
        }
        return false;
    }

    public void Drop()
    {
        if (carriedObj != null)
        {
            carriedObj.transform.SetParent(null);
            Destroy(carriedObj);
            carriedObj = null;
        }
    }

    #region Phermone Methods
    public Pheromone GetMinPheromone(PheromoneType type)
    {
        Pheromone minPhero = null;
        foreach (var phero in pheromoneMap.GetPheromones(Position, pheromoneDetectionRadius, type))
        {
            if (phero.Value > pheromoneDetectionThreshold && (minPhero == null || phero.Value < minPhero.Value))
            {
                minPhero = phero;
            }
        }
        return minPhero;
    }
    public Pheromone GetMaxPheromone(PheromoneType type)
    {
        Pheromone maxPhero = null;
        foreach (var phero in pheromoneMap.GetPheromones(Position, pheromoneDetectionRadius, type))
        {
            if (phero.Value > pheromoneDetectionThreshold && (maxPhero == null || phero.Value > maxPhero.Value))
            {
                maxPhero = phero;
            }
        }
        return maxPhero;
    }

    public void AddPheromone(PheromoneType type)
    {
        pheromoneMap.AddPheromone(transform.position, pheromoneDepositRate, type);
    }
    #endregion

    public override string ToString()
    {
        return $"Ant at {transform.position}, {currentState.GetType().Name}";
    }

    private void OnTriggerEnter(Collider other)
    {
        currentState?.OnTriggerEnter(other);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
        if (nestObj != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(nestObj.transform.position, 1.5f);
        }
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(target.position, Vector3.one);
        }
    }
}
