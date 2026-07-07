using System;
using System.Collections.Generic;
using UnityEngine;

// NEW: This forces Unity to add these scripts automatically. No more missing component errors!
[RequireComponent(typeof(GridPlacementManager))] 
[RequireComponent(typeof(GridCombatLogic))]
public class GridUIManager : MonoBehaviour 
{
    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _gridParent; 

    private Dictionary<Vector2Int, Tile> _uiTiles;

    [Header("Grid Layout Settings")]
    [SerializeField] private float _tileSize = 100f; 
    [SerializeField] private Vector2 _gridOffset; 
    
    [Header("Inventory Setup")]
    public HorizontalCardHolder inventoryCardHolder;

    private List<GridEntity> _activeBouncers = new List<GridEntity>();
    
    private TurretGridData _currentGridData;
    
    public int GridWidth => _width;
    public int GridHeight => _height;
    public TurretGridData CurrentGridData => _currentGridData; // Public getter for the cards!
    
    private GridPlacementManager _gridPlacementManager;
    private GridCombatLogic _gridCombatLogic;
    private Turret _linkedTurret; 
    
    private void Awake()
    {
        // Grab sibling components automatically
        _gridPlacementManager = GetComponent<GridPlacementManager>();
        _gridCombatLogic = GetComponent<GridCombatLogic>();
    }

    public void InitializeAndLoadGrid(TurretGridData MyGridData, List<GridData> pendingCards, Turret myTurret) 
    {
        _linkedTurret = myTurret; 

        // 1. INITIALIZE THE PLACEMENT MANAGER (This was missing and causing errors!)
        _gridPlacementManager.Initialize(myTurret, this, _gridCombatLogic);

        // 2. Initialize Combat Logic
        _gridCombatLogic.InitializeGridLogic(MyGridData, this, myTurret);

        _currentGridData = MyGridData;

        GenerateUIGrid();

        if (inventoryCardHolder != null)
        {
            inventoryCardHolder.LoadHand(pendingCards);
        }
        
        if (MyGridData == null || MyGridData.TileStates == null) return;

        foreach (var kvp in _uiTiles)
        {
            Vector2Int pos = kvp.Key;
            Tile tileUI = kvp.Value;
            
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
    
    public Transform GetTileTransform(Vector2Int gridPosition)
    {
        if (_uiTiles != null && _uiTiles.ContainsKey(gridPosition))
        {
            return _uiTiles[gridPosition].transform;
        }
        return null;
    }
    
    public Tile GetTileAt(Vector2Int gridPosition)
    {
        if (_uiTiles != null && _uiTiles.ContainsKey(gridPosition))
        {
            return _uiTiles[gridPosition];
        }
        return null;
    }

    private void LoadSavedBoardState()
    {
        if (_currentGridData == null || _currentGridData.SavedCards == null) return;

        foreach (PlacedCardSaveState savedCard in _currentGridData.SavedCards)
        {
            // NEW: Pull the default prefab directly from the placement manager
            GridEntity loadedEntity = _gridPlacementManager.SpawnEntityOnTile(
                _gridPlacementManager.DefaultEntityPrefab, 
                savedCard.CardData, 
                savedCard.GridPosition, 
                true
            );
            
            if (loadedEntity != null)
            {
                loadedEntity.SetDirection(savedCard.Direction); 
                _gridCombatLogic.RegisterEntity(loadedEntity);
            }
        }
    }

    private void GenerateUIGrid() 
    {
        _uiTiles = new Dictionary<Vector2Int, Tile>();

        float startX = -(_width * _tileSize) / 2f + (_tileSize / 2f) + _gridOffset.x;
        float startY = -(_height * _tileSize) / 2f + (_tileSize / 2f) + _gridOffset.y;

        for (int x = 0; x < _width; x++) 
        {
            for (int y = 0; y < _height; y++) 
            {
                var spawnedTile = Instantiate(_tilePrefab, _gridParent);
                spawnedTile.name = $"UI Tile {x} {y}";

                RectTransform rect = spawnedTile.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(
                    startX + (x * _tileSize), 
                    startY + (y * _tileSize)
                );

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                Vector2Int pos = new Vector2Int(x, y);
                spawnedTile.Init(isOffset, pos);
            
                _uiTiles[pos] = spawnedTile;
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

        foreach (GridEntity entity in _gridCombatLogic.ActiveEntities)
        {
            if(entity.MyCardData is IRoundListener iRoundListener)
            {
                iRoundListener.OnRoundStart(_gridPlacementManager, _currentGridData, entity);
            }
        }
    }

    private void HandleRoundEnded()
    {
        foreach (GridEntity bouncer in _activeBouncers)
        {
            if (bouncer != null) Destroy(bouncer.gameObject);
        }
        _activeBouncers.Clear();

        if (_gridCombatLogic != null)
        {
            foreach (GridEntity entity in _gridCombatLogic.ActiveEntities)
            {
                if(entity.MyCardData is IRoundListener iRoundListener)
                {
                    entity.gameObject.SetActive(true);
                    iRoundListener.OnRoundEnd(_gridPlacementManager, _currentGridData, entity); 
                }
            }
        }
    }

    public void RecalculateBoard()
    {
        _gridCombatLogic.RecalculateBoard();
    }
    
    // NEW: Allow the Placement Manager to track active bouncers securely
    public void TrackBouncer(GridEntity bouncer)
    {
        _activeBouncers.Add(bouncer);
    }
}