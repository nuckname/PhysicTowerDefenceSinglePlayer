using UnityEngine;
using UnityEngine.InputSystem;

public class RoundOverState : RoundBaseState
{
    public override void EnterState(RoundStateManager roundStateManager)
    {
        Debug.Log("Round Over! Preparation phase started.");
        
        // Advance to the next round index for when we start again
        roundStateManager.currentRoundIndex++;
        
        RoundStateManager.TriggerRoundEnded();
    }

    public override void UpdateState(RoundStateManager roundStateManager)
    {
        // Example: Press 'Space' or click a UI button to trigger the next wave
        if (Keyboard.current.spaceKey.isPressed)
        {
            if (roundStateManager.currentRoundIndex < roundStateManager.rounds.Length)
            {
                roundStateManager.SwitchState(roundStateManager.roundInProgressState);
            }
            else
            {
                Debug.Log("All rounds completed! Game Won!");
            }
        }
    }

    public override void OnCollisionEnter(RoundStateManager roundStateManager, Collision other) { }
}