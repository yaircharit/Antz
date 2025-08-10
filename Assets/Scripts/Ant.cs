using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ant : MonoBehaviour
{
    public enum AntState
    {
        FollowingPlayer,
        SeekingFood,
        CarryingFood
    }

    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float detectRadius = 5f;
    [SerializeField] private GameObject nestObj; // Position of the nest
    [SerializeField] private LayerMask foodLayer; // Layer for food objects
    [SerializeField, Range(0f, 1f)] private float strength = 0f; // 0 = slowest, 1 = no penalty
    [SerializeField] private float carryHeight = 0.75f; // Height above ant to hold food
    [SerializeField] private Transform headTransform; // Assign the head collider transform in inspector

    private Transform player;
    private AntState currentState = AntState.FollowingPlayer;
    private GameObject carriedFood;
    private Transform targetFood;

    void Start()
    {
        // Find the player by name
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player not found! Please name the player GameObject as 'Player'.");
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
            case AntState.FollowingPlayer:
                if (targetFood != null)
                {
                    currentState = AntState.SeekingFood;
                }
                else
                {
                    FollowPlayer();
                }
                break;
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

        //Debug.Log($"Current State: {currentState}, Target Food: {targetFood?.name ?? "None"}, Carried Food: {carriedFood?.name ?? "None"}");
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

        // Only update targetFood if not already seeking/carrying
        if (closestFood != null && currentState != AntState.CarryingFood)
        {
            targetFood = closestFood;
        }
        else if (closestFood == null)
        {
            targetFood = null;
        }
    }

    void FollowPlayer()
    {
        if (player == null) return;
        Vector3 targetPosition = player.position - (player.forward * followDistance);
        targetPosition.y = 0; // Always stay on the ground
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, followSpeed * Time.deltaTime);
        
        // Look at the player, but only rotate around y axis
        Vector3 lookTarget = new Vector3(player.position.x, 0, player.position.z);
        Vector3 direction = lookTarget - transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }
    }

    void SeekFood()
    {
        if (targetFood == null)
        {
            currentState = AntState.FollowingPlayer;
            return;
        }

        // Move towards food
        Vector3 targetPosition = targetFood.position;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Look at food
        Vector3 direction = targetPosition - transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }

        // PickupFood is now handled by OnTriggerEnter
    }

    void PickupFood()
    {
        if (targetFood != null)
        {
            Debug.Log($"Ant picking up food2: {targetFood.name}");
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
        // Calculate carrying speed with strength modifier
        float speedFactor = Mathf.Lerp(0.5f, 1f, strength); // 0 strength = 0.5x, 1 strength = 1x
        float carryingSpeed = followSpeed * speedFactor;

        // Move towards nest
        Vector3 targetPosition = nestObj.transform.position;
        targetPosition.y = 0;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, carryingSpeed * Time.deltaTime);

        // Look at nest
        Vector3 direction = targetPosition - transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0);
        }

        // Check if we reached the nest
        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
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
            currentState = AntState.FollowingPlayer;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only pickup if seeking food, and the trigger is the head, and the collider is the target food
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
        // Visualize detection radius in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
        
        // Visualize nest position
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
}
