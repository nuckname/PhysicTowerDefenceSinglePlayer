using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    
    // Track the current health privately
    private float _currentHealth;

    // The event the EnemySpawner is listening for
    public event Action OnDeath;

    // Prevents the death event from triggering multiple times
    private bool _isDead = false;

    private void Awake()
    {
        // Initialize health when the enemy is spawned
        _currentHealth = maxHealth;
    }

    /// <summary>
    /// Call this method from your projectiles or traps when they hit the enemy.
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        // Ignore damage if already dead to prevent duplicate death triggers
        if (_isDead) return;

        _currentHealth -= damageAmount;

        // Check if health has dropped to or below zero
        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        _isDead = true;
        
        // Safely invoke the event. The '?' ensures it only fires if something is listening.
        OnDeath?.Invoke();

        // Remove the enemy from the scene
        // Note: If you upgrade to Object Pooling later, you would disable the object here instead of Destroying it.
        Destroy(gameObject);
    }
}