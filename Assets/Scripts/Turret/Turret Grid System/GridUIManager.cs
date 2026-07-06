using System;
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
    
    [Header("Dependencies")]
    [SerializeField] private GridCombatLogic _combatLogic; // Drag your Turret's combat logic component here

    private TurretGridData _currentGridData;
    
    private GridCombatLogic _gridCombatLogic;
    private void Awake()
    {
        _gridCombatLogic = GetComponent<GridCombatLogic>();
    }

    // We remove the Awake/Instance setup. 
    // This is now called right after we Instantiate the prefab.
    public void InitializeAndLoadGrid(TurretGridData MyGridData, List<GridData> pendingCards, Turret myTurret) 
    {
        // Pass the raw data and dimensions over to the logic brain
        if (_combatLogic != null)
        {
            _combatLogic.InitializeGridLogic(MyGridData, _width, _height, myTurret);
        }

        _currentGridData = MyGridData;

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

        LoadSavedBoardState();
    }
    
    /// <summary>
    /// Reads the saved data from the turret and automatically repopulates the visual UI board.
    /// </summary>
    private void LoadSavedBoardState()
    {
        if (_currentGridData == null || _currentGridData.SavedCards == null) return;

        foreach (PlacedCardSaveState savedCard in _currentGridData.SavedCards)
        {
            // 1. Double check the tile still exists in the UI
            if (!_uiTiles.ContainsKey(savedCard.GridPosition)) continue;

            Tile targetTile = _uiTiles[savedCard.GridPosition];

            // 2. Spawn the entity just like we do when a player drops a card
            GridEntity loadedEntity = Instantiate(_entityPrefab, targetTile.transform);
            
            // 3. Initialize it with the saved data
            loadedEntity.Initialize(savedCard.CardData, savedCard.GridPosition, this);
            
            // 4. Force the rotation/direction to match what was saved
            // Assuming you add a quick setter in your GridEntity script:
            loadedEntity.SetDirection(savedCard.Direction); 
            
            // 5. Register it with the combat logic
            if (_combatLogic != null)
            {
                _combatLogic.RegisterEntity(loadedEntity);
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
    public void PlaceCardOnGrid(GridData cardData, Vector2Int gridPosition)
    {
        // 1. Double check the tile exists
        if (!_uiTiles.ContainsKey(gridPosition)) return;

        Tile targetTile = _uiTiles[gridPosition];

        // 2. Spawn the entity as a child of the tile so it perfectly overlaps
        GridEntity newEntity = Instantiate(_entityPrefab, targetTile.transform);
        
        // 3. Initialize its data
        newEntity.Initialize(cardData, gridPosition, this);
        
        // 4. Add it to our tracking list (Now delegated to the Logic Brain)
        // 5. Run the math! (Triggered automatically upon registration)
        if (_combatLogic != null)
        {
            _combatLogic.RegisterEntity(newEntity);
        }
    }

    public void RecalculateBoard()
    {
        _gridCombatLogic.RecalculateBoard();
    }
}