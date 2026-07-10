using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Laser Card", menuName = "Grid System/Cards/Laser Upgrade")]
public class LaserGridTile : GridData, IEntityCollision
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
    
    public bool IsSolidWall() => false;

    public void OnHitByEntity(GridEntity activeEntity, GridEntity stationaryEntity, Turret linkedTurret)
    {
        // The active entity (Bouncer) entered the laser beam
        if (activeEntity != null)
        {
            Debug.Log($"*BZZT!* {activeEntity.gameObject.name} entered a laser and was destroyed!");
            Destroy(activeEntity.gameObject);
        }
    }

    public override void SpawnVisuals(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, GridUIManager uiManager, List<GameObject> spawnedVisualsTracker)
    {
        if (LaserBeamPrefab == null) return;

        Vector2Int currentPos = startPos + direction;

        while (currentPos.x >= 0 && currentPos.x < uiManager.GridWidth && currentPos.y >= 0 && currentPos.y < uiManager.GridHeight)
        {
            if (gridData.TileStates.TryGetValue(currentPos, out int stateValue) && stateValue != 0) break; 

            // Grab the actual Tile object so we can interact with its state
            Tile actualTile = uiManager.GetTileAt(currentPos);
            
            if (actualTile != null)
            {
                GridEntity laserSegment = Instantiate(LaserBeamPrefab, actualTile.transform);
                laserSegment.gameObject.name = $"Laser Segment {currentPos}";

                laserSegment.Initialize(this, currentPos, uiManager, true);

                // MARK THE TILE AS OCCUPIED BY THE LASER
                actualTile.SetOccupied(true, laserSegment);

                if (LaserBeamSprite != null && laserSegment.Artwork != null)
                {
                    laserSegment.Artwork.sprite = LaserBeamSprite;
                }

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                laserSegment.transform.rotation = Quaternion.Euler(0, 0, angle);

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