using UnityEngine;

public class Ant : MonoBehaviour
{
    //----------LOCAL------------//
    /* current energy (affected by speed, holding, size, #hidden_neurons)
     * age
     * speed
     * location (rotation?)
     * holding
     */

    //--------GENETICS-----------//
    /* size
     * max speed
     * strength(?)
     * vision range(?)
     * offsprings count
     * offspring tax -> baby health / mutation chance modifier
     * offspring hatchtime
     * brain
     */


    //---------INPUTS-----------//
    /* coordinates 3
     * current energy level 1
     * current speed 1
     * current holding (None / block / food / ant) 1
     * neighboring area - blocks  / phromons  / ants / food 27
     * random 1
     */

    //---------OUTPUTS---------//
    /* Move x,y,z 3
     * pick/put/use(eat/attack) tool 1 
     * set pheromons 1
     * reproduce 1
     */

    public LayerMask ground;
    public LayerMask entities;
    private Rigidbody rb;

    [SerializeField] private int max_hidden_layers = 1;
    [SerializeField] private int max_hidden_neurons = 10;


    private NeuralNetwork brain;
    [SerializeField] private float vision_range;
    [SerializeField] private float speed;
    [SerializeField] private float rotate_speed;


    private Vector3 colony_pos;
    private Collider[] neighbors;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        int inputs_count = 14;
        int outputs_count = 6;

        colony_pos = gameObject.transform.position;
        brain = new NeuralNetwork(max_hidden_layers, max_hidden_neurons, inputs_count, outputs_count);
    }

    // Update is called once per frame
    void Update()
    {
        if ( gameObject.transform.position.y < -20 )
            Destroy(gameObject);


        // brain.Forward() ...

    }



    // PhisycsUpdate
    private void FixedUpdate()
    {



        neighbors = Physics.OverlapSphere(transform.position, vision_range, entities, QueryTriggerInteraction.Ignore);


        if ( neighbors.Length > 0 )
        {
            print(neighbors.Length);
            var dest = neighbors[0].transform.position;

            var dist = (dest - transform.position).magnitude;
            var dir = (dest - transform.position).normalized;

            transform.forward += (dir * rotate_speed * Time.deltaTime);

            if ( dist > 1 )
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
            }
        }
        RaycastHit hitInfo;

        // handle rotation to floor
        Ray ray = new Ray(transform.position, Vector3.down);
        if ( Physics.Raycast(ray, out hitInfo, 2, ground, QueryTriggerInteraction.Ignore) )
        {
            //Debug.DrawLine(ray.origin,hitInfo.point,Color.red);
            //transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, vision_range);

    }

    void OnMouseDown()
    {
        Debug.Log("YO");
    }
}
