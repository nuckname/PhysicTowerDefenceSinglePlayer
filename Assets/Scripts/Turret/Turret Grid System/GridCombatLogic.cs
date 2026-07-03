using System;
using UnityEngine;

public class GridCombatLogic : MonoBehaviour
{
    private Turret _turret;
    
    private void Awake()
    {
        _turret = GetComponent<Turret>();
    }

    // Call this whenever the grid changes (a card is added, moved, or removed)
    public void ApplyCardToTurret(TurretCard playedCard, Vector2Int startPos, Vector2Int direction, TurretGridData gridData)
    {
        // Polymorphism in action! 
        // If it's a laser, it runs the laser math. If it's a shotgun, it runs the shotgun math.
        // need to run correct grid size here.
        Debug.LogWarning("GridCombatLogic: ApplyCardToTurret called. Make sure to pass the correct grid size for your game.");
        
        // The card now returns a bundle of stats instead of just an int
        TurretStats cardModifiers = playedCard.CalculateEffect(startPos, direction, gridData, 5, 5);
        
        // Tell the turret to deal that damage to an enemy... 
        // (We do this by sending the modifiers to the Turret script)
        _turret.UpdateModifiers(cardModifiers);
    }
}