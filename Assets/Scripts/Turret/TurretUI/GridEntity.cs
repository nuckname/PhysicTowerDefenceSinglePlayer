using UnityEngine;
using UnityEngine.UI;

// Requires a UI Image component to show the artwork
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Canvas))] // Automatically adds a Canvas so we can pop it over other UI elements
public class GridEntity : MonoBehaviour
{
    [Header("Entity Data")]
    public TurretCard MyCardData { get; private set; }
    public Vector2Int CurrentGridPosition { get; private set; }
    public Vector2Int CurrentDirection { get; private set; } = Vector2Int.up; // Defaults to facing North (0, 1)

    // Public getters so our movement script can access them
    public GridUIManager MyGridManager { get; private set; }
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
    public void Initialize(TurretCard cardData, Vector2Int startPos, GridUIManager manager)
    {
        MyCardData = cardData;
        CurrentGridPosition = startPos;
        MyGridManager = manager;

        // Set the UI visual to match the card
        if (cardData.CardArtwork != null)
        {
            Artwork.sprite = cardData.CardArtwork;
        }

        CurrentDirection = Vector2Int.up;

        // Tell the movement script to reset its rotation and scale state
        if (TryGetComponent(out GridEntityMovement movementScript))
        {
            movementScript.ResetMovementState();
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
        if (CurrentDirection == Vector2Int.up) CurrentDirection = Vector2Int.right;
        else if (CurrentDirection == Vector2Int.right) CurrentDirection = Vector2Int.down;
        else if (CurrentDirection == Vector2Int.down) CurrentDirection = Vector2Int.left;
        else if (CurrentDirection == Vector2Int.left) CurrentDirection = Vector2Int.up;
    }

    // NEW: Added counter-clockwise math for the scroll wheel up!
    public void RotateDirectionCounterClockwise()
    {
        if (CurrentDirection == Vector2Int.up) CurrentDirection = Vector2Int.left;
        else if (CurrentDirection == Vector2Int.left) CurrentDirection = Vector2Int.down;
        else if (CurrentDirection == Vector2Int.down) CurrentDirection = Vector2Int.right;
        else if (CurrentDirection == Vector2Int.right) CurrentDirection = Vector2Int.up;
    }
}