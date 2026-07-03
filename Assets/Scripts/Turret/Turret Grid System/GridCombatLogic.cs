using System;
using UnityEngine;

public class GridCombatLogic : MonoBehaviour
{
    private Turret _turret;
    private void Awake()
    {
        _turret = GetComponent<Turret>();
    }

    public void FireWeapon(TurretCard playedCard, Vector2Int startPos, Vector2Int direction, TurretGridData gridData)
    {
        // Polymorphism in action! 
        // If it's a laser, it runs the laser math. If it's a shotgun, it runs the shotgun math.
        // need to run correct grid size here.
        Debug.LogWarning("GridCombatLogic: FireWeapon called. Make sure to pass the correct grid size for your game.");
        int finalDamage = playedCard.CalculateEffect(startPos, direction, gridData, 5, 5);
        
        //_turret.
        
        // Tell the turret to deal that damage to an enemy...
    }
}