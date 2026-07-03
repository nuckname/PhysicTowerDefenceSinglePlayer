using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Implements UI pointer events for Canvas interaction
public class Tile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler 
{
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private Image _renderer;
    [SerializeField] private GameObject _highlight;
    
    // Add a reference to know where this tile is on the grid
    public Vector2Int GridPosition { get; private set; }

    public void Init(bool isOffset, Vector2Int gridPos) 
    {
        _renderer.color = isOffset ? _offsetColor : _baseColor;
        GridPosition = gridPos;
    }

    public void OnPointerEnter(PointerEventData eventData) 
    {
        _highlight.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _highlight.SetActive(false);
    }
}