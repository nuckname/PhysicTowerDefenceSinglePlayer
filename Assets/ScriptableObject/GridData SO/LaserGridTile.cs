using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Laser Card", menuName = "Grid System/Cards/Laser Upgrade")]
public class LaserGridTile : GridData
{
    [Header("Laser Specifics")]
    public int DamagePerSquare = 5;

    [Header("Visuals")]
    public Sprite LaserBeamSprite; 
    public GridEntity LaserBeamPrefab;

    public override List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        int squaresTraveled = GetTravelDistance(startPos, direction, gridData, gridWidth, gridHeight);
        int calculatedDamageBonus = baseDamage + (squaresTraveled * DamagePerSquare);

        List<StatModifier> modifiers = new List<StatModifier>();
        modifiers.Add(new StatModifier { Type = StatType.Damage, Value = calculatedDamageBonus });
        return modifiers;
    }

    public override void SpawnVisuals(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, GridUIManager uiManager, List<GameObject> spawnedVisualsTracker)
    {
        if (LaserBeamPrefab == null) return;

        Vector2Int currentPos = startPos + direction;

        // Loop through the grid step-by-step
        while (currentPos.x >= 0 && currentPos.x < uiManager.GridWidth && currentPos.y >= 0 && currentPos.y < uiManager.GridHeight)
        {
            if (gridData.TileStates.TryGetValue(currentPos, out int stateValue) && stateValue != 0) 
            {
                break; // Hit something
            }

            // 1. Get the actual UI Tile from the manager
            Transform tileTransform = uiManager.GetTileTransform(currentPos);
            
            if (tileTransform != null)
            {
                // 2. Spawn a laser segment directly ON this specific tile
                GridEntity laserSegment = Instantiate(LaserBeamPrefab, tileTransform);
                laserSegment.gameObject.name = $"Laser Segment {currentPos}";

                // 3. Initialize it so it acts like a real entity on this cell
                laserSegment.Initialize(this, currentPos, uiManager);

                // 4. Update the visual sprite
                if (LaserBeamSprite != null && laserSegment.Artwork != null)
                {
                    laserSegment.Artwork.sprite = LaserBeamSprite;
                }

                // 5. Rotate it to match the beam's direction
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                laserSegment.transform.rotation = Quaternion.Euler(0, 0, angle);

                // 6. Track it so the CombatLogic clears it next recalculation
                spawnedVisualsTracker.Add(laserSegment.gameObject);
            }

            currentPos += direction; 
        }
    }

    private int GetTravelDistance(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        int squaresTraveled = 0;
        Vector2Int currentPos = startPos + direction;

        while (currentPos.x >= 0 && currentPos.x < gridWidth && currentPos.y >= 0 && currentPos.y < gridHeight)
        {
            if (gridData.TileStates.TryGetValue(currentPos, out int stateValue) && stateValue != 0) break;
            squaresTraveled++;
            currentPos += direction; 
        }

        return squaresTraveled;
    }
}