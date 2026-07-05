using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Requires a UI Image component to show the artwork
[RequireComponent(typeof(Image))]
public class GridEntity : MonoBehaviour, IPointerClickHandler
{
    [Header("Entity Data")]
    public TurretCard MyCardData { get; private set; }
    public Vector2Int CurrentGridPosition { get; private set; }
    public Vector2Int CurrentDirection { get; private set; } = Vector2Int.up; // Defaults to facing North (0, 1)

    private GridUIManager _myGridManager;
    private Image _artwork;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _artwork = GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();
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

        // Reset rotation just in case
        _rectTransform.localRotation = Quaternion.identity;
        CurrentDirection = Vector2Int.up;
    }

    // Move the entity to a new tile
    public void UpdatePosition(Vector2Int newPos, Vector3 newWorldPosition)
    {
        CurrentGridPosition = newPos;
        transform.position = newWorldPosition;
    }

    // Using Unity's UI Event System to detect clicks directly on this specific entity
    public void OnPointerClick(PointerEventData eventData)
    {
        // Check if the user Right-Clicked this entity
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            RotateEntity();
        }
    }

    private void RotateEntity()
    {
        // 1. Math Rotation (Update the Vector2Int direction clockwise)
        if (CurrentDirection == Vector2Int.up) CurrentDirection = Vector2Int.right;
        else if (CurrentDirection == Vector2Int.right) CurrentDirection = Vector2Int.down;
        else if (CurrentDirection == Vector2Int.down) CurrentDirection = Vector2Int.left;
        else if (CurrentDirection == Vector2Int.left) CurrentDirection = Vector2Int.up;

        // 2. Visual Rotation (Rotate the UI RectTransform -90 degrees on the Z axis)
        _rectTransform.Rotate(0, 0, -90f);

        // 3. Tell the Grid Manager that the board state has changed so it can recalculate!
        if (_myGridManager != null)
        {
            _myGridManager.RecalculateBoard();
        }
    }
}