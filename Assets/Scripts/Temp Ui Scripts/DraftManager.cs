using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DraftManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject DraftScreenContainer; // The parent panel holding the background and buttons

    [Header("References")]
    public GameObject CardUIPrefab; // Drag your physical UI Card Prefab here (from HorizontalCardHolder)
    public DraftButton[] RewardButtons; // Drag your 3 DraftButton objects here

    [Header("Reward Pool")]
    // Drag ALL possible cards in your game into this list in the Inspector
    public List<TurretCard> MasterCardPool = new List<TurretCard>(); 

    private void Start()
    {
        // Ensure the screen is hidden when the game starts
        DraftScreenContainer.SetActive(false);
    }

    private void Update()
    {
        // DEBUG TOOL: Press 'R' to instantly trigger a draft screen
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            OpenDraftScreen();
        }
    }

    /// <summary>
    /// Opens the UI and generates 3 random rewards from the Master Pool.
    /// </summary>
    public void OpenDraftScreen()
    {
        if (MasterCardPool.Count < RewardButtons.Length)
        {
            Debug.LogError("DraftManager: Not enough cards in the Master Pool to fill the buttons!");
            return;
        }

        // 1. Create a temporary copy of the pool so we don't pick duplicates
        List<TurretCard> tempPool = new List<TurretCard>(MasterCardPool);

        // 2. Loop through all 3 buttons and assign them a random card
        foreach (DraftButton button in RewardButtons)
        {
            // Pick a random index from the remaining available cards
            int randomIndex = Random.Range(0, tempPool.Count);
            
            // Setup the button visuals and data
            button.SetupButton(tempPool[randomIndex], this);
            
            // Remove it from the temporary pool so the next button can't pick it
            tempPool.RemoveAt(randomIndex);
        }

        // 3. Turn on the screen and pause the game (optional)
        DraftScreenContainer.SetActive(true);
        Time.timeScale = 0f; // Pauses gameplay behind the UI
    }

    /// <summary>
    /// Called by the DraftButton when the player clicks it.
    /// </summary>
    public void ClaimReward(TurretCard chosenCard, Vector3 buttonScreenPosition)
    {
        if (HorizontalCardHolder.Instance != null)
        {
            // Inject the chosen card into the hand! 
            // We pass the button's position so the card physically flies out of the UI button down into the hand.
            HorizontalCardHolder.Instance.AddCardToHand(CardUIPrefab, chosenCard, buttonScreenPosition);
        }
        else
        {
            Debug.LogError("DraftManager: HorizontalCardHolder instance is missing!");
        }

        // Close the screen and resume the game
        DraftScreenContainer.SetActive(false);
        Time.timeScale = 1f; 
    }
}