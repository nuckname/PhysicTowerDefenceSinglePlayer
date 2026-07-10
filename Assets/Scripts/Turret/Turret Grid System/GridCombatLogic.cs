using System.Collections.Generic;
using UnityEngine;

public class GridCombatLogic : MonoBehaviour
{
    private GridUIManager _uiManager;
    
    // Assigned via initialization, NOT GetComponent
    private Turret _activeTurret;
    
    public List<GridEntity> ActiveEntities => _activeEntities;
    
    // Entity tracking
    private List<GridEntity> _activeEntities = new List<GridEntity>(); 
    private TurretGridData _currentGridData;
    
    private List<GameObject> _spawnedVisuals = new List<GameObject>();

    private bool _isInitialized = false;
    
    // We completely remove Awake() since we don't want GetComponent here anymore.

    /// <summary>
    /// Call this from GridUIManager when opening the grid UI.
    /// Passes the active turret from the game world into the UI logic.
    /// </summary>
    public void InitializeGridLogic(TurretGridData gridData, GridUIManager manager, Turret linkedTurret) // UPDATED PARAMETERS
    {
        if (_isInitialized) return;
        
        _isInitialized = true;
        
        _currentGridData = gridData;
        _uiManager = manager; // Now we save the manager reference directly
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
        ClearOldVisuals();

        List<StatModifier> allCalculatedModifiers = new List<StatModifier>();
        if (_currentGridData != null) _currentGridData.SavedCards.Clear();
        
        foreach (GridEntity entity in _activeEntities)
        {
            if (_currentGridData != null)
            {
                _currentGridData.SavedCards.Add(new PlacedCardSaveState(
                    entity.MyCardData, entity.CurrentGridPosition, entity.CurrentDirection
                ));
            }
            
            // 2. MATH: Ask the UIManager for the grid dimensions
            List<StatModifier> pieceModifiers = entity.MyCardData.CalculateEffect(
                entity.CurrentGridPosition, 
                entity.CurrentDirection, 
                _currentGridData, 
                _uiManager.GridWidth, 
                _uiManager.GridHeight
            );

            if (pieceModifiers != null) allCalculatedModifiers.AddRange(pieceModifiers);

            // 3. VISUALS: Pass the UIManager and our Tracking List
            entity.MyCardData.SpawnVisuals(
                entity.CurrentGridPosition, 
                entity.CurrentDirection, 
                _currentGridData, 
                _uiManager,         // Pass the manager so the card can request tile transforms
                _spawnedVisuals     // Pass the list so the card can log what it spawned
            );
        }

        if (_activeTurret != null) _activeTurret.UpdateModifiers(allCalculatedModifiers);
    }

    /// <summary>
    /// Destroys all currently spawned visual child objects.
    /// </summary>
    private void ClearOldVisuals()
    {
        // Destroy all tracked visual segments
        foreach (GameObject visualSegment in _spawnedVisuals)
        {
            if (visualSegment != null) Destroy(visualSegment);
        }
        _spawnedVisuals.Clear();
    }
}