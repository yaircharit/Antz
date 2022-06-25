using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float mouse_senst = 2;
    [SerializeField] private float speed = 10;
    [SerializeField] private float sprint_modifier = 2;
    [SerializeField] private float max_speed = 50;
    [SerializeField] private bool paused = false;
    [SerializeField] private Texture2D curser;

    private Camera camera;
    private Vector2 midScreen;
    private Rigidbody rb;
    public event Action OnPause;

    
    private float Z_change, X_change, Y_change, MouseX,MouseY;
    private bool is_sprint;

    private Vector3 look_change, pos_change,store_v,temp;
    
    // Start is called before the first frame update
    void Start()
    {
        //Init variables
        camera = GetComponent<Camera>();
        rb = GetComponent<Rigidbody>();
        OnPause += Pause;
        
        // Set Player position to middle of map
        Vector2 new_pos = FindObjectOfType<MeshGenerator>().GetBoardSize() / 2;
        transform.position = new Vector3(new_pos.x,2,new_pos.y);

        // Set player limitations
        rb.maxAngularVelocity = max_speed;
        midScreen = new Vector2(camera.scaledPixelWidth/2, 0);
        print(midScreen);
        Cursor.SetCursor(curser, Vector2.zero, CursorMode.ForceSoftware);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

    }

    // Update is called once per frame
    void Update()
    {
        if ( Input.GetKeyDown(KeyCode.Escape) )
        {
            OnPause();
        }

        if ( !paused )
        {
            pos_change = new Vector3(Input.GetAxisRaw("HorizontalX"), Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("HorizontalZ"));
            look_change = new Vector3 (Input.GetAxisRaw("Mouse Y"), Input.GetAxisRaw("Mouse X"),0);
            is_sprint = Input.GetAxis("Sprint") != 0;
        }
    }

    // PhisycsUpdate
    private void FixedUpdate()
    {
        if ( !paused )
        {
            //print($"look: {look_change}");
            if ( look_change != Vector3.zero )
            {
                transform.eulerAngles += (look_change*mouse_senst);
            }
            //print($"pos: {pos_change}");
            if ( pos_change != Vector3.zero )
            {
                pos_change = pos_change.normalized*  speed * ((is_sprint) ? sprint_modifier : 1);
                transform.Translate(pos_change *Time.deltaTime,Space.Self);
            }
        }
    }
    
    private void Pause()
    {
        paused = !paused;

        // Lock mouse
        Cursor.lockState = (paused) ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = true;

        // Freeze Player
        rb.useGravity = !paused;
        
        temp = rb.velocity;
        rb.velocity = store_v;
        store_v = temp;
        
    }
}
