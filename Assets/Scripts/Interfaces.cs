using UnityEngine;

public interface IWallBouncer
{
    // Virtual method to handle wall bounces.
    // We pass in everything the card might need to calculate a cool effect.
    void OnWallBounce(Vector2Int bouncePos, TurretGridData gridData, GridUIManager uiManager, Turret linkedTurret);
}

public interface IRoundListener
{
    void OnRoundStart(GridUIManager uiManager, GridEntity sourceEntity);
    void OnRoundEnd(GridUIManager uiManager, GridEntity sourceEntity);
}