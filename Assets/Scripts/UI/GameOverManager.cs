using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    public Button restartButton;
    public Button mainMenuButton;
    public TMPro.TextMeshProUGUI gameOverText;
    
    void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(BackToMainMenu);
            
        if (gameOverText != null)
            gameOverText.text = "Game Over!";
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene("Gameplay");
    }
    
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}