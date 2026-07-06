using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.InputSystem; 

// https://www.youtube.com/watch?v=I1dAZuWurw4
// https://github.com/mixandjam/balatro-feel

public class CardMovement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("Card Data")]
    public GridData CardData; // The actual stats/upgrade this UI card represents
    
    private Image _imageComponent;
    [SerializeField] private bool instantiateVisual = true;
    private VisualCardsHandler _visualHandler;
    private Camera _mainCamera; // Added to handle 3D Raycasting
    
    private Vector3 offset; 

    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 3000; 

    [Header("Selection")]
    public bool selected;
    public float selectionOffset = 50;
    private float _pointerDownTime;
    private float _pointerUpTime;

    [Header("Visual")]
    [SerializeField] private GameObject cardVisualPrefab;
    [HideInInspector] public CardAnimator cardAnimator;

    [Header("Play Area Threshold")]
    [Tooltip("How high up the screen we the spawn the pin, in pixels")]
    [SerializeField] private float playAreaThresholdY = 500f; 
    
    [HideInInspector] public bool isPreviewingInWorld = false;

    [Header("States")]
    public bool isHovering;
    public bool isDragging;
    [HideInInspector] public bool wasDragged;

    [Header("Events")]
    [HideInInspector] public UnityEvent<CardMovement> PointerEnterEvent;
    [HideInInspector] public UnityEvent<CardMovement> PointerExitEvent;
    [HideInInspector] public UnityEvent<CardMovement, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<CardMovement> PointerDownEvent;
    [HideInInspector] public UnityEvent<CardMovement> BeginDragEvent;
    [HideInInspector] public UnityEvent<CardMovement> EndDragEvent;
    [HideInInspector] public UnityEvent<CardMovement, bool> SelectEvent;
    
    [HideInInspector] public UnityEvent OnEnterPlayArea;
    [HideInInspector] public UnityEvent OnExitPlayArea;
    [HideInInspector] public UnityEvent OnDropInPlayArea;

    private void Awake()
    {
        _imageComponent = GetComponent<Image>();
        _visualHandler = GameObject.FindGameObjectWithTag("VisualCardHandler").GetComponent<VisualCardsHandler>();
        _mainCamera = FindAnyObjectByType<Camera>(); // Cache the camera for the raycast
    }

    void Start()
    {
        if (!instantiateVisual)
            return;

        cardAnimator = Instantiate(cardVisualPrefab, _visualHandler.transform).GetComponent<CardAnimator>();
        cardAnimator.Initialize(this);
    }

    // Call this right after spawning the card to give it its data and artwork
    public void InitializeCardData(GridData data)
    {
        CardData = data;
        if (_imageComponent != null && data.girdArtwork != null)
        {
            _imageComponent.sprite = data.girdArtwork;
        }
    }

    public void ForceStartDrag()
    {
        BeginDragEvent.Invoke(this); 
        offset = Vector3.zero; 
        isDragging = true;
        wasDragged = true;
        _imageComponent.raycastTarget = false; 
    }

    void Update()
    {
        ClampPosition();

        if (isDragging)
        {
            if (!isPreviewingInWorld)
            {
                Vector3 pointerPosition = Pointer.current != null ? (Vector3)Pointer.current.position.ReadValue() : Vector3.zero;
                transform.position = pointerPosition - offset;
            }

            HandlePlayAreaTransition();

            // We must manually pass null or recreate event data here for custom forced drops
            if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                // To be safe with UI raycasts, we let the real OnEndDrag handle this naturally,
                // but if forced, we trigger it.
                if (isDragging) 
                {
                    PointerEventData pointerData = new PointerEventData(EventSystem.current)
                    {
                        position = Pointer.current.position.ReadValue()
                    };
                    OnEndDrag(pointerData); 
                }
            }
        }
    }

    public void OnDrag(PointerEventData eventData) { }

    private void HandlePlayAreaTransition()
    {
        // NEW: If a Turret Grid is currently open, we don't want the card turning invisible 
        // while we are dragging it up to place it on the UI grid!
        if (FindAnyObjectByType<GridUIManager>() != null)
        {
            return; 
        }

        Vector3 pointerPosition = Pointer.current != null ? (Vector3)Pointer.current.position.ReadValue() : Vector3.zero;
        bool isMouseInPlayArea = pointerPosition.y > playAreaThresholdY;

        if (isMouseInPlayArea && !isPreviewingInWorld)
        {
            isPreviewingInWorld = true;
            _imageComponent.enabled = false;
            cardAnimator.gameObject.SetActive(false);
            OnEnterPlayArea?.Invoke();
        }
        else if (!isMouseInPlayArea && isPreviewingInWorld)
        {
            isPreviewingInWorld = false;
            _imageComponent.enabled = true;
            cardAnimator.gameObject.SetActive(true);
            OnExitPlayArea?.Invoke();
        }
    }

    void ClampPosition()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, 0, Screen.width);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, 0, Screen.height);
        transform.position = clampedPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragEvent.Invoke(this);
        Vector3 pointerPosition = Pointer.current != null ? (Vector3)Pointer.current.position.ReadValue() : Vector3.zero;
        offset = pointerPosition - transform.position;
        isDragging = true;
        _imageComponent.raycastTarget = false;
        wasDragged = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        // 1. Try dropping on the UI Grid FIRST
        if (AttemptPlayOnGrid(eventData))
        {
            EndDragEvent.Invoke(this);
            return; // Successfully placed on the grid, stop here!
        }
        
        // 2. Try dropping in the 3D World (Turret Phase 1)
        if (isPreviewingInWorld)
        {
            EndDragEvent.Invoke(this);
            OnDropInPlayArea?.Invoke();
            
            // We dropped the card in the world! Let's see if we hit a turret.
            AttemptPlayOnTurret();
        }
        else
        {
            // 3. Missed everything, return to hand
            if (transform.parent != null && !transform.parent.CompareTag("Slot") && HorizontalCardHolder.Instance != null)
            {
                HorizontalCardHolder.Instance.AssignSlotToCard(this);
            }

            EndDragEvent.Invoke(this);
            ReturnToHand();
        }
    }

    // ==========================================
    // NEW: The core logic for checking the UI Grid beneath the mouse
    // ==========================================
    private bool AttemptPlayOnGrid(PointerEventData eventData)
    {
        if (eventData == null || CardData == null) return false;

        // Fire a raycast through all UI elements under the mouse
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            // Check if we hit a specific UI Tile
            Tile hitTile = result.gameObject.GetComponent<Tile>();
            if (hitTile != null)
            {
                GridUIManager gridManager = FindAnyObjectByType<GridUIManager>();
                if (gridManager != null)
                {
                    gridManager.PlaceCardOnGrid(CardData, hitTile.Position);
                    
                    Debug.Log($"Dropped {CardData.gridName} onto UI Grid!");
                    SuccessfulPlay();
                    return true;
                }
            }
        }

        return false;
    }

    // 2. The core logic for checking the 3D world beneath the mouse
    private void AttemptPlayOnTurret()
    {
        if (CardData == null)
        {
            Debug.LogWarning("Card dropped, but it has no CardData assigned!");
            ReturnToHand();
            return;
        }

        Vector2 mousePosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        Ray ray = _mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.collider.CompareTag("Turret"))
            {
                Turret targetTurret = hit.collider.GetComponent<Turret>();

                if (targetTurret != null)
                {
                    Debug.Log($"Dropped {CardData.gridName} onto {targetTurret.gameObject.name}!");
                    
                    // Add the card to the turret's pending inventory
                    targetTurret.AddCardToInventory(CardData);
                    
                    // The play was successful, destroy the UI card
                    SuccessfulPlay();
                    return; 
                }
            }
        }

        // If we missed the turret, or clicked the ground, bounce back to the hand
        Debug.Log("Missed the turret. Returning to hand.");
        ReturnToHand();
    }

    public void ReturnToHand()
    {
        Debug.Log("CardMovement Return to hand");
        isPreviewingInWorld = false;

        _imageComponent.enabled = true;
        if (cardAnimator != null) 
            cardAnimator.gameObject.SetActive(true);

        _imageComponent.raycastTarget = true;

        StartCoroutine(FrameWait());
    }

    private IEnumerator FrameWait()
    {
        yield return new WaitForEndOfFrame();
        wasDragged = false;
    }

    public void SuccessfulPlay()
    {
        if (HorizontalCardHolder.Instance != null)
        {
            HorizontalCardHolder.Instance.cardsInHand.Remove(this);
        }

        if (transform.parent != null && transform.parent.CompareTag("Slot"))
        {
            transform.parent.SetParent(null);
            Destroy(transform.parent.gameObject);
        }
        
        if (HorizontalCardHolder.Instance != null)
        {
            HorizontalCardHolder.Instance.RebuildHandVisuals();
        }

        Destroy(gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke(this);
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitEvent.Invoke(this);
        isHovering = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        PointerDownEvent.Invoke(this);
        _pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        _pointerUpTime = Time.time;
        PointerUpEvent.Invoke(this, _pointerUpTime - _pointerDownTime > .2f);

        if (_pointerUpTime - _pointerDownTime > .2f) return;
        if (wasDragged) return;

        selected = !selected;
        SelectEvent.Invoke(this, selected);

        if (selected)
            transform.localPosition += (cardAnimator.transform.up * selectionOffset);
        else
            transform.localPosition = Vector3.zero;
    }

    public void Deselect()
    {
        if (selected)
        {
            selected = false;
            transform.localPosition = Vector3.zero;
        }
    }

    public int SiblingAmount()
    {
        return transform.parent != null && transform.parent.parent != null && transform.parent.CompareTag("Slot") ? transform.parent.parent.childCount - 1 : 0;
    }

    public int ParentIndex()
    {
        return transform.parent != null && transform.parent.CompareTag("Slot") ? transform.parent.GetSiblingIndex() : 0;
    }

    public float NormalizedPosition()
    {
        return transform.parent != null && transform.parent.parent != null && transform.parent.CompareTag("Slot") ? ExtensionMethods.Remap((float)ParentIndex(), 0, (float)(transform.parent.parent.childCount - 1), 0, 1) : 0;
    }

    private void OnDestroy()
    {
        if(cardAnimator != null)
            Destroy(cardAnimator.gameObject);
    }
}