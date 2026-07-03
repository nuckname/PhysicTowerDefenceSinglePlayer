using UnityEngine;

// The Master List of all possible stats in your entire game
public enum StatType
{
    Damage,
    Range,
    FireRate,
    CritChance,
    PoisonTickRate,
    ArmorPiercing
}

// What the Card hands to the Turret
[System.Serializable]
public struct StatModifier
{
    public StatType Type;
    public float Value;
}

// What the Turret uses to track its own numbers
[System.Serializable]
public class TurretStat
{
    public StatType Type;
    public float BaseValue;
    [HideInInspector] public float CurrentValue;
}