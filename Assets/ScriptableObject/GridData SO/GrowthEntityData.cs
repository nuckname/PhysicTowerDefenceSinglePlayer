using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Growth Item", menuName = "Grid System/Cards/Growth Entity")]
public class GrowthEntityData : GridData, ICooldownHandler
{
    public int cooldownTurns = 5;
    public int damagePerGrowthSpace = 2;
    public GridEntity growthPrefab;

    public int MaxCooldown => cooldownTurns;
    public int CurrentCooldown { get; set; }

    public override List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        List<StatModifier> mods = new List<StatModifier>();
        mods.Add(new StatModifier { Type = StatType.Damage, Value = damagePerGrowthSpace });
        return mods;
    }

    public void OnCooldownZero(TurretGridData gridData, GridEntity sourceEntity, GridUIManager uiManager, GridPlacementManager placementManager)
    {
        Vector2Int[] adjacentDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (var dir in adjacentDirections)
        {
            Vector2Int targetPos = sourceEntity.CurrentGridPosition + dir;

            if (targetPos.x >= 0 && targetPos.x < uiManager.GridWidth && targetPos.y >= 0 && targetPos.y < uiManager.GridHeight)
            {
                if (!gridData.TileStates.TryGetValue(targetPos, out int stateValue) || stateValue == 0)
                {
                    placementManager.SpawnSubEntity(growthPrefab, this, targetPos, Vector2Int.up);
                    break; // Only grow one tile per cooldown
                }
            }
        }
    }
}