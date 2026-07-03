using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shotgun Card", menuName = "Grid System/Cards/Shotgun Upgrade")]
public class ShotgunCard : TurretCard
{
    [Header("Shotgun Specifics")]
    public int FalloffPerSquare = 2;

    public override List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        // Simple logic: measure the distance to the edge of the grid 
        int distanceToEdge = gridWidth - startPos.x; // (Assuming shooting right)
        
        int finalDamage = BaseDamage - (distanceToEdge * FalloffPerSquare);
        
        List<StatModifier> modifiers = new List<StatModifier>();
        
        modifiers.Add(new StatModifier 
        { 
            Type = StatType.Damage, 
            Value = finalDamage 
        });

        return modifiers;
    }
}