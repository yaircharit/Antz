using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ant : MonoBehaviour
{
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float followDistance = 2f;
    private Transform player;

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
}
