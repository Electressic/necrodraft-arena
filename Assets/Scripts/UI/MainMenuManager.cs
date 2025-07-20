using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    public Button startGameButton;
    public Button continueButton;
    public Button settingsButton;
    public Button quitButton;
    
    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Button closeSettingsButton;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip buttonClickSound;
    public AudioClip buttonOpenSound;
    public AudioClip buttonCloseSound;
    
    void Start()
    {
        SetupButtonListeners();
        CheckContinueButtonState();
        
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    private void SetupButtonListeners()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(() => OnButtonClick(StartGame));
            
        if (continueButton != null)
            continueButton.onClick.AddListener(() => OnButtonClick(ContinueGame));
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => OnButtonClick(OpenSettings, buttonOpenSound));
            
        if (quitButton != null)
            quitButton.onClick.AddListener(() => OnButtonClick(QuitGame));
            
        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(() => OnButtonClick(CloseSettings, buttonCloseSound));
    }
    
    private void OnButtonClick(System.Action action, AudioClip customSound = null)
    {
        PlayButtonSound(customSound);
        action?.Invoke();
    }
    
    private void PlayButtonSound(AudioClip customSound = null)
    {
        if (audioSource != null)
        {
            AudioClip soundToPlay = customSound ?? buttonClickSound;
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay);
            }
        }
    }
    
    private void CheckContinueButtonState()
    {
        bool hasSaveData = SaveSystem.HasSaveData();
        if (continueButton != null)
        {
            continueButton.interactable = hasSaveData;
            
        }
    }
    
    public void StartGame()
    {
        SaveSystem.DeleteSaveData();
        
        PlayerInventory.ClearInventory();
        MinionManager.ClearRoster();
        SceneManager.LoadScene("ClassSelection");
    }
    
    public void ContinueGame()
    {
        bool loadSuccess = SaveSystem.LoadGame();
        
        if (loadSuccess)
        {
            Debug.Log("[MainMenuManager] Save data loaded successfully, continuing game");
            SceneManager.LoadScene("MinionAssembly");
        }
        else
        {
            Debug.LogWarning("[MainMenuManager] Failed to load save data, starting new game");
            StartGame();
        }
    }
    
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Settings panel not assigned!");
        }
    }
    
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
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
