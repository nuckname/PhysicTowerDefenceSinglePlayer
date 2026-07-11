using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Kinetic Bouncer", menuName = "Grid System/Cards/Kinetic Bouncer")]
public class KineticBouncerData : GridData, IWallBouncer, IRoundListener, ICooldownHandler
{
    [Header("Bounce Specifics")]
    [Tooltip("How much damage to grant the turret every time this hits a wall.")]
    public int DamagePerBounce = 3;

    [Header("Cooldown Settings")]
    [Tooltip("How many turns/ticks it takes for the bouncer to move.")]
    public int MoveCooldown = 1;

    // This is the standard effect it applies while traveling over the grid
    public override List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight)
    {
        List<StatModifier> mods = new List<StatModifier>();
        
        // It can still provide its base stat just for existing on the board
        mods.Add(new StatModifier { Type = StatType.Damage, Value = baseDamage });
        
        return mods;
    }

    // THIS is our custom trigger whenever the movement script detects a wall or corner
    public void OnWallBounce(Vector2Int bouncePos, TurretGridData gridData, GridUIManager uiManager, Turret linkedTurret)
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
    
    public void OnRoundStart(GridPlacementManager placementManager, TurretGridData gridData, GridEntity sourceEntity)
    {
        placementManager.ClearTileOccupation(sourceEntity.CurrentGridPosition);
        
        // Pass the CurrentDirection of the placed entity into the spawner
        placementManager.SpawnBouncingItem(this, sourceEntity.CurrentGridPosition, sourceEntity.CurrentDirection, gridData);
        
        sourceEntity.gameObject.SetActive(false); 
    }

    public void OnRoundEnd(GridPlacementManager placementManager, TurretGridData gridData, GridEntity sourceEntity)
    {
        // Added to satisfy the IRoundListener contract.
    }

    // Satisfying the ICooldownHandler contract natively
    public int MaxCooldown => MoveCooldown;
    
    // Auto-property to track the current state for this specific card
    public int CurrentCooldown { get; set; }

    public void OnCooldownZero(TurretGridData gridData, GridEntity sourceEntity, GridUIManager uiManager, GridPlacementManager placementManager)
    {
        // Grab the movement script from the active entity on the board and command it to step forward
        GridBouncingMovement movementScript = sourceEntity.GetComponent<GridBouncingMovement>();
        
        if (movementScript != null)
        {
            movementScript.StepForward();
        }
        else
        {
            Debug.LogWarning("Bouncer entity triggered OnCooldownZero but has no GridBouncingMovement attached!");
        }
    }
}