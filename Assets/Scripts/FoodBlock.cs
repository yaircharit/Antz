using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodBlock : MonoBehaviour
{
    public static int count = 0; // Static counter for food blocks

    public float mass = 5f; // Mass of each food block

    // Start is called before the first frame update
    void Start()
    {        // Set the mass of the rigidbody if it exists
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.mass = mass;
        }
        count++;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -10f)
        {
            // Destroy the food block if it falls below a certain height
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        count--; // Decrement the counter when a food block is destroyed

        if (count == 0)
            FoodGenerator.SpawnAppleShape(); // Spawn a new apple shape if no food blocks are left
    }
}
