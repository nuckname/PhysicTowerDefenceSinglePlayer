using UnityEngine;

public class GridPlacementManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GridEntity DefaultEntityPrefab; 
    public GridEntity BouncingPrefab;      

    // Cached references
    private GridUIManager _uiManager;
    private GridCombatLogic _combatLogic;
    private Turret _linkedTurret;

    // Called by GridUIManager when the UI opens
    public void Initialize(Turret linkedTurret, GridUIManager uiManager, GridCombatLogic combatLogic)
    {
        _linkedTurret = linkedTurret;
        _uiManager = uiManager;
        _combatLogic = combatLogic;
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
        
        if (targetTile == null || targetTile.IsOccupied) return false;

        GridEntity newCardEntity = SpawnEntityOnTile(DefaultEntityPrefab, cardData, gridPosition, true);
        
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
        
        if (targetTile == null || targetTile.IsOccupied) return false;

        Tile oldTile = _uiManager.GetTileAt(entityToMove.CurrentGridPosition);
        if (oldTile != null) oldTile.SetOccupied(false);

        targetTile.SetOccupied(true);

        entityToMove.SetGridPosition(newPosition);
        entityToMove.transform.SetParent(targetTile.transform, false);

        _combatLogic.RecalculateBoard();

        return true;
    }

    /// <summary>
    /// Specific method: Spawning a bouncing orb.
    /// </summary>
    public GridEntity SpawnBouncingItem(GridData itemData, Vector2Int startPos, TurretGridData currentGridData)
    {
        if (BouncingPrefab == null) return null;

        GridEntity bouncer = SpawnEntityOnTile(BouncingPrefab, itemData, startPos, false); 
        
        if (bouncer != null && bouncer.TryGetComponent(out GridBouncingMovement bounceScript))
        {
            bounceScript.Launch(_linkedTurret, currentGridData);
            _uiManager.TrackBouncer(bouncer); // Hand the bouncer over to the UI manager for destruction at round end
        }

        return bouncer;
    }
}