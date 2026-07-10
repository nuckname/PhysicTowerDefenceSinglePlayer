using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Steam Generator", menuName = "Grid System/Cards/Steam Generator")]
public class SteamGeneratorData : GridData, ICooldownHandler
{
    [Header("Steam Specifics")]
    public int cooldownTurns = 3;
    public GridEntity steamPrefab;
    
    [Tooltip("The ScriptableObject data for the steam entity itself.")]
    public GridData steamEntityData;

    public int MaxCooldown => cooldownTurns;
    public int CurrentCooldown { get; set; }

    public override List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        return new List<StatModifier>();
    }

    public void OnCooldownZero(TurretGridData gridData, GridEntity sourceEntity, GridUIManager uiManager, GridPlacementManager placementManager)
    {
        Vector2Int targetPos = sourceEntity.CurrentGridPosition + Vector2Int.up;

        // Check bounds and empty state
        if (targetPos.y < uiManager.GridHeight)
        {
            if (!gridData.TileStates.TryGetValue(targetPos, out int stateValue) || stateValue == 0)
            {
                placementManager.SpawnSubEntity(steamPrefab, steamEntityData, targetPos, Vector2Int.up);
            }
        }
    }
}