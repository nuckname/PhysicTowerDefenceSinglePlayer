using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Base Targeting Attributes")]
    public float baseRange = 15f;
    public string enemyTag = "Enemy";

    [Header("Base Shooting Attributes")]
    public float baseFireRate = 1f;
    public int baseDamage = 10; // Added base damage
    private float _fireCountdown = 0f;

    [Header("References")]
    public Transform partToRotate;
    public float turnSpeed = 10f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    
    // Runtime variables
    private Transform _target;
    
    // The actual stats used in gameplay
    [HideInInspector] public float currentRange;
    [HideInInspector] public float currentFireRate;
    [HideInInspector] public int currentDamage;

    private void Start()
    {
        // Initialize current stats to match base stats on spawn
        currentRange = baseRange;
        currentFireRate = baseFireRate;
        currentDamage = baseDamage;

        // Call UpdateTarget twice a second instead of every frame to save performance
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
    }

    // NEW METHOD: Called by GridCombatLogic when the grid changes
    public void UpdateModifiers(TurretStats modifiers)
    {
        // Always calculate off the base stat so we don't stack infinitely
        currentRange = baseRange + modifiers.RangeBonus;
        currentFireRate = baseFireRate + modifiers.FireRateBonus;
        currentDamage = baseDamage + modifiers.DamageBonus;
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
        // NOTE: Updated to use 'currentRange' instead of 'range'
        if (nearestEnemy != null && shortestDistance <= currentRange)
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
            // NOTE: Updated to use 'currentFireRate' instead of 'fireRate'
            _fireCountdown = 1f / currentFireRate;
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
            
            // NOTE: You will need to add a method to your Bullet script to receive currentDamage!
            // Example: bullet.SetDamage(currentDamage);
        }
    }

    // Draws a wireframe sphere in the editor so you can easily see the turret's range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // NOTE: Updated to show the modified range in the editor if playing, otherwise base range
        Gizmos.DrawWireSphere(transform.position, Application.isPlaying ? currentRange : baseRange);
    }
}