using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControls : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float verticalSpeed = 3f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float sprintMultiplier = 2f;

    [Header("UI")]
    [SerializeField] private GameObject pauseMenuPrefab;
    private GameObject pauseMenuInstance;

    private Rigidbody rb;
    private Camera playerCamera;
    private float xRotation = 0f;
    private bool isPaused = false;

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
        LockCursor();
        SetupPauseMenu();
        SetPauseMenuActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                UnlockCursor();
                SetPauseMenuActive(true);
                Time.timeScale = 0f;
                isPaused = true;
            }
            else
            {
                LockCursor();
                SetPauseMenuActive(false);
                Time.timeScale = 1f;
                isPaused = false;
            }
        }

        if (isPaused) return;

        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        transform.Rotate(Vector3.up * mouseX);
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
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            currentMoveSpeed *= sprintMultiplier;
        }

        Vector3 move = (transform.forward * vertical + transform.right * horizontal).normalized * currentMoveSpeed * Time.fixedDeltaTime;
        Vector3 verticalMove = transform.up * upDown * verticalSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move + verticalMove);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
