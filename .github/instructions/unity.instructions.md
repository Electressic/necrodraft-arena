---
applyTo: '**'
---

# NecroDraft Arena - Unity Development Standards & Best Practices

## Project Overview
- **Game**: NecroDraft Arena (Top-down Autobattler with modular minion crafting)
- **Engine**: Unity 6+ (2D Core template)
- **Platform**: PC (Windows primary)
- **Scope**: University prototype focusing on core gameplay loop
- **Art Style**: Dark pixel art with modular sprite layering
- **Target**: 3-5 waves, 1-3 minions, 8-12 part cards for MVP

## Coding Standards

### Naming Conventions
- **Classes**: PascalCase (`GameManager`, `PartData`, `MinionAssembler`)
- **Methods**: PascalCase (`StartGame()`, `SelectCard()`, `CalculateStats()`)
- **Variables**: camelCase (`currentWave`, `selectedPart`, `playerInventory`)
- **Constants**: UPPER_SNAKE_CASE (`MAX_MINIONS`, `DEFAULT_HP`)
- **Unity Scene Objects**: PascalCase with descriptive names (`MainMenuPanel`, `Card1Button`, `ZombieHeadSprite`)
- **ScriptableObjects**: Descriptive names (`ZombieHead`, `SkeletonTorso`, `WaveOneConfig`)

### File Organization Standards
```
Assets/
├── _Project/                    # Main project folder (underscore for top sorting)
│   ├── Art/
│   │   ├── Sprites/
│   │   │   ├── BodyParts/      # 16x16 or 32x32 pixel sprites
│   │   │   ├── Enemies/        # Enemy sprites  
│   │   │   ├── UI/             # UI elements
│   │   │   └── Environment/    # Arena/background
│   │   └── Animations/         # Animator Controllers & Clips
│   ├── Audio/
│   │   ├── Music/              # Background tracks
│   │   └── SFX/                # Sound effects
│   ├── Prefabs/
│   │   ├── Minions/            # Minion prefab variants
│   │   ├── Enemies/            # Enemy prefabs
│   │   └── UI/                 # UI prefabs
│   ├── Scenes/                 # All game scenes
│   ├── Scripts/
│   │   ├── Core/               # GameManager, state machines
│   │   ├── Minions/            # Minion logic, part systems
│   │   ├── Combat/             # Combat calculations, targeting
│   │   ├── UI/                 # All UI controllers
│   │   └── Data/               # ScriptableObject definitions
│   └── ScriptableObjects/
│       ├── Parts/              # Body part data assets
│       ├── EnemyTypes/         # Enemy configuration
│       └── WaveConfigs/        # Wave progression data
```

### Script Structure Standards
```csharp
using UnityEngine;
using UnityEngine.UI;              // Group Unity imports
using System.Collections.Generic;  // System imports after Unity

public class ExampleManager : MonoBehaviour
{
    [Header("Core References")]        // Group related fields with headers
    public Button actionButton;
    public Transform spawnPoint;
    
    [Header("Configuration")]
    public int maxUnits = 3;
    public float combatDelay = 1.5f;
    
    [Header("Debug")]                  // Separate debug/testing fields
    public bool enableDebugMode = false;
    
    // Private fields
    private List<GameObject> activeUnits;
    private GameState currentState;
    
    // Unity lifecycle methods first
    void Awake() { }
    void Start() { }
    void Update() { }
    
    // Public methods
    public void StartCombat() { }
    
    // Private methods
    private void CalculateDamage() { }
}
```

## Unity Setup Instructions Format

All Unity setup instructions should follow this detailed step-by-step format:

### Template Example:
```
## Step X: [Feature Name] Setup (Detailed Unity Steps)

1. **Open/Create the scene:**
   - Double-click `SceneName.unity` in the Project window
   - OR File → New Scene → Save as "SceneName" in Assets/_Project/Scenes/

2. **Create the basic structure:**
   - Right-click in Hierarchy → UI → Canvas
   - Right-click Canvas → UI → Panel (rename to "DescriptivePanel")
   - Set Panel color to [specific color/transparency]

3. **Add UI elements:**
   - Right-click Panel → UI → Button - TextMeshPro (rename to "ActionButton")
   - Position: [specific position description]
   - Text: Change to "Button Text"
   - Size: [specific dimensions if important]

4. **Create manager object:**
   - Right-click in Hierarchy → Create Empty (rename to "ManagerName")
   - With Manager selected, click Add Component in Inspector
   - Search for and add your `ManagerScript` component

5. **Wire up references:**
   - Select the Manager object
   - In the Inspector, drag from Hierarchy:
     - ActionButton → into the "Action Button" field
     - [Continue for all references]

6. **Configure settings:**
   - [Any specific configuration steps]

7. **Save the scene:** Ctrl+S
```

## Asset Import Standards

### Sprites (Pixel Art)
- **Texture Type**: Sprite (2D and UI)
- **Sprite Mode**: Single (unless sprite sheets)
- **Pixels Per Unit**: 16 (consistent across all sprites)
- **Filter Mode**: Point (no filter) - for crisp pixels
- **Compression**: None - for crisp pixels
- **Max Size**: 512 (or smallest power of 2 that fits)

### Audio
- **Import Settings**: 
  - Music: Compressed, Vorbis, Quality 70%
  - SFX: PCM, 44.1kHz, Mono where possible
- **3D Audio**: Disabled for 2D game

## ScriptableObject Patterns

### Data Definition Template
```csharp
[CreateAssetMenu(fileName = "NewItemName", menuName = "NecroDraft/Category")]
public class DataType : ScriptableObject
{
    [Header("Basic Info")]
    public string displayName;
    public Sprite icon;
    
    [Header("Stats")]
    public int statValue;
    
    [Header("Description")]
    [TextArea(2, 4)]
    public string description;
}
```

## Scene Management Standards

### Scene Flow
1. **MainMenu** (index 0) - Entry point
2. **CardSelection** (index 1) - Part drafting
3. **Gameplay** (index 2) - Combat and placement
4. **GameOver** (index 3) - End state

### Scene Loading Pattern
```csharp
// Always use scene names, not indices
SceneManager.LoadScene("SceneName");

// For data persistence between scenes, use:
// - Static classes (PlayerInventory)
// - DontDestroyOnLoad objects
// - ScriptableObjects
```

## UI Standards

### Hierarchy Organization
```
Canvas
├── BackgroundPanel          # Full-screen background
├── MainContentPanel         # Primary content area
│   ├── HeaderPanel         # Top UI (title, wave counter)
│   ├── BodyPanel           # Main interaction area
│   └── FooterPanel         # Bottom UI (buttons)
└── OverlayPanel            # Popups, modals
```

### Button Setup Standards
- **Always use TextMeshPro** for button text
- **Consistent sizing**: Define standard button sizes
- **Color feedback**: Use button ColorBlock for selection states
- **Audio**: Add click sounds via script, not Unity Events

## Testing & Debug Standards

### Debug Logging
```csharp
// Use structured logging
Debug.Log($"[GameManager] Wave {currentWave} started with {enemyCount} enemies");

// Use different log levels appropriately
Debug.LogWarning("Part slot already occupied, replacing...");
Debug.LogError("Critical: No spawn points found!");
```

### Testing Components
- Create simple testing scripts (like `PartDataTester`) for isolated feature testing
- Add `[Header("Debug")]` sections to expose testing variables
- Use `#if UNITY_EDITOR` for editor-only testing code

## Version Control Standards

### Commit Message Format
```
[CATEGORY] Brief description

Categories:
- [SETUP] - Initial setup, project configuration
- [FEATURE] - New gameplay features
- [UI] - User interface changes
- [FIX] - Bug fixes
- [ART] - Art asset additions/changes
- [AUDIO] - Audio implementation
- [REFACTOR] - Code cleanup/reorganization
```

### Unity Version Control Settings (CRITICAL)
- **Version Control Mode**: Visible Meta Files
- **Asset Serialization**: Force Text
- **Always commit**: .meta files alongside assets

## Performance Guidelines

### For Prototype Scope
- **Don't over-optimize**: Focus on working features first
- **Object Pooling**: Not needed for <20 units
- **Update vs FixedUpdate**: Use Update for UI, FixedUpdate for physics
- **String concatenation**: Use StringBuilder for frequent text updates

## Common Patterns for This Project

### Singleton GameManager
```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

### Static Data Managers
```csharp
// For simple data persistence (like PlayerInventory)
public static class PlayerInventory
{
    private static List<PartData> parts = new List<PartData>();
    
    public static void AddPart(PartData part) { parts.Add(part); }
    public static List<PartData> GetParts() { return new List<PartData>(parts); }
}
```

## Troubleshooting Guide

### Common Issues & Solutions
1. **Buttons not responding**: Check EventSystem exists in scene
2. **Sprites blurry**: Verify Pixels Per Unit and Filter Mode settings
3. **Scripts not compiling**: Check for missing using statements
4. **Scene not loading**: Verify scene is in Build Settings
5. **References lost**: Ensure proper prefab workflow

Remember: For this prototype, **functionality over perfection**. Get the core loop working, then iterate and polish.