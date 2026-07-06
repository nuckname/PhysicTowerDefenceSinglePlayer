using UnityEngine;

public class RoundInProgressState : RoundBaseState
{
    private RoundStateManager _manager;

    public override void EnterState(RoundStateManager roundStateManager)
    {
        _manager = roundStateManager;
        WaveDataSO currentWave = _manager.GetCurrentWaveData();

        if (currentWave != null && _manager.enemySpawner != null)
        {
            Debug.Log($"Starting Round: {currentWave.roundIndex}");
            
            // Subscribe to wave completion
            _manager.enemySpawner.OnWaveCompleted += OnRoundFinished;
            
            // Start spawning
            _manager.enemySpawner.StartWave(currentWave);
            
            RoundStateManager.TriggerRoundStarted();
        }
        else
        {
            Debug.LogWarning("No wave data available or spawner missing! Ending round.");
            _manager.SwitchState(_manager.roundOverState);
        }
    }
    
    public override void UpdateState(RoundStateManager roundStateManager)
    {
        // Handle round-in-progress logic here (e.g., updating UI timers)
    }

    public override void OnCollisionEnter(RoundStateManager roundStateManager, Collision other) { }

    private void OnRoundFinished()
    {
        // Unsubscribe to prevent memory leaks or duplicate calls
        _manager.enemySpawner.OnWaveCompleted -= OnRoundFinished;
        
        // Transition to RoundOverState
        _manager.SwitchState(_manager.roundOverState);
    }
}