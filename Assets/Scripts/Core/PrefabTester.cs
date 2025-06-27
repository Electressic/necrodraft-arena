using UnityEngine;

public class PrefabTester : MonoBehaviour
{
    [Header("Prefab References")]
    public GameObject minionPrefab;
    public GameObject enemyPrefab;
    
    [Header("Test Data")]
    public MinionData testMinionData;
    public EnemyData testEnemyData;
    
    void Start()
    {
        // Test minion spawning
        if (minionPrefab != null && testMinionData != null)
        {
            GameObject minion = Instantiate(minionPrefab, new Vector3(-2, 0, 0), Quaternion.identity);
            // Note: You'll need to create a test Minion data structure
            // For now, just test that the prefab spawns
        }
        
        // Test enemy spawning
        if (enemyPrefab != null && testEnemyData != null)
        {
            GameObject enemy = Instantiate(enemyPrefab, new Vector3(2, 0, 0), Quaternion.identity);
            EnemyController enemyController = enemy.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.Initialize(testEnemyData);
            }
        }
    }
}