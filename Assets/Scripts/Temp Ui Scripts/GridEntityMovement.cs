using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(GridEntity))] // Guarantees the data script is on the same object
public class GridEntityMovement : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Juice & Feel Parameters")]
    [SerializeField] private float scaleOnHover = 1.15f;
    [SerializeField] private float scaleOnDrag = 1.25f;
    [SerializeField] private float scaleTransition = 0.15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;
    [SerializeField] private float hoverPunchAngle = 5f;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float rotationAmount = 20f;

    private GridEntity _gridEntity;
    private Vector3 _magneticTargetPosition; // Keeps track of where the piece SHOULD be while dragging
    
    // Dragging & Interaction State
    private bool _isDragging = false;
    private bool _wasDragged = false;
    private bool _isHovering = false; // Tracks if the mouse is currently pointing at the piece
    private bool _isHoverPunching = false; // Prevents Update from fighting DOTween
    private Vector3 _offset;
    private Transform _originalParent;
    
    // Rotation & Math State
    private Vector3 _lastPosition;
    private Vector3 _movementDelta;
    private float _targetZRotation = 0f; // Keeps track of the 90-degree board rotations safely

    private void Awake()
    {
        // Cache our data script
        _gridEntity = GetComponent<GridEntity>();
    }

    // Called by GridEntity.Initialize() to ensure fresh state when spawned from a deck/pool
    public void ResetMovementState()
    {
        _targetZRotation = 0f;
        transform.DOKill(); // Stop any active tweens
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        _lastPosition = transform.position;
        _movementDelta = Vector3.zero;
        
        _isDragging = false;
        _wasDragged = false;
        _isHovering = false;
        _isHoverPunching = false;
    }

    private void Update()
    {
        if (_isDragging || _isHovering)
        {
            // R-Key Rotation: Always Clockwise
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                RotateVisualsAndData(true);
            }

            // Scroll Wheel Rotation
            if (Mouse.current != null)
            {
                float scrollY = Mouse.current.scroll.ReadValue().y;
                
                if (scrollY > 0f) // Scrolled Up -> Counter-Clockwise
                {
                    RotateVisualsAndData(false);
                }
                else if (scrollY < 0f) // Scrolled Down -> Clockwise
                {
                    RotateVisualsAndData(true);
                }
            }
        }

        if (_isDragging)
        {
            // 1. Smoothly glide to either the mouse OR the magnetically snapped tile
            transform.position = Vector3.Lerp(transform.position, _magneticTargetPosition, 25f * Time.deltaTime);

            // 2. Follow Rotation Math (Adapted directly from your CardAnimator)
            Vector3 movement = transform.position - _lastPosition;
            _movementDelta = Vector3.Lerp(_movementDelta, movement, 25 * Time.deltaTime);

            Vector3 movementRotation = _movementDelta * (rotationAmount / 100f);
            
            // Apply the drag-tilt while maintaining our actual Grid Z-Rotation
            float targetX = Mathf.Clamp(movementRotation.y, -60, 60);
            float targetY = Mathf.Clamp(-movementRotation.x, -60, 60);
            
            // Multiply the tilt and base rotation so the tilt is always relative to the screen!
            Quaternion tiltRot = Quaternion.Euler(targetX, targetY, 0);
            Quaternion baseZRot = Quaternion.Euler(0, 0, _targetZRotation);
            Quaternion targetRotation = tiltRot * baseZRot;
            
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else if (!_isHoverPunching) 
        {
            // Smoothly return to a flat resting state, maintaining the Z-Rotation
            Quaternion targetRotation = Quaternion.Euler(0, 0, _targetZRotation);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        _lastPosition = transform.position;
    }
    
    // Called when loading a saved board state to sync the movement script's rotation
    public void ForceZRotation(float zRotation)
    {
        _targetZRotation = zRotation;
        
        // Immediately snap the rotation so it doesn't try to Lerp from 0 to the saved angle
        transform.localRotation = Quaternion.Euler(0, 0, _targetZRotation);
    }

    // ==========================================
    // INTERACTION & DRAG EVENTS
    // ==========================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isHovering = true;
        
        if (_isDragging) return;
        
        // Safely clear all active tweens to prevent scale/rotation compounding
        transform.DOKill();
        
        transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);
        
        // Kill existing punches to prevent compounding, then punch!
        _isHoverPunching = true;
        transform.DOPunchRotation(Vector3.forward * hoverPunchAngle, scaleTransition, 20, 1)
                 .OnComplete(() => _isHoverPunching = false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
        
        if (_isDragging) return;
        if (!_wasDragged) transform.DOScale(1f, scaleTransition).SetEase(scaleEase);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        transform.DOScale(scaleOnDrag, scaleTransition).SetEase(scaleEase);
    }

    public void OnPointerUp(PointerEventData eventData) { }

    public void OnPointerClick(PointerEventData eventData) { }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // Force stop any active hover juice so it doesn't corrupt our drag state
        transform.DOKill();
        _isHoverPunching = false; 

        Vector3 pointerPosition = Pointer.current != null ? (Vector3)Pointer.current.position.ReadValue() : Vector3.zero;
        _offset = pointerPosition - transform.position;
    
        _magneticTargetPosition = pointerPosition - _offset; // NEW: Set initial target

        _isDragging = true;
        _wasDragged = true;
        _gridEntity.Artwork.raycastTarget = false; 
        _originalParent = transform.parent; 

        _gridEntity.EntityCanvas.overrideSorting = true;
        _gridEntity.EntityCanvas.sortingOrder = 100;
    }

    public void OnDrag(PointerEventData eventData) 
    {
        if (!_isDragging) return;

        // 1. Default assumption: Follow the mouse exactly
        Vector3 pointerPosition = Pointer.current != null ? (Vector3)Pointer.current.position.ReadValue() : Vector3.zero;
        _magneticTargetPosition = pointerPosition - _offset;

        // 2. Fire a raycast through the UI to see what we are hovering over
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            Tile hitTile = result.gameObject.GetComponent<Tile>();
            if (hitTile != null)
            {
                // Check if the tile is empty
                GridEntity occupyingEntity = hitTile.GetComponentInChildren<GridEntity>();
                if (occupyingEntity == null || occupyingEntity == _gridEntity)
                {
                    // MAGNETIC SNAP! Override the mouse position and target the tile's exact center
                    _magneticTargetPosition = hitTile.transform.position;
                    break; // Stop checking other UI elements
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        
        _isDragging = false;
        _gridEntity.Artwork.raycastTarget = true;
        _gridEntity.EntityCanvas.overrideSorting = false;
        
        transform.DOScale(1f, scaleTransition).SetEase(scaleEase);

        // Try to find a new tile to drop onto
        if (!AttemptMoveToNewTile(eventData))
        {
            // Missed! Snap back to the original tile perfectly
            transform.SetParent(_originalParent);
            transform.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.OutBack);
        }
        
        StartCoroutine(FrameWait());
    }

    
    private IEnumerator FrameWait()
    {
        yield return new WaitForEndOfFrame();
        _wasDragged = false;
    }

    private bool AttemptMoveToNewTile(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            Tile hitTile = result.gameObject.GetComponent<Tile>();
            if (hitTile != null)
            {
                GridPlacementManager placementManager = _gridEntity.MyGridManager.GetComponent<GridPlacementManager>();
                
                if (placementManager != null)
                {
                    bool moveSuccessful = placementManager.TryMoveExistingEntity(_gridEntity, hitTile.Position);
                    
                    if (moveSuccessful)
                    {
                        // The item was assigned to the new parent; seamlessly glide it to the center!
                        transform.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.OutBack);
                        return true;
                    }
                }
            }
        }
        return false; 
    }
    
    private void RotateVisualsAndData(bool clockwise)
    {
        if (clockwise)
        {
            float step = _gridEntity.RotateDirectionClockwise();
            _targetZRotation -= step;
        }
        else
        {
            float step = _gridEntity.RotateDirectionCounterClockwise();
            _targetZRotation += step;
        }

        _isHoverPunching = false;

        transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), 0.2f, 10, 1);

        // 3. Tell the Grid Manager that the board state has changed
        if (_gridEntity.MyGridManager != null)
        {
            _gridEntity.MyGridManager.RecalculateBoard();
        }
    }
}