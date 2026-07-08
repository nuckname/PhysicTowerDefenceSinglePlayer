using UnityEngine;
using UnityEngine.UI;

// Requires a UI Image component to show the artwork
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Canvas))] // Automatically adds a Canvas so we can pop it over other UI elements
public class GridEntity : MonoBehaviour
{
    [Header("Entity Data")]
    public GridData MyCardData { get; private set; }
    public Vector2Int CurrentGridPosition { get; private set; }
    public Vector2Int CurrentDirection { get; private set; } = Vector2Int.up; // Defaults to facing North (0, 1)

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