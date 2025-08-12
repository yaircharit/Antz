using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ant : MonoBehaviour
{
    public enum AntState
    {
        SeekingFood,
        CarryingFood
    }

    [Header("Basic Settings")]
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float detectRadius = 5f;
    [SerializeField] private GameObject nestObj; // Position of the nest
    [SerializeField] private LayerMask foodLayer; // Layer for food objects
    [SerializeField, Range(0f, 1f)] private float strength = 0f; // 0 = slowest, 1 = no penalty
    [SerializeField] private float carryHeight = 0.75f; // Height above ant to hold food

    [Header("ACO Settings")]
    [SerializeField] private float pheromoneDepositRate = 1f;
    [SerializeField] private float pheromoneInfluence = 2f;
    [SerializeField] private float explorationRate = 0.2f;
    [SerializeField] private float sampleRadius = 2f;
    [SerializeField] private int samplePoints = 8;

    private AntState currentState = AntState.SeekingFood;
    private GameObject carriedFood;
    private Transform targetFood;
    private PheromoneMap pheromoneMap;
    private List<Vector3> pathHistory = new List<Vector3>();
    private float lastPheromoneDepositTime;
    private Vector3 currentWanderDirection = Vector3.forward;

    void Start()
    {
        pheromoneMap = FindObjectOfType<PheromoneMap>();
        if (pheromoneMap == null)
        {
            Debug.LogError("No PheromoneMap found in scene!");
        }
    }

    void Update()
    {
        // If not carrying food, always look for the nearest food
        if (currentState != AntState.CarryingFood)
        {
            FindNearestFood();
        }

        switch (currentState)
        {
            case AntState.SeekingFood:
                SeekFood();
                break;
            case AntState.CarryingFood:
                CarryFoodToNest();
                break;
        }
        // If carrying food, update its world position above the ant
        if (carriedFood != null)
        {
            Vector3 aboveAnt = transform.position + Vector3.up * carryHeight;
            carriedFood.transform.position = aboveAnt;
            carriedFood.transform.rotation = Quaternion.identity;
        }

        // Record path for pheromone deposit
        if (currentState == AntState.SeekingFood || currentState == AntState.CarryingFood)
        {
            RecordPath();
        }
    }

    void FindNearestFood()
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

        if (closestFood != null && currentState != AntState.CarryingFood)
        {
            targetFood = closestFood;
        }
        else if (closestFood == null)
        {
            targetFood = null;
        }
    }

    void SeekFood()
    {
        Vector3 finalDirection = Vector3.zero;
        if (targetFood == null)
        {
            // Wander and follow food pheromone if present, deposit home pheromone
            Vector3 pheromoneDirection = GetDirectionFromPheromones(Pheromone.PheromoneType.Food);
            if (pheromoneDirection.sqrMagnitude > 0.01f)
            {
                finalDirection = Vector3.Lerp(currentWanderDirection, pheromoneDirection, pheromoneInfluence);
            }
            else
            {
                // Wander as before
                float currentPheromoneLevel = pheromoneMap.GetPheromone(transform.position + currentWanderDirection * sampleRadius, Pheromone.PheromoneType.Home);
                if (currentPheromoneLevel > 0.8f)
                {
                    float leftPheromone = pheromoneMap.GetPheromone(transform.position + Quaternion.Euler(0, -90f, 0) * currentWanderDirection * sampleRadius, Pheromone.PheromoneType.Home);
                    float rightPheromone = pheromoneMap.GetPheromone(transform.position + Quaternion.Euler(0, 90f, 0) * currentWanderDirection * sampleRadius, Pheromone.PheromoneType.Home);
                    float steerAngle = (leftPheromone > rightPheromone) ? 20f : -20f;
                    currentWanderDirection = Quaternion.Euler(0, steerAngle, 0) * currentWanderDirection;
                }
                else
                {
                    currentWanderDirection = Quaternion.Euler(0, Random.Range(-10f, 10f), 0) * currentWanderDirection;
                }
                finalDirection = currentWanderDirection;
            }
            // Deposit home pheromone while exploring
            pheromoneMap.AddPheromone(transform.position, pheromoneDepositRate * Time.deltaTime, Pheromone.PheromoneType.Home);
        }
        else
        {
            Vector3 targetDirection = (targetFood.position - transform.position).normalized;
            Vector3 pheromoneDirection = GetDirectionFromPheromones(Pheromone.PheromoneType.Food);
            if (pheromoneDirection.sqrMagnitude > 0.01f)
            {
                finalDirection = Vector3.Lerp(targetDirection, pheromoneDirection, pheromoneInfluence);
            }
            else
            {
                if (Random.value > explorationRate)
                {
                    finalDirection = targetDirection;
                }
                else
                {
                    finalDirection = Quaternion.Euler(0, Random.Range(-45f, 45f), 0) * targetDirection;
                }
            }
            // Deposit home pheromone while seeking food
            pheromoneMap.AddPheromone(transform.position, pheromoneDepositRate * Time.deltaTime, Pheromone.PheromoneType.Home);
        }
        Vector3 targetPosition = transform.position + finalDirection;
        targetPosition.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, followSpeed * Time.deltaTime);
        if (finalDirection.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(finalDirection, Vector3.up);
            transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }
    }

    void PickupFood()
    {
        if (targetFood != null)
        {
            carriedFood = targetFood.gameObject;
            carriedFood.transform.SetParent(gameObject.transform);
            carriedFood.transform.position = transform.position + Vector3.up * carryHeight;
            carriedFood.GetComponent<Rigidbody>().isKinematic = true; // Disable physics while carrying
            currentState = AntState.CarryingFood;
            targetFood = null;
        }
    }

    void CarryFoodToNest()
    {
        float speedFactor = Mathf.Lerp(0.5f, 1f, strength);
        float carryingSpeed = followSpeed * speedFactor;
        // Follow home pheromone trail if present
        Vector3 homeDirection = GetDirectionFromPheromones(Pheromone.PheromoneType.Home);
        Vector3 nestDirection = (nestObj.transform.position - transform.position).normalized;
        Vector3 finalDirection = (homeDirection.sqrMagnitude > 0.01f)
            ? Vector3.Lerp(nestDirection, homeDirection, pheromoneInfluence)
            : nestDirection;
        Vector3 targetPosition = transform.position + finalDirection;
        targetPosition.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, carryingSpeed * Time.deltaTime);
        if (finalDirection.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(finalDirection, Vector3.up);
            transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }
        // Deposit food pheromone while carrying food
        float distanceToNest = Vector3.Distance(transform.position, nestObj.transform.position);
        float depositMultiplier = 1f + (1f / (distanceToNest + 1f));
        pheromoneMap.AddPheromone(transform.position, pheromoneDepositRate * depositMultiplier * Time.deltaTime, Pheromone.PheromoneType.Food);
        if (Vector3.Distance(transform.position, nestObj.transform.position) < 0.5f)
        {
            foreach (Vector3 position in pathHistory)
            {
                pheromoneMap.AddPheromone(position, pheromoneDepositRate * 2f, Pheromone.PheromoneType.Food);
            }
            pathHistory.Clear();
            DropFoodInNest();
        }
    }

    void DropFoodInNest()
    {
        if (carriedFood != null)
        {
            carriedFood.transform.SetParent(null);
            Destroy(carriedFood); // Food is consumed by the nest
            carriedFood = null;
            currentState = AntState.SeekingFood;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentState == AntState.SeekingFood && targetFood != null)
        {            
            if ( other.transform == targetFood)
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

    void RecordPath()
    {
        if (pathHistory.Count == 0 || Vector3.Distance(pathHistory[pathHistory.Count - 1], transform.position) > 0.5f)
        {
            pathHistory.Add(transform.position);
        }
    }

    Vector3 GetDirectionFromPheromones(Pheromone.PheromoneType type)
    {
        Vector3 pheromoneDirection = Vector3.zero;
        float maxPheromone = 0.01f;
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
