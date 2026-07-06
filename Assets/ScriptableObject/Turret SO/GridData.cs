using System.Collections.Generic;
using UnityEngine;

public abstract class GridData : ScriptableObject
{
    [Header("Common Info")]
    public string gridName;
    public Sprite girdArtwork;
    public int baseDamage = 10;

    public abstract List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight);

    // NEW: Virtual method to handle visual spawning. Base does nothing.
    public virtual void SpawnVisuals(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight, Transform gridParent)
    {
        // Default implementation is empty. Override this in specific cards.
    }
}