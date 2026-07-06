using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Kinetic Bouncer", menuName = "Grid System/Cards/Kinetic Bouncer")]
public class KineticBouncerData : GridData
{
    [Header("Bounce Specifics")]
    [Tooltip("How much damage to grant the turret every time this hits a wall.")]
    public int DamagePerBounce = 3;

    // This is the standard effect it applies while traveling over the grid
    public override List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        List<StatModifier> mods = new List<StatModifier>();
        
        // It can still provide its base stat just for existing on the board
        mods.Add(new StatModifier { Type = StatType.Damage, Value = baseDamage });
        
        return mods;
    }

    // THIS is our custom trigger whenever the movement script detects a wall or corner
    public override void OnWallBounce(Vector2Int bouncePos, TurretGridData gridData, GridUIManager uiManager, Turret linkedTurret)
    {
        List<StatModifier> bounceModifiers = new List<StatModifier>();
        
        bounceModifiers.Add(new StatModifier 
        { 
            Type = StatType.Damage, 
            Value = DamagePerBounce 
        });

        // Send it directly to the turret
        if (linkedTurret != null)
        {
            linkedTurret.UpdateModifiers(bounceModifiers);
            
            // Optional: A nice debug log so you can watch it stack up in the console
            Debug.Log($"*CLANG!* Bouncer hit a wall at {bouncePos}. Turret gained +{DamagePerBounce} Damage!");
        }
    }
    
    // THE MAGIC HAPPENS HERE: When the FSM starts the round, this Turret tells 
    // the UI to spawn the bouncing prefab, but paints it with THIS Turret's data!
    public override void OnRoundStart(GridUIManager uiManager, Vector2Int position)
    {
        // By passing 'this', the bouncing prefab becomes an exact extension of this card.
        uiManager.SpawnBouncingItem(this, position);
    }
}