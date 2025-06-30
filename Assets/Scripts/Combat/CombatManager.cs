using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI; // Added for Button

public class CombatManager : MonoBehaviour
{
    [Header("References")]
    public GameManager gameplayUIManager;
    public Transform[] minionSpawnPoints;
    public Transform[] enemySpawnPoints;

    [Header("Prefabs")]
    public GameObject minionPrefab; // Should have MinionController script
    public GameObject enemyPrefab;  // Should have EnemyController script

    [Header("Wave Configuration")]
    public List<WaveData> waveConfigs;
    private int currentWaveIndex = 0;

    [Header("Combat Settings")]
    public float combatStartDelay = 2f;
    public float waveCompleteDelay = 3f;

    [Header("Debug")]
    public bool enableDebugMode = false;

    [Header("Testing")]
    public Button testSpawnButton; // Added for testing

    // Private fields
    private List<MinionController> activeMinions = new List<MinionController>();
    private List<EnemyController> activeEnemies = new List<EnemyController>();
    private bool combatActive = false;

    void Start()
    {
        if (gameplayUIManager == null)
            gameplayUIManager = FindFirstObjectByType<GameManager>();
        
        // Sync with GameData's current wave progress
        int gameDataWave = GameData.GetCurrentWave();
        currentWaveIndex = gameDataWave - 1; // Convert to 0-based index
        
        Debug.Log($"[CombatManager] Starting combat for Wave {gameDataWave} (index {currentWaveIndex})");
        
        // Start the current wave after a delay
        Invoke(nameof(StartNextWave), combatStartDelay);

        // Add test button listener
        if (testSpawnButton != null)
            testSpawnButton.onClick.AddListener(TestSpawnUnits);
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
                
                Debug.Log($"[CombatManager] Spawned minion: {minionData.minionName}");
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
        
        // Use wave-based enemy spawning instead of fixed wave configs
        var enemyWaveInfo = GetEnemyInfoForWave(currentWave);
        
        foreach (var enemyInfo in enemyWaveInfo)
        {
            // Wait for spawn delay
            yield return new WaitForSeconds(enemyInfo.spawnDelay);
            
            // Spawn the specified number of this enemy type
            for (int i = 0; i < enemyInfo.count; i++)
            {
                // Check if we have spawn points
                if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
                {
                    Debug.LogError("[CombatManager] No enemy spawn points assigned!");
                    yield break;
                }
                
                if (spawnPointIndex >= enemySpawnPoints.Length)
                    spawnPointIndex = 0; // Wrap around if we run out of spawn points
                
                GameObject enemyInstance = Instantiate(enemyPrefab, enemySpawnPoints[spawnPointIndex].position, Quaternion.identity);
                EnemyController enemyController = enemyInstance.GetComponent<EnemyController>();
                
                if (enemyController != null)
                {
                    // Always use BasicZombie as base, but scale for current wave
                    EnemyData baseEnemyData = Resources.Load<EnemyData>("BasicZombie");
                    if (baseEnemyData == null)
                    {
                        // Fallback to wave config if BasicZombie not found in Resources
                        baseEnemyData = waveData.enemyGroups[0].enemyType;
                    }
                    
                    enemyController.Initialize(baseEnemyData);
                    
                    // Override the enemy name with our custom wave-based name
                    enemyController.gameObject.name = $"{enemyInfo.displayName} (W{currentWave})";
                    
                    enemyController.OnDeath += OnEnemyDeath;
                    activeEnemies.Add(enemyController);
                    
                    Debug.Log($"[CombatManager] Spawned {enemyInfo.displayName} (W{currentWave}) - HP: {enemyController.maxHP}, ATK: {enemyController.attackPower}");
                }
                else
                {
                    Debug.LogError($"[CombatManager] Enemy prefab is missing EnemyController script!");
                    Destroy(enemyInstance);
                }
                
                spawnPointIndex++;
                
                // Small delay between individual spawns
                if (i < enemyInfo.count - 1)
                    yield return new WaitForSeconds(0.8f);
            }
        }
    }
    
    // Generate wave-appropriate enemy spawn info
    System.Collections.Generic.List<(string displayName, int count, float spawnDelay)> GetEnemyInfoForWave(int wave)
    {
        var enemies = new System.Collections.Generic.List<(string, int, float)>();
        
        if (wave <= 3)
        {
            // Early waves: 2-3 basic enemies
            enemies.Add(("Basic Enemy", 2 + (wave - 1), 0f));
        }
        else if (wave <= 7)
        {
            // Act 1 end: Mixed enemy groups
            enemies.Add(("Shambling Corpse", 2, 0f));
            enemies.Add(("Rotting Zombie", 1 + (wave - 4), 1.5f));
        }
        else if (wave <= 12)
        {
            // Act 2: Stronger enemies in groups
            enemies.Add(("Undead Soldier", 2, 0f));
            enemies.Add(("Bone Warrior", 1, 1f));
            enemies.Add(("Corrupted Guardian", 1, 2f));
        }
        else if (wave <= 17)
        {
            // Act 2 end: Elite enemies
            enemies.Add(("Skeletal Champion", 1, 0f));
            enemies.Add(("Death Knight", 1, 1.5f));
            enemies.Add(("Undead Brute", 2, 2.5f));
        }
        else
        {
            // Act 3: Boss-tier enemies
            enemies.Add(("Bone Colossus", 1, 0f));
            enemies.Add(("Lich Commander", 1, 2f));
            enemies.Add(("Undead Horde", wave - 16, 3f));
        }
        
        return enemies;
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
    }

    void OnEnemyDeath(Unit enemy)
    {
        Debug.Log($"[CombatManager] Enemy {enemy.name} died!");
    }

    void OnWaveComplete()
    {
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
}
