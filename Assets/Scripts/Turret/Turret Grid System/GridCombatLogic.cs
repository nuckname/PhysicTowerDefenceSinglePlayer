using System.Collections.Generic;
using UnityEngine;

public class GridCombatLogic : MonoBehaviour
{
    [Header("Cooldown Settings")]
    public float timePerTick = 1.0f;
    private float _tickTimer = 0f;
    private bool _isRoundActive = false; 

    private GridUIManager _uiManager;
    private GridPlacementManager _placementManager; 
    private Turret _activeTurret;
    
     public List<GridEntity> ActiveEntities => _activeEntities;
    
     [SerializeField] private List<GridEntity> _activeEntities = new List<GridEntity>(); 
    private TurretGridData _currentGridData;
    
    private List<GameObject> _spawnedVisuals = new List<GameObject>();

    /// <summary>
    /// Call this from GridUIManager when opening the grid UI.
    /// Passes the active turret from the game world into the UI logic.
    /// </summary>
    public void InitializeGridLogic(TurretGridData gridData, GridUIManager manager, Turret linkedTurret, GridPlacementManager placementManager) 
    {
        _currentGridData = gridData;
        _uiManager = manager; 
        _activeTurret = linkedTurret; 
        
        // Save the reference so the interface hooks can use it!
        _placementManager = placementManager; 
    }

    private void Update()
    {
        if (_isRoundActive)
        {
            _tickTimer += Time.deltaTime;

            if (_tickTimer >= timePerTick)
            {
                // 1 Second has passed! Tick everything down.
                Debug.unityLogger.Log("Tick");
                TickGridCooldowns();
                
                // Reset the timer
                _tickTimer -= timePerTick; 
            }
        }
    }

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

    public void RecalculateBoard()
    {
        // 1. CLEANUP: Instantly remove any entities that were destroyed 
        _activeEntities.RemoveAll(item => item == null);

        ClearOldVisuals();

        List<StatModifier> allCalculatedModifiers = new List<StatModifier>();
        if (_currentGridData != null) _currentGridData.SavedCards.Clear();
        
        // ===============================================
        // LOOP 1: "Take a picture of the board"
        // We MUST save every card to memory before doing ANY math!
        // ===============================================
        foreach (GridEntity entity in _activeEntities)
        {
            if (_currentGridData != null)
            {
                _currentGridData.SavedCards.Add(new PlacedCardSaveState(
                    entity.MyCardData, entity.CurrentGridPosition, entity.CurrentDirection
                ));
            }
        }

        // ===============================================
        // LOOP 2: "Calculate Math and Draw Graphics"
        // Now when the Laser asks "is there a Triangle here?", the answer is Yes!
        // ===============================================
        foreach (GridEntity entity in _activeEntities)
        {
            List<StatModifier> pieceModifiers = entity.MyCardData.CalculateEffect(
                entity.CurrentGridPosition, 
                entity.CurrentDirection, 
                _currentGridData, 
                _uiManager.GridWidth, 
                _uiManager.GridHeight
            );

            if (pieceModifiers != null) allCalculatedModifiers.AddRange(pieceModifiers);

            entity.MyCardData.SpawnVisuals(
                entity.CurrentGridPosition, 
                entity.CurrentDirection, 
                _currentGridData, 
                _uiManager,         
                _spawnedVisuals     
            );
        }

        if (_activeTurret != null) _activeTurret.UpdateModifiers(allCalculatedModifiers);
    }
    
    private void ClearOldVisuals()
    {
        foreach (GameObject visualSegment in _spawnedVisuals)
        {
            if (visualSegment != null) Destroy(visualSegment);
        }
        _spawnedVisuals.Clear();
    }

    // ==========================================
    // --- ROUND EVENTS & INTERFACE DIRECTORS ---
    // ==========================================

    public void NotifyRoundStart()
    {
        // Turn on the Update() timer!
        _isRoundActive = true;
        _tickTimer = 0f; 

        List<GridEntity> currentEntities = new List<GridEntity>(_activeEntities);
        foreach (var entity in currentEntities)
        {
            if (entity != null && entity.MyCardData is IRoundListener roundListener)
            {
                roundListener.OnRoundStart(_placementManager, _currentGridData, entity);
            }
        }
    }

    public void NotifyRoundEnd()
    {
        // Pause the Update() timer!
        _isRoundActive = false;

        List<GridEntity> currentEntities = new List<GridEntity>(_activeEntities);
        foreach (var entity in currentEntities)
        {
            if (entity != null && entity.MyCardData is IRoundListener roundListener)
            {
                roundListener.OnRoundEnd(_placementManager, _currentGridData, entity);
            }
        }
    }

    public void TickGridCooldowns()
    {
        for (int i = _activeEntities.Count - 1; i >= 0; i--)
        {
            var entity = _activeEntities[i];
            if (entity != null)
            {
                entity.TickCooldown(_currentGridData, _placementManager);
            }
        }
    }

    public void NotifyEnemyKilled()
    {
        foreach (var entity in _activeEntities)
        {
            if (entity != null && entity.MyCardData is IEnemyDeathListener deathListener)
            {
                deathListener.OnEnemyKilled(_activeTurret);
            }
        }
    }

    public GridEntity GetSolidEntityAt(Vector2Int gridPosition)
    {
        foreach (var entity in _activeEntities)
        {
            if (entity.CurrentGridPosition == gridPosition && entity.MyCardData is IEntityCollision collisionData)
            {
                if (collisionData.IsSolidWall()) return entity;
            }
        }
        return null;
    }
}