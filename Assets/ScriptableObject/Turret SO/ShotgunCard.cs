using UnityEngine;

[CreateAssetMenu(fileName = "New Shotgun Card", menuName = "Grid System/Cards/Shotgun Upgrade")]
public class ShotgunCard : TurretCard
{
    [Header("Shotgun Specifics")]
    public int FalloffPerSquare = 2;

    public override int CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        // Simple logic: measure the distance to the edge of the grid 
        // and subtract the falloff. No loop needed for this specific math!
        int distanceToEdge = gridWidth - startPos.x; // (Assuming shooting right)
        
        int finalDamage = BaseDamage - (distanceToEdge * FalloffPerSquare);
        
        // Ensure it never heals the enemy by going below 0
        return Mathf.Max(finalDamage, 1); 
    }
}