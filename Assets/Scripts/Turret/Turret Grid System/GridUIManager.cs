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
    
    private List<GridEntity> _activeBouncers = new List<GridEntity>();
    
    // NEW: Add a slot for your bouncing prefab
    [SerializeField] private GridEntity _bouncingPrefab; 
    
    [Header("Dependencies")]
    [SerializeField] private GridCombatLogic _combatLogic; // Drag your Turret's combat logic component here

    private TurretGridData _currentGridData;
    
    public int GridWidth => _width;
    public int GridHeight => _height;
    
    private GridCombatLogic _gridCombatLogic;
    private Turret _linkedTurret; // We need to store this so the bouncing item can access it
    
    private void Awake()
    {
        _gridCombatLogic = GetComponent<GridCombatLogic>();
    }

    // We remove the Awake/Instance setup. 
    // This is now called right after we Instantiate the prefab.
    public void InitializeAndLoadGrid(TurretGridData MyGridData, List<GridData> pendingCards, Turret myTurret) 
    {
        _linkedTurret = myTurret; // Store it for later

        // Pass the raw data and dimensions over to the logic brain
        if (_combatLogic != null)
        {
            _combatLogic.InitializeGridLogic(MyGridData, this, myTurret);
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
    /// Returns the transform of a specific UI tile so we can parent entities to it.
    /// </summary>
    public Transform GetTileTransform(Vector2Int gridPosition)
    {
        if (_uiTiles != null && _uiTiles.ContainsKey(gridPosition))
        {
            return _uiTiles[gridPosition].transform;
        }
        return null;
    }
    
    /// <summary>
    /// Reads the saved data from the turret and automatically repopulates the visual UI board.
    /// </summary>
    private void LoadSavedBoardState()
    {
        if (_currentGridData == null || _currentGridData.SavedCards == null) return;

        foreach (PlacedCardSaveState savedCard in _currentGridData.SavedCards)
        {
            // We can now use our new modular spawner here too!
            GridEntity loadedEntity = SpawnEntityOnTile(_entityPrefab, savedCard.CardData, savedCard.GridPosition);
            
            if (loadedEntity != null)
            {
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
    /// Handles the physical instantiation and parenting of ANY entity onto a grid tile.
    /// Does NOT trigger combat math. Safe for visual effects, lasers, and bouncing items.
    /// </summary>
    public GridEntity SpawnEntityOnTile(GridEntity prefabToSpawn, GridData cardData, Vector2Int gridPosition)
    {
        // 1. Double check the tile exists
        if (!_uiTiles.ContainsKey(gridPosition)) return null;

        Tile targetTile = _uiTiles[gridPosition];

        // 2. Spawn the entity as a child of the tile so it perfectly overlaps
        GridEntity newEntity = Instantiate(prefabToSpawn, targetTile.transform);
        
        // 3. Initialize its data
        newEntity.Initialize(cardData, gridPosition, this);

        return newEntity;
    }

    /// <summary>
    /// Call this when the player drops a card from their hand onto a valid tile.
    /// </summary>
    public void PlaceCardOnGrid(GridData cardData, Vector2Int gridPosition)
    {
        // 1. Reuse our new modular spawner
        GridEntity newCardEntity = SpawnEntityOnTile(_entityPrefab, cardData, gridPosition);
        
        // 4. Add it to our tracking list (Now delegated to the Logic Brain)
        // 5. Run the math! (Triggered automatically upon registration)
        if (newCardEntity != null && _combatLogic != null)
        {
            _linkedTurret.PendingCards.Remove(cardData);
            _combatLogic.RegisterEntity(newCardEntity);
        }
    }

    /// <summary>
    /// Spawns a bouncing physics item on the grid and starts its independent loop.
    /// Notice how we DO NOT register it to the _combatLogic!
    /// </summary>
    public void SpawnBouncingItem(GridData itemData, Vector2Int startPos)
    {
        if (_bouncingPrefab == null) return;

        GridEntity bouncer = SpawnEntityOnTile(_bouncingPrefab, itemData, startPos);

        if (bouncer != null)
        {
            if (bouncer.TryGetComponent(out GridBouncingMovement bounceScript))
            {
                bounceScript.Launch(_linkedTurret, _currentGridData);
                
                _activeBouncers.Add(bouncer); 
            }
        }
    }
    
    private void OnEnable()
    {
        RoundStateManager.OnRoundStarted += HandleRoundStarted;
        RoundStateManager.OnRoundEnded += HandleRoundEnded;
    }

    private void OnDisable()
    {
        RoundStateManager.OnRoundStarted -= HandleRoundStarted;
        RoundStateManager.OnRoundEnded -= HandleRoundEnded;
    }

    private void HandleRoundStarted()
    {
        if (_gridCombatLogic == null) return;

        // Loop through the ACTUAL spawned entities on the board
        foreach (GridEntity entity in _gridCombatLogic.ActiveEntities)
        {
            if(entity.MyCardData is IRoundListener iRoundListener)
            {
                iRoundListener.OnRoundStart(this, entity);
            }
        }
    }

    private void HandleRoundEnded()
    {
        // 1. Destroy all the bouncing orbs
        foreach (GridEntity bouncer in _activeBouncers)
        {
            if (bouncer != null) Destroy(bouncer.gameObject);
        }
        _activeBouncers.Clear();

        // 2. Loop through all the hidden base entities and turn them back on!
        if (_gridCombatLogic != null)
        {
            foreach (GridEntity entity in _gridCombatLogic.ActiveEntities)
            {
                if(_gridCombatLogic.ActiveEntities is IRoundListener iRoundListener)
                {
                    // Turn the visual UI card back on
                    entity.gameObject.SetActive(true);
                
                    // Trigger the end round event just in case any cards need it
                    iRoundListener.OnRoundEnd(this, entity); 
                }

            }
        }
    }

    public void RecalculateBoard()
    {
        _gridCombatLogic.RecalculateBoard();
    }
}