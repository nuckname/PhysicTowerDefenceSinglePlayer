using UnityEngine;
using UnityEngine.InputSystem; // Added this namespace

public class PlanarCameraMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 15f;
    public float sprintMultiplier = 2.5f;

    void Update()
    {
        MoveCamera();
    }

    private void MoveCamera()
    {
        // Safety check to ensure a keyboard is actually connected
        if (Keyboard.current == null) return;

        // 1. Get raw input from WASD or Arrow Keys (Raw gives snappy, instant stops)
        float horizontal = 0f;
        float vertical = 0f;

        // Replicating GetAxisRaw manually for the new system
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
        
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;

        // 2. Get the camera's forward and right vectors
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // 3. Flatten the vectors to lock movement to the horizontal plane
        forward.y = 0f;
        right.y = 0f;

        // Normalize to ensure consistent speed regardless of where the camera is looking
        forward.Normalize();
        right.Normalize();

        // 4. Calculate final movement direction
        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;

        // 5. Optional: Hold Left Shift to move faster
        float currentSpeed = Keyboard.current.leftShiftKey.isPressed ? moveSpeed * sprintMultiplier : moveSpeed;

        // 6. Apply movement
        transform.position += moveDirection * currentSpeed * Time.deltaTime;
    }
}