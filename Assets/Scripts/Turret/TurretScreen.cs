using UnityEngine;

public class TurretScreen : MonoBehaviour
{
    [Header("UI Spawning")]
    [SerializeField] private GridUIManager _canvasPrefab; 
    
    // We keep a permanent reference to this turret's specific grid
    private GridUIManager _myGridInstance; 

    private TurretGridManager _myGridManager;
    private Turret _myTurret;

    private void Awake()
    {
        // Cache the grid manager attached to this specific turret
        _myGridManager = GetComponent<TurretGridManager>();
        _myTurret = GetComponent<Turret>();
    }

    private void Start()
    {
        // 1. Spawn the UI as soon as the Turret is built
        if (_canvasPrefab != null && _myGridManager != null && _myTurret != null)
        {
            _myGridInstance = Instantiate(_canvasPrefab);
            
            // 2. Initialize the background logic
            _myGridInstance.InitializeGrid(_myGridManager.MyGridData, _myTurret);
            
            // 3. Hide it immediately so it runs invisibly until the player clicks on it
            _myGridInstance.SetVisualsActive(false); 
        }
        else
        {
            Debug.LogError($"Required scripts or prefab missing on {gameObject.name}!");
        }
    }

    public void OpenScreen()
    {
        if (_myGridInstance != null)
        {
            // Turn the Canvas ON and load any pending cards into the UI hand
            _myGridInstance.SetVisualsActive(true, _myTurret.PendingCards);
        }
    }

    public void CloseScreen()
    {
        if (_myGridInstance != null)
        {
            // Turn the Canvas OFF (logic and bouncers keep running in the background!)
            _myGridInstance.SetVisualsActive(false);
        }
    }
}