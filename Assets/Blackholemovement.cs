using UnityEngine;

public class BlackHoleMovement : MonoBehaviour
{
    public float baseSpeed = 3f; // Base movement speed
    public float sprintMultiplier = 2.0f; // Sprint speed multiplier
    public float verticalSpeed = 5f; // Speed for vertical movement
    public float mouseSensitivity = 2f; // Mouse sensitivity for rotation
    public float cameraDistance = 1.5f; // Initial distance of the camera from the black hole
    public float zoomSpeed = 2f; // Speed of zooming in and out
    public float minCameraDistance = 0.1f; // Minimum camera distance
    public float maxCameraDistance = 10f; // Maximum camera distance
    public float sprintZoomOutDistance = 1.0f; // Distance to zoom out when sprinting
    public float zoomOutSpeed = 1.0f; // Speed at which the camera zooms out when sprinting
    public float smoothZoomSpeed = 5f; // Smooth zoom transition speed

    private Camera mainCamera;
    private float rotationX = 0f;
    private float rotationY = 0f;
    private bool isSprinting = false; // Track if the player is sprinting
    private float manualZoomDistance; // Store the manual zoom distance
    private float currentCameraDistance; // Current camera distance being applied

    void Start()
    {
        mainCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked; // Lock cursor to center of the screen
        manualZoomDistance = cameraDistance; // Initialize manual zoom distance
        currentCameraDistance = manualZoomDistance; // Initialize current camera distance
    }

    void Update()
    {
        // Rotate the camera based on mouse movement
        RotateCamera();

        // Zoom in and out with the scroll wheel (Manual Zoom)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            manualZoomDistance -= scroll * zoomSpeed; // Adjust manual zoom distance based on scroll input
            manualZoomDistance = Mathf.Clamp(manualZoomDistance, minCameraDistance, maxCameraDistance); // Clamp distance
        }

        // Check for sprinting
        if (Input.GetKey(KeyCode.LeftShift))
        {
            isSprinting = true; // Set sprinting flag
        }
        else
        {
            isSprinting = false; // Reset sprinting flag
        }

        // Set the target camera distance
        float targetCameraDistance = manualZoomDistance;
        if (isSprinting)
        {
            targetCameraDistance += sprintZoomOutDistance; // Zoom out slightly when sprinting
        }

        // Smoothly transition to the target camera distance
        currentCameraDistance = Mathf.Lerp(currentCameraDistance, targetCameraDistance, smoothZoomSpeed * Time.deltaTime);

        // Ensure camera position is set
        mainCamera.transform.position = transform.position - mainCamera.transform.forward * currentCameraDistance;

        // Move the black hole based on keyboard input
        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;

        // Adjust forward vector for lateral movement
        right.y = 0; // Ignore vertical movement for lateral movement
        forward.y = 0; // Ignore vertical component for forward/backward movement
        forward.Normalize(); // Normalize the forward direction
        right.Normalize(); // Normalize the right direction

        Vector3 movement = (forward * Input.GetAxis("Vertical") + right * Input.GetAxis("Horizontal")).normalized;
        transform.position += movement * baseSpeed * Time.deltaTime;

        // Allow vertical movement based on camera angle
        if (Input.GetAxis("Vertical") != 0)
        {
            transform.position += Vector3.up * mainCamera.transform.forward.y * verticalSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
        }

        // Check for Ctrl key for downward movement
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            transform.position += Vector3.down * verticalSpeed * Time.deltaTime;
        }

        // Check for Space key for upward movement
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += Vector3.up * verticalSpeed * Time.deltaTime;
        }
    }

    void RotateCamera()
    {
        // Get mouse input
        rotationX += Input.GetAxis("Mouse X") * mouseSensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Clamp the vertical rotation to avoid flipping
        rotationY = Mathf.Clamp(rotationY, -45f, 45f); // Adjust the angle limit

        // Apply rotation to the camera
        mainCamera.transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0);
    }
}
