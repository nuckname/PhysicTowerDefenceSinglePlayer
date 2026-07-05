using UnityEngine;
using UnityEngine.UI;
using TMPro; // Assuming you use TextMeshPro for UI text

public class TurretCardUI : MonoBehaviour
{
    [Header("UI References")]
    public Image artworkImage;
    public TextMeshProUGUI nameText;

    // Keep track of which ScriptableObject this UI represents
    public TurretCard MyCardData { get; private set; }

    // Call this right after spawning the prefab
    public void Setup(TurretCard cardData)
    {
        MyCardData = cardData;

        // Apply the ScriptableObject data to the visual UI elements
        if (artworkImage != null) artworkImage.sprite = cardData.CardArtwork;
        if (nameText != null) nameText.text = cardData.CardName;
    }
}