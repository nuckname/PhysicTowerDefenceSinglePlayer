using UnityEngine;

// We make this abstract so it acts purely as a template.
public abstract class TurretCard : ScriptableObject
{
    [Header("Common Info")]
    public string CardName;
    public Sprite CardArtwork;
    public int BaseDamage = 10;

    // By making this abstract, we force EVERY child card to write its own custom math, 
    // while guaranteeing to the rest of the game that this method will always exist.
    public abstract int CalculateEffect(Vector2Int startPos, Vector2Int direction, TurretGridData gridData, int gridWidth, int gridHeight);
}