using UnityEngine;

public class RoundStateManager : MonoBehaviour
{
    [Header("References")]
    public EnemySpawner enemySpawner;
    public WaveDataSO[] rounds;
    
    [Header("Runtime")]
    public int currentRoundIndex = 0;

    public RoundBaseState currentState;
    public RoundOverState roundOverState = new RoundOverState();
    public RoundInProgressState roundInProgressState = new RoundInProgressState();

    void Start()
    {
        currentState = roundOverState;
        currentState.EnterState(this);
    }

    void Update()
    {
        currentState?.UpdateState(this);
    }

    private void OnCollisionEnter(Collision other)
    {
        currentState?.OnCollisionEnter(this, other);
    }

    public void SwitchState(RoundBaseState roundBaseState)
    {
        currentState = roundBaseState;
        roundBaseState.EnterState(this);
    }

    public WaveDataSO GetCurrentWaveData()
    {
        if (currentRoundIndex < rounds.Length)
            return rounds[currentRoundIndex];
        
        return null;
    }
}