using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayMinionListEntry : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI minionNameText;
    public TextMeshProUGUI minionLevelText;
    public Image minionIconImage; // We can use this for a portrait later

    private Minion representedMinion;

    public void Initialize(Minion minion)
    {
        representedMinion = minion;
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (representedMinion == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (minionNameText != null)
        {
            minionNameText.text = representedMinion.minionName;
        }

        if (minionLevelText != null)
        {
            minionLevelText.text = $"Lvl: {representedMinion.level}";
        }

        if (minionIconImage != null)
        {
            // For now, we'll just use a default color.
            // Later, we can assign a specific sprite for each minion.
            minionIconImage.color = Color.gray;
        }
    }
} 