using System.Collections.Generic;
using UnityEngine;

public abstract class TurretCard : ScriptableObject
{
    [Header("Common Info")]
    public string CardName;
    public Sprite CardArtwork;
    public int BaseDamage = 10;

    public abstract List<StatModifier> CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight);
}