using System.Collections.Generic;
using UnityEngine;

public class GridCombatLogic : MonoBehaviour
{
    [Header("Visual Dependencies")]
    [Tooltip("Create an empty UI object inside your grid to hold the laser beams, and drag it here.")]
    [SerializeField] private Transform _visualsParent; 
    
    // Assigned via initialization, NOT GetComponent
    private Turret _activeTurret;
    
    // Entity tracking
    private List<GridEntity> _activeEntities = new List<GridEntity>(); 
    private TurretGridData _currentGridData;
    
    private int _gridWidth;
    private int _gridHeight;

    // We completely remove Awake() since we don't want GetComponent here anymore.

    /// <summary>
    /// Call this from GridUIManager when opening the grid UI.
    /// Passes the active turret from the game world into the UI logic.
    /// </summary>
    public void InitializeGridLogic(TurretGridData gridData, int width, int height, Turret linkedTurret)
    {
        _currentGridData = gridData;
        _gridWidth = width;
        _gridHeight = height;
        _activeTurret = linkedTurret; // Inject the reference
    }

    // Called by the UI or Player Controller when a card is successfully dropped
    public void RegisterEntity(GridEntity newEntity)
    {
        if (!_activeEntities.Contains(newEntity))
        {
            _activeEntities.Add(newEntity);
            RecalculateBoard();
        }
    }

    public void RemoveEntity(GridEntity entityToRemove)
    {
        if (_activeEntities.Remove(entityToRemove))
        {
            RecalculateBoard();
        }
    }

    // This is the master function: Clears the board -> Does the Math -> Draws the Visuals
    public void RecalculateBoard()
    {
        // 1. Wipe old visuals so lasers don't stack infinitely
        ClearOldVisuals();

        List<StatModifier> allCalculatedModifiers = new List<StatModifier>();

        foreach (GridEntity entity in _activeEntities)
        {
            // 2. MATH: Calculate modifiers
            List<StatModifier> pieceModifiers = entity.MyCardData.CalculateEffect(
                entity.CurrentGridPosition, 
                entity.CurrentDirection, 
                _currentGridData, 
                _gridWidth, 
                _gridHeight
            );

            if (pieceModifiers != null)
            {
                // Aggregating all modifiers natively 
                allCalculatedModifiers.AddRange(pieceModifiers);
            }

            // 3. VISUALS: Spawn the dynamic beams/sprites
            if (_visualsParent != null)
            {
                entity.MyCardData.SpawnVisuals(
                    entity.CurrentGridPosition, 
                    entity.CurrentDirection, 
                    _currentGridData, 
                    _gridWidth, 
                    _gridHeight, 
                    _visualsParent
                );
            }
        }

        // 4. APPLY: Send the total accumulated board state to the game world Turret
        if (_activeTurret != null)
        {
            _activeTurret.UpdateModifiers(allCalculatedModifiers);
        }
        else
        {
            Debug.LogWarning("GridCombatLogic: No Turret is linked! Modifiers were calculated but not applied.");
        }
    }

    /// <summary>
    /// Destroys all currently spawned visual child objects.
    /// </summary>
    private void ClearOldVisuals()
    {
        if (_visualsParent == null) return;

        // Loop backwards when destroying children to avoid index shifting issues
        for (int i = _visualsParent.childCount - 1; i >= 0; i--)
        {
            Destroy(_visualsParent.GetChild(i).gameObject);
        }
    }
}