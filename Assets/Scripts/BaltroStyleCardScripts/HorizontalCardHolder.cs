using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.InputSystem; // 1. ADDED NAMESPACE

// https://www.youtube.com/watch?v=I1dAZuWurw4
// https://github.com/mixandjam/balatro-feel

public class HorizontalCardHolder : MonoBehaviour
{
    public static HorizontalCardHolder Instance { get; private set; }

    [SerializeField] private CardMovement selectedCardMovement;
    [SerializeReference] private CardMovement hoveredCardMovement;

    [SerializeField] private GameObject slotPrefab;
    private RectTransform rect;

    [Header("Current Inventory")]
    public List<CardMovement> cardsInHand;

    bool isCrossing = false;
    [SerializeField] private bool tweenCardReturn = true;

    private void Awake()
    {
        // Whenever a new Canvas opens, make THIS the active card holder
        Instance = this; 
    }

    void Start()
    {
        rect = GetComponent<RectTransform>();

        // Layout rebuilding happens after instantiation now, via LoadHand()
    }

    /// <summary>
    /// Called by the GridUIManager when the screen opens to load the turret's real cards.
    /// </summary>
    public void LoadHand(List<GridData> pendingCards)
    {
        // 1. Clean out any dummy slots or previous UI elements
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        cardsInHand.Clear();

        // 2. Loop through the turret's pending inventory and spawn them
        for (int i = 0; i < pendingCards.Count; i++)
        {
            GridData cardData = pendingCards[i];
            
            // Instantiate slot
            GameObject newSlot = Instantiate(slotPrefab, transform);
            
            CardMovement newCardMovement = newSlot.GetComponentInChildren<CardMovement>();

            if (newCardMovement != null)
            {
                newCardMovement.InitializeCardData(cardData);
                RegisterCardEvents(newCardMovement, i);
                cardsInHand.Add(newCardMovement);
            }
        }

        StartCoroutine(Frame());

        IEnumerator Frame()
        {
            // Give the layout group a tiny fraction of a second to arrange the new slots
            yield return new WaitForSecondsRealtime(.1f);
            RebuildHandVisuals();
        }
    }
    
    public void ClearHand()
    {
        // Clear the internal list
        if (cardsInHand != null)
        {
            cardsInHand.Clear();
        }

        // Destroy all the spawned card slots/visuals currently attached to this UI panel
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Safely injects a new card back into the hand from the world 
    /// ensuring it is placed in a Slot and its events are wired correctly.
    /// Now accepts a start position to animate from the mouse cursor!
    /// </summary>
    // 1. ADDED TurretCard cardData parameter
    public void AddCardToHand(GameObject cardPrefabToSpawn, GridData cardData, Vector3? startScreenPosition = null)
    {
        // Instantiate a new slot so the layout group handles it properly
        GameObject newSlot = Instantiate(slotPrefab, transform);
        
        // Instantiate the actual UI card inside it
        GameObject newCardObj = Instantiate(cardPrefabToSpawn, newSlot.transform);
        CardMovement newCardMovement = newCardObj.GetComponent<CardMovement>();

        if (newCardMovement != null)
        {
            // 2. INJECT THE DATA INTO THE CARD!
            newCardMovement.InitializeCardData(cardData);
            
            RegisterCardEvents(newCardMovement, transform.childCount - 1);
        }

        RebuildHandVisuals();

        // Fling the card from the mouse position into its slot!
        if (startScreenPosition.HasValue)
        {
            // Force the UI card to the mouse position
            newCardObj.transform.position = startScreenPosition.Value;
            
            // Animate it back to the center of its slot (Vector3.zero)
            // Using a slightly longer duration (0.3f) so you can actually see it fly into the hand
            newCardObj.transform.DOLocalMove(Vector3.zero, tweenCardReturn ? 0.3f : 0f).SetEase(Ease.OutBack);
        }
    }

    private void RegisterCardEvents(CardMovement card, int index)
    {
        card.PointerEnterEvent.AddListener(CardPointerEnter);
        card.PointerExitEvent.AddListener(CardPointerExit);
        card.BeginDragEvent.AddListener(BeginDrag);
        card.EndDragEvent.AddListener(EndDrag);
        card.name = index.ToString();
    }

    private void BeginDrag(CardMovement cardMovement)
    {
        selectedCardMovement = cardMovement;
    }

    void EndDrag(CardMovement cardMovement)
    {
        if (selectedCardMovement == null)
            return;

        selectedCardMovement.transform.DOLocalMove(selectedCardMovement.selected ? new Vector3(0,selectedCardMovement.selectionOffset,0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

        // Forces layout rebuild trigger
        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;

        selectedCardMovement = null;
    }

    void CardPointerEnter(CardMovement cardMovement)
    {
        hoveredCardMovement = cardMovement;
    }

    void CardPointerExit(CardMovement cardMovement)
    {
        hoveredCardMovement = null;
    }

    void Update()
    {
        HandleCardSwapping();
    }

    private void HandleCardSwapping()
    {
        if (selectedCardMovement == null || isCrossing)
            return;

        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (selectedCardMovement.transform.position.x > cardsInHand[i].transform.position.x)
            {
                if (selectedCardMovement.ParentIndex() < cardsInHand[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }

            if (selectedCardMovement.transform.position.x < cardsInHand[i].transform.position.x)
            {
                if (selectedCardMovement.ParentIndex() > cardsInHand[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Spawns a card and immediately forces it into the user's hand as an active drag,
    /// skipping the "return to slot" animation entirely.
    /// </summary>
    public void SpawnAndDragCard(GameObject cardPrefabToSpawn)
    {
        // Spawn the card directly under the Canvas (transform.parent) instead of inside a slot.
        // This keeps it free-floating and perfectly locked to the mouse coordinates!
        GameObject newCardObj = Instantiate(cardPrefabToSpawn, transform.parent);
        CardMovement newCardMovement = newCardObj.GetComponent<CardMovement>();

        if (newCardMovement != null)
        {
            RegisterCardEvents(newCardMovement, cardsInHand.Count);
            
            // Immediately snap the UI element to the mouse
            // 3. NEW INPUT SYSTEM
            Vector3 pointerPosition = Pointer.current != null ? (Vector3)Pointer.current.position.ReadValue() : Vector3.zero;
            newCardObj.transform.position = pointerPosition;
            
            // Force the card into an active drag state
            newCardMovement.ForceStartDrag();
        }
        
        // We do not rebuild visuals yet because we haven't added a slot to the hand!
    }

    /// <summary>
    /// Safely wraps a floating card into a layout slot when it is finally dropped in the hand.
    /// </summary>
    public void AssignSlotToCard(CardMovement card)
    {
        GameObject newSlot = Instantiate(slotPrefab, transform);
        card.transform.SetParent(newSlot.transform);
        
        RebuildHandVisuals();
    }

    void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = selectedCardMovement.transform.parent;
        Transform crossedParent = cardsInHand[index].transform.parent;

        cardsInHand[index].transform.SetParent(focusedParent);
        cardsInHand[index].transform.localPosition = cardsInHand[index].selected ? new Vector3(0, cardsInHand[index].selectionOffset, 0) : Vector3.zero;
        selectedCardMovement.transform.SetParent(crossedParent);

        isCrossing = false;

        if (cardsInHand[index].cardAnimator == null)
            return;

        bool swapIsRight = cardsInHand[index].ParentIndex() > selectedCardMovement.ParentIndex();
        cardsInHand[index].cardAnimator.Swap(swapIsRight ? -1 : 1);

        //Updated Visual Indexes
        RebuildHandVisuals();
    }

    /// <summary>
    /// Evaluates the current slots in the hand and updates all visual cards 
    /// with their correct sibling index and the new total hand length.
    /// </summary>
    public void RebuildHandVisuals()
    {
        // Wipe the list completely clean
        cardsInHand.Clear();

        // Count the physical slots remaining in the hand
        int currentLength = transform.childCount;

        for (int i = 0; i < currentLength; i++)
        {
            Transform slot = transform.GetChild(i);
            CardMovement cardSlot = slot.GetComponentInChildren<CardMovement>();

            // Re-add only the valid cards that are actually in the slots right now
            if (cardSlot != null)
            {
                cardsInHand.Add(cardSlot);

                if (cardSlot.cardAnimator != null)
                {
                    // Update the visual's sibling index to match the slot's order
                    cardSlot.cardAnimator.transform.SetSiblingIndex(i);

                    // Pass the new length to the animator for curve/spacing math
                    cardSlot.cardAnimator.UpdateLength(currentLength);
                }
            }
        }
    }
}