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
        
        // Start the first wave after a delay
        Invoke(nameof(StartNextWave), combatStartDelay);

        // Add test button listener
        if (testSpawnButton != null)
            testSpawnButton.onClick.AddListener(TestSpawnUnits);
    }

    public void StartNextWave()
    {
        if (currentWaveIndex >= waveConfigs.Count)
        {
            Debug.Log("[CombatManager] All waves completed! Victory!");
            HandleGameVictory();
            return;
        }

        WaveData currentWaveData = waveConfigs[currentWaveIndex];
        
        // Update UI
        if (gameplayUIManager != null)
        {
            gameplayUIManager.currentWave = currentWaveIndex + 1;
            // Call UpdateUI method if it exists
            var updateMethod = gameplayUIManager.GetType().GetMethod("UpdateUI");
            updateMethod?.Invoke(gameplayUIManager, null);
        }
        
        Debug.Log($"[CombatManager] Starting {currentWaveData.waveName}");
        
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
        int spawnPointIndex = 0;
        
        foreach (EnemySpawnInfo spawnInfo in waveData.enemyGroups)
        {
            if (spawnInfo.enemyType == null) continue;
            
            // Wait for spawn delay
            yield return new WaitForSeconds(spawnInfo.spawnDelay);
            
            // Spawn the specified number of this enemy type
            for (int i = 0; i < spawnInfo.count; i++)
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
                    enemyController.Initialize(spawnInfo.enemyType);
                    enemyController.OnDeath += OnEnemyDeath;
                    activeEnemies.Add(enemyController);
                    
                    Debug.Log($"[CombatManager] Spawned enemy: {spawnInfo.enemyType.enemyName}");
                }
                else
                {
                    Debug.LogError($"[CombatManager] Enemy prefab is missing EnemyController script!");
                    Destroy(enemyInstance);
                }
                
                spawnPointIndex++;
                
                // Small delay between individual spawns
                if (i < spawnInfo.count - 1)
                    yield return new WaitForSeconds(waveData.timeBetweenSpawns);
            }
        }
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
        
        // Mark wave as complete in GameData
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
        
        // For prototype: repeat wave 1 indefinitely for testing
        // In full game, you'd increment currentWaveIndex here
        Debug.Log("[CombatManager] Proceeding to card selection for next wave");
        
        // Always go to card selection after completing a wave
        SceneManager.LoadScene("CardSelection");
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
