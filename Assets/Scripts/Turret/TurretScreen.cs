using UnityEngine;

public class TurretScreen : MonoBehaviour
{
    [Header("UI Spawning")]
    [SerializeField] private GridUIManager _canvasPrefab; // Drag your new Canvas Prefab here
    private GridUIManager _activeCanvasInstance; // Keeps track of the currently open grid

    private TurretGridManager _myGridManager;
    private Turret _myTurret;

    private void Awake()
    {
        // Cache the grid manager attached to this specific turret
        _myGridManager = GetComponent<TurretGridManager>();
        _myTurret = GetComponent<Turret>();
    }

    public void OpenScreen()
    {
        // Prevent spawning multiple canvases if this turret is already open
        if (_activeCanvasInstance != null) return;

        // 2. Spawn the new Canvas
        _activeCanvasInstance = Instantiate(_canvasPrefab);

        // 3. Tell the spawned canvas to generate and load the specific turret's data
        if (_myGridManager != null && _myTurret != null)
        {
            _activeCanvasInstance.InitializeAndLoadGrid(_myGridManager.MyGridData, _myTurret.PendingCards, _myTurret);
        }
        else
        {
            Debug.LogError($"Required scripts missing on {gameObject.name}!");
        }
    }

    public void CloseScreen()
    {
        // 1. If a grid is already open, destroy it so we don't overlap canvases
        if (_activeCanvasInstance != null)
        {
            Destroy(_activeCanvasInstance.gameObject);
            _activeCanvasInstance = null; // Clear the reference
        }
    }
}