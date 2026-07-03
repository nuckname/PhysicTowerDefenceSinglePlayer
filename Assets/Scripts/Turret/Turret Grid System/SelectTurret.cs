using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SelectTurret : MonoBehaviour
{
    [Header("UI Spawning")]
    [SerializeField] private GridUIManager _canvasPrefab; // Drag your new Canvas Prefab here
    private GridUIManager _activeCanvasInstance; // Keeps track of the currently open grid

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
                TurretGridManager clickedTurretsGrid = hit.collider.GetComponent<TurretGridManager>();

                if (clickedTurret != null && clickedTurretsGrid != null)
                {
                    Debug.Log($"Turret Selected: {clickedTurret.gameObject.name}");

                    // 1. If a grid is already open, destroy it so we don't overlap canvases
                    if (_activeCanvasInstance != null)
                    {
                        Destroy(_activeCanvasInstance.gameObject);
                    }

                    // 2. Spawn the new Canvas
                    _activeCanvasInstance = Instantiate(_canvasPrefab);

                    // 3. Tell the spawned canvas to generate and load the specific turret's data
                    _activeCanvasInstance.InitializeAndLoadGrid(clickedTurretsGrid.MyGridData);
                }
            }
            else 
            {
                // Optional: If you click the ground/empty space, close the active grid
                if (_activeCanvasInstance != null)
                {
                    Destroy(_activeCanvasInstance.gameObject);
                }
            }
        }
    }
}