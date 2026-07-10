using UnityEngine;

[RequireComponent(typeof(GridEntity))]
public class GridEntityRotation : MonoBehaviour
{
    private GridEntity _gridEntity;
    public RotationType AllowedRotation { get; private set; } = RotationType.EightWay;

    private void Awake()
    {
        _gridEntity = GetComponent<GridEntity>();
    }

    public void Initialize(RotationType rotationType)
    {
        AllowedRotation = rotationType;
    }

    /// <summary>
    /// Rotates the data direction clockwise and returns the visual angle step (45f or 90f).
    /// </summary>
    public float RotateClockwise()
    {
        if (AllowedRotation == RotationType.EightWay)
        {
            if (_gridEntity.CurrentDirection == Vector2Int.up) _gridEntity.CurrentDirection = new Vector2Int(1, 1);
            else if (_gridEntity.CurrentDirection == new Vector2Int(1, 1)) _gridEntity.CurrentDirection = Vector2Int.right;
            else if (_gridEntity.CurrentDirection == Vector2Int.right) _gridEntity.CurrentDirection = new Vector2Int(1, -1);
            else if (_gridEntity.CurrentDirection == new Vector2Int(1, -1)) _gridEntity.CurrentDirection = Vector2Int.down;
            else if (_gridEntity.CurrentDirection == Vector2Int.down) _gridEntity.CurrentDirection = new Vector2Int(-1, -1);
            else if (_gridEntity.CurrentDirection == new Vector2Int(-1, -1)) _gridEntity.CurrentDirection = Vector2Int.left;
            else if (_gridEntity.CurrentDirection == Vector2Int.left) _gridEntity.CurrentDirection = new Vector2Int(-1, 1);
            else if (_gridEntity.CurrentDirection == new Vector2Int(-1, 1)) _gridEntity.CurrentDirection = Vector2Int.up;
            
            return 45f;
        }
        else // FourWay
        {
            if (_gridEntity.CurrentDirection == Vector2Int.up) _gridEntity.CurrentDirection = Vector2Int.right;
            else if (_gridEntity.CurrentDirection == Vector2Int.right) _gridEntity.CurrentDirection = Vector2Int.down;
            else if (_gridEntity.CurrentDirection == Vector2Int.down) _gridEntity.CurrentDirection = Vector2Int.left;
            else if (_gridEntity.CurrentDirection == Vector2Int.left) _gridEntity.CurrentDirection = Vector2Int.up;
            else _gridEntity.CurrentDirection = Vector2Int.up; // Failsafe if it was stuck on a diagonal
            
            return 90f;
        }
    }

    /// <summary>
    /// Rotates the data direction counter-clockwise and returns the visual angle step (45f or 90f).
    /// </summary>
    public float RotateCounterClockwise()
    {
        if (AllowedRotation == RotationType.EightWay)
        {
            if (_gridEntity.CurrentDirection == Vector2Int.up) _gridEntity.CurrentDirection = new Vector2Int(-1, 1);
            else if (_gridEntity.CurrentDirection == new Vector2Int(-1, 1)) _gridEntity.CurrentDirection = Vector2Int.left;
            else if (_gridEntity.CurrentDirection == Vector2Int.left) _gridEntity.CurrentDirection = new Vector2Int(-1, -1);
            else if (_gridEntity.CurrentDirection == new Vector2Int(-1, -1)) _gridEntity.CurrentDirection = Vector2Int.down;
            else if (_gridEntity.CurrentDirection == Vector2Int.down) _gridEntity.CurrentDirection = new Vector2Int(1, -1);
            else if (_gridEntity.CurrentDirection == new Vector2Int(1, -1)) _gridEntity.CurrentDirection = Vector2Int.right;
            else if (_gridEntity.CurrentDirection == Vector2Int.right) _gridEntity.CurrentDirection = new Vector2Int(1, 1);
            else if (_gridEntity.CurrentDirection == new Vector2Int(1, 1)) _gridEntity.CurrentDirection = Vector2Int.up;
            
            return 45f;
        }
        else // FourWay
        {
            if (_gridEntity.CurrentDirection == Vector2Int.up) _gridEntity.CurrentDirection = Vector2Int.left;
            else if (_gridEntity.CurrentDirection == Vector2Int.left) _gridEntity.CurrentDirection = Vector2Int.down;
            else if (_gridEntity.CurrentDirection == Vector2Int.down) _gridEntity.CurrentDirection = Vector2Int.right;
            else if (_gridEntity.CurrentDirection == Vector2Int.right) _gridEntity.CurrentDirection = Vector2Int.up;
            else _gridEntity.CurrentDirection = Vector2Int.up; // Failsafe
            
            return 90f;
        }
    }

    /// <summary>
    /// Applies visual rotation safely using the component's calculated angle.
    /// </summary>
    public void ApplyRotationVisuals(Vector2Int savedDirection)
    {
        _gridEntity.CurrentDirection = savedDirection;

        float angle = 0f;
        if (_gridEntity.CurrentDirection == Vector2Int.up) angle = 0f;
        else if (_gridEntity.CurrentDirection == new Vector2Int(1, 1)) angle = -45f;
        else if (_gridEntity.CurrentDirection == Vector2Int.right) angle = -90f;
        else if (_gridEntity.CurrentDirection == new Vector2Int(1, -1)) angle = -135f;
        else if (_gridEntity.CurrentDirection == Vector2Int.down) angle = -180f;
        else if (_gridEntity.CurrentDirection == new Vector2Int(-1, -1)) angle = 135f; 
        else if (_gridEntity.CurrentDirection == Vector2Int.left) angle = 90f;
        else if (_gridEntity.CurrentDirection == new Vector2Int(-1, 1)) angle = 45f;

        if (TryGetComponent(out GridEntityMovement movementScript))
        {
            movementScript.ForceZRotation(angle);
        }
        
        if (_gridEntity.Artwork != null)
        {
            _gridEntity.Artwork.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}