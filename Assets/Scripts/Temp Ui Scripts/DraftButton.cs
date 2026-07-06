using UnityEngine;
using UnityEngine.UI;
using TMPro; // Assuming you use TextMeshPro for UI text

public class DraftButton : MonoBehaviour
{
    [Header("UI References")]
    public Image CardIcon;
    public TMP_Text CardNameText;
    
    // The specific data this button currently holds
    private GridData _assignedCardData;
    private DraftManager _manager;

    // The manager calls this to set up the button visually
    public void SetupButton(GridData cardData, DraftManager manager)
    {
        _assignedCardData = cardData;
        _manager = manager;

        if (CardIcon != null && cardData.girdArtwork != null)
        {
            CardIcon.sprite = cardData.girdArtwork;
        }

        if (CardNameText != null)
        {
            CardNameText.text = cardData.gridName;
        }
    }

    // Link this to the Unity UI Button "On Click ()" event in the Inspector!
    public void OnClickClaimCard()
    {
        if (_assignedCardData == null || _manager == null) return;

        // Tell the manager we picked this card so it can close the screen
        _manager.ClaimReward(_assignedCardData, transform.position);
    }
}