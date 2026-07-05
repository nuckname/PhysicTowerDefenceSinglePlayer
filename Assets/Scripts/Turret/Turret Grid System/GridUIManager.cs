using System.Collections.Generic;
using UnityEngine;

public class GridUIManager : MonoBehaviour 
{
    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _gridParent; // e.g., a Canvas Panel with a GridLayoutGroup

    private Dictionary<Vector2Int, Tile> _uiTiles;

    [Header("Grid Layout Settings")]
    [SerializeField] private float _tileSize = 100f; // The width/height of your UI Image
    [SerializeField] private Vector2 _gridOffset; // Manual tweak to shift the whole grid
    
    [Header("Inventory Setup")]
    public HorizontalCardHolder inventoryCardHolder;

    [Header("Entity Management")]
    [SerializeField] private GridEntity _entityPrefab; // Drag your new Grid Entity UI Prefab here
    private List<GridEntity> _activeEntities = new List<GridEntity>(); // Keeps track of what is on the board
    private TurretGridData _currentGridData; // Cache this so we can pass it to the cards

    // We remove the Awake/Instance setup. 
    // This is now called right after we Instantiate the prefab.
    public void InitializeAndLoadGrid(TurretGridData MyGridData, List<TurretCard> pendingCards) 
    {
        _currentGridData = MyGridData; // Cache the data
        GenerateUIGrid();

        if (inventoryCardHolder != null)
        {
            inventoryCardHolder.LoadHand(pendingCards);
        }
        
        // Safety check
        if (MyGridData == null || MyGridData.TileStates == null) return;

        // Loop through the UI tiles and update them based on the saved data
        foreach (var kvp in _uiTiles)
        {
            Vector2Int pos = kvp.Key;
            Tile tileUI = kvp.Value;
            
            // Check if the turret has data for this position
            if (MyGridData.TileStates.TryGetValue(pos, out int stateValue))
            {
                tileUI.SetState(stateValue);
            }
            else
            {
                tileUI.ResetState();
            }
        }
    }

    private void GenerateUIGrid() 
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

    // ==========================================
    // NEW ENTITY & MATH LOGIC
    // ==========================================

    /// <summary>
    /// Call this when the player drops a card from their hand onto a valid tile.
    /// </summary>
    public void PlaceCardOnGrid(TurretCard cardData, Vector2Int gridPosition)
    {
        // 1. Double check the tile exists
        if (!_uiTiles.ContainsKey(gridPosition)) return;

        Tile targetTile = _uiTiles[gridPosition];

        // 2. Spawn the entity as a child of the tile so it perfectly overlaps
        GridEntity newEntity = Instantiate(_entityPrefab, targetTile.transform);
        
        // 3. Initialize its data
        newEntity.Initialize(cardData, gridPosition, this);
        
        // 4. Add it to our tracking list
        _activeEntities.Add(newEntity);

        // 5. Run the math!
        RecalculateBoard();
    }

    /// <summary>
    /// The master math function. Wipes the slate clean and asks every piece to recalculate.
    /// </summary>
    public void RecalculateBoard()
    {
        List<StatModifier> allCalculatedModifiers = new List<StatModifier>();

        foreach (GridEntity entity in _activeEntities)
        {
            // Call the abstract CalculateEffect on the ScriptableObject
            List<StatModifier> pieceModifiers = entity.MyCardData.CalculateEffect(
                entity.CurrentGridPosition, 
                entity.CurrentDirection, 
                _currentGridData, 
                _width, 
                _height
            );

            // Add this piece's specific modifiers to the master list
            if (pieceModifiers != null)
            {
                allCalculatedModifiers.AddRange(pieceModifiers);
            }
        }

        // NOTE: Here is where you will eventually hand 'allCalculatedModifiers' 
        // back to the Turret.cs script!
        Debug.Log($"Board Recalculated! Generated {allCalculatedModifiers.Count} total modifiers.");
    }
}