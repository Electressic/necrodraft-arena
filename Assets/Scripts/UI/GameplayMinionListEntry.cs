using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameplayMinionListEntry : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI minionNameText;
    public TextMeshProUGUI minionLevelText;
    public Image minionIconImage;

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
            minionIconImage.color = Color.gray;
        }
    }
} 