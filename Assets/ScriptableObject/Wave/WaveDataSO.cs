using UnityEngine;

[CreateAssetMenu(fileName = "NewWaveData", menuName = "Tower Defense/Wave Data")]
public class WaveDataSO : ScriptableObject
{
    [Header("Round Info")]
    public int roundIndex = 1;
    
    [Header("Spawning Configuration")]
    public GameObject enemyPrefab;
    public int totalEnemiesToSpawn = 10;
    public float timeBetweenSpawns = 1.0f;
}