using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public Button startGameButton;
    public Button quitButton;
    
    void Start()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }
    
    public void StartGame()
    {
        PlayerInventory.ClearInventory();
        MinionManager.ClearRoster();

        SceneManager.LoadScene("ClassSelection");
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
