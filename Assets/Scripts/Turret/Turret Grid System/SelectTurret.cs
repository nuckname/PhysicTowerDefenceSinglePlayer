using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SelectTurret : MonoBehaviour
{
    private Camera _mainCamera;
    
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
                Turret clickedTurret = hit.collider.GetComponent<Turret>();

                if (clickedTurret != null)
                {
                    Debug.Log($"Turret Selected: {clickedTurret.gameObject.name}");
                    TurretGridManager clickedTurretsGrid = clickedTurret.GetComponent<TurretGridManager>();
                    
                    if (GridUIManager.Instance != null)
                    {
                        GridUIManager.Instance.LoadTurretGrid(clickedTurretsGrid.MyGridData);
                    }
                }
            }
        }
    }
}