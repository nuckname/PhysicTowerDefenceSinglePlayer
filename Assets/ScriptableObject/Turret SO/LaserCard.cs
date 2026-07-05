using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for rendering sprites on a UI Canvas

[CreateAssetMenu(fileName = "New Laser Card", menuName = "Grid System/Cards/Laser Upgrade")]
public class LaserCard : TurretCard
{
    [Header("Laser Specifics")]
    public int DamagePerSquare = 5;

    [Header("Visuals")]
    public Sprite LaserBeamSprite; // Now asking for a raw Sprite

    public override List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        int squaresTraveled = GetTravelDistance(startPos, direction, gridData, gridWidth, gridHeight);
        
        int calculatedDamageBonus = BaseDamage + (squaresTraveled * DamagePerSquare);

        List<StatModifier> modifiers = new List<StatModifier>();
        
        modifiers.Add(new StatModifier 
        { 
            Type = StatType.Damage, 
            Value = calculatedDamageBonus 
        });

        return modifiers;
    }

    public override void SpawnVisuals(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight, Transform gridParent)
    {
        if (LaserBeamSprite == null) return;

        // 1. Get the same distance used for the damage calculation
        int distance = GetTravelDistance(startPos, direction, gridData, gridWidth, gridHeight);
        if (distance <= 0) return; 

        // 2. Create a new empty GameObject and parent it to your UI Grid
        GameObject laserVisual = new GameObject("Laser Beam Visual");
        laserVisual.transform.SetParent(gridParent, false);

        // 3. Attach an Image component and assign the Sprite
        Image visualImage = laserVisual.AddComponent<Image>();
        visualImage.sprite = LaserBeamSprite;

        // Note: If you eventually move this off the Canvas and into 2D world space, 
        // swap 'Image' above for 'SpriteRenderer' and remove the using UnityEngine.UI tag.

        // 4. Calculate rotation based on the direction vector
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        laserVisual.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 5. Stretch the laser to match the distance
        laserVisual.transform.localScale = new Vector3(distance, 1, 1);
    }

    private int GetTravelDistance(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
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

        return squaresTraveled;
    }
}