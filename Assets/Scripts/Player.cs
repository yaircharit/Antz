using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 30f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float minPitch = 10f;   // how far down camera can tilt
    [SerializeField] private float maxPitch = 80f;   // how far up camera can tilt

    [Header("UI")]
    [SerializeField] private GameObject pauseMenuPrefab;
    private GameObject pauseMenuInstance;

    private Rigidbody rb;
    private Camera playerCamera;
    private bool isPaused = false;

    private bool isDragging = false;
    private Vector3 lastMousePosition;
    private float yaw = 0f;
    private float pitch = 45f; // default downward tilt

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false; // Floating
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.mass = 1f;
        rb.drag = 2f;

        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            Debug.LogError("No Camera found as child of player for first person view.");
        }

        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;

        SetupPauseMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                SetPauseMenuActive(true);
                Time.timeScale = 0f;
                isPaused = true;
            }
            else
            {
                SetPauseMenuActive(false);
                Time.timeScale = 1f;
                isPaused = false;
            }
        }

        if (isPaused) return;

        // Start drag with right click (or middle click)
        if (Input.GetMouseButtonDown(1))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;

            yaw += delta.x * mouseSensitivity * Time.deltaTime;
            pitch -= delta.y * mouseSensitivity * Time.deltaTime;

            // Clamp vertical rotation
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

            lastMousePosition = Input.mousePosition;
        }

        // Clear ant selection if clicking on something other than an ant
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Ant ant = hit.collider.GetComponentInParent<Ant>();
                if (ant != null )
                {
                    return; // Clicked on an ant, do nothing
                }
            }
            if (Ant.SelectedAnt != null)
            {
                Ant.SelectedAnt.RaiseOnDeselected();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isPaused) return;
        // WASD movement (floating)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        float upDown = 0f;
        if (Input.GetKey(KeyCode.Space)) upDown += 1f;
        if (Input.GetKey(KeyCode.LeftShift)) upDown -= 1f;

        // Sprinting
        float currentMoveSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentMoveSpeed *= sprintMultiplier;
        }

        Vector3 move = currentMoveSpeed * Time.fixedDeltaTime * (transform.forward * vertical + transform.right * horizontal).normalized;
        Vector3 verticalMove = currentMoveSpeed * Time.fixedDeltaTime * upDown * transform.up;
        rb.MovePosition(rb.position + move + verticalMove);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    private void SetupPauseMenu()
    {
        if (pauseMenuPrefab != null && pauseMenuInstance == null)
        {
            pauseMenuInstance = Instantiate(pauseMenuPrefab);
            pauseMenuInstance.SetActive(false);
        }
    }

    private void SetPauseMenuActive(bool active)
    {
        if (pauseMenuInstance != null)
        {
            pauseMenuInstance.SetActive(active);
        }
    }
}
