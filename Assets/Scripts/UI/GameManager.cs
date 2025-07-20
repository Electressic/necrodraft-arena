using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public bool gameRunning = true;
    
    [Header("UI References")]
    public Button backToMenuButton;
    public TMPro.TextMeshProUGUI waveText;
    public TMPro.TextMeshProUGUI actText;
    public TMPro.TextMeshProUGUI progressText;
    
    void Start()
    {
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(BackToMainMenu);
            
        UpdateUI();
        
        Debug.Log("Gameplay scene loaded - prototype ready!");
    }
    
    void UpdateUI()
    {
        int currentWave = GameData.GetCurrentWave();
        
        if (waveText != null)
            waveText.text = $"Wave {currentWave}";
        
        if (actText != null)
        {
            string actInfo = GetActInfo(currentWave);
            actText.text = actInfo;
        }
        
        if (progressText != null)
        {
            string progressInfo = GetProgressionInfo(currentWave);
            progressText.text = progressInfo;
        }
    }
    
    string GetActInfo(int wave)
    {
        if (wave <= 7)
            return "ACT 1: Foundation";
        else if (wave <= 14)
            return "ACT 2: Mastery";
        else if (wave <= 20)
            return "ACT 3: Endgame";
        else
            return "BEYOND";
    }
    
    string GetProgressionInfo(int wave)
    {
        int currentMinions = MinionManager.GetMinionCount();
        int maxMinions = MinionManager.GetCurrentMaxMinions();
        
        string info = $"Minions: {currentMinions}/{maxMinions}";
        
        if (wave < 17)
        {
            int nextUnlockWave = GetNextMinionUnlockWave(wave);
            if (nextUnlockWave > 0)
            {
                info += $" • Next unlock: Wave {nextUnlockWave}";
            }
        }
        
        string rarityInfo = GetCurrentRarityInfo(wave);
        if (!string.IsNullOrEmpty(rarityInfo))
        {
            info += $"\n{rarityInfo}";
        }
        
        return info;
    }
    
    int GetNextMinionUnlockWave(int currentWave)
    {
        if (currentWave < 5) return 5;
        if (currentWave < 9) return 9;
        if (currentWave < 13) return 13;
        if (currentWave < 17) return 17;
        return 0;
    }
    
    string GetCurrentRarityInfo(int wave)
    {
        if (wave <= 3)
            return "Rarity: Common parts only";
        else if (wave <= 7)
            return "Rarity: Common + Uncommon parts";
        else if (wave <= 10)
            return "Rarity: Rare parts appearing";
        else if (wave <= 14)
            return "Rarity: Mostly Rare parts";
        else if (wave <= 18)
            return "Rarity: Epic parts available";
        else if (wave == 19)
            return "🏆 TREASURE WAVE: All Epic parts!";
        else if (wave == 20)
            return "⚔️ FINAL BOSS";
        else
            return "";
    }
    
    public void RefreshWaveDisplay()
    {
        UpdateUI();
    }
    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
