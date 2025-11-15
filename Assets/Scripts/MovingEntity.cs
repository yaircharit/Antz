using System.Collections;
using UnityEngine;

public abstract class MovingEntity : MonoBehaviour
{

    public virtual string Name { get; protected set; } = "MovingEntity"; // Name of the ant, can be used for identification
    public int ID { get; protected set; } = -1; // Unique ID for the ant, can be used for identification
    public static int Count { get; protected set; } = 0; // Static counter to keep track of the number of ants

    //[Header("Basic Settings")]
    public float CarryHeight { get; protected set; } = 0.75f;
    public float CarrySpeedModifier { get; protected set; } = 0.5f;
    public bool IsCarrying => CarriedObj != null;


    public float maxHealth = 100; // Maximum health of the ant
    public float currentHealth; // Current health of the ant, can be used for health management
    public float maxEnergy = 100f; // Maximum energy of the ant
    public float currentEnergy; // Current energy of the ant, can be used for energy management

    protected Genome genome; // Genome of the ant, can be used for genetic algorithms or traits
    public float actualSpeed;
    public float actualStrength;

    public GameObject CarriedObj { get; protected set; }
    public float CarriedMass { get; protected set; } = 0f; // Mass of the carried object, used to calculate speed when carrying food

    protected Transform _target;
    public Transform Target
    {
        get { return (_target) ? _target : null; } // Return null if _target is not set 
        set { _target = value; _targetPosition = Vector3.down; }// Set target and reset target position (Vector3.down indicates no target position, y >= 0)
    }

    protected Vector3 _targetPosition;
    public Vector3 TargetPosition
    {
        get { return (Target) ? Target.position : _targetPosition; }
        set { Target = null; _targetPosition = value; }
    }

    public StateBase currentState;
    protected Vector3 lastPosition;

    protected MeshRenderer meshRenderer;
    public Color baseColor = Color.black;
    public Color highlightColor = Color.yellow;
    public Color damageColor = Color.red;
    public float damageFlashDuration = 0.2f;
    public Color healColor = Color.green;
    public float healFlashDuration = 0.3f;

    public event System.Action OnSelected;
    public event System.Action OnDeselected;
    public event System.Action OnDestroyed;

    public event System.Action<float> OnDamageTaken;
    public event System.Action<float> OnHealed;
    public event System.Action<float> OnEnergyChanged;
    public event System.Action<StateBase> OnStateChanged;


    public virtual void Init()
    {

        ID = Count++; // Increment the static ant count
        Name = $"{Name}_{ID}"; // Set the name based on the ID

        OnDamageTaken += (x) => FlashColor(damageColor, 0.2f);
        OnDamageTaken += TakeDamage;

        OnHealed += (x) => FlashColor(healColor, 0.2f);
        OnHealed += Heal;
        OnHealed += (value) => ReduceEnergy(value * 5); // Healing costs energy

        genome = new Genome(); // Initialize genome with default values
        genome.Mutate(true); // Mutate the genome to get random traits

        float size = genome["Size"].Value;

        maxHealth = 100f * size; // Set maximum health based on size trait
        maxEnergy = 100f * size; // Set maximum energy based on size trait
        transform.localScale = Vector3.one * size; // Set the scale of the ant based on size trait

        currentHealth = maxHealth; // Initialize current health
        currentEnergy = maxEnergy;
        actualSpeed = genome["Speed"].Value / size; // Initialize actual speed
        actualStrength = genome["Strength"].Value * size;

        transform.rotation = Quaternion.Euler(0, Random.value * 360f, 0); // Randomize initial rotation
        lastPosition = transform.position; // Store the initial position as last position
    }

    public void ChangeState(StateBase newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void MoveTowards(Vector3 targetDirection, float speed)
    {
        if (targetDirection == Vector3.zero) targetDirection = Vector3.forward; // Avoid division by zero or invalid movement
        transform.forward = targetDirection;
        var distance = speed * Time.fixedDeltaTime * targetDirection;
        transform.position += distance;
        ReduceEnergy(GetEnergyCost() * Time.fixedDeltaTime); // Decrease energy with each movement
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
        return GetRandomDirection(direction, ACO.Instance.ExplorationAngle);
    }

    public Vector3 GetRandomDirection()
    {
        return GetRandomDirection(ACO.Instance.ExplorationAngle);
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
        return (transform.position - targetPosition).magnitude;
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
        return IsInRange(target, genome["ViewDistance"].Value) && Vector3.Angle(transform.forward, GetDirectionTo(target)) <= genome["ViewAngle"].Value / 2;
    }


    public bool Pickup(Transform obj)
    {
        if (obj != null)
        {
            CarriedObj = obj.gameObject;
            CarriedObj.GetComponent<Collider>().enabled = false;
            var rb = CarriedObj.GetComponent<Rigidbody>();
            rb.isKinematic = true;

            CarriedObj.transform.SetParent(gameObject.transform);
            CarriedObj.transform.position = transform.position + Vector3.up * CarryHeight;
            CarriedMass = rb.mass;

            // !! SPEED ADJUSTMENT !!   normal speed * carrrying multiplier (1 is normal speed, 0.5 is half speed, etc.)
            actualSpeed = ((genome["Speed"].Value / genome["Size"].Value) / Mathf.Max(1, CarriedMass / actualStrength * CarrySpeedModifier)); // Adjust speed based on carried mass

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
            // Place the carried object in front of the ant and reset its properties
            CarriedObj.transform.SetParent(null);
            CarriedObj.GetComponent<Rigidbody>().isKinematic = false;
            CarriedObj.GetComponent<Collider>().enabled = true;
            CarriedObj.transform.position = transform.position + transform.forward + Vector3.up * CarryHeight; // Place it infront and above the ant
            CarriedObj.transform.rotation = Quaternion.identity; // Reset rotation
            */

            // Destroy the carried object
            CarriedObj.transform.SetParent(null);
            Destroy(CarriedObj);
            CarriedObj = null;
            CarriedMass = 0f;

            // !! SPEED ADJUSTMENT !!
            actualSpeed = genome["Speed"].Value / genome["Size"].Value; // Reset speed to normal when not carrying anything
        }
    }

    public void ReduceEnergy(float amount)
    {
        currentEnergy -= amount; // Decrease energy by the specified amount
        if (currentEnergy < 0)
        {
            currentEnergy = 0; // Ensure energy doesn't go below zero
        }
        OnEnergyChanged?.Invoke(currentEnergy);
    }

    public void AddEnergy(float amount)
    {
        currentEnergy += amount; // Increase energy by the specified amount
        if (currentEnergy > maxEnergy)
        {
            currentEnergy = maxEnergy; // Cap energy at maximum
        }
        OnEnergyChanged?.Invoke(currentEnergy);
    }

    public bool IsHungry
    {
        get { return currentEnergy <= maxEnergy * genome["HungerThreshold"].Value; } // Check if the ant is hungry based on the hunger threshold defined in genome traits
    }
    public bool IsFull
    {
        get { return currentEnergy >= maxEnergy * genome["IsFullThreshold"].Value; }
    }

    public float GetEnergyCost()
    {
        // TODO: place this in a config file or somethin
        //TODO: view angle should raise energy cost, not lower it.
        float modifiers = 0.01f * genome["Size"].Value * genome["ViewAngle"].Value / 90f * genome["ViewDistance"].Value;


        return modifiers * actualSpeed // Base energy cost for moving
        * (IsCarrying ? (CarriedMass / actualStrength) : 1);// Energy cost multiplier for carrying food
    }

    public void CheckVitals()
    {
        if (currentEnergy == 0)
        {
            // Set on Hunger?
            OnDamageTaken?.Invoke(10);
        }
        else if (!IsHungry)
        {
            // Set on Hunger?
            OnHealed?.Invoke(1);
        }
        if (IsDead())
        {
            Kill();
        }
    }

    private void Heal(float value)
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += value;
            if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth; // Cap health at maximum
            }
        }
    }

    public void Kill()
    {
        // TODO: Visual indication, than die
        Debug.Log(this + " is dead!");
        Destroy(gameObject);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
    }

    public IEnumerable FlashColor(Color color, float duration)
    {
        if (meshRenderer != null)
        {
            Color prevColor = meshRenderer.material.color;
            meshRenderer.material.color = color;
            yield return new WaitForSeconds(duration);
            meshRenderer.material.color = prevColor;
        }
    }

    public bool IsDead()
    {
        return currentHealth <= 0; // Check if the ant is dead based on current health
    }


    public override string ToString()
    {
        return $"{Name}<{currentState}>-{GetDistanceTo(lastPosition)}";
    }

    public void Highlight(bool enable)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = enable ? highlightColor : baseColor;
        }
    }

    public void RaiseOnSelected()
    {
        OnSelected?.Invoke();
    }

    public void RaiseOnDeselected()
    {
        OnDeselected?.Invoke();
    }

    public void RaiseOnStateChanged()
    {
        OnStateChanged?.Invoke(currentState);
    }
    private void OnCollisionEnter(Collision collision)
    {
        currentState?.OnCollisionEnter(collision);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, ACO.Instance.DetectionDistance);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
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

    private void OnDestroy()
    {
        OnDeselected?.Invoke();
        OnDestroyed?.Invoke();
    }
}
