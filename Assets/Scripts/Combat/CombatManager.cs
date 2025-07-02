using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI; // Added for Button

public class CombatManager : MonoBehaviour
{
    [Header("References")]
    public GameManager gameplayUIManager;
    public Transform[] minionSpawnPoints;  // Legacy support
    public Transform[] enemySpawnPoints;   // Legacy support
    public GridSpawnManager gridSpawnManager;

    [Header("Prefabs")]
    public GameObject minionPrefab; // Should have MinionController script
    public GameObject enemyPrefab;  // Should have EnemyController script

    [Header("Wave Configuration")]
    public List<WaveData> waveConfigs;
    private int currentWaveIndex = 0;

    [Header("Combat Settings")]
    public float combatStartDelay = 2f;
    public float waveCompleteDelay = 3f;
    public bool enablePreviewMode = true;
    
    [Header("Preview UI")]
    public UnityEngine.UI.Button startCombatButton;
    public TMPro.TextMeshProUGUI previewInfoText;
    public GameObject previewPanel;

    [Header("Repositioning")]
    public Material selectedIndicatorMaterial; // A bright material to highlight the selected unit
    public GameObject minionNumberPrefab; // TextMeshPro prefab for minion numbers
    public int selectedMinionForRepositioning { get; private set; } = -1;
    private Material[] originalIndicatorMaterials;

    [Header("Debug")]
    public bool enableDebugMode = false;

    [Header("Testing")]
    public Button testSpawnButton; // Added for testing

    // Private fields
    private List<MinionController> activeMinions = new List<MinionController>();
    private List<EnemyController> activeEnemies = new List<EnemyController>();
    private bool combatActive = false;
    public CombatState currentState { get; private set; } = CombatState.Preparing;
    
    // Preview data
    private List<Vector3> previewMinionPositions = new List<Vector3>();
    private List<Vector3> previewEnemyPositions = new List<Vector3>();
    private WaveData currentWaveData;
    
    // Visual preview indicators
    private List<GameObject> previewIndicators = new List<GameObject>();
    private GameObject minionIndicatorPrefab;
    private GameObject enemyIndicatorPrefab;
    
    public enum CombatState
    {
        Preparing,
        Preview,
        Active,
        Complete
    }

    void Start()
    {
        if (gameplayUIManager == null)
            gameplayUIManager = FindFirstObjectByType<GameManager>();
        
        // Sync with GameData's current wave progress
        int gameDataWave = GameData.GetCurrentWave();
        currentWaveIndex = gameDataWave - 1; // Convert to 0-based index
        
        Debug.Log($"[CombatManager] Starting combat for Wave {gameDataWave} (index {currentWaveIndex})");
        
        // Start wave preparation (preview or direct combat)
        if (enablePreviewMode)
        {
            Invoke(nameof(StartWavePreview), combatStartDelay);
        }
        else
        {
            Invoke(nameof(StartNextWave), combatStartDelay);
        }

        // Add test button listener
        if (testSpawnButton != null)
            testSpawnButton.onClick.AddListener(TestSpawnUnits);
            
        // Set up preview UI
        if (startCombatButton != null)
            startCombatButton.onClick.AddListener(StartCombatFromPreview);
            
        // Initialize preview panel state
        if (previewPanel != null)
            previewPanel.SetActive(false);
            
        // Create indicator prefabs
        CreateIndicatorPrefabs();
    }
    
    void CreateIndicatorPrefabs()
    {
        // Create minion preview using actual minion prefab
        if (minionPrefab != null)
        {
            minionIndicatorPrefab = Instantiate(minionPrefab);
            minionIndicatorPrefab.name = "MinionIndicatorPrefab";
            
            // Make it semi-transparent and add preview styling
            SetupPreviewStyling(minionIndicatorPrefab, new Color(0, 1, 0, 0.6f)); // Green tint
            
            // Disable any controllers/scripts that might interfere
            DisablePreviewComponents(minionIndicatorPrefab);
            
            minionIndicatorPrefab.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[CombatManager] Minion prefab not assigned, using fallback indicator");
            CreateFallbackMinionIndicator();
        }
        
        // Create enemy preview using actual enemy prefab
        if (enemyPrefab != null)
        {
            enemyIndicatorPrefab = Instantiate(enemyPrefab);
            enemyIndicatorPrefab.name = "EnemyIndicatorPrefab";
            
            // Make it semi-transparent and add preview styling
            SetupPreviewStyling(enemyIndicatorPrefab, new Color(1, 0, 0, 0.6f)); // Red tint
            
            // Disable any controllers/scripts that might interfere
            DisablePreviewComponents(enemyIndicatorPrefab);
            
            enemyIndicatorPrefab.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[CombatManager] Enemy prefab not assigned, using fallback indicator");
            CreateFallbackEnemyIndicator();
        }
        
        Debug.Log("[CombatManager] Created preview indicators using actual combat prefabs");
    }
    
    void SetupPreviewStyling(GameObject previewObject, Color tintColor)
    {
        // Find all renderers and make them semi-transparent with color tint
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = new Material[renderer.materials.Length];
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                // Create a new material instance to avoid modifying the original
                materials[i] = new Material(renderer.materials[i]);
                
                // Try to make it transparent
                if (materials[i].HasProperty("_Color"))
                {
                    Color originalColor = materials[i].color;
                    materials[i].color = new Color(
                        originalColor.r * tintColor.r, 
                        originalColor.g * tintColor.g, 
                        originalColor.b * tintColor.b, 
                        tintColor.a
                    );
                }
                
                // Try to set rendering mode to transparent if possible
                if (materials[i].HasProperty("_Mode"))
                {
                    materials[i].SetInt("_Mode", 3); // Transparent mode
                }
                if (materials[i].HasProperty("_SrcBlend"))
                {
                    materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                }
                if (materials[i].HasProperty("_DstBlend"))
                {
                    materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                }
                if (materials[i].HasProperty("_ZWrite"))
                {
                    materials[i].SetInt("_ZWrite", 0);
                }
                materials[i].DisableKeyword("_ALPHATEST_ON");
                materials[i].EnableKeyword("_ALPHABLEND_ON");
                materials[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                materials[i].renderQueue = 3000;
            }
            renderer.materials = materials;
        }
    }
    
    void DisablePreviewComponents(GameObject previewObject)
    {
        // Disable controllers and any components that might cause issues
        MinionController minionController = previewObject.GetComponent<MinionController>();
        if (minionController != null) minionController.enabled = false;
        
        EnemyController enemyController = previewObject.GetComponent<EnemyController>();
        if (enemyController != null) enemyController.enabled = false;
        
        Unit unit = previewObject.GetComponent<Unit>();
        if (unit != null) unit.enabled = false;
        
        // Make colliders triggers instead of disabling them
        Collider[] colliders = previewObject.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.isTrigger = true;
        }
        
        Collider2D[] colliders2D = previewObject.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D collider in colliders2D)
        {
            collider.isTrigger = true;
        }
        
        // Disable rigidbodies to prevent movement
        Rigidbody rb = previewObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        
        Rigidbody2D rb2D = previewObject.GetComponent<Rigidbody2D>();
        if (rb2D != null) rb2D.bodyType = RigidbodyType2D.Kinematic;
    }
    
    void CreateFallbackMinionIndicator()
    {
        minionIndicatorPrefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        minionIndicatorPrefab.name = "MinionIndicatorPrefab_Fallback";
        minionIndicatorPrefab.transform.localScale = new Vector3(1.5f, 0.5f, 1.5f);
        
        Renderer renderer = minionIndicatorPrefab.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Unlit/Color"));
        renderer.material.color = new Color(0, 1, 0, 0.6f);
        
        Collider collider = minionIndicatorPrefab.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        
        minionIndicatorPrefab.SetActive(false);
    }
    
    void CreateFallbackEnemyIndicator()
    {
        enemyIndicatorPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemyIndicatorPrefab.name = "EnemyIndicatorPrefab_Fallback";
        enemyIndicatorPrefab.transform.localScale = new Vector3(1.2f, 0.5f, 1.2f);
        enemyIndicatorPrefab.transform.rotation = Quaternion.Euler(0, 45, 0);
        
        Renderer renderer = enemyIndicatorPrefab.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Unlit/Color"));
        renderer.material.color = new Color(1, 0, 0, 0.6f);
        
        Collider collider = enemyIndicatorPrefab.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        
        enemyIndicatorPrefab.SetActive(false);
    }

    public void StartWavePreview()
    {
        currentState = CombatState.Preview;
        
        // Get the wave data - use the first config as a template if we don't have enough
        if (currentWaveIndex < waveConfigs.Count)
        {
            currentWaveData = waveConfigs[currentWaveIndex];
        }
        else if (waveConfigs.Count > 0)
        {
            currentWaveData = waveConfigs[waveConfigs.Count - 1];
        }
        else
        {
            Debug.LogError("[CombatManager] No wave configurations available!");
            HandleGameVictory();
            return;
        }
        
        // Calculate spawn positions for preview
        CalculatePreviewPositions();
        
        // Show preview UI
        ShowPreviewUI();
        
        Debug.Log($"[CombatManager] Wave {GameData.GetCurrentWave()} preview ready!");
    }
    
    void CalculatePreviewPositions()
    {
        previewMinionPositions.Clear();
        previewEnemyPositions.Clear();
        
        // Calculate minion positions
        List<Minion> playerRoster = MinionManager.GetMinionRoster();
        if (gridSpawnManager != null)
        {
            previewMinionPositions = gridSpawnManager.GetFormationPositions(
                GridSpawnManager.GridZone.PlayerZone,
                playerRoster.Count,
                GridSpawnManager.FormationType.Line
            );
        }
        else if (minionSpawnPoints != null)
        {
            for (int i = 0; i < playerRoster.Count && i < minionSpawnPoints.Length; i++)
            {
                previewMinionPositions.Add(minionSpawnPoints[i].position);
            }
        }
        
        // Calculate enemy positions
        int currentWave = GameData.GetCurrentWave();
        var enemyWaveInfo = GetEnemyInfoForWave(currentWave);
        
        foreach (var enemyInfo in enemyWaveInfo)
        {
            if (gridSpawnManager != null)
            {
                List<Vector3> groupPositions = gridSpawnManager.GetFormationPositions(
                    GridSpawnManager.GridZone.EnemyZone,
                    enemyInfo.count,
                    enemyInfo.formation
                );
                previewEnemyPositions.AddRange(groupPositions);
            }
            else if (enemySpawnPoints != null)
            {
                for (int i = 0; i < enemyInfo.count && previewEnemyPositions.Count < enemySpawnPoints.Length; i++)
                {
                    int index = previewEnemyPositions.Count % enemySpawnPoints.Length;
                    previewEnemyPositions.Add(enemySpawnPoints[index].position);
                }
            }
        }
        
        Debug.Log($"[CombatManager] Preview calculated: {previewMinionPositions.Count} minions, {previewEnemyPositions.Count} enemies");
    }
    
    void ShowPreviewUI()
    {
        if (previewPanel != null)
            previewPanel.SetActive(true);
            
        if (previewInfoText != null)
        {
            int currentWave = GameData.GetCurrentWave();
            var enemyInfo = GetEnemyInfoForWave(currentWave);
            
            string infoText = $"WAVE {currentWave} PREVIEW\n\n";
            infoText += $"Your Minions: {previewMinionPositions.Count}\n";
            infoText += $"Enemy Forces: {previewEnemyPositions.Count}\n\n";
            
            infoText += "Enemy Groups:\n";
            foreach (var enemy in enemyInfo)
            {
                infoText += $"• {enemy.count}x {enemy.displayName} ({enemy.formation})\n";
            }
            
            infoText += "\nReview positions and start when ready!";
            previewInfoText.text = infoText;
        }
        
        if (startCombatButton != null)
            startCombatButton.gameObject.SetActive(true);
            
        // Create visual position indicators
        CreatePreviewIndicators();
    }
    
    void CreatePreviewIndicators()
    {
        ClearPreviewIndicators();

        // Create Minion Indicators
        for (int i = 0; i < previewMinionPositions.Count; i++)
        {
            Vector3 pos = previewMinionPositions[i];
            GameObject indicator = Instantiate(minionIndicatorPrefab, pos, Quaternion.identity);
            indicator.name = $"MinionPreview_{i}";
            indicator.SetActive(true);
            
            // Add the click handler
            var clickHandler = indicator.GetComponent<PreviewUnitClickHandler>();
            if (clickHandler == null)
            {
                clickHandler = indicator.AddComponent<PreviewUnitClickHandler>();
            }
            clickHandler.combatManager = this;
            clickHandler.previewIndex = i;
            clickHandler.selectedMaterial = selectedIndicatorMaterial;
            
            // Ensure 2D collider exists for proper clicking
            if (indicator.GetComponent<Collider2D>() == null)
            {
                BoxCollider2D collider = indicator.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                collider.size = Vector2.one * 0.8f; // Slightly smaller than cell size
            }
            
            // Add numbered text label (1-based)
            AddMinionNumberLabel(indicator, i + 1);
            
            previewIndicators.Add(indicator);
        }

        // Create Enemy Indicators (enemies don't need numbering or clicking)
        for (int i = 0; i < previewEnemyPositions.Count; i++)
        {
            Vector3 pos = previewEnemyPositions[i];
            GameObject indicator = Instantiate(enemyIndicatorPrefab, pos, Quaternion.identity);
            indicator.name = $"EnemyPreview_{i}";
            indicator.SetActive(true);
            previewIndicators.Add(indicator);
        }

        Debug.Log($"[CombatManager] Created {previewMinionPositions.Count} minion previews and {previewEnemyPositions.Count} enemy previews");
    }
    
    void ClearPreviewIndicators()
    {
        // Deselect any minion before clearing
        DeselectMinion();

        foreach (var indicator in previewIndicators)
        {
            if (indicator != null)
                Destroy(indicator);
        }
        previewIndicators.Clear();
    }
    
    void OnDestroy()
    {
        // Clean up any remaining indicators
        ClearPreviewIndicators();
        
        // Clean up prefabs
        if (minionIndicatorPrefab != null)
            Destroy(minionIndicatorPrefab);
        if (enemyIndicatorPrefab != null)
            Destroy(enemyIndicatorPrefab);
    }
    
    public void StartCombatFromPreview()
    {
        if (currentState != CombatState.Preview) return;
        
        // Hide preview UI
        if (previewPanel != null)
            previewPanel.SetActive(false);
            
        // Start actual combat
        currentState = CombatState.Active;
        StartCoroutine(SpawnWave(currentWaveData));
        
        Debug.Log("[CombatManager] Combat started from preview!");
        
        // Clear preview data and indicators
        previewMinionPositions.Clear();
        previewEnemyPositions.Clear();
        ClearPreviewIndicators();
    }

    public void StartNextWave()
    {
        // Get the wave data - use the first config as a template if we don't have enough
        WaveData currentWaveData;
        
        if (currentWaveIndex < waveConfigs.Count)
        {
            currentWaveData = waveConfigs[currentWaveIndex];
        }
        else if (waveConfigs.Count > 0)
        {
            // Use the last wave config as a template and scale it up
            currentWaveData = waveConfigs[waveConfigs.Count - 1];
            Debug.Log($"[CombatManager] Using wave template {waveConfigs.Count} for wave {currentWaveIndex + 1}");
        }
        else
        {
            Debug.LogError("[CombatManager] No wave configurations available!");
            HandleGameVictory();
            return;
        }
        
        // Update UI with current wave from GameData
        if (gameplayUIManager != null)
        {
            gameplayUIManager.RefreshWaveDisplay();
        }
        
        string waveName = currentWaveData.waveName;
        if (currentWaveIndex >= waveConfigs.Count)
        {
            waveName = $"Wave {GameData.GetCurrentWave()}"; // Generate name for scaled waves
        }
        
        Debug.Log($"[CombatManager] Starting {waveName} (GameData Wave: {GameData.GetCurrentWave()})");
        
        // Display detailed wave information
        DisplayWaveInfo(GameData.GetCurrentWave());
        
        StartCoroutine(SpawnWave(currentWaveData));
    }

    IEnumerator SpawnWave(WaveData waveData)
    {
        // Clear previous wave
        ClearActiveUnits();
        
        // Spawn player minions
        SpawnPlayerMinions();
        
        yield return new WaitForSeconds(1f);
        
        // Spawn enemies
        yield return StartCoroutine(SpawnEnemies(waveData));
        
        // Start combat
        combatActive = true;
        
        // Start checking for wave completion
        StartCoroutine(CheckWaveCompletion());
    }

    void SpawnPlayerMinions()
    {
        List<Minion> playerRoster = MinionManager.GetMinionRoster();

        // Use grid spawning if available, otherwise fall back to legacy spawn points
        if (gridSpawnManager != null)
        {
            SpawnMinionsOnGrid(playerRoster);
        }
        else
        {
            SpawnMinionsLegacy(playerRoster);
        }
    }
    
    void SpawnMinionsOnGrid(List<Minion> playerRoster)
    {
        // Get formation positions for all minions at once
        List<Vector3> spawnPositions = gridSpawnManager.GetFormationPositions(
            GridSpawnManager.GridZone.PlayerZone, 
            playerRoster.Count, 
            GridSpawnManager.FormationType.Line
        );
        
        for (int i = 0; i < playerRoster.Count && i < spawnPositions.Count; i++)
        {
            Minion minionData = playerRoster[i];
            if (minionData == null || minionPrefab == null) continue;

            Vector3 spawnPosition = spawnPositions[i];
            GameObject minionInstance = Instantiate(minionPrefab, spawnPosition, Quaternion.identity);
            MinionController minionController = minionInstance.GetComponent<MinionController>();
            
            if (minionController != null)
            {
                minionController.Initialize(minionData);
                minionController.OnDeath += OnMinionDeath;
                activeMinions.Add(minionController);
                
                // Mark grid position as occupied
                gridSpawnManager.SetOccupied(spawnPosition, minionInstance);
                
                Debug.Log($"[CombatManager] Spawned minion: {minionData.minionName} at grid position {spawnPosition}");
            }
            else
            {
                Debug.LogError($"[CombatManager] Minion prefab is missing MinionController script!");
                Destroy(minionInstance);
            }
        }
    }
    
    void SpawnMinionsLegacy(List<Minion> playerRoster)
    {
        for (int i = 0; i < playerRoster.Count && i < minionSpawnPoints.Length; i++)
        {
            Minion minionData = playerRoster[i];
            if (minionData == null || minionPrefab == null) continue;

            GameObject minionInstance = Instantiate(minionPrefab, minionSpawnPoints[i].position, Quaternion.identity);
            MinionController minionController = minionInstance.GetComponent<MinionController>();
            
            if (minionController != null)
            {
                minionController.Initialize(minionData);
                minionController.OnDeath += OnMinionDeath;
                activeMinions.Add(minionController);
                
                Debug.Log($"[CombatManager] Spawned minion: {minionData.minionName} (Legacy)");
            }
            else
            {
                Debug.LogError($"[CombatManager] Minion prefab is missing MinionController script!");
                Destroy(minionInstance);
            }
        }
    }

    IEnumerator SpawnEnemies(WaveData waveData)
    {
        int currentWave = GameData.GetCurrentWave();
        int spawnPointIndex = 0;
        
        // Use wave-based enemy spawning with formations
        var enemyWaveInfo = GetEnemyInfoForWave(currentWave);
        
        foreach (var enemyInfo in enemyWaveInfo)
        {
            // Wait for spawn delay
            yield return new WaitForSeconds(enemyInfo.spawnDelay);
            
            // Get formation positions for this enemy group
            List<Vector3> spawnPositions;
            
            if (gridSpawnManager != null)
            {
                // Use grid formation spawning
                spawnPositions = gridSpawnManager.GetFormationPositions(
                    GridSpawnManager.GridZone.EnemyZone, 
                    enemyInfo.count, 
                    enemyInfo.formation
                );
                
                // If we don't get enough positions, fill with best available positions
                while (spawnPositions.Count < enemyInfo.count)
                {
                    Vector3 fallbackPosition = gridSpawnManager.GetBestEnemySpawnPosition();
                    if (fallbackPosition != Vector3.zero)
                        spawnPositions.Add(fallbackPosition);
                    else
                        break; // Grid is full
                }
            }
            else
            {
                // Legacy spawning fallback
                spawnPositions = new List<Vector3>();
                for (int i = 0; i < enemyInfo.count; i++)
                {
                    if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
                    {
                        Debug.LogError("[CombatManager] No enemy spawn points assigned!");
                        yield break;
                    }
                    
                    if (spawnPointIndex >= enemySpawnPoints.Length)
                        spawnPointIndex = 0;
                    
                    spawnPositions.Add(enemySpawnPoints[spawnPointIndex].position);
                    spawnPointIndex++;
                }
            }
            
            // Spawn enemies at formation positions
            for (int i = 0; i < enemyInfo.count && i < spawnPositions.Count; i++)
            {
                Vector3 spawnPosition = spawnPositions[i];
                GameObject enemyInstance = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                EnemyController enemyController = enemyInstance.GetComponent<EnemyController>();
                
                if (enemyController != null)
                {
                    // Load base enemy data
                    EnemyData baseEnemyData = Resources.Load<EnemyData>("BasicZombie");
                    if (baseEnemyData == null && waveData != null && waveData.enemyGroups.Count > 0)
                    {
                        baseEnemyData = waveData.enemyGroups[0].enemyType;
                    }
                    
                    if (baseEnemyData != null)
                    {
                        enemyController.Initialize(baseEnemyData);
                        
                        // Scale enemy stats based on wave progression
                        ScaleEnemyForWave(enemyController, currentWave);
                    }
                    
                    // Set enemy name and type
                    enemyController.gameObject.name = $"{enemyInfo.displayName} (W{currentWave})";
                    
                    enemyController.OnDeath += OnEnemyDeath;
                    activeEnemies.Add(enemyController);
                    
                    // Mark grid position as occupied
                    if (gridSpawnManager != null)
                    {
                        gridSpawnManager.SetOccupied(spawnPosition, enemyInstance);
                    }
                    
                    Debug.Log($"[CombatManager] Spawned {enemyInfo.displayName} (W{currentWave}) in {enemyInfo.formation} formation at {spawnPosition}");
                }
                else
                {
                    Debug.LogError($"[CombatManager] Enemy prefab is missing EnemyController script!");
                    Destroy(enemyInstance);
                }
                
                // Small delay between individual spawns in the same group
                if (i < enemyInfo.count - 1)
                    yield return new WaitForSeconds(0.3f);
            }
            
            // Longer delay between different enemy groups
            yield return new WaitForSeconds(0.5f);
        }
    }
    
    // Comprehensive 20-wave enemy configuration with grid formations
    System.Collections.Generic.List<(string displayName, int count, float spawnDelay, GridSpawnManager.FormationType formation)> GetEnemyInfoForWave(int wave)
    {
        var enemies = new System.Collections.Generic.List<(string, int, float, GridSpawnManager.FormationType)>();
        
        switch (wave)
        {
            // ACT 1: FOUNDATION (Waves 1-7) - Learning basic mechanics
            case 1:
                enemies.Add(("Shambling Corpse", 1, 0f, GridSpawnManager.FormationType.Line));
                break;
            case 2:
                enemies.Add(("Rotting Zombie", 2, 0f, GridSpawnManager.FormationType.Line));
                break;
            case 3:
                enemies.Add(("Basic Skeleton", 2, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Shambling Corpse", 1, 1.5f, GridSpawnManager.FormationType.Line));
                break;
            case 4:
                enemies.Add(("Undead Scout", 3, 0f, GridSpawnManager.FormationType.Spread));
                break;
            case 5: // First minion unlock wave
                enemies.Add(("Bone Warrior", 2, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Rotting Zombie", 2, 1f, GridSpawnManager.FormationType.Line));
                break;
            case 6:
                enemies.Add(("Skeletal Archer", 3, 0f, GridSpawnManager.FormationType.Triangle));
                enemies.Add(("Undead Brute", 1, 2f, GridSpawnManager.FormationType.Line));
                break;
            case 7: // Act 1 finale
                enemies.Add(("Death Guard", 2, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Bone Warrior", 3, 1f, GridSpawnManager.FormationType.Triangle));
                break;
                
            // ACT 2: MASTERY (Waves 8-14) - Complex formations and tactics
            case 8:
                enemies.Add(("Undead Soldier", 4, 0f, GridSpawnManager.FormationType.Spread));
                enemies.Add(("Skeletal Mage", 1, 1.5f, GridSpawnManager.FormationType.Line));
                break;
            case 9: // Second minion unlock wave
                enemies.Add(("Corrupted Guardian", 2, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Death Cultist", 3, 1f, GridSpawnManager.FormationType.Triangle));
                break;
            case 10:
                enemies.Add(("Bone Colossus", 1, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Skeletal Warrior", 4, 1f, GridSpawnManager.FormationType.Spread));
                break;
            case 11:
                enemies.Add(("Undead Champion", 3, 0f, GridSpawnManager.FormationType.Triangle));
                enemies.Add(("Shadow Wraith", 2, 2f, GridSpawnManager.FormationType.Line));
                break;
            case 12:
                enemies.Add(("Death Knight", 2, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Bone Spearman", 4, 1f, GridSpawnManager.FormationType.Triangle));
                break;
            case 13: // Third minion unlock wave
                enemies.Add(("Lich Acolyte", 1, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Undead Horde", 5, 1f, GridSpawnManager.FormationType.Spread));
                break;
            case 14: // Act 2 finale
                enemies.Add(("Bone Dragon", 1, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Death Knight", 2, 2f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Skeletal Archer", 3, 3f, GridSpawnManager.FormationType.Triangle));
                break;
                
            // ACT 3: ENDGAME (Waves 15-20) - Maximum difficulty and all spawn points
            case 15:
                enemies.Add(("Undead Titan", 2, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Shadow Legion", 6, 1f, GridSpawnManager.FormationType.Spread));
                break;
            case 16:
                enemies.Add(("Lich Commander", 1, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Death Knight", 3, 1f, GridSpawnManager.FormationType.Triangle));
                enemies.Add(("Bone Colossus", 2, 2f, GridSpawnManager.FormationType.Line));
                break;
            case 17: // Fourth minion unlock wave
                enemies.Add(("Ancient Bone Lord", 1, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Undead Army", 8, 1f, GridSpawnManager.FormationType.Spread));
                break;
            case 18:
                enemies.Add(("Shadow Overlord", 2, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Death Incarnate", 1, 2f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Undead Elite", 6, 3f, GridSpawnManager.FormationType.Triangle));
                break;
            case 19: // Treasure wave with maximum enemies
                enemies.Add(("Bone Emperor", 1, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Lich Supreme", 2, 1f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Death Legion", 10, 2f, GridSpawnManager.FormationType.Spread));
                break;
            case 20: // Final boss wave
                enemies.Add(("Necro Overlord", 1, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Ancient Death Knight", 3, 2f, GridSpawnManager.FormationType.Triangle));
                enemies.Add(("Undead Apocalypse", 12, 4f, GridSpawnManager.FormationType.Spread));
                break;
                
            default: // Waves beyond 20 (endless mode)
                int endlessMultiplier = wave - 20;
                enemies.Add(("Endless Horror", 1 + endlessMultiplier, 0f, GridSpawnManager.FormationType.Line));
                enemies.Add(("Void Legion", 5 + endlessMultiplier * 2, 1f, GridSpawnManager.FormationType.Spread));
                break;
        }
        
        return enemies;
    }
    
    // Scale enemy stats based on wave progression for balanced difficulty
    void ScaleEnemyForWave(EnemyController enemy, int wave)
    {
        if (enemy == null) return;

        // Don't scale wave 1 enemies
        if (wave <= 1) return;

        // --- Base Scaling Factors (Reduced) ---
        // Health scaling reduced from 20% to 12% per wave
        // Attack scaling reduced from 10% to 7% per wave
        float hpMultiplier = 1f + (wave - 1) * 0.12f;
        float atkMultiplier = 1f + (wave - 1) * 0.07f;

        // --- Act-based Scaling (Kept from original logic) ---
        if (wave >= 8) // Act 2
        {
            hpMultiplier *= 1.2f; // Was 1.5f
            atkMultiplier *= 1.15f; // Was 1.3f
        }
        if (wave >= 15) // Act 3
        {
            hpMultiplier *= 1.5f; // Was 2f
            atkMultiplier *= 1.3f; // Was 1.5f
        }

        // Apply scaling directly to the EnemyController's stats (since it inherits from Unit)
        enemy.maxHP = Mathf.RoundToInt(enemy.maxHP * hpMultiplier);
        enemy.currentHP = enemy.maxHP; // Heal to full scaled HP
        enemy.attackPower = Mathf.RoundToInt(enemy.attackPower * atkMultiplier);
        
        if (enableDebugMode)
            Debug.Log($"[CombatManager] Scaled {enemy.name} for Wave {wave}: HP={enemy.maxHP}, ATK={enemy.attackPower}");
    }
    
    // Display wave information for debugging and player awareness
    public void DisplayWaveInfo(int wave)
    {
        var enemyInfo = GetEnemyInfoForWave(wave);
        string waveDescription = $"=== WAVE {wave} CONFIGURATION ===\n";
        
        // Add act information
        if (wave <= 7)
            waveDescription += "ACT 1: FOUNDATION\n";
        else if (wave <= 14)
            waveDescription += "ACT 2: MASTERY\n";
        else
            waveDescription += "ACT 3: ENDGAME\n";
        
        int totalEnemies = 0;
        foreach (var enemy in enemyInfo)
        {
            totalEnemies += enemy.count;
            waveDescription += $"• {enemy.count}x {enemy.displayName} [{enemy.formation}] (Delay: {enemy.spawnDelay}s)\n";
        }
        
        waveDescription += $"Total Enemies: {totalEnemies}\n";
        waveDescription += $"Grid Utilization: {Mathf.RoundToInt((float)totalEnemies / 18f * 100f)}%\n";
        
        // Special wave markers
        if (wave == 5 || wave == 9 || wave == 13 || wave == 17)
            waveDescription += "🎯 MINION UNLOCK WAVE!\n";
        if (wave == 19)
            waveDescription += "🏆 TREASURE WAVE - Maximum rewards!\n";
        if (wave == 20)
            waveDescription += "⚔️ FINAL BOSS WAVE!\n";
        
        Debug.Log(waveDescription);
    }

    IEnumerator CheckWaveCompletion()
    {
        while (combatActive)
        {
            // Check if all enemies are dead (wave complete)
            bool allEnemiesDead = true;
            foreach (EnemyController enemy in activeEnemies)
            {
                if (enemy != null && enemy.isAlive)
                {
                    allEnemiesDead = false;
                    break;
                }
            }
            
            if (allEnemiesDead && activeEnemies.Count > 0)
            {
                combatActive = false;
                yield return new WaitForSeconds(waveCompleteDelay);
                OnWaveComplete();
                yield break;
            }
            
            // Check if all minions are dead (game over)
            bool allMinionsDead = true;
            foreach (MinionController minion in activeMinions)
            {
                if (minion != null && minion.isAlive)
                {
                    allMinionsDead = false;
                    break;
                }
            }
            
            if (allMinionsDead && activeMinions.Count > 0)
            {
                combatActive = false;
                HandleGameOver();
                yield break;
            }
            
            yield return new WaitForSeconds(0.5f); // Check every half second
        }
    }

    void OnMinionDeath(Unit minion)
    {
        Debug.Log($"[CombatManager] Minion {minion.name} died!");
        
        // Free up grid position if using grid system
        if (gridSpawnManager != null && minion != null)
        {
            gridSpawnManager.SetFree(minion.transform.position);
        }
    }

    void OnEnemyDeath(Unit enemy)
    {
        Debug.Log($"[CombatManager] Enemy {enemy.name} died!");
        
        // Free up grid position if using grid system
        if (gridSpawnManager != null && enemy != null)
        {
            gridSpawnManager.SetFree(enemy.transform.position);
        }
    }

    void OnWaveComplete()
    {
        currentState = CombatState.Complete;
        Debug.Log($"[CombatManager] Wave {currentWaveIndex + 1} completed!");
        
        // Mark wave as complete in GameData (this increments the wave)
        GameData.CompleteWave();
        
        // Add wave rewards to inventory
        if (currentWaveIndex < waveConfigs.Count)
        {
            WaveData completedWave = waveConfigs[currentWaveIndex];
            foreach (PartData reward in completedWave.rewardParts)
            {
                if (reward != null)
                {
                    PlayerInventory.AddPart(reward);
                    Debug.Log($"[CombatManager] Awarded {reward.partName}!");
                }
            }
        }
        
        // Advance to next wave
        currentWaveIndex++;
        int newWave = GameData.GetCurrentWave();
        
        Debug.Log($"[CombatManager] Wave progression: {currentWaveIndex} -> {newWave}. Proceeding to minion assembly for next wave");
        
        // Check if game is complete
        if (newWave > 20) // Assuming 20 waves total
        {
            Debug.Log("[CombatManager] All waves completed! Victory!");
            HandleGameVictory();
            return;
        }
        
        // Return to minion assembly where the card selection overlay is available
        SceneManager.LoadScene("MinionAssembly");
    }

    void HandleGameOver()
    {
        Debug.Log("[CombatManager] Game Over - All minions defeated!");
        SceneManager.LoadScene("GameOver");
    }

    void HandleGameVictory()
    {
        Debug.Log("[CombatManager] Victory - All waves completed!");
        // You could load a victory scene or the game over scene with a victory flag
        SceneManager.LoadScene("GameOver");
    }

    void ClearActiveUnits()
    {
        // Clean up any remaining units from previous waves
        foreach (MinionController minion in activeMinions)
        {
            if (minion != null)
                Destroy(minion.gameObject);
        }
        activeMinions.Clear();
        
        foreach (EnemyController enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        activeEnemies.Clear();
        
        // Also clear any lingering preview indicators
        ClearPreviewIndicators();
    }

    void OnDrawGizmos()
    {
        if (!enableDebugMode) return;
        
        // Draw minion spawn points as green spheres
        if (minionSpawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in minionSpawnPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.5f);
            }
        }
        
        // Draw enemy spawn points as red spheres  
        if (enemySpawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform point in enemySpawnPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 0.5f);
            }
        }
    }

    // Add this method to your CombatManager for testing
    public void TestSpawnUnits()
    {
        Debug.Log("[CombatManager] Testing unit spawning...");
        
        // Test minion spawn
        if (minionPrefab != null && minionSpawnPoints != null && minionSpawnPoints.Length > 0)
        {
            GameObject testMinion = Instantiate(minionPrefab, minionSpawnPoints[0].position, Quaternion.identity);
            Debug.Log("Spawned test minion at: " + minionSpawnPoints[0].position);
        }
        else
        {
            Debug.LogWarning("Cannot test minion spawn - missing prefab or spawn points");
        }
        
        // Test enemy spawn
        if (enemyPrefab != null && enemySpawnPoints != null && enemySpawnPoints.Length > 0)
        {
            GameObject testEnemy = Instantiate(enemyPrefab, enemySpawnPoints[0].position, Quaternion.identity);
            Debug.Log("Spawned test enemy at: " + enemySpawnPoints[0].position);
        }
        else
        {
            Debug.LogWarning("Cannot test enemy spawn - missing prefab or spawn points");
        }
    }

    // Debug method to make spawned enemies visible with basic sprites
    public void FixEnemyVisibility()
    {
        Debug.Log("[CombatManager] Fixing enemy visibility issues...");
        
        // Find all active enemies and make them visible
        EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        
        foreach (EnemyController enemy in enemies)
        {
            if (enemy != null)
            {
                // Ensure SpriteRenderer exists and is visible
                SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // Create a simple colored square sprite for visibility
                    if (sr.sprite == null)
                    {
                        // Create a temporary visible sprite (red square)
                        Texture2D texture = new Texture2D(32, 32);
                        Color[] colors = new Color[32 * 32];
                        for (int i = 0; i < colors.Length; i++) colors[i] = Color.red;
                        texture.SetPixels(colors);
                        texture.Apply();
                        
                        Sprite tempSprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 16f);
                        sr.sprite = tempSprite;
                        sr.color = Color.red;
                        
                        Debug.Log($"[CombatManager] Made enemy {enemy.name} visible at position {enemy.transform.position}");
                    }
                }
                
                // Log enemy position for debugging
                Debug.Log($"[CombatManager] Enemy {enemy.name} is at position: {enemy.transform.position}");
            }
        }
    }

    void Update()
    {
        // The GridClickHandler now handles input during preview state
        // No need for HandleMinionRepositioning() here anymore
    }

    public void SelectMinionForRepositioning(int previewIndex)
    {
        if (selectedMinionForRepositioning == previewIndex)
        {
            // Deselect if clicking the same minion again
            DeselectMinion();
            return;
        }

        DeselectMinion(); // Deselect any previously selected indicator

        selectedMinionForRepositioning = previewIndex;
        
        // Update visual feedback through click handler
        if (previewIndex < previewIndicators.Count)
        {
            PreviewUnitClickHandler clickHandler = previewIndicators[previewIndex].GetComponent<PreviewUnitClickHandler>();
            if (clickHandler != null)
            {
                clickHandler.SetSelected(true);
            }
        }
        
        Debug.Log($"[CombatManager] Selected minion at index {previewIndex} for repositioning.");
    }

    private void HandleMinionRepositioning()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[CombatManager] Left-click detected for repositioning.");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"[CombatManager] Raycast hit: {hit.collider.gameObject.name}");
                // Check if we clicked on another minion preview
                PreviewUnitClickHandler clickedUnit = hit.collider.GetComponent<PreviewUnitClickHandler>();
                if (clickedUnit != null)
                {
                    SelectMinionForRepositioning(clickedUnit.previewIndex);
                    return;
                }

                // We clicked somewhere on the grid, attempt to move
                Vector2Int gridPos = gridSpawnManager.WorldToGridPosition(hit.point);

                if (gridSpawnManager.grid[gridPos.x, gridPos.y].zone == GridSpawnManager.GridZone.PlayerZone &&
                    !IsCellOccupiedByOtherMinion(gridPos, selectedMinionForRepositioning))
                {
                    // Move the minion
                    Vector3 newPosition = gridSpawnManager.grid[gridPos.x, gridPos.y].worldPosition;
                    previewMinionPositions[selectedMinionForRepositioning] = newPosition;
                    previewIndicators[selectedMinionForRepositioning].transform.position = newPosition;

                    Debug.Log($"[CombatManager] Moved minion {selectedMinionForRepositioning} to grid cell {gridPos.x},{gridPos.y}.");

                    // Deselect after moving
                    DeselectMinion();
                }
                else
                {
                    Debug.Log($"[CombatManager] Invalid move location: cell {gridPos.x},{gridPos.y}.");
                }
            }
            else
            {
                Debug.Log("[CombatManager] Raycast did not hit any object.");
            }
        }

        if (Input.GetMouseButtonDown(1)) // Right-click to cancel
        {
            DeselectMinion();
        }
    }

    public bool IsCellOccupiedByOtherMinion(Vector2Int gridPos, int requestingMinionIndex)
    {
        for (int i = 0; i < previewMinionPositions.Count; i++)
        {
            if (i == requestingMinionIndex) continue; // Don't check against self

            Vector2Int otherMinionGridPos = gridSpawnManager.WorldToGridPosition(previewMinionPositions[i]);
            if (otherMinionGridPos == gridPos)
            {
                return true; // Cell is occupied by another minion
            }
        }
        return false;
    }

    private void HighlightSelectedIndicator(GameObject indicator)
    {
        var renderer = indicator.GetComponentInChildren<Renderer>();
        if (renderer != null && selectedIndicatorMaterial != null)
        {
            originalIndicatorMaterials = renderer.materials;
            renderer.material = selectedIndicatorMaterial;
        }
    }

    public void DeselectMinion()
    {
        if (selectedMinionForRepositioning != -1)
        {
            // Update visual feedback through click handler
            if (selectedMinionForRepositioning < previewIndicators.Count)
            {
                PreviewUnitClickHandler clickHandler = previewIndicators[selectedMinionForRepositioning].GetComponent<PreviewUnitClickHandler>();
                if (clickHandler != null)
                {
                    clickHandler.SetSelected(false);
                }
            }
            
            selectedMinionForRepositioning = -1;
            Debug.Log("[CombatManager] Deselected minion");
        }
    }
    
    public void MoveMinionToPosition(Vector2Int gridPos)
    {
        if (selectedMinionForRepositioning != -1)
        {
            Vector3 newPosition = gridSpawnManager.grid[gridPos.x, gridPos.y].worldPosition;
            previewMinionPositions[selectedMinionForRepositioning] = newPosition;
            previewIndicators[selectedMinionForRepositioning].transform.position = newPosition;
            
            Debug.Log($"[CombatManager] Moved minion {selectedMinionForRepositioning + 1} to grid position {gridPos}");
            
            DeselectMinion();
        }
    }
    
    void AddMinionNumberLabel(GameObject minionIndicator, int minionNumber)
    {
        // Use reliable 3D TextMesh - always works!
        GameObject textObj = new GameObject($"Number_{minionNumber}");
        textObj.transform.SetParent(minionIndicator.transform);
        
        // Position it above the minion  
        textObj.transform.localPosition = new Vector3(0, 0.5f, -0.1f);
        textObj.transform.localScale = Vector3.one * 0.5f;
        
        // Add 3D TextMesh component
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = minionNumber.ToString();
        textMesh.fontSize = 20;
        textMesh.color = Color.white;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        
        // Ensure it renders properly
        MeshRenderer meshRenderer = textObj.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // Use default material that always works
            meshRenderer.material = Resources.GetBuiltinResource<Material>("Default-Material.mat");
            meshRenderer.material.color = Color.white;
        }
        
        Debug.Log($"[CombatManager] Added 3D text number label '{minionNumber}' to minion indicator");
    }
}
