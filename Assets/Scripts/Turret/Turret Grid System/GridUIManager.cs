using System.Collections.Generic;
using UnityEngine;

public class GridUIManager : MonoBehaviour 
{
    public static GridUIManager Instance;

    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _gridParent; // e.g., a Canvas Panel with a GridLayoutGroup

    private Dictionary<Vector2Int, Tile> _uiTiles;
    private Turret _currentActiveTurret;

    [Header("Grid Layout Settings")]
    [SerializeField] private float _tileSize = 100f; // The width/height of your UI Image
    [SerializeField] private Vector2 _gridOffset; // Manual tweak to shift the whole grid
    
    void Awake() 
    {
        Instance = this;
    }

    void GenerateUIGrid() 
    {
        _uiTiles = new Dictionary<Vector2Int, Tile>();

        // 1. Calculate the starting X and Y so the entire grid is perfectly centered 
        // around the _gridParent's pivot point, applying your manual offset on top.
        float startX = -(_width * _tileSize) / 2f + (_tileSize / 2f) + _gridOffset.x;
        float startY = -(_height * _tileSize) / 2f + (_tileSize / 2f) + _gridOffset.y;

        for (int x = 0; x < _width; x++) 
        {
            for (int y = 0; y < _height; y++) 
            {
                var spawnedTile = Instantiate(_tilePrefab, _gridParent);
                spawnedTile.name = $"UI Tile {x} {y}";

                // 2. Grab the RectTransform and apply the calculated position
                RectTransform rect = spawnedTile.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(
                    startX + (x * _tileSize), 
                    startY + (y * _tileSize)
                );

                // 3. Standard init logic
                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                Vector2Int pos = new Vector2Int(x, y);
                spawnedTile.Init(isOffset, pos);
            
                _uiTiles[pos] = spawnedTile;
            }
        }
    }

    // Called when a turret is clicked
    public void LoadTurretGrid(TurretGridData MyGridData) 
    {
        // This is temp as we should have already generated a grid before we have spawned a turret.
        if (_uiTiles == null || _uiTiles.Count == 0)
        {
            GenerateUIGrid();
        }
        
        // Loop through the UI tiles and update them based on the saved data
        foreach (var kvp in _uiTiles)
        {
            Vector2Int pos = kvp.Key;
            Tile tileUI = kvp.Value;
            
            // Check if the turret has data for this position
            if (MyGridData.TileStates.TryGetValue(pos, out int stateValue))
            {
                // Update UI visually based on the stateValue (e.g., show an item, change color)
            }
            else
            {
                // Reset UI to empty/default state
            }
        }
    }
}