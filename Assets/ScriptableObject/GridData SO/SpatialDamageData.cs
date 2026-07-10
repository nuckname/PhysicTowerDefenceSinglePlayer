using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Spatial Damage Item", menuName = "Grid System/Cards/Spatial Damage")]
public class SpatialDamageData : GridData
{
    [Header("Spatial Specifics")]
    public bool buffFromEmptySpace = true;
    public int damagePerTile = 5;

    public override List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        int validTiles = 0;
        
        Vector2Int[] adjacentDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in adjacentDirections)
        {
            Vector2Int checkPos = startPos + dir;
            
            if (checkPos.x >= 0 && checkPos.x < gridWidth && checkPos.y >= 0 && checkPos.y < gridHeight)
            {
                bool isOccupied = gridData.TileStates.TryGetValue(checkPos, out int stateValue) && stateValue != 0;
                
                if (buffFromEmptySpace && !isOccupied)
                {
                    validTiles++;
                }
                else if (!buffFromEmptySpace && isOccupied)
                {
                    validTiles++;
                }
            }
        }

        List<StatModifier> mods = new List<StatModifier>();
        mods.Add(new StatModifier { Type = StatType.Damage, Value = baseDamage + (validTiles * damagePerTile) });
        
        return mods;
    }
}