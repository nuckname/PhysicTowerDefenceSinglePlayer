using System.Collections.Generic;
using UnityEngine;

public enum RotationType
{
    EightWay,
    FourWay
}

public abstract class GridData : ScriptableObject
{
    [Header("Common Info")]
    public string gridName;
    public Sprite girdArtwork;
    public int baseDamage = 10;
    
    [Header("Rotation Settings")]
    [Tooltip("Defines if this piece can rotate in 8 directions (diagonals) or 4 directions (cardinal).")]
    public RotationType allowedRotation = RotationType.EightWay;

    public abstract List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight);

    public virtual void SpawnVisuals(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, GridUIManager uiManager, List<GameObject> spawnedVisualsTracker)
    {
        
    }
}