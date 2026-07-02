using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    public float speed = 30f;
    public float damage = 25f;

    private Transform _target;

    // Called by the Turret right after the bullet is spawned
    public void Seek(Transform target)
    {
        _target = target;
    }

    private void Update()
    {
        // If the target died while the bullet was in the air, destroy the bullet
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Calculate direction and distance to move this frame
        Vector3 direction = _target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // If the distance to the target is less than our movement step, we hit it
        if (direction.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        // Move the bullet forward and make it look at the target
        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        transform.LookAt(_target);
    }

    private void HitTarget()
    {
        // Grab your existing EnemyHealth script and apply damage
        EnemyHealth enemyHealth = _target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }

        // Optional: Spawn a particle effect here for the impact

        // Destroy the bullet after it hits
        Destroy(gameObject);
    }
}