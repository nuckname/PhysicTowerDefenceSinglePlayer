using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Targeting Attributes")]
    public string enemyTag = "Enemy";

    [Header("Dynamic Stats")]
    // Configure this in the Inspector! Add Damage, Range, FireRate, etc.
    public List<TurretStat> Stats = new List<TurretStat>();

    [Header("References")]
    public Transform partToRotate;
    public float turnSpeed = 10f;
    public GameObject bulletPrefab;
    public Transform firePoint;
    
    // Runtime variables
    private Transform _target;
    private float _fireCountdown = 0f;

    [Header("Pending Inventory")]
    // Cards sitting in the turret, waiting to be placed on the grid
    public List<TurretCard> PendingCards = new List<TurretCard>();


    private void Start()
    {
        // Initialize all stats to their base values
        foreach (var stat in Stats)
        {
            stat.CurrentValue = stat.BaseValue;
        }

        // Call UpdateTarget twice a second instead of every frame to save performance
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.5f);
    }

    public void UpdateModifiers(List<StatModifier> incomingModifiers)
    {
        // 1. Reset everything back to base before applying new grid calculations
        // Not sure if we need this
        foreach (var stat in Stats)
        {
            Debug.Log("Reset everything to base before applying new grid calculations");
            stat.CurrentValue = stat.BaseValue;
        }

        // 2. Loop through every modifier handed to us by the grid
        foreach (var mod in incomingModifiers)
        {
            // 3. Find the matching stat on the turret and add the value
            TurretStat matchingStat = Stats.Find(s => s.Type == mod.Type);
            if (matchingStat != null)
            {
                matchingStat.CurrentValue += mod.Value;
            }
        }
    }

    // Helper method to easily grab a current stat anywhere else in the script
    public float GetStat(StatType type)
    {
        TurretStat matchingStat = Stats.Find(s => s.Type == type);
        return matchingStat != null ? matchingStat.CurrentValue : 0f;
    }
    
    public void AddCardToInventory(TurretCard card)
    {
        if (!PendingCards.Contains(card))
        {
            PendingCards.Add(card);
            Debug.Log($"{card.CardName} added to turret inventory!");
        }
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

        float currentRange = GetStat(StatType.Range);

        // If an enemy is found and is within our range, set it as the target
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
            
            float currentFireRate = GetStat(StatType.FireRate);
            // Prevent division by zero if fire rate is 0
            _fireCountdown = currentFireRate > 0 ? 1f / currentFireRate : 1f;
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
            // NOTE: Pass the calculated damage to the bullet here if your Bullet script supports it
            // Example: bullet.SetDamage(GetStat(StatType.Damage));
        }
    }

    // Draws a wireframe sphere in the editor so you can easily see the turret's range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        
        // If the game is running, show the modified range, otherwise default to a preview or 0
        float rangeToShow = Application.isPlaying ? GetStat(StatType.Range) : 15f; 
        Gizmos.DrawWireSphere(transform.position, rangeToShow);
    }
}