using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

// Requires a UI Image component to show the artwork
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Canvas))] // Automatically adds a Canvas so we can pop it over other UI elements
public class GridEntity : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Entity Data")]
    public TurretCard MyCardData { get; private set; }
    public Vector2Int CurrentGridPosition { get; private set; }
    public Vector2Int CurrentDirection { get; private set; } = Vector2Int.up; // Defaults to facing North (0, 1)

    private GridUIManager _myGridManager;
    private Image _artwork;
    private Canvas _canvas;

    [Header("Juice & Feel Parameters")]
    [SerializeField] private float scaleOnHover = 1.15f;
    [SerializeField] private float scaleOnDrag = 1.25f;
    [SerializeField] private float scaleTransition = 0.15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;
    [SerializeField] private float hoverPunchAngle = 5f;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float rotationAmount = 20f;

    private Vector3 _magneticTargetPosition; // Keeps track of where the piece SHOULD be while dragging
    
    // Dragging & Interaction State
    private bool _isDragging = false;
    private bool _wasDragged = false;
    private Vector3 _offset;
    private Transform _originalParent;
    
    // Rotation & Math State
    private Vector3 _lastPosition;
    private Vector3 _movementDelta;
    private float _targetZRotation = 0f; // Keeps track of the 90-degree board rotations safely

    private void Awake()
    {
        _artwork = GetComponent<Image>();
        _canvas = GetComponent<Canvas>();
        
        // Ensure it can render over standard tiles when dragged
        _canvas.overrideSorting = false;
    }

    // Called by the GridUIManager the moment this is dropped onto a tile
    public void Initialize(TurretCard cardData, Vector2Int startPos, GridUIManager manager)
    {
        MyCardData = cardData;
        CurrentGridPosition = startPos;
        _myGridManager = manager;

        // Set the UI visual to match the card
        if (cardData.CardArtwork != null)
        {
            _artwork.sprite = cardData.CardArtwork;
        }

        // Reset rotation and scale just in case
        _targetZRotation = 0f;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        CurrentDirection = Vector2Int.up;
        
        _lastPosition = transform.position;
    }

    private void Update()
    {
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
            
            Quaternion targetRotation = Quaternion.Euler(targetX, targetY, _targetZRotation);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Smoothly return to a flat resting state, maintaining the Z-Rotation
            Quaternion targetRotation = Quaternion.Euler(0, 0, _targetZRotation);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        _lastPosition = transform.position;
    }

    // ==========================================
    // INTERACTION & DRAG EVENTS
    // ==========================================

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isDragging) return;
        transform.DOScale(scaleOnHover, scaleTransition).SetEase(scaleEase);
        
        // Kill existing punches to prevent compounding, then punch!
        DOTween.Kill(transform);
        transform.DOPunchRotation(Vector3.forward * hoverPunchAngle, scaleTransition, 20, 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isDragging) return;
        if (!_wasDragged) transform.DOScale(1f, scaleTransition).SetEase(scaleEase);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        transform.DOScale(scaleOnDrag, scaleTransition).SetEase(scaleEase);
    }

    public void OnPointerUp(PointerEventData eventData) { }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Using Right-Click to rotate the entity
        if (eventData.button == PointerEventData.InputButton.Right && !_isDragging)
        {
            RotateEntity();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        Vector3 pointerPosition = Pointer.current != null ? (Vector3)Pointer.current.position.ReadValue() : Vector3.zero;
        _offset = pointerPosition - transform.position;
    
        _magneticTargetPosition = pointerPosition - _offset; // NEW: Set initial target

        _isDragging = true;
        _wasDragged = true;
        _artwork.raycastTarget = false; 
        _originalParent = transform.parent; 

        _canvas.overrideSorting = true;
        _canvas.sortingOrder = 100;
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
                if (occupyingEntity == null || occupyingEntity == this)
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
        _artwork.raycastTarget = true;
        _canvas.overrideSorting = false;
        
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

    // ==========================================
    // GRID LOGIC
    // ==========================================

    private bool AttemptMoveToNewTile(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            Tile hitTile = result.gameObject.GetComponent<Tile>();
            if (hitTile != null)
            {
                // Check if the tile is empty (doesn't have another GridEntity sitting on it)
                GridEntity occupyingEntity = hitTile.GetComponentInChildren<GridEntity>();
                
                if (occupyingEntity == null || occupyingEntity == this)
                {
                    // Success! Snap to the new tile
                    transform.SetParent(hitTile.transform);
                    transform.DOLocalMove(Vector3.zero, 0.2f).SetEase(Ease.OutBack);
                    
                    CurrentGridPosition = hitTile.Position;
                    
                    // Tell the manager the board changed!
                    if (_myGridManager != null) _myGridManager.RecalculateBoard();
                    
                    return true;
                }
            }
        }

        // If we want to add "Return to Hand" logic, it would go right here!
        return false;
    }

    private void RotateEntity()
    {
        // 1. Math Rotation (Update the Vector2Int direction clockwise)
        if (CurrentDirection == Vector2Int.up) CurrentDirection = Vector2Int.right;
        else if (CurrentDirection == Vector2Int.right) CurrentDirection = Vector2Int.down;
        else if (CurrentDirection == Vector2Int.down) CurrentDirection = Vector2Int.left;
        else if (CurrentDirection == Vector2Int.left) CurrentDirection = Vector2Int.up;

        // 2. Visual Rotation (Target Z rotation for our Update loop to smooth towards)
        _targetZRotation -= 90f;

        // Juice it!
        transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), 0.2f, 10, 1);

        // 3. Tell the Grid Manager that the board state has changed
        if (_myGridManager != null)
        {
            _myGridManager.RecalculateBoard();
        }
    }
}