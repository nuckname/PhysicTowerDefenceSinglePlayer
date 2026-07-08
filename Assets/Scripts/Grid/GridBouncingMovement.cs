using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(GridEntity))]
public class GridBouncingMovement : MonoBehaviour
{
    [Header("Bounce Settings")]
    [Tooltip("How many seconds between each grid jump.")]
    public float MoveInterval = 0.5f; 
    
    private float _timer;
    private GridEntity _entity;
    private bool _isBouncing = false;
    
    // We pass these in when launching so it knows what it is interacting with
    private Turret _linkedTurret;
    private TurretGridData _gridData;

    /// <summary>
    /// Call this immediately after spawning the entity to start the bouncing loop.
    /// </summary>
    public void Launch(Turret linkedTurret, TurretGridData gridData)
    {
        _entity = GetComponent<GridEntity>();
        _linkedTurret = linkedTurret;
        _gridData = gridData;
        
        _isBouncing = true;
        _timer = MoveInterval;
    }

    private void Update()
    {
        if (!_isBouncing) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            StepForward();
            _timer = MoveInterval;
        }
    }

    private void StepForward()
    {
        Vector2Int currentPos = _entity.CurrentGridPosition;
        Vector2Int direction = _entity.CurrentDirection;
        GridUIManager uiManager = _entity.MyGridManager;

        int nextX = currentPos.x + direction.x;
        int nextY = currentPos.y + direction.y;

        bool hitWallX = false;
        bool hitWallY = false;
        bool cornerHit = false;

        if (direction.x != 0 && (nextX < 0 || nextX >= uiManager.GridWidth || IsTileSolid(new Vector2Int(nextX, currentPos.y))))
        {
            hitWallX = true;
            direction.x *= -1; 
        }

        if (direction.y != 0 && (nextY < 0 || nextY >= uiManager.GridHeight || IsTileSolid(new Vector2Int(currentPos.x, nextY))))
        {
            hitWallY = true;
            direction.y *= -1; 
        }

        if (!hitWallX && !hitWallY && direction.x != 0 && direction.y != 0 && IsTileSolid(new Vector2Int(nextX, nextY)))
        {
            cornerHit = true;
            direction.x *= -1;
            direction.y *= -1;
        }

        // Notify the Card Data
        // If we hit any wall or corner, tell the GridData so it can trigger its override effect!
        if (hitWallX || hitWallY || cornerHit)
        {
            if (_entity.MyCardData is IWallBouncer bouncerCard)
            {
                bouncerCard.OnWallBounce(currentPos, _gridData, uiManager, _linkedTurret);
            }
        }

        // Update Entity Data
        _entity.SetDirection(direction);
        Vector2Int newPos = currentPos + direction;
        _entity.SetGridPosition(newPos);

        Transform targetTile = uiManager.GetTileTransform(newPos);
        if (targetTile != null) transform.SetParent(targetTile, false);

        TriggerTileEffect();
    }

    /// <summary>
    /// Checks if a grid coordinate has an object on it, treating it as a wall to bounce off of.
    /// </summary>
    private bool IsTileSolid(Vector2Int pos)
    {
        // First check if another card or piece is physically occupying this tile on the board
        Tile tile = _entity.MyGridManager.GetTileAt(pos);
        if (tile != null && tile.IsOccupied)
        {
            return true;
        }

        // Then check the underlying grid data state for permanent walls or holes
        if (_gridData.TileStates.TryGetValue(pos, out int stateValue))
        {
            return stateValue != 0; // Assuming 0 is empty space
        }
        
        return false;
    }

    private void TriggerTileEffect()
    {
        // Ask the card data to calculate its modifiers for this single tile footprint
        List<StatModifier> hitModifiers = _entity.MyCardData.CalculateEffect(
            _entity.CurrentGridPosition, 
            _entity.CurrentDirection, 
            _gridData, 
            _entity.MyGridManager.GridWidth, 
            _entity.MyGridManager.GridHeight
        );

        if (hitModifiers != null && _linkedTurret != null)
        {
            // Send the modifiers directly to the turret.
            // Note: Because this happens instantly every tick, you'll want the Turret to process 
            // this as an "instant application" (like instantly granting +1 ammo or dealing 5 damage) 
            // rather than a permanent passive stat override.
            _linkedTurret.UpdateModifiers(hitModifiers); 
        }
    }
}