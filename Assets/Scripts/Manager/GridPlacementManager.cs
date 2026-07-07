using UnityEngine;
using System.Collections.Generic;

public class GridPlacementManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GridUIManager _uiManager;
    [SerializeField] private GridCombatLogic _combatLogic;
    
    [Header("Prefabs")]
    public GridEntity DefaultEntityPrefab; // Moved from UIManager
    public GridEntity BouncingPrefab;      // Moved from UIManager

    private Turret _linkedTurret;

    public void Initialize(Turret linkedTurret)
    {
        _linkedTurret = linkedTurret;
    }

    /// <summary>
    /// Base method: Spawns ANY entity (Cards, Lasers, Bouncers) onto a tile.
    /// </summary>
    public GridEntity SpawnEntityOnTile(GridEntity prefabToSpawn, GridData cardData, Vector2Int gridPosition, bool occupyTile = false)
    {
        Tile targetTile = _uiManager.GetTileAt(gridPosition);
        if (targetTile == null) return null;

        GridEntity newEntity = Instantiate(prefabToSpawn, targetTile.transform);
        newEntity.Initialize(cardData, gridPosition, _uiManager);

        if (occupyTile)
        {
            targetTile.SetOccupied(true);
        }

        return newEntity;
    }

    /// <summary>
    /// Specific method: Dropping a card from the UI Hand onto the board.
    /// </summary>
    public bool TryPlaceCardFromHand(GridData cardData, Vector2Int gridPosition)
    {
        Tile targetTile = _uiManager.GetTileAt(gridPosition);
        
        // 1. Validate the drop
        if (targetTile == null || targetTile.IsOccupied) return false;

        // 2. Spawn and occupy
        GridEntity newCardEntity = SpawnEntityOnTile(DefaultEntityPrefab, cardData, gridPosition, true);
        
        // 3. Update logic
        if (newCardEntity != null && _combatLogic != null)
        {
            _linkedTurret.PendingCards.Remove(cardData);
            _combatLogic.RegisterEntity(newCardEntity);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Specific method: Dragging an existing piece across the board.
    /// </summary>
    public bool TryMoveExistingEntity(GridEntity entityToMove, Vector2Int newPosition)
    {
        Tile targetTile = _uiManager.GetTileAt(newPosition);
        
        // 1. Validate the move
        if (targetTile == null || targetTile.IsOccupied) return false;

        // 2. Free up the OLD tile
        Tile oldTile = _uiManager.GetTileAt(entityToMove.CurrentGridPosition);
        if (oldTile != null) oldTile.SetOccupied(false);

        // 3. Occupy the NEW tile
        targetTile.SetOccupied(true);

        // 4. Update the entity's data and visual parent
        entityToMove.SetGridPosition(newPosition);
        entityToMove.transform.SetParent(targetTile.transform, false);

        // 5. Tell the logic to recalculate
        _combatLogic.RecalculateBoard();

        return true;
    }

    /// <summary>
    /// Specific method: Spawning a bouncing orb.
    /// </summary>
    public GridEntity SpawnBouncingItem(GridData itemData, Vector2Int startPos, TurretGridData currentGridData)
    {
        if (BouncingPrefab == null) return null;

        GridEntity bouncer = SpawnEntityOnTile(BouncingPrefab, itemData, startPos, false); // Bouncers don't occupy!
        
        if (bouncer != null && bouncer.TryGetComponent(out GridBouncingMovement bounceScript))
        {
            bounceScript.Launch(_linkedTurret, currentGridData);
        }

        return bouncer;
    }
}