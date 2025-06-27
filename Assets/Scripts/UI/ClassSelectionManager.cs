using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClassSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public Button[] classButtons = new Button[3];
    public TMPro.TextMeshProUGUI[] classButtonTexts = new TMPro.TextMeshProUGUI[3];
    public TMPro.TextMeshProUGUI titleText;
    public TMPro.TextMeshProUGUI classNameText;
    public TMPro.TextMeshProUGUI classDescriptionText;
    public TMPro.TextMeshProUGUI classBonusText;
    public Button backButton;
    public Button selectButton;
    
    [Header("Necromancer Classes")]
    public NecromancerClass[] availableClasses = new NecromancerClass[3];
    
    [Header("Debug")]
    public bool enableDebugLogging = true;
    
    // Private fields
    private int selectedClassIndex = -1;
    private NecromancerClass selectedClass;
    
    void Start()
    {
        InitializeUI();
        SetupButtonListeners();
        
        if (enableDebugLogging)
            Debug.Log("[ClassSelectionManager] Class selection scene initialized");
    }
    
    void InitializeUI()
    {
        // Set title
        if (titleText != null)
            titleText.text = "Choose Your Necromancer Class";
        
        // Hide select button initially
        if (selectButton != null)
            selectButton.gameObject.SetActive(false);
        
        // Set up class buttons
        for (int i = 0; i < classButtons.Length && i < availableClasses.Length; i++)
        {
            if (availableClasses[i] != null && classButtonTexts[i] != null)
            {
                classButtonTexts[i].text = availableClasses[i].className;
            }
        }
        
        // Clear description panel
        if (classNameText != null) classNameText.text = "";
        if (classDescriptionText != null) classDescriptionText.text = "Select a class to see details";
        if (classBonusText != null) classBonusText.text = "";
    }
    
    void SetupButtonListeners()
    {
        // Set up class selection buttons
        for (int i = 0; i < classButtons.Length; i++)
        {
            int classIndex = i; // Capture for closure
            if (classButtons[i] != null)
            {
                classButtons[i].onClick.AddListener(() => SelectClass(classIndex));
            }
        }
        
        // Set up navigation buttons
        if (backButton != null)
            backButton.onClick.AddListener(BackToMainMenu);
        
        if (selectButton != null)
            selectButton.onClick.AddListener(ConfirmClassSelection);
    }
    
    void SelectClass(int classIndex)
    {
        if (classIndex < 0 || classIndex >= availableClasses.Length || availableClasses[classIndex] == null)
            return;
        
        selectedClassIndex = classIndex;
        selectedClass = availableClasses[classIndex];
        
        // Update visual feedback
        UpdateClassSelectionVisuals();
        
        // Update description panel
        UpdateClassDescription();
        
        // Show select button
        if (selectButton != null)
            selectButton.gameObject.SetActive(true);
        
        if (enableDebugLogging)
            Debug.Log($"[ClassSelectionManager] Selected class: {selectedClass.className}");
    }
    
    void UpdateClassSelectionVisuals()
    {
        for (int i = 0; i < classButtons.Length; i++)
        {
            if (classButtons[i] != null)
            {
                ColorBlock colors = classButtons[i].colors;

                if (i == selectedClassIndex)
                {
                    // Highlight selected class
                    colors.normalColor = Color.green;
                    colors.highlightedColor = new Color(0.8f, 1f, 0.8f);
                    colors.pressedColor = new Color(0.7f, 0.9f, 0.7f);
                    colors.selectedColor = Color.green;
                }
                else
                {
                    colors.normalColor = Color.white;
                    colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f);
                    colors.pressedColor = new Color(0.78f, 0.78f, 0.78f);
                    colors.selectedColor = Color.white;
                }

                classButtons[i].colors = colors;
                
                classButtons[i].OnDeselect(null);
                classButtons[i].OnSelect(null);
            }
        }
    }
    
    void UpdateClassDescription()
    {
        if (selectedClass == null) return;
        
        if (classNameText != null)
            classNameText.text = selectedClass.className;
        
        if (classDescriptionText != null)
            classDescriptionText.text = selectedClass.classDescription;
        
        if (classBonusText != null)
        {
            string bonusText = $"<b>Specialty:</b> {selectedClass.specialtyDescription}\n\n";
            bonusText += $"<b>Bonuses:</b>\n";
            bonusText += $"• HP Multiplier: {selectedClass.hpBonusMultiplier}x\n";
            bonusText += $"• Attack Multiplier: {selectedClass.attackBonusMultiplier}x\n";
            bonusText += $"• Bonus Card Picks: +{selectedClass.bonusCardPicks}";
            
            classBonusText.text = bonusText;
        }
    }
    
    void ConfirmClassSelection()
    {
        if (selectedClass == null)
        {
            Debug.LogWarning("[ClassSelectionManager] No class selected!");
            return;
        }
        
        // Store selected class and initialize starting resources
        GameData.SetSelectedClass(selectedClass);
        
        if (enableDebugLogging)
            Debug.Log($"[ClassSelectionManager] Confirmed class: {selectedClass.className}. Proceeding to minion assembly.");
        
        // Go to minion assembly to build starting roster
        SceneManager.LoadScene("MinionAssembly");
    }
    
    void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
