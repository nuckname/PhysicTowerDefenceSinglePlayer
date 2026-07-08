using UnityEngine;

public interface IWallBouncer
{
    // Virtual method to handle wall bounces.
    // We pass in everything the card might need to calculate a cool effect.
    void OnWallBounce(Vector2Int bouncePos, TurretGridData gridData, GridUIManager uiManager, Turret linkedTurret);
}

public interface IRoundListener
{
    void OnRoundStart(GridPlacementManager placementManager, TurretGridData gridData, GridEntity sourceEntity);
    void OnRoundEnd(GridPlacementManager placementManager, TurretGridData gridData, GridEntity sourceEntity);
}

public interface IEntityCollision
{
    /// <summary>
    /// Defines if this entity acts as a solid wall that bouncers reflect off of.
    /// </summary>
    bool IsSolidWall();

    /// <summary>
    /// Triggered when an active entity (like a Bouncer) moves onto this entity's tile.
    /// </summary>
    void OnHitByEntity(GridEntity activeEntity, GridEntity stationaryEntity, Turret linkedTurret);
}