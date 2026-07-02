using System;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spawnPoint;
    
    private WaveDataSO _currentWaveData;

    // Runtime state
    private int _enemiesSpawnedThisRound = 0;
    private int _activeEnemiesCount = 0;
    private bool _isSpawning = false;

    public Action OnWaveCompleted; // Fired when all enemies are spawned AND destroyed

    public void StartWave(WaveDataSO waveData)
    {
        _currentWaveData = waveData;
        _enemiesSpawnedThisRound = 0;
        _activeEnemiesCount = 0;
        _isSpawning = true;

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (_enemiesSpawnedThisRound < _currentWaveData.totalEnemiesToSpawn && _isSpawning)
        {
            SpawnEnemy();
            _enemiesSpawnedThisRound++;
            
            yield return new WaitForSeconds(_currentWaveData.timeBetweenSpawns);
        }

        _isSpawning = false;
    }

    private void SpawnEnemy()
    {
        if (_currentWaveData.enemyPrefab == null) return;

        GameObject enemyInstance = Instantiate(_currentWaveData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        _activeEnemiesCount++;

        // Hook into the enemy's destruction event to track active enemies
        // Assuming your enemy script calls OnEnemyDestroyed when it dies or reaches the goal
        EnemyHealth enemyHealth = enemyInstance.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
           enemyHealth.OnDeath += HandleEnemyDestroyed;
        }
    }

    // Should do this in FSM
    public void HandleEnemyDestroyed()
    {
        _activeEnemiesCount--;

        // Check if all enemies were spawned and none remain on the map
        if (!_isSpawning && _enemiesSpawnedThisRound >= _currentWaveData.totalEnemiesToSpawn && _activeEnemiesCount <= 0)
        {
            OnWaveCompleted?.Invoke();
        }
    }

    public void StopSpawning()
    {
        _isSpawning = false;
        StopAllCoroutines();
    }
}