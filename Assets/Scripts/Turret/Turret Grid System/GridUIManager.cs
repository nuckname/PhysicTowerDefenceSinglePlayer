using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridPlacementManager))] 
[RequireComponent(typeof(GridCombatLogic))]
public class GridUIManager : MonoBehaviour 
{
    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _gridParent; 
    
    [SerializeField] private Canvas gridCanvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

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
    public TurretGridData CurrentGridData => _currentGridData; 
    
    private GridPlacementManager _gridPlacementManager;
    private GridCombatLogic _gridCombatLogic;
    private Turret _linkedTurret;
    
    private bool _isInitialized = false;

    private void Awake()
    {
        _gridPlacementManager = GetComponent<GridPlacementManager>();
        _gridCombatLogic = GetComponent<GridCombatLogic>();
        
        gridCanvas = GetComponent<Canvas>();
        graphicRaycaster = GetComponent<GraphicRaycaster>();
    }

    // Call this ONCE when the Turret is built
    public void InitializeGrid(TurretGridData MyGridData, Turret myTurret) 
    {
        if (_isInitialized) return; // Prevent generating the grid twice!

        _linkedTurret = myTurret; 
        _currentGridData = MyGridData;

        _gridPlacementManager.Initialize(myTurret, this, _gridCombatLogic);
        _gridCombatLogic.InitializeGridLogic(MyGridData, this, myTurret);

        GenerateUIGrid();
        
        if (MyGridData != null && MyGridData.TileStates != null)
        {
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
        }

        _isInitialized = true;
    }

    // Call this when opening or closing the UI for this specific turret
    public void SetVisualsActive(bool isVisible, List<GridData> pendingCards = null)
    {
        gridCanvas.enabled = isVisible; 
        graphicRaycaster.enabled = isVisible;

        // If we are opening the UI, load the pending hand
        if (isVisible && inventoryCardHolder != null && pendingCards != null)
        {
            inventoryCardHolder.LoadHand(pendingCards);
        }
    }
    
    public Transform GetTileTransform(Vector2Int gridPosition)
    {
        if (_uiTiles != null && _uiTiles.ContainsKey(gridPosition)) return _uiTiles[gridPosition].transform;
        return null;
    }
    
    public Tile GetTileAt(Vector2Int gridPosition)
    {
        if (_uiTiles != null && _uiTiles.ContainsKey(gridPosition)) return _uiTiles[gridPosition];
        return null;
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
                rect.anchoredPosition = new Vector2(startX + (x * _tileSize), startY + (y * _tileSize));

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
                    // Re-enable the card visually (it will be hidden if the Canvas is disabled anyway)
                    entity.gameObject.SetActive(true);
                    iRoundListener.OnRoundEnd(_gridPlacementManager, _currentGridData, entity); 
                }
            }
        }
    }

    public void RecalculateBoard() => _gridCombatLogic.RecalculateBoard();
    
    public void TrackBouncer(GridEntity bouncer) => _activeBouncers.Add(bouncer);
}