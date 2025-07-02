using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ClassSelectionManager : MonoBehaviour
{
    [Header("Scene Elements")]
    public TextMeshProUGUI sceneTitle;
    
    [Header("Class Cards")]
    public Button[] classCards = new Button[3];
    public Sprite cardBackgroundSprite; // Card background sprite for all cards
    public Image[] classPortraits = new Image[3];
    public TextMeshProUGUI[] classNameTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] classSubtitleTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] classDescriptionTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] classBonusTexts = new TextMeshProUGUI[3];
    public TextMeshProUGUI[] classPlaystyleTexts = new TextMeshProUGUI[3];
    
    [Header("Navigation")]
    public Button backButton;
    public Button selectButton;
    
    [Header("Necromancer Classes")]
    public NecromancerClass[] availableClasses = new NecromancerClass[3];
    
    [Header("Class Theming")]
    public Color[] classAccentColors = new Color[3] 
    {
        new Color(0.97f, 0.98f, 0.99f, 1f), // Bone Weaver - Bone white
        new Color(0.18f, 0.52f, 0.35f, 1f), // Flesh Sculptor - Dark green  
        new Color(0.33f, 0.24f, 0.60f, 1f)  // Soul Binder - Deep purple
    };
    
    public Color[] classHighlightColors = new Color[3]
    {
        new Color(0.98f, 0.83f, 0.55f, 1f), // Bone Weaver - Aged bone yellow
        new Color(0.41f, 0.83f, 0.57f, 1f), // Flesh Sculptor - Sickly green
        new Color(0.62f, 0.48f, 0.92f, 1f)  // Soul Binder - Mystical purple
    };
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip cardHoverSound;
    public AudioClip cardSelectSound;
    
    [Header("Debug")]
    public bool enableDebugLogging = true;
    
    // Private fields
    private int selectedClassIndex = -1;
    private int hoveredClassIndex = -1;
    private NecromancerClass selectedClass;
    
    void Start()
    {
        InitializeUI();
        SetupButtonListeners();
        SetupAudio();
        
        if (enableDebugLogging)
            Debug.Log("[ClassSelectionManager] Class selection scene initialized");
    }
    
    void InitializeUI()
    {
        // Set scene title
        if (sceneTitle != null)
            sceneTitle.text = "SELECT YOUR NECROMANCER CLASS";
        
        // Show select button but make it non-interactable initially (greyed out)
        if (selectButton != null)
        {
            selectButton.gameObject.SetActive(true);
            selectButton.interactable = false;
        }
        
        // Set up class cards
        for (int i = 0; i < classCards.Length && i < availableClasses.Length; i++)
        {
            if (availableClasses[i] != null)
            {
                SetupClassCard(i);
            }
        }
    }
    
    void SetupClassCard(int index)
    {
        var necroClass = availableClasses[index];
        
        // Ensure card has background sprite
        if (classCards[index] != null && classCards[index].image != null && cardBackgroundSprite != null)
        {
            classCards[index].image.sprite = cardBackgroundSprite;
        }
        
        // Set class name
        if (classNameTexts[index] != null)
            classNameTexts[index].text = necroClass.className;
        
        // Set subtitle
        if (classSubtitleTexts[index] != null)
            classSubtitleTexts[index].text = GetClassSubtitle(necroClass.className);
        
        // Set portrait
        if (classPortraits[index] != null && necroClass.classIcon != null)
            classPortraits[index].sprite = necroClass.classIcon;
        
        // Set description
        if (classDescriptionTexts[index] != null)
            classDescriptionTexts[index].text = necroClass.classDescription;
        
        // Set bonus details
        if (classBonusTexts[index] != null)
        {
            string bonusText = $"<b>Specialty:</b> {necroClass.specialtyDescription}\n\n";
            bonusText += $"<b>Starting Bonuses:</b>\n";
            bonusText += $"• HP Multiplier: {necroClass.hpBonusMultiplier}x\n";
            bonusText += $"• Attack Multiplier: {necroClass.attackBonusMultiplier}x\n";
            bonusText += $"• Bonus Card Picks: +{necroClass.bonusCardPicks}";
            classBonusTexts[index].text = bonusText;
        }
        
        // Set playstyle
        if (classPlaystyleTexts[index] != null)
        {
            string playstyleText = $"<b>Playstyle:</b>\n{GetClassPlaystyle(necroClass.className)}";
            classPlaystyleTexts[index].text = playstyleText;
        }
        
        // Apply class theming
        ApplyClassTheme(index, false);
    }
    
    string GetClassSubtitle(string className)
    {
        switch (className)
        {
            case "Bone Weaver": return "Master of Bones";
            case "Flesh Sculptor": return "Lord of Decay (Coming Soon)";
            case "Soul Binder": return "Spirit Guide (Coming Soon)";
            default: return "Necromancer";
        }
    }
    
    string GetClassPlaystyle(string className)
    {
        switch (className)
        {
            case "Bone Weaver": return "Glass Cannon - High Risk/Reward\nExcel at critical strikes and swift movement";
            case "Flesh Sculptor": return "Tank & Sustain - Outlast Enemies\nSpecialize in vampiric healing and heavy armor";
            case "Soul Binder": return "Flexible & Safe - Adaptable Builds\nBalanced approach, perfect for beginners";
            default: return "Balanced Playstyle";
        }
    }
    
    void SetupAudio()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    void SetupButtonListeners()
    {
        // Set up class card buttons
        for (int i = 0; i < classCards.Length; i++)
        {
            int classIndex = i; // Capture for closure
            if (classCards[i] != null)
            {
                // Click listener
                classCards[i].onClick.AddListener(() => SelectClass(classIndex));
                
                // Hover listeners using EventTrigger component
                var eventTrigger = classCards[i].GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (eventTrigger == null)
                    eventTrigger = classCards[i].gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                
                // Hover enter
                var hoverEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                hoverEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                hoverEntry.callback.AddListener((data) => { HoverClass(classIndex); });
                eventTrigger.triggers.Add(hoverEntry);
                
                // Hover exit
                var hoverExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                hoverExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                hoverExit.callback.AddListener((data) => { ExitHover(); });
                eventTrigger.triggers.Add(hoverExit);
            }
        }
        
        // Navigation buttons
        if (backButton != null)
            backButton.onClick.AddListener(BackToMainMenu);
        
        if (selectButton != null)
            selectButton.onClick.AddListener(ConfirmClassSelection);
    }
    
    void HoverClass(int classIndex)
    {
        if (classIndex == hoveredClassIndex) return;
        
        hoveredClassIndex = classIndex;
        PlaySound(cardHoverSound);
        
        // Update visual feedback
        UpdateClassCardVisuals();
    }
    
    void ExitHover()
    {
        hoveredClassIndex = -1;
        UpdateClassCardVisuals();
    }
    
    void SelectClass(int classIndex)
    {
        if (classIndex < 0 || classIndex >= availableClasses.Length || availableClasses[classIndex] == null)
            return;
        
        selectedClassIndex = classIndex;
        selectedClass = availableClasses[classIndex];
        
        PlaySound(cardSelectSound);
        
        // Update visual feedback
        UpdateClassCardVisuals();
        
        // Enable select button (make it interactable)
        if (selectButton != null)
            selectButton.interactable = true;
        
        if (enableDebugLogging)
            Debug.Log($"[ClassSelectionManager] Selected class: {selectedClass.className}");
    }
    
    void UpdateClassCardVisuals()
    {
        for (int i = 0; i < classCards.Length; i++)
        {
            if (classCards[i] != null)
            {
                bool isSelected = (i == selectedClassIndex);
                bool isHovered = (i == hoveredClassIndex);
                
                ApplyClassTheme(i, isSelected || isHovered);
            }
        }
    }
    
    void ApplyClassTheme(int index, bool highlighted)
    {
        if (index >= classAccentColors.Length) return;
        
        // Apply background color with different alpha values per class
        if (classCards[index].image != null)
        {
            Color targetColor = highlighted ? classHighlightColors[index] : classAccentColors[index];
            
            // Adjust alpha based on class - darker colors need higher alpha to be visible
            float normalAlpha, highlightedAlpha;
            switch (index)
            {
                case 0: // Bone Weaver - light colors, lower alpha
                    normalAlpha = 0.4f;
                    highlightedAlpha = 0.6f;
                    break;
                case 1: // Flesh Sculptor - medium colors, medium alpha  
                    normalAlpha = 0.4f;
                    highlightedAlpha = 0.6f;
                    break;
                case 2: // Soul Binder - dark colors, higher alpha
                    normalAlpha = 0.4f;
                    highlightedAlpha = 0.6f;
                    break;
                default:
                    normalAlpha = 0.4f;
                    highlightedAlpha = 0.6f;
                    break;
            }
            
            targetColor.a = highlighted ? highlightedAlpha : normalAlpha;
            classCards[index].image.color = targetColor;
        }
        
        // Apply text color to class name
        if (classNameTexts[index] != null)
        {
            classNameTexts[index].color = highlighted ? classHighlightColors[index] : Color.white;
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
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
