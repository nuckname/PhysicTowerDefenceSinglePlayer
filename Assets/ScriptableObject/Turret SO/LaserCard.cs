using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Laser Card", menuName = "Grid System/Cards/Laser Upgrade")]
public class LaserCard : TurretCard
{
    [Header("Laser Specifics")]
    public int DamagePerSquare = 5;

    public override List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        int squaresTraveled = 0;
        Vector2Int currentPos = startPos + direction;

        while (currentPos.x >= 0 && currentPos.x < gridWidth && currentPos.y >= 0 && currentPos.y < gridHeight)
        {
            if (gridData.TileStates.TryGetValue(currentPos, out int stateValue) && stateValue != 0) 
            {
                break; // Hit something
            }
            squaresTraveled++;
            currentPos += direction; 
        }
        
        int calculatedDamageBonus = BaseDamage + (squaresTraveled * DamagePerSquare);

        List<StatModifier> modifiers = new List<StatModifier>();
        
        modifiers.Add(new StatModifier 
        { 
            Type = StatType.Damage, 
            Value = calculatedDamageBonus 
        });

        return modifiers;
    }
}