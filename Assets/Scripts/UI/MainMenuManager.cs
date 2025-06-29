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
        
        // Make sure settings panel is closed initially
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
            
        // Create audio source if not assigned
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
        // TODO: Check if save data exists
        // For now, disable continue button
        if (continueButton != null)
            continueButton.interactable = false;
    }
    
    public void StartGame()
    {
        PlayerInventory.ClearInventory();
        MinionManager.ClearRoster();
        SceneManager.LoadScene("ClassSelection");
    }
    
    public void ContinueGame()
    {
        // TODO: Load save data
        Debug.Log("Continue game - Save system not implemented yet");
        SceneManager.LoadScene("ClassSelection");
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
