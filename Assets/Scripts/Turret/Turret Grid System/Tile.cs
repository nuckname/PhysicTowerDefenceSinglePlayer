using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Implements UI pointer events for Canvas interaction
public class Tile : MonoBehaviour 
{
    [SerializeField] private Image _tileImage;
    
    [Header("Visual States")]
    [SerializeField] private Color _defaultColor = Color.white;
    [SerializeField] private Color _offsetColor = new Color(0.9f, 0.9f, 0.9f); // The darker checkerboard color
    [SerializeField] private Color _occupiedColor = Color.green;
    [SerializeField] private Color _blockedColor = Color.red;
    
    // Add a reference to know where this tile is on the grid
    public Vector2Int GridPosition { get; private set; }
    
    private bool _isOffset; // Store this tile's offset state

    public void Init(bool isOffset, Vector2Int gridPos) 
    {
        GridPosition = gridPos;
        _isOffset = isOffset;
        
        // Apply the correct base color immediately when spawned
        ResetState();
    }
    
    public void SetState(int stateValue)
    {
        // Example: Change color based on the integer state
        switch (stateValue)
        {
            case 1:
                _tileImage.color = _occupiedColor;
                break;
            case 2:
                _tileImage.color = _blockedColor;
                break;
            default:
                ResetState(); // Fallback to the checkerboard logic
                break;
        }

        // If you are using Sprites instead of colors:
        // _tileImage.sprite = GetSpriteFromState(stateValue);
        // _tileImage.enabled = true; 
    }
    
    public void ResetState()
    {
        // Use a ternary operator to switch between the two base colors
        _tileImage.color = _isOffset ? _offsetColor : _defaultColor;
    }
}