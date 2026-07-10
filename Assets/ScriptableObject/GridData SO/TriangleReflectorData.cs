using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Triangle Reflector", menuName = "Grid System/Cards/Triangle Reflector")]
public class TriangleReflectorData : GridData, ITrajectoryModifier, IEntityCollision
{
    public override List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        return new List<StatModifier>();
    }

    public bool IsSolidWall() => true;

    public void OnHitByEntity(GridEntity activeEntity, GridEntity stationaryEntity, Turret linkedTurret)
    {
        // Trajectory is handled before this by the movement manager checking ITrajectoryModifier
    }

    public Vector2Int GetRedirectedDirection(Vector2Int incomingDirection, Vector2Int itemFacingDirection)
    {
        // Example math for a 45-degree mirror reflection. 
        // You'll likely need to tweak this based on how your item rotation (itemFacingDirection) is structured.
        if (incomingDirection == Vector2Int.up) return Vector2Int.right;
        if (incomingDirection == Vector2Int.left) return Vector2Int.down;
        
        return -incomingDirection; // Default fallback
    }
}