using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Targeting Attributes")]
    public float range = 15f;
    public string enemyTag = "Enemy";

    [Header("Shooting Attributes")]
    public float fireRate = 1f;
    private float _fireCountdown = 0f;

    [Header("References")]
    public Transform partToRotate;
    public float turnSpeed = 10f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    // Runtime variables
    private Transform _target;

    private void Start()
    {
        // Call UpdateTarget twice a second instead of every frame to save performance
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
    }

    private void UpdateTarget()
    {
        // Find all enemies in the scene
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        
        if (enemies == null)
        {
            return;
        }
        
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        // Loop through enemies to find the closest one
        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        // If an enemy is found and is within our range, set it as the target
        if (nearestEnemy != null && shortestDistance <= range)
        {
            _target = nearestEnemy.transform;
        }
        else
        {
            _target = null;
        }
    }

    private void Update()
    {
        if (_target == null)
            return;

        // 1. Aim at the target
        Vector3 direction = _target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        
        // Smoothly rotate the turret head towards the target (ignoring X and Z tilt)
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        // 2. Handle Shooting
        if (_fireCountdown <= 0f)
        {
            Shoot();
            _fireCountdown = 1f / fireRate;
        }

        _fireCountdown -= Time.deltaTime;
    }

    private void Shoot()
    {
        // Instantiate the bullet
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        
        // Grab the Bullet script and assign the target
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Seek(_target);
        }
    }

    // Draws a wireframe sphere in the editor so you can easily see the turret's range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}