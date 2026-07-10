using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(GridEntity))]
public class GridBouncerReflectHandler : MonoBehaviour
{
    /// <summary>
    /// Checks the target tile for a trajectory modifier (like a Triangle Reflector).
    /// Returns true if the ball should enter the reflector and outputs the new direction.
    /// </summary>
    public bool CheckForReflection(Vector2Int targetPos, Vector2Int currentDir, TurretGridData gridData, out Vector2Int nextDir)
    {
        nextDir = currentDir;
        
        PlacedCardSaveState targetCard = gridData.SavedCards.Find(c => c.GridPosition == targetPos);
        
        if (targetCard != null && targetCard.CardData is ITrajectoryModifier modifier)
        {
            Vector2Int reflectedDir = modifier.GetRedirectedDirection(currentDir, targetCard.Direction);
            
            // If the direction changed, it means we hit the slanted side and can enter it!
            if (reflectedDir != Vector2Int.zero && reflectedDir != currentDir)
            {
                nextDir = reflectedDir;
                return true;
            }
        }

        return false;
    }
}