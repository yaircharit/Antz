using System.Collections;
using UnityEngine;

public abstract class MovingEntity : MonoBehaviour
{
    public virtual string Name { get; set; } = "MovingEntity"; // Name of the ant, can be used for identification
    public int ID { get; protected set; } = -1; // Unique ID for the ant, can be used for identification
    public static int Count { get; protected set; } = 0; // Static counter to keep track of the number of ants

    //[Header("Basic Settings")]
    public float CarryHeight { get; protected set; } = 0.75f;
    public float CarrySpeedModifier { get; protected set; } = 0.5f;
    public bool IsCarrying => CarriedObj != null;


    public float CurrentSpeed { get; protected set; }
    public float CurrentHealth { get; protected set; } // Current health of the ant, can be used for health management
    public float CurrentEnergy { get; protected set; } // Current energy of the ant, can be used for energy management

    public Genome genome { get; protected set; } // Genome of the ant, can be used for genetic algorithms or traits
    public float MaxHealth { get { return genome.MaxHealth; } }
    public float MaxEnergy { get { return genome.MaxEnergy; } }
    public GameObject CarriedObj { get; protected set; }
    public float CarriedMass { get; protected set; } = 0f; // Mass of the carried object, used to calculate speed when carrying food

    public bool isSelected = false;

    public bool isStuck = false; // Flag to indicate if the ant is stuck


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

    public BaseState currentState;
    protected Vector3 lastPosition;

    protected MeshRenderer meshRenderer;
    //TODO: Move these to a config file or somethin, ScriptableObject maybe?
    public static Color baseColor = Color.black;
    public static Color highlightColor = Color.yellow;
    public static Color damageColor = Color.red;
    public static float damageFlashDuration = 0.5f;
    public static Color healColor = Color.green;
    public static float healFlashDuration = 0.5f;
    private Coroutine flashingCoroutine = null;

    public event System.Action OnSelected;
    public event System.Action OnDeselected;
    public event System.Action OnKilled;

    public event System.Action<float> OnDamageTaken;
    public event System.Action<float> OnHealed;
    public event System.Action<float> OnEnergyChanged;
    public event System.Action<BaseState> OnStateChanged;


    public virtual void Init(Genome genes)
    {
        ID = Count++; // Increment the static ant count
        Name = $"{Name}_{ID}"; // Set the name based on the ID

        OnDamageTaken += (_) =>
        {
            flashingCoroutine ??= StartCoroutine(FlashColor(damageColor, damageFlashDuration));
        };
        OnDamageTaken += TakeDamage;

        OnHealed += (_) =>
        {
            flashingCoroutine ??= StartCoroutine(FlashColor(healColor, healFlashDuration));
        };
        OnHealed += Heal;

        OnKilled += Kill;

        genome = genes;

        transform.localScale = Vector3.one * genome["Size"].Value; // Set the scale of the ant based on size trait
        CurrentSpeed = genome.EffectiveSpeed; // Initialize current speed based on genome
        CurrentHealth = genome.MaxHealth; // Initialize current health
        CurrentEnergy = genome.MaxEnergy;

        transform.rotation = Quaternion.Euler(0, Random.value * 360f, 0); // Randomize initial rotation
        lastPosition = transform.position; // Store the initial position as last position
    }

    public void ChangeState(BaseState newState)
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
        return GetRandomDirection(direction, genome["ACO_ExplorationAngle"].Value);
    }

    public Vector3 GetRandomDirection()
    {
        return GetRandomDirection(genome["ACO_ExplorationAngle"].Value);
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

            // !! SPEED ADJUSTMENT !!   normal speed / carrrying multiplier (1 is normal speed, 0.5 is half speed, etc.)
            CurrentSpeed = (genome.EffectiveSpeed) / Mathf.Max(1, CarriedMass / genome.EffectiveStrength * CarrySpeedModifier); // Adjust speed based on carried mass

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
            CurrentSpeed = genome.EffectiveSpeed; // Reset speed to normal when not carrying anything
        }
    }

    public void ReduceEnergy(float amount)
    {
        CurrentEnergy -= amount; // Decrease energy by the specified amount
        if (CurrentEnergy < 0)
        {
            CurrentEnergy = 0; // Ensure energy doesn't go below zero
            OnDamageTaken?.Invoke(amount); // Reduce health based on requried energy
        }
        OnEnergyChanged?.Invoke(CurrentEnergy);
    }

    public void AddEnergy(float amount)
    {
        CurrentEnergy += amount; // Increase energy by the specified amount
        if (CurrentEnergy > MaxEnergy)
        {
            CurrentEnergy = MaxEnergy; // Cap energy at maximum
        }
        OnEnergyChanged?.Invoke(CurrentEnergy);
    }

    public bool IsHungry
    {
        get { return CurrentEnergy <= MaxEnergy * genome["HungerThreshold"].Value; } // Check if the ant is hungry based on the hunger threshold defined in genome traits
    }
    public bool IsFull
    {
        get { return CurrentEnergy >= MaxEnergy * genome["IsFullThreshold"].Value; }
    }

    public float GetEnergyCost()
    {
        return genome.GetEnergyCost() * currentState.GetEnergyModifier();
    }


    private void Heal(float value)
    {
        CurrentHealth += value;
        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth; // Cap health at maximum
        }
    }

    public void Kill()
    {
        // TODO: Visual indication, than die
        OnDeselected?.Invoke();
        Debug.Log(this + " is dead!");

        Destroy(gameObject);
    }

    public void TakeDamage(float amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            OnKilled?.Invoke();
        }
    }

    public IEnumerator FlashColor(Color color, float duration)
    {
        if (flashingCoroutine != null)
            yield return null;

        Color prevColor = meshRenderer.material.color;
        meshRenderer.material.color = color;
        yield return new WaitForSeconds(duration);
        meshRenderer.material.color = prevColor;
        yield return new WaitForSeconds(duration);
        flashingCoroutine = null;
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

    public void RaiseOnHealed()
    {
        OnHealed?.Invoke(genome.HealingRate);
    }

    private void OnCollisionEnter(Collision collision)
    {
        currentState?.OnCollisionEnter(collision);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, genome["ACO_PheromoneDetectionDistance"].Value);
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

}
