using Unity.VisualScripting;
using UnityEngine;

public class Ant : MonoBehaviour
{
    [Header("Basic Settings")]

    [SerializeField] public GameObject nestObj;
    [SerializeField] public LayerMask foodLayer;
    [SerializeField] public float carryHeight = 0.75f;
    [SerializeField] public float carrySpeedModifier = 0.5f;

    public bool IsCarrying => carriedObj != null;


    [Header("Genome Settings")]
    [SerializeField] public float strength = 1.4f;
    [SerializeField] public float speed = 8f;
    [SerializeField] public float detectRadius = 10f;

    public float ActualSpeed => speed * (IsCarrying ? (carrySpeedModifier * strength) : 1f);


    [Header("ACO Settings")]
    [SerializeField] public float pheromoneDepositRate = 0.5f;
    [SerializeField, Range(0,1)] public float explorationRate = 0.2f;
    [SerializeField, Range(0,180)] public float explorationAngle = 10f;

    [HideInInspector] public GameObject carriedObj;
    [HideInInspector] public Transform target;
    [HideInInspector] public PheromoneMap pheromoneMap;
    [HideInInspector] public Rigidbody rb;




    private AntStateBase currentState;
    public AntStateBase CurrentState => currentState;

    void Start()
    {
        pheromoneMap = FindObjectOfType<PheromoneMap>();
        rb = GetComponent<Rigidbody>();
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
        transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.forward, speed );
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
        MoveTowards(targetPosition, ActualSpeed * Time.deltaTime);
    }
    public void MoveInDirection(Vector3 targetDirection)
    {
        MoveInDirection(targetDirection, ActualSpeed * Time.deltaTime);
    }
    #endregion

    #region Disnce & Range Methods
    public Transform FindNearest(LayerMask layer)
    {
        return FindNearest(layer, detectRadius);
    }
    public Transform FindNearest(LayerMask layer, float range)
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, range, layer);
        float closestDistance = float.MaxValue;
        Transform nearestObj = null;
        foreach (Collider col in nearbyObjects)
        {
            float distance = GetDistanceTo(col.transform);
            if (distance < closestDistance)
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
        return target != null && IsInRange(target, detectRadius);
    }

    public bool IsInRange(Transform target, float range)
    {
        return target != null && IsInRange(target.position,range);
    }
    public bool IsInRange(Transform target)
    {
        return target != null && IsInRange(target, detectRadius);
    }

    public bool IsInRange(Vector3 target, float range)
    {
        return GetDistanceTo(target) <= range;
    }
    public bool IsInRange(Vector3 target)
    {
        return IsInRange(target, detectRadius);
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
        return pheromoneMap.GetMinPheromone(transform.position, detectRadius, type);
    }
    public Pheromone GetMaxPheromone(PheromoneType type)
    {
        return pheromoneMap.GetMaxPheromone(transform.position, detectRadius, type);
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
        Gizmos.DrawWireSphere(transform.position, detectRadius);
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
