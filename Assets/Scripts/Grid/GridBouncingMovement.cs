using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(GridEntity))]
[RequireComponent(typeof(GridBouncerReflectHandler))] // Automatically adds your new script!
public class GridBouncingMovement : MonoBehaviour
{
    [Header("Bounce Settings")]
    [Tooltip("How many seconds between each grid jump.")]
    public float MoveInterval = 0.5f; 
    
    private float _timer;
    private GridEntity _entity;
    private GridBouncerReflectHandler _reflectHandler;
    private bool _isBouncing = false;
    
    private Turret _linkedTurret;
    private TurretGridData _gridData;

    public void Launch(Turret linkedTurret, TurretGridData gridData)
    {
        _entity = GetComponent<GridEntity>();
        _reflectHandler = GetComponent<GridBouncerReflectHandler>();
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
        Vector2Int currentDir = _entity.CurrentDirection;
        GridUIManager uiManager = _entity.MyGridManager;

        // Calculate where we WANT to go
        int targetX = currentPos.x + currentDir.x;
        int targetY = currentPos.y + currentDir.y;
        Vector2Int targetPos = new Vector2Int(targetX, targetY);

        bool hitWallX = false;
        bool hitWallY = false;
        bool cornerHit = false;
        
        Vector2Int nextDir = currentDir;
        Vector2Int newPos;

        // ==========================================
        // 1. EXTRACTED LOGIC CALL
        // Ask the new handler if we are entering a reflector
        // ==========================================
        bool enteringReflector = _reflectHandler.CheckForReflection(targetPos, currentDir, _gridData, out nextDir);

        // 2. Resolve Movement & Bouncing
        if (enteringReflector)
        {
            // Move INTO the triangle, and face the new direction ready for the next turn
            newPos = targetPos;
            
            // Notify the Bouncer Card of the "bounce" so you gain your damage buffs!
            if (_entity.MyCardData is IWallBouncer bouncerCard)
            {
                bouncerCard.OnWallBounce(targetPos, _gridData, uiManager, _linkedTurret);
            }
        }
        else
        {
            // Standard Wall Physics (Blocks, Boundaries, and flat backs of Triangles)
            if (currentDir.x != 0 && (targetX < 0 || targetX >= uiManager.GridWidth || IsTileSolid(new Vector2Int(targetX, currentPos.y))))
            {
                hitWallX = true;
                currentDir.x *= -1; 
            }

            if (currentDir.y != 0 && (targetY < 0 || targetY >= uiManager.GridHeight || IsTileSolid(new Vector2Int(currentPos.x, targetY))))
            {
                hitWallY = true;
                currentDir.y *= -1; 
            }

            if (!hitWallX && !hitWallY && currentDir.x != 0 && currentDir.y != 0 && IsTileSolid(targetPos))
            {
                cornerHit = true;
                currentDir.x *= -1;
                currentDir.y *= -1;
            }

            if (hitWallX || hitWallY || cornerHit)
            {
                if (_entity.MyCardData is IWallBouncer bouncerCard)
                {
                    bouncerCard.OnWallBounce(currentPos, _gridData, uiManager, _linkedTurret);
                }
            }
            
            nextDir = currentDir;
            newPos = currentPos + nextDir;
        }

        // Clamp safety: Prevents crashes if the player builds a trap that points out of bounds
        newPos.x = Mathf.Clamp(newPos.x, 0, uiManager.GridWidth - 1);
        newPos.y = Mathf.Clamp(newPos.y, 0, uiManager.GridHeight - 1);

        // 3. Apply state
        _entity.SetDirection(nextDir);
        _entity.SetGridPosition(newPos);

        Transform targetTileUI = uiManager.GetTileTransform(newPos);
        if (targetTileUI != null) transform.SetParent(targetTileUI, false);

        if (CheckCollision(uiManager, newPos)) return;

        TriggerTileEffect();
    }

    private bool CheckCollision(GridUIManager uiManager, Vector2Int newPos)
    {
        Tile landedTile = uiManager.GetTileAt(newPos);
        if (landedTile != null && landedTile.IsOccupied && landedTile.OccupyingEntity != null)
        {
            if (landedTile.OccupyingEntity.MyCardData is IEntityCollision collidable)
            {
                collidable.OnHitByEntity(_entity, landedTile.OccupyingEntity, _linkedTurret);
                if (this == null || gameObject == null) return true;
            }
        }
        return false;
    }

    private bool IsTileSolid(Vector2Int pos)
    {
        Tile tile = _entity.MyGridManager.GetTileAt(pos);
        if (tile != null && tile.IsOccupied)
        {
            if (tile.OccupyingEntity != null && tile.OccupyingEntity.MyCardData is IEntityCollision collidable)
            {
                return collidable.IsSolidWall(); 
            }
            return true;
        }

        if (_gridData.TileStates.TryGetValue(pos, out int stateValue))
        {
            return stateValue != 0; 
        }
        
        return false;
    }

    private void TriggerTileEffect()
    {
        List<StatModifier> hitModifiers = _entity.MyCardData.CalculateEffect(
            _entity.CurrentGridPosition, 
            _entity.CurrentDirection, 
            _gridData, 
            _entity.MyGridManager.GridWidth, 
            _entity.MyGridManager.GridHeight
        );

        if (hitModifiers != null && _linkedTurret != null)
        {
            _linkedTurret.UpdateModifiers(hitModifiers); 
        }
    }
}