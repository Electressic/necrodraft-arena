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
    public Sprite cardBackgroundSprite;
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
        if (sceneTitle != null)
            sceneTitle.text = "SELECT YOUR NECROMANCER CLASS";
        
        if (selectButton != null)
        {
            selectButton.gameObject.SetActive(true);
            selectButton.interactable = false;
        }
        
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
        
        if (classCards[index] != null && classCards[index].image != null && cardBackgroundSprite != null)
        {
            classCards[index].image.sprite = cardBackgroundSprite;
        }
        
        if (classNameTexts[index] != null)
            classNameTexts[index].text = necroClass.className;
        
        if (classSubtitleTexts[index] != null)
            classSubtitleTexts[index].text = GetClassSubtitle(necroClass.className);
        
        if (classPortraits[index] != null && necroClass.classIcon != null)
            classPortraits[index].sprite = necroClass.classIcon;
        
        if (classDescriptionTexts[index] != null)
            classDescriptionTexts[index].text = necroClass.classDescription;
        
        if (classBonusTexts[index] != null)
        {
            string bonusText = $"<b>Specialty:</b> {necroClass.specialtyDescription}\n\n";
            bonusText += $"<b>Starting Bonuses:</b>\n";
            bonusText += $"• HP Multiplier: {necroClass.hpBonusMultiplier}x\n";
            bonusText += $"• Attack Multiplier: {necroClass.attackBonusMultiplier}x\n";
            bonusText += $"• Bonus Card Picks: +{necroClass.bonusCardPicks}";
            classBonusTexts[index].text = bonusText;
        }
        
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
        for (int i = 0; i < classCards.Length; i++)
        {
            int classIndex = i;
            if (classCards[i] != null)
            {
                classCards[i].onClick.AddListener(() => SelectClass(classIndex));
                
                var eventTrigger = classCards[i].GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (eventTrigger == null)
                    eventTrigger = classCards[i].gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                
                var hoverEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                hoverEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                hoverEntry.callback.AddListener((data) => { HoverClass(classIndex); });
                eventTrigger.triggers.Add(hoverEntry);
                
                var hoverExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                hoverExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                hoverExit.callback.AddListener((data) => { ExitHover(); });
                eventTrigger.triggers.Add(hoverExit);
            }
        }
        
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
        
        UpdateClassCardVisuals();
        
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
        
        if (classCards[index].image != null)
        {
            Color targetColor = highlighted ? classHighlightColors[index] : classAccentColors[index];
            
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
        
        GameData.SetSelectedClass(selectedClass);
        
        if (enableDebugLogging)
            Debug.Log($"[ClassSelectionManager] Confirmed class: {selectedClass.className}. Proceeding to minion assembly.");
        
        SceneManager.LoadScene("MinionAssembly");
    }
    
    void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
