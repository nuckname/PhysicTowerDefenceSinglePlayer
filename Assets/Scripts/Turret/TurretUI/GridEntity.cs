using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; 

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Canvas))] 
[RequireComponent(typeof(GridEntityRotation))] // Added constraint
public class GridEntity : MonoBehaviour
{
    [Header("Entity Data")]
    public GridData MyCardData { get; private set; }
    public Vector2Int CurrentGridPosition { get; private set; }
    public Vector2Int CurrentDirection { get; set; } = Vector2Int.up; // Changed to public set
    
    [Header("UI Elements")]
    public Image CooldownOverlay;

    public int CurrentCooldown { get; set; }

    public GridUIManager MyGridManager { get; private set; }
    public GridCombatLogic MyGridCombatLogic { get; private set; }
    public Image Artwork { get; private set; }
    public Canvas EntityCanvas { get; private set; }
    
    private GridEntityRotation _rotationComponent; // Cached component

    private void Awake()
    {
        Artwork = GetComponent<Image>();
        EntityCanvas = GetComponent<Canvas>();
        _rotationComponent = GetComponent<GridEntityRotation>(); 
        
        EntityCanvas.overrideSorting = false;
    }

    public void Initialize(GridData cardData, Vector2Int startPos, GridUIManager manager, bool isVisualOnly = false)
    {
        MyCardData = cardData;
        CurrentGridPosition = startPos;
        MyGridManager = manager;

        if (cardData.girdArtwork != null)
        {
            Artwork.sprite = cardData.girdArtwork;
        }

        CurrentDirection = Vector2Int.up;
        
        // Pass the Card's chosen rotation setting to the new component
        _rotationComponent.Initialize(cardData.allowedRotation);

        if (MyCardData is ICooldownHandler cooldownHandler)
        {
            CurrentCooldown = cooldownHandler.MaxCooldown;
            if (CooldownOverlay != null)
            {
                CooldownOverlay.gameObject.SetActive(true);
                CooldownOverlay.fillAmount = 1f; 
                CooldownOverlay.DOKill(); 
            }
        }
        else
        {
            if (CooldownOverlay != null) CooldownOverlay.gameObject.SetActive(false);
        }

        if (TryGetComponent(out GridEntityMovement movementScript))
        {
            movementScript.ResetMovementState();
        }
        
        if (!isVisualOnly && MyGridManager != null)
        {
            GridCombatLogic logic = MyGridManager.GetComponent<GridCombatLogic>();
            if (logic != null)
            {
                logic.RegisterEntity(this);
            }
        }
    }
    
    public void TickCooldown(TurretGridData gridData, GridPlacementManager placementManager)
    {
        if (MyCardData is ICooldownHandler cooldownHandler)
        {
            CurrentCooldown--;

            if (CooldownOverlay != null)
            {
                float targetFillAmount = (float)CurrentCooldown / cooldownHandler.MaxCooldown;
                CooldownOverlay.DOFillAmount(targetFillAmount, 0.5f).SetEase(Ease.InOutSine);
            }

            if (CurrentCooldown <= 0)
            {
                cooldownHandler.OnCooldownZero(gridData, this, MyGridManager, placementManager);
                CurrentCooldown = cooldownHandler.MaxCooldown;

                if (CooldownOverlay != null)
                {
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
        if (CooldownOverlay != null) CooldownOverlay.DOKill();

        if (MyGridManager != null)
        {
            Tile myTile = MyGridManager.GetTileAt(CurrentGridPosition);
            if (myTile != null && myTile.OccupyingEntity == this)
            {
                myTile.SetOccupied(false, null);
            }
        }
    }

    public void SetGridPosition(Vector2Int newPos)
    {
        CurrentGridPosition = newPos;
    }

    public float RotateDirectionClockwise()
    {
        return _rotationComponent.RotateClockwise();
    }

    public float RotateDirectionCounterClockwise()
    {
        return _rotationComponent.RotateCounterClockwise();
    }
    
    public void SetDirection(Vector2Int savedDirection)
    {
        _rotationComponent.ApplyRotationVisuals(savedDirection);
    }
}