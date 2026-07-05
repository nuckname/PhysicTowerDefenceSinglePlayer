using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SelectTurret : MonoBehaviour
{
    private Camera _mainCamera;
    
    // Keeps track of the currently selected turret so we can close its screen
    private TurretScreen _currentlySelectedTurret;
    
    private void Awake()
    {
        _mainCamera = FindAnyObjectByType<Camera>(); // Note: Camera.main is usually slightly faster if the camera is tagged "MainCamera"
    }

    void Update()
    {
        // Changed to wasPressedThisFrame so we only raycast exactly once per click, not every frame it's held
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Prevent clicking through UI elements (like the Canvas grid itself)
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return; 
            }

            PerformRaycast();
        }
    }

    private void PerformRaycast()
    {
        // Read the mouse position using the new Input System
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.collider.CompareTag("Turret"))
            {
                // Grab the new script attached to the Turret
                TurretScreen clickedTurretScreen = hit.collider.GetComponent<TurretScreen>();

                if (clickedTurretScreen != null)
                {
                    Debug.Log($"Turret Selected: {hit.collider.gameObject.name}");

                    // 1. If a different turret's grid is already open, close it so we don't overlap canvases
                    if (_currentlySelectedTurret != null && _currentlySelectedTurret != clickedTurretScreen)
                    {
                        _currentlySelectedTurret.CloseScreen();
                    }

                    // Set the newly clicked turret as the active one and open it
                    _currentlySelectedTurret = clickedTurretScreen;
                    _currentlySelectedTurret.OpenScreen();
                }
            }
            else 
            {
                // Optional: If you click the ground/empty space, close the active grid
                if (_currentlySelectedTurret != null)
                {
                    _currentlySelectedTurret.CloseScreen();
                    _currentlySelectedTurret = null; // Clear the selection
                }
            }
        }
    }
}