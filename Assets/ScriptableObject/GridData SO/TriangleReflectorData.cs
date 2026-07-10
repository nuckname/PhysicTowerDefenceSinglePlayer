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
        // Map the 4 rotations of the triangle to the two types of 45-degree mirrors.
        
        if (itemFacingDirection == Vector2Int.up || itemFacingDirection == Vector2Int.down)
        {
            return new Vector2Int(incomingDirection.y, incomingDirection.x);
        }
        else 
        {
            return new Vector2Int(-incomingDirection.y, -incomingDirection.x);
        }
    }
}