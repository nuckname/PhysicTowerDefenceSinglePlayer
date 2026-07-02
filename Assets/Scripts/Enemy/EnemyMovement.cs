using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    
    // Distance check to determine when we've "reached" a waypoint
    private readonly float _waypointThreshold = 0.2f; 
    
    private Transform _targetWaypoint;
    private int _waypointIndex = 0;
    private EnemyHealth _health;

    private void Start()
    {
        _health = GetComponent<EnemyHealth>();

        // Ensure there are waypoints to follow
        if (Waypoints.points == null || Waypoints.points.Length == 0)
        {
            Debug.LogError("No waypoints found! Make sure your Waypoints object is active in the scene.");
            return;
        }

        // Set the first target
        _targetWaypoint = Waypoints.points[0];
    }

    private void Update()
    {
        MoveTowardsWaypoint();
    }

    private void MoveTowardsWaypoint()
    {
        if (_targetWaypoint == null) return;

        // Calculate direction and move
        Vector3 direction = _targetWaypoint.position - transform.position;
        transform.Translate(direction.normalized * speed * Time.deltaTime, Space.World);

        // Check if we are close enough to the target waypoint to switch to the next one
        if (Vector3.Distance(transform.position, _targetWaypoint.position) <= _waypointThreshold)
        {
            GetNextWaypoint();
        }
    }

    private void GetNextWaypoint()
    {
        // Check if this was the last waypoint
        if (_waypointIndex >= Waypoints.points.Length - 1)
        {
            EndPath();
            return;
        }

        // Increment index and set new target
        _waypointIndex++;
        _targetWaypoint = Waypoints.points[_waypointIndex];
        
        // Optional: Make the enemy face the waypoint
        transform.LookAt(_targetWaypoint);
    }

    private void EndPath()
    {
        // TODO: Subtract health/lives from the player's base here.
        Debug.Log("Enemy reached the base!");

        // We use TakeDamage to kill the enemy so the OnDeath event fires.
        // This ensures the EnemySpawner correctly updates its _activeEnemiesCount.
        if (_health != null)
        {
            _health.TakeDamage(_health.maxHealth);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}