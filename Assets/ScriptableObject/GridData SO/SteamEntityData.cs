using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Steam Tile", menuName = "Grid System/Cards/Steam Entity")]
public class SteamEntityData : GridData, IEnemyDeathListener
{
    public int goldPerKill = 2;

    public override List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        return new List<StatModifier>();
    }

    public void OnEnemyKilled(Turret linkedTurret)
    {
        // Add gold to the player's bank. 
        // Assuming your Turret has a reference to the main game manager or economy manager.
        //linkedTurret.AddGold(goldPerKill);
        Debug.Log($"Steam provided +{goldPerKill} gold!");
    }
}