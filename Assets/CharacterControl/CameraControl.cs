using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float movementSpeed = 10f;      // Adjust movement speed in Inspector

    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;  // Adjust mouse sensitivity in Inspector

    private float pitch = 0f; // Up/Down rotation
    private float yaw = 0f;   // Left/Right rotation

    void Start()
    {
        // Lock the cursor to the game window
        Cursor.lockState = CursorLockMode.Locked;
        
        // Initialize rotation values based on current rotation
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        // --- Mouse Look ---
        // Read mouse inputs
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f); // Prevent flipping

        // Apply rotation
        transform.eulerAngles = new Vector3(pitch, yaw, 0f);

        // --- Movement ---
        // Calculate forward and right vectors for horizontal movement (ignore vertical component)
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = Vector3.zero;

        // WASD movement on the XZ plane
        if (Input.GetKey(KeyCode.W))
            moveDirection += forward;
        if (Input.GetKey(KeyCode.S))
            moveDirection -= forward;
        if (Input.GetKey(KeyCode.A))
            moveDirection -= right;
        if (Input.GetKey(KeyCode.D))
            moveDirection += right;

        // Vertical movement
        if (Input.GetKey(KeyCode.Space))
            moveDirection += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            moveDirection += Vector3.down;

        // Move the camera
        transform.position += moveDirection * movementSpeed * Time.deltaTime;
    }
}

