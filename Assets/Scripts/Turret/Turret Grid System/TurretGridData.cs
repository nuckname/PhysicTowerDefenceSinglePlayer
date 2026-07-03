using System.Collections.Generic;
using UnityEngine;

public class TurretGridData 
{
    // Using Vector2Int is best practice for Dictionaries to avoid floating point precision issues.
    // The integer could represent an item ID, a tile type, or an upgrade index (0 = empty).
    public Dictionary<Vector2Int, int> TileStates = new Dictionary<Vector2Int, int>();
}