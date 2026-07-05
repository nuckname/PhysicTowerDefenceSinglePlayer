using System.Collections.Generic;
using UnityEngine;

public abstract class TurretCard : ScriptableObject
{
    [Header("Common Info")]
    public string CardName;
    public Sprite CardArtwork;
    public int BaseDamage = 10;

    public abstract List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight);

    // NEW: Virtual method to handle visual spawning. Base does nothing.
    public virtual void SpawnVisuals(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight, Transform gridParent)
    {
        // Default implementation is empty. Override this in specific cards.
    }
}