using System.Collections.Generic;
using UnityEngine;

// Create a tiny class to hold the exact state of a single placed card
[System.Serializable]
public class PlacedCardSaveState
{
    public GridData CardData;
    public Vector2Int GridPosition;
    public Vector2Int Direction;

    public PlacedCardSaveState(GridData data, Vector2Int pos, Vector2Int dir)
    {
        CardData = data;
        GridPosition = pos;
        Direction = dir;
    }
}

[System.Serializable]
public class TurretGridData
{
    public Dictionary<Vector2Int, int> TileStates = new Dictionary<Vector2Int, int>();
    
    // A list to remember exactly what cards are currently on this turret
    public List<PlacedCardSaveState> SavedCards = new List<PlacedCardSaveState>();
}