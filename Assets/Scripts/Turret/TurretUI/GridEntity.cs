using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Required for the smooth fill animation

// Requires a UI Image component to show the artwork
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Canvas))] // Automatically adds a Canvas so we can pop it over other UI elements
public class GridEntity : MonoBehaviour
{
    [Header("Entity Data")]
    public GridData MyCardData { get; private set; }
    public Vector2Int CurrentGridPosition { get; private set; }
    public Vector2Int CurrentDirection { get; private set; } = Vector2Int.up; // Defaults to facing North (0, 1)
    
    [Header("UI Elements")]
    public Image CooldownOverlay;

    // Instance State Variables
    public int CurrentCooldown { get; set; }

    // Public getters so our movement script can access them
    public GridUIManager MyGridManager { get; private set; }
    public GridCombatLogic MyGridCombatLogic { get; private set; }
    public Image Artwork { get; private set; }
    public Canvas EntityCanvas { get; private set; }

    private void Awake()
    {
        Artwork = GetComponent<Image>();
        EntityCanvas = GetComponent<Canvas>();
        
        // Ensure it can render over standard tiles when dragged
        EntityCanvas.overrideSorting = false;
    }

    // Called by the GridUIManager the moment this is dropped onto a tile
    public void Initialize(GridData cardData, Vector2Int startPos, GridUIManager manager)
    {
        MyCardData = cardData;
        CurrentGridPosition = startPos;
        MyGridManager = manager;

        // Set the UI visual to match the card
        if (cardData.girdArtwork != null)
        {
            Artwork.sprite = cardData.girdArtwork;
        }

        CurrentDirection = Vector2Int.up;

        // --- COOLDOWN INITIALIZATION ---
        if (MyCardData is ICooldownHandler cooldownHandler)
        {
            CurrentCooldown = cooldownHandler.MaxCooldown;
            if (CooldownOverlay != null)
            {
                CooldownOverlay.gameObject.SetActive(true);
                CooldownOverlay.fillAmount = 1f; // Start completely full
                CooldownOverlay.DOKill(); // Clear out any old tweens just in case
            }
        }
        else
        {
            // If this item doesn't have a cooldown (like a basic laser), hide the overlay entirely
            if (CooldownOverlay != null) CooldownOverlay.gameObject.SetActive(false);
        }

        // Tell the movement script to reset its rotation and scale state
        if (TryGetComponent(out GridEntityMovement movementScript))
        {
            movementScript.ResetMovementState();
        }
        
        if (MyGridManager != null)
        {
            MyGridCombatLogic = MyGridManager.GetComponent<GridCombatLogic>();
            if (MyGridCombatLogic != null)
            {
                MyGridCombatLogic.RegisterEntity(this);
            }
        }
    }
    
    // Helper to tick down cooldowns cleanly on this specific instance
    public void TickCooldown(TurretGridData gridData, GridPlacementManager placementManager)
    {
        if (MyCardData is ICooldownHandler cooldownHandler)
        {
            CurrentCooldown--;

            if (CooldownOverlay != null)
            {
                // Calculate the new percentage (e.g. 2 / 5 = 0.4f)
                float targetFillAmount = (float)CurrentCooldown / cooldownHandler.MaxCooldown;
                
                // Smoothly animate the fill going down over 0.5 seconds
                CooldownOverlay.DOFillAmount(targetFillAmount, 0.5f).SetEase(Ease.InOutSine);
            }

            if (CurrentCooldown <= 0)
            {
                // Trigger the effect and pass THIS entity as the source
                cooldownHandler.OnCooldownZero(gridData, this, MyGridManager, placementManager);
                
                // Reset cooldown math
                CurrentCooldown = cooldownHandler.MaxCooldown;

                if (CooldownOverlay != null)
                {
                    // Delay snapping the visual back to full by 0.5s so the DOTween animation above has time to hit 0 first
                    DOVirtual.DelayedCall(0.5f, () => 
                    {
                        if (this != null && CooldownOverlay != null) 
                        {
                            CooldownOverlay.fillAmount = 1f;
                        }
                    });
                }
            }
        }
    }

    private void OnDestroy()
    {
        // Stop any active tweens on the overlay so they don't error out when the object dies
        if (CooldownOverlay != null) CooldownOverlay.DOKill();

        // Auto-free the tile if this entity is destroyed 
        if (MyGridManager != null)
        {
            Tile myTile = MyGridManager.GetTileAt(CurrentGridPosition);
            if (myTile != null && myTile.OccupyingEntity == this)
            {
                myTile.SetOccupied(false, null);
            }
        }
    }

    // Helper method for the movement script to update the grid position safely
    public void SetGridPosition(Vector2Int newPos)
    {
        CurrentGridPosition = newPos;
    }

    // Helper method for the movement script to handle the math rotation
    public void RotateDirectionClockwise()
    {
        if (CurrentDirection == Vector2Int.up) CurrentDirection = new Vector2Int(1, 1);
        else if (CurrentDirection == new Vector2Int(1, 1)) CurrentDirection = Vector2Int.right;
        else if (CurrentDirection == Vector2Int.right) CurrentDirection = new Vector2Int(1, -1);
        else if (CurrentDirection == new Vector2Int(1, -1)) CurrentDirection = Vector2Int.down;
        else if (CurrentDirection == Vector2Int.down) CurrentDirection = new Vector2Int(-1, -1);
        else if (CurrentDirection == new Vector2Int(-1, -1)) CurrentDirection = Vector2Int.left;
        else if (CurrentDirection == Vector2Int.left) CurrentDirection = new Vector2Int(-1, 1);
        else if (CurrentDirection == new Vector2Int(-1, 1)) CurrentDirection = Vector2Int.up;
    }

    // Added counter-clockwise math for the scroll wheel up
    public void RotateDirectionCounterClockwise()
    {
        if (CurrentDirection == Vector2Int.up) CurrentDirection = new Vector2Int(-1, 1);
        else if (CurrentDirection == new Vector2Int(-1, 1)) CurrentDirection = Vector2Int.left;
        else if (CurrentDirection == Vector2Int.left) CurrentDirection = new Vector2Int(-1, -1);
        else if (CurrentDirection == new Vector2Int(-1, -1)) CurrentDirection = Vector2Int.down;
        else if (CurrentDirection == Vector2Int.down) CurrentDirection = new Vector2Int(1, -1);
        else if (CurrentDirection == new Vector2Int(1, -1)) CurrentDirection = Vector2Int.right;
        else if (CurrentDirection == Vector2Int.right) CurrentDirection = new Vector2Int(1, 1);
        else if (CurrentDirection == new Vector2Int(1, 1)) CurrentDirection = Vector2Int.up;
    }
    
    public void SetDirection(Vector2Int savedDirection)
    {
        CurrentDirection = savedDirection;

        // Apply visual rotation based on the cardinal direction vector
        float angle = 0f;
        if (CurrentDirection == Vector2Int.up) angle = 0f;
        else if (CurrentDirection == new Vector2Int(1, 1)) angle = -45f;
        else if (CurrentDirection == Vector2Int.right) angle = -90f;
        else if (CurrentDirection == new Vector2Int(1, -1)) angle = -135f;
        else if (CurrentDirection == Vector2Int.down) angle = -180f;
        else if (CurrentDirection == new Vector2Int(-1, -1)) angle = 135f; 
        else if (CurrentDirection == Vector2Int.left) angle = 90f;
        else if (CurrentDirection == new Vector2Int(-1, 1)) angle = 45f;

        // Tell the movement script exactly what rotation it should hold
        if (TryGetComponent(out GridEntityMovement movementScript))
        {
            movementScript.ForceZRotation(angle);
        }
        
        if (Artwork != null)
        {
            Artwork.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}