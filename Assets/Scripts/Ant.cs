using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ant : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] public float speed = 3f;
    [SerializeField] public float detectRadius = 5f;
    [SerializeField] public GameObject nestObj;
    [SerializeField] public LayerMask foodLayer;
    [SerializeField, Range(0f, 1f)] public float strength = 0f;
    [SerializeField] public float carryHeight = 0.75f;

    [Header("ACO Settings")]
    [SerializeField] public float pheromoneDepositRate = 1f;
    [SerializeField] public float pheromoneInfluence = 2f;
    [SerializeField] public float explorationRate = 0.2f;
    [SerializeField] public float sampleRadius = 2f;
    [SerializeField] public int samplePoints = 8;

    [HideInInspector] public GameObject carriedFood;
    [HideInInspector] public Transform targetFood;
    [HideInInspector] public PheromoneMap pheromoneMap;
    [HideInInspector] public List<Vector3> pathHistory = new List<Vector3>();
    [HideInInspector] public Vector3 currentWanderDirection = Vector3.forward;

    private AntStateBase currentState;
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

    void Update()
    {
        currentState?.Update();
        
        if (transform.position.y < -10f)
        {
            // Reset ant position if it falls below a certain height
            transform.position = new Vector3(Random.Range(-10f, 10f), 1f, Random.Range(-10f, 10f));
            currentWanderDirection = Vector3.forward;
        }
    }

    public void ChangeState(AntStateBase newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void FindNearestFood()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, detectRadius, foodLayer);
        float closestDistance = float.MaxValue;
        Transform closestFood = null;
        foreach (Collider col in nearbyObjects)
        {
            float distance = Vector3.Distance(transform.position, col.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestFood = col.transform;
            }
        }
        if (closestFood != null && carriedFood == null)
        {
            targetFood = closestFood;
        }
        else if (closestFood == null)
        {
            targetFood = null;
        }
    }

    public void PickupFood()
    {
        if (targetFood != null)
        {
            carriedFood = targetFood.gameObject;
            carriedFood.transform.SetParent(gameObject.transform);
            carriedFood.transform.position = transform.position + Vector3.up * carryHeight;
            carriedFood.GetComponent<Rigidbody>().isKinematic = true;
            ChangeState(new CarryingFoodState(this));
            targetFood = null;
        }
    }

    public void DropFoodInNest()
    {
        if (carriedFood != null)
        {
            carriedFood.transform.SetParent(null);
            Destroy(carriedFood);
            carriedFood = null;
            //ChangeState(new SeekingFoodState(this));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentState is SeekingFoodState && targetFood != null)
        {
            if (other.transform == targetFood)
            {
                PickupFood();
            }
        }
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
        if (targetFood != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(targetFood.position, Vector3.one);
        }
    }

    public void RecordPath()
    {
        if (pathHistory.Count == 0 || Vector3.Distance(pathHistory[pathHistory.Count - 1], transform.position) > 0.5f)
        {
            pathHistory.Add(transform.position);
        }
    }

    public Vector3 GetDirectionFromPheromones(Pheromone.PheromoneType type)
    {
        Vector3 pheromoneDirection = Vector3.zero;
        float maxPheromone = 0.001f;
        for (int i = 0; i < samplePoints; i++)
        {
            float angle = i * (360f / samplePoints);
            Vector3 samplePoint = transform.position + Quaternion.Euler(0, angle, 0) * Vector3.forward * sampleRadius;
            float pheromone = pheromoneMap.GetPheromone(samplePoint, type);
            if (pheromone > maxPheromone)
            {
                maxPheromone = pheromone;
                pheromoneDirection = (samplePoint - transform.position).normalized;
            }
        }
        return pheromoneDirection;
    }
}
