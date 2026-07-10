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

    private struct LaserNode
    {
        public Vector2Int Position;
        public Vector2Int VisualDirection;
    }

    public override List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        List<LaserNode> laserPath = GetLaserPath(startPos, direction, gridData, gridWidth, gridHeight);
        int calculatedDamageBonus = baseDamage + (laserPath.Count * DamagePerSquare);

        List<StatModifier> modifiers = new List<StatModifier>();
        modifiers.Add(new StatModifier { Type = StatType.Damage, Value = calculatedDamageBonus });
        return modifiers;
    }
    
    public override void SpawnVisuals(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, GridUIManager uiManager, List<GameObject> spawnedVisualsTracker)
    {
        if (LaserBeamPrefab == null) return;

        List<LaserNode> laserPath = GetLaserPath(startPos, direction, gridData, uiManager.GridWidth, uiManager.GridHeight);

        foreach (LaserNode node in laserPath)
        {
            Tile actualTile = uiManager.GetTileAt(node.Position);
            
            if (actualTile != null)
            {
                GridEntity laserSegment = Instantiate(LaserBeamPrefab, actualTile.transform);
                laserSegment.gameObject.name = $"Laser Segment {node.Position}";
                laserSegment.Initialize(this, node.Position, uiManager, true);

                if (LaserBeamSprite != null && laserSegment.Artwork != null)
                {
                    laserSegment.Artwork.sprite = LaserBeamSprite;
                }

                float angle = Mathf.Atan2(node.VisualDirection.y, node.VisualDirection.x) * Mathf.Rad2Deg;
                laserSegment.transform.rotation = Quaternion.Euler(0, 0, angle);

                spawnedVisualsTracker.Add(laserSegment.gameObject);
            }
        }
    }

    private List<LaserNode> GetLaserPath(Vector2Int startPos, Vector2Int startDirection, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        List<LaserNode> path = new List<LaserNode>();
        if (startDirection == Vector2Int.zero || gridData == null) return path;

        Vector2Int currentPos = startPos + startDirection;
        Vector2Int currentDir = startDirection;
        int safetyLimit = 0;

        Debug.Log($"<color=cyan>[LASER START]</color> Origin: {startPos}, Initial Dir: {startDirection}");

        while (currentPos.x >= 0 && currentPos.x < gridWidth && currentPos.y >= 0 && currentPos.y < gridHeight && safetyLimit < 50)
        {
            Debug.Log($"[LASER STEP {safetyLimit}] Checking Tile: {currentPos}. Moving in Dir: {currentDir}");

            // 1. Check for Holes in the board
            if (gridData.TileStates.TryGetValue(currentPos, out int stateValue) && stateValue != 0) 
            {
                Debug.Log($"<color=red>[LASER STOP]</color> Hit a board obstacle/hole at {currentPos}.");
                break; 
            }

            // 2. Check the raw data for placed cards
            PlacedCardSaveState hitCard = gridData.SavedCards.Find(c => c.GridPosition == currentPos);
            
            if (hitCard != null)
            {
                Debug.Log($"[LASER HIT] Found card '{hitCard.CardData.name}' at {currentPos}.");

                if (hitCard.CardData is ITrajectoryModifier modifier)
                {
                    Vector2Int oldDir = currentDir;
                    currentDir = modifier.GetRedirectedDirection(currentDir, hitCard.Direction);
                    
                    Debug.Log($"<color=yellow>[LASER REFLECT]</color> Bounced off {hitCard.CardData.name}! Dir changed from {oldDir} to {currentDir}");
                    
                    if (currentDir == Vector2Int.zero) 
                    {
                        Debug.LogWarning("[LASER BROKE] Reflector returned a zero vector! Stopping.");
                        break; 
                    }
                    
                    currentPos += currentDir;
                    safetyLimit++;
                    continue; 
                }
                else
                {
                    Debug.Log($"<color=red>[LASER STOP]</color> Hit normal card {hitCard.CardData.name} at {currentPos}. Lasers stop at normal cards.");
                    break; 
                }
            }

            Debug.Log($"[LASER PATH] Tile {currentPos} is empty. Adding to path.");
            path.Add(new LaserNode { Position = currentPos, VisualDirection = currentDir });
            
            currentPos += currentDir; 
            safetyLimit++;
        }

        Debug.Log($"<color=cyan>[LASER END]</color> Finished calculating. Total path length: {path.Count}");
        return path;
    }

    public bool IsSolidWall() => false;

    public void OnHitByEntity(GridEntity activeEntity, GridEntity stationaryEntity, Turret linkedTurret)
    {
        if (activeEntity != null)
        {
            Destroy(activeEntity.gameObject);
        }
    }
}