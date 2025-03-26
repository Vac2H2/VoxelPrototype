using Unity.Mathematics;
using UnityEngine;

public class ProtagonistControl : MonoBehaviour
{
    [Header("Movement Settings")]
    public float movementSpeed = 10f;      // Adjust movement speed in Inspector

    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;  // Adjust mouse sensitivity in Inspector

    float pitch = 0f; // Up/Down rotation
    float yaw = 0f;   // Left/Right rotation

    public Transform CameraTransform;
    public float3 velocity;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f); // Prevent flipping

        CameraTransform.eulerAngles = new Vector3(pitch, yaw, 0f);

        Vector3 forward = CameraTransform.forward;
        Vector3 right = CameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            moveDirection += forward;
        if (Input.GetKey(KeyCode.S))
            moveDirection -= forward;
        if (Input.GetKey(KeyCode.A))
            moveDirection -= right;
        if (Input.GetKey(KeyCode.D))
            moveDirection += right;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            moveDirection += Vector3.down;


        float3 newVelocity = moveDirection * movementSpeed;
        velocity = new float3(newVelocity.x, velocity.y, newVelocity.z);

        if (Input.GetKey(KeyCode.Space))
            velocity = new float3(0, 6f, 0);
    }
}
