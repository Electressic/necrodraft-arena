using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public int currentWave = 1;
    public bool gameRunning = true;
    
    [Header("UI References")]
    public Button backToMenuButton;
    public TMPro.TextMeshProUGUI waveText;
    
    void Start()
    {
        if (backToMenuButton != null)
            backToMenuButton.onClick.AddListener(BackToMainMenu);
            
        UpdateUI();
        
        Debug.Log("Gameplay scene loaded - prototype ready!");
    }
    
    void UpdateUI()
    {
        if (waveText != null)
            waveText.text = $"Wave: {currentWave}";
    }
    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
