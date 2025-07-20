using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WaveData", menuName = "NecroDraft/WaveData")]
public class WaveData : ScriptableObject
{
    [Header("Wave Info")]
    public string waveName = "Wave 1";
    public int waveNumber = 1;
    
    [Header("Enemy Configuration")]
    public List<EnemySpawnInfo> enemyGroups = new List<EnemySpawnInfo>();
    
    [Header("Wave Settings")]
    public float spawnDelay = 1f;
    public float timeBetweenSpawns = 2f;
    
    [Header("Rewards")]
    public List<PartData> rewardParts = new List<PartData>();
}

[System.Serializable]
public class EnemySpawnInfo
{
    public EnemyData enemyType;
    public int count = 1;
    public float spawnDelay = 0f;
}