using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System.Linq; // Added for FirstOrDefault

public class CardStyleCombatManager : MonoBehaviour
{
    [Header("Core References")]
    public GameManager gameplayUIManager;
    public CardSlotManager cardSlotManager;
    public MinionMovementManager minionMovementManager;
    
    [Header("Prefabs")]
    public GameObject minionPrefab;
    public GameObject enemyPrefab;
    
    [Header("Wave Configuration")]
    public List<WaveData> waveConfigs;
    private int currentWaveIndex = 0;
    
    [Header("Combat Settings")]
    public float combatStartDelay = 2f;
    public float waveCompleteDelay = 3f;
    public bool enablePreviewMode = true;
    public float turnDuration = 1.0f; 
    
    [Header("Position-Dependent Bonuses")]
    public float flankingDamageBonus = 0.5f; 
    public float backRowRangeBonus = 0.75f; 
    public bool enablePositionBonuses = true;
    
    [Header("Preview UI")]
    public Button startCombatButton;
    public TMPro.TextMeshProUGUI previewInfoText;
    public GameObject previewPanel;
    
    [Header("Visual Effects")]
    public GameObject damageNumberPrefab;
    public Transform effectsContainer;
    
    [Header("Debug")]
    public bool enableDebugMode = false;
    
    [Header("Enemy Art")]
    public Sprite bruiserSprite;
    public Sprite archerSprite;
    public Sprite bomberSprite;
    public Sprite assassinSprite;

    private List<MinionController> activeMinions = new List<MinionController>();
    private List<EnemyController> activeEnemies = new List<EnemyController>();
    private bool combatActive = false;
    public CombatState currentState { get; private set; } = CombatState.Preparing;
    
    private Queue<Unit> actionQueue = new Queue<Unit>();
    
    public enum CombatState
    {
        Preparing,
        Preview,
        Fighting,
        Complete
    }
    
    void Start()
    {
        InitializeCombatSystem();
        int gameDataWave = GameData.GetCurrentWave();
        currentWaveIndex = gameDataWave - 1;
        if (enablePreviewMode)
        {
            Invoke(nameof(StartWavePreview), combatStartDelay);
        }
        else
        {
            Invoke(nameof(StartCombat), combatStartDelay);
        }
    }
    
    void InitializeCombatSystem()
    {
        if (gameplayUIManager == null)
            gameplayUIManager = FindFirstObjectByType<GameManager>();
            
        if (cardSlotManager == null)
            cardSlotManager = FindFirstObjectByType<CardSlotManager>();
            
        if (minionMovementManager == null)
            minionMovementManager = FindFirstObjectByType<MinionMovementManager>();
        
        if (startCombatButton != null)
            startCombatButton.onClick.AddListener(StartCombatFromPreview);
            
        if (previewPanel != null)
            previewPanel.SetActive(false);
            
        if (effectsContainer == null)
        {
            GameObject container = new GameObject("EffectsContainer");
            container.transform.SetParent(transform);
            effectsContainer = container.transform;
        }
    }
    
    void StartWavePreview()
    {
        currentState = CombatState.Preview;
        WaveData waveData = null;
        if (waveConfigs.Count > 0)
        {
            waveData = waveConfigs[0];
        }
        else
        {
            return;
        }
        SpawnPlayerMinions();
        SpawnWaveEnemies();
        if (previewPanel != null)
        {
            previewPanel.SetActive(true);
            UpdatePreviewUI();
        }
    }
    
    void SpawnPlayerMinions()
    {
        List<Minion> playerMinions = MinionManager.GetMinionRoster();
        for (int i = 0; i < playerMinions.Count && i < (cardSlotManager.frontRowSlots + cardSlotManager.backRowSlots); i++)
        {
            GameObject minionObj = Instantiate(minionPrefab);
            MinionController controller = minionObj.GetComponent<MinionController>();
            if (controller != null)
            {
                controller.minionData = playerMinions[i];
                controller.Initialize(playerMinions[i].totalHP, playerMinions[i].totalAttack, 2f);
                controller.attackRange = 1.5f;
                controller.attackSpeed = 1f;
                CardSlotManager.RowType preferredRow = i < cardSlotManager.frontRowSlots ? 
                    CardSlotManager.RowType.Front : CardSlotManager.RowType.Back;
                if (cardSlotManager.PlaceUnitInSlot(minionObj, CardSlotManager.SlotType.Player, preferredRow))
                {
                    activeMinions.Add(controller);
                    controller.enableAutonomousCombat = false;
                    CardCombatAnimator animator = minionObj.GetComponent<CardCombatAnimator>();
                    if (animator == null)
                        animator = minionObj.AddComponent<CardCombatAnimator>();
                    UpdateMinionNumberText(minionObj, i + 1);
                    if (minionMovementManager != null)
                    {
                        minionMovementManager.OnMinionCreated(controller);
                    }
                }
                else
                {
                    Destroy(minionObj);
                }
            }
        }
    }
    
    void UpdateMinionNumberText(GameObject minionObj, int minionNumber)
    {
        int updatedCount = 0;
        FindAndUpdateAllNumberTexts(minionObj.transform, minionNumber, ref updatedCount);
    }

    void FindAndUpdateAllNumberTexts(Transform parent, int minionNumber, ref int updatedCount)
    {
        foreach (Transform child in parent)
        {
            if (child.name == "NumberText")
            {
                var tmpUGUI = child.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmpUGUI != null)
                {
                    tmpUGUI.text = minionNumber.ToString();
                    updatedCount++;
                }
                var tmp = child.GetComponent<TMPro.TextMeshPro>();
                if (tmp != null)
                {
                    tmp.text = minionNumber.ToString();
                    updatedCount++;
                }
            }
        }
    }
    
    void SpawnWaveEnemies()
    {
        var enemyInfo = GetEnemyInfoForWave(GameData.GetCurrentWave());
        
        if (enemyInfo.Count > 0)
        {
            SpawnDynamicEnemies(enemyInfo);
        }
        else
        {
            if (currentWaveIndex < waveConfigs.Count)
            {
                SpawnFromWaveData(waveConfigs[currentWaveIndex]);
            }
        }
    }
    
    void SpawnDynamicEnemies(System.Collections.Generic.List<(string displayName, int count, float spawnDelay)> enemyInfo)
    {
        int totalEnemies = 0;
        
        foreach (var enemy in enemyInfo)
        {
            totalEnemies += enemy.count;
        }
        
        int maxEnemies = Mathf.Min(totalEnemies, cardSlotManager.frontRowSlots + cardSlotManager.backRowSlots);
        int spawnedCount = 0;
        
        foreach (var enemy in enemyInfo)
        {
            for (int i = 0; i < enemy.count && spawnedCount < maxEnemies; i++)
            {
                GameObject enemyObj = CreateEnemyFromName(enemy.displayName);
                if (enemyObj != null)
                {
                CardSlotManager.RowType preferredRow = DetermineEnemyRowPlacement(spawnedCount, totalEnemies);
                        
                    if (cardSlotManager.PlaceUnitInSlot(enemyObj, CardSlotManager.SlotType.Enemy, preferredRow))
                    {
                        activeEnemies.Add(enemyObj.GetComponent<EnemyController>());
                        spawnedCount++;
                    }
                    else
                    {
                        Destroy(enemyObj);
                    }
                }
            }
        }
        
    }
    
    void SpawnFromWaveData(WaveData waveData)
    {
        int frontRowCount = 0;
        int backRowCount = 0;
        
        foreach (EnemySpawnInfo enemyGroup in waveData.enemyGroups)
        {
            for (int j = 0; j < enemyGroup.count; j++)
            {
                if (frontRowCount + backRowCount >= cardSlotManager.frontRowSlots + cardSlotManager.backRowSlots)
                    break;
                    
                GameObject enemyObj = Instantiate(enemyPrefab);
                EnemyController controller = enemyObj.GetComponent<EnemyController>();
                
                if (controller != null)
                {
                    controller.Initialize(enemyGroup.enemyType);
                    
                    CardSlotManager.RowType preferredRow = DetermineEnemyRowPlacement(
                        frontRowCount + backRowCount, 
                        waveData.enemyGroups.Sum(group => group.count),
                        enemyGroup.enemyType
                    );
                    
                    if (cardSlotManager.PlaceUnitInSlot(enemyObj, CardSlotManager.SlotType.Enemy, preferredRow))
                    {
                        activeEnemies.Add(controller);
                        
                        if (preferredRow == CardSlotManager.RowType.Front)
                            frontRowCount++;
                        else
                            backRowCount++;
                        
                        controller.enableAutonomousCombat = false;
                        
                    }
                    else
                    {
                        Debug.LogWarning($"[CardStyleCombatManager] Could not place enemy: {enemyGroup.enemyType.enemyName}");
                        Destroy(enemyObj);
                    }
                }
            }
        }
        
    }
    
    GameObject CreateEnemyFromName(string enemyName)
    {
        GameObject enemyObj = Instantiate(enemyPrefab);
        EnemyController controller = enemyObj.GetComponent<EnemyController>();
        
        if (controller != null)
        {
            EnemyData enemyData = GetEnemyDataByName(enemyName);
            
            if (enemyData != null)
            {
                controller.Initialize(enemyData);
            }
            else
            {
                if (waveConfigs.Count > 0 && waveConfigs[0].enemyGroups.Count > 0)
                {
                    controller.Initialize(waveConfigs[0].enemyGroups[0].enemyType);
                }
            }
            
            enemyObj.name = enemyName;
            
            controller.enableAutonomousCombat = false;
            
            CardCombatAnimator animator = enemyObj.GetComponent<CardCombatAnimator>();
            if (animator == null)
                animator = enemyObj.AddComponent<CardCombatAnimator>();
        }
        
        return enemyObj;
    }
    
    EnemyData GetEnemyDataByName(string enemyName)
    {
        EnemyData tempData = ScriptableObject.CreateInstance<EnemyData>();
        
        switch (enemyName.ToLower())
        {
            case "shambling corpse":
            case "rotting zombie":
            case "undead brute":
            case "bone warrior":
            case "death guard":
            case "undead soldier":
            case "corrupted guardian":
                SetupBruiserEnemy(tempData, enemyName);
                break;
                
            case "skeletal archer":
            case "bone marksman":
            case "undead archer":
            case "spectral bowman":
                SetupArcherEnemy(tempData, enemyName);
                break;
                
            case "undead scout":
            case "shadow wraith":
            case "void stalker":
            case "phantom assassin":
            case "cursed stalker":
                SetupAssassinEnemy(tempData, enemyName);
                break;
                
            case "skeletal mage":
            case "death cultist":
            case "lich minion":
            case "bone seer":
            case "spectral caster":
                SetupSniperEnemy(tempData, enemyName);
                break;

            case "explosive skeleton":
            case "plague bearer":
            case "toxic corpse":
            case "volatile wraith":
                SetupBomberEnemy(tempData, enemyName);
                break;

            default:
                SetupDefaultEnemy(tempData, enemyName);
                break;
        }
        
        return tempData;
    }
    
    private Sprite GetEnemySpriteForTypeOrName(string enemyName, EnemyTargetingType type)
    {
        string lowerName = enemyName.ToLower();
        switch (type)
        {
            case EnemyTargetingType.Bruiser:
                return bruiserSprite;
            case EnemyTargetingType.Archer:
                return archerSprite;
            case EnemyTargetingType.Bomber:
                return bomberSprite;
            case EnemyTargetingType.Assassin:
                return assassinSprite;
            case EnemyTargetingType.Sniper:
                return archerSprite;
            default:
                return bruiserSprite;
        }
    }
    
    void SetupBruiserEnemy(EnemyData data, string name)
    {
        data.enemyName = name;
        data.targetingType = EnemyTargetingType.Bruiser;
        data.maxHP = 18 + Random.Range(-3, 4);
        data.attackPower = 4 + Random.Range(-1, 2);
        data.attackSpeed = 1.5f;
        data.moveSpeed = 1.0f;
        data.attackRange = 1.5f;
        data.enemyColor = new Color(0.7f, 0.3f, 0.3f, 1f); // Dark red
        data.description = "A sturdy undead that focuses on breaking through front-line defenses.";
        data.enemySprite = GetEnemySpriteForTypeOrName(name, EnemyTargetingType.Bruiser);
    }
    
    void SetupArcherEnemy(EnemyData data, string name)
    {
        data.enemyName = name;
        data.targetingType = EnemyTargetingType.Archer;
        data.maxHP = 12 + Random.Range(-2, 3);
        data.attackPower = 6 + Random.Range(-1, 2);
        data.attackSpeed = 2.0f;
        data.moveSpeed = 1.2f;
        data.attackRange = 2.5f;
        data.enemyColor = new Color(0.9f, 0.9f, 0.7f, 1f); // Bone white
        data.description = "A ranged attacker that ignores front-line protection to target the back row.";
        data.enemySprite = GetEnemySpriteForTypeOrName(name, EnemyTargetingType.Archer);
    }
    
    void SetupAssassinEnemy(EnemyData data, string name)
    {
        data.enemyName = name;
        data.targetingType = EnemyTargetingType.Assassin;
        data.maxHP = 8 + Random.Range(-1, 3);
        data.attackPower = 8 + Random.Range(-1, 3);
        data.attackSpeed = 2.5f;
        data.moveSpeed = 1.8f;
        data.attackRange = 1.8f;
        data.enemyColor = new Color(0.3f, 0.1f, 0.5f, 1f); // Dark purple
        data.description = "A swift assassin that hunts the most vulnerable targets.";
        data.enemySprite = GetEnemySpriteForTypeOrName(name, EnemyTargetingType.Assassin);
    }
    
    void SetupSniperEnemy(EnemyData data, string name)
    {
        data.enemyName = name;
        data.targetingType = EnemyTargetingType.Sniper;
        data.maxHP = 10 + Random.Range(-1, 3);
        data.attackPower = 10 + Random.Range(-2, 3);
        data.attackSpeed = 1.2f;
        data.moveSpeed = 0.8f;
        data.attackRange = 3.5f;
        data.enemyColor = new Color(0.1f, 0.6f, 0.9f, 1f);
        data.description = "An elite marksman that eliminates the most dangerous threats.";
        data.enemySprite = GetEnemySpriteForTypeOrName(name, EnemyTargetingType.Sniper);
    }
    
    void SetupBomberEnemy(EnemyData data, string name)
    {
        data.enemyName = name;
        data.targetingType = EnemyTargetingType.Bomber;
        data.maxHP = 14 + Random.Range(-2, 3);
        data.attackPower = 5 + Random.Range(-1, 2);
        data.attackSpeed = 1.8f;
        data.moveSpeed = 1.1f;
        data.attackRange = 2.0f;
        data.enemyColor = new Color(0.4f, 0.7f, 0.2f, 1f);
        data.description = "A volatile enemy that seeks packed formations for maximum area damage.";
        data.enemySprite = GetEnemySpriteForTypeOrName(name, EnemyTargetingType.Bomber);
    }
    
    void SetupDefaultEnemy(EnemyData data, string name)
    {
        data.enemyName = name;
        data.targetingType = EnemyTargetingType.Bruiser;
        data.maxHP = 15;
        data.attackPower = 4;
        data.attackSpeed = 1.5f;
        data.moveSpeed = 1.0f;
        data.attackRange = 1.5f;
        data.enemyColor = Color.red;
        data.description = "A basic undead enemy.";
        data.enemySprite = GetEnemySpriteForTypeOrName(name, EnemyTargetingType.Bruiser);
    }
    
    void UpdatePreviewUI()
    {
        if (previewInfoText != null)
        {
            string info = $"Wave {currentWaveIndex + 1}\n";
            info += $"Minions: {activeMinions.Count}\n";
            info += $"Enemies: {activeEnemies.Count}\n";
            info += "Ready to fight?";
            
            previewInfoText.text = info;
        }
    }
    
    public void StartCombatFromPreview()
    {
        if (previewPanel != null)
            previewPanel.SetActive(false);
            
        StartCombat();
    }
    
    void StartCombat()
    {
        currentState = CombatState.Fighting;
        combatActive = true;
        
        if (minionMovementManager != null)
        {
            minionMovementManager.SetDragAndDropEnabled(false);
            minionMovementManager.SetClickToMoveEnabled(false);
        }   
        Debug.Log($"[CardStyleCombatManager] Combat started! {activeMinions.Count} vs {activeEnemies.Count}");
        
        Debug.Log($"[CardStyleCombatManager] All units ready for combat");
        
        StartCoroutine(CombatLoop());
    }
    
    IEnumerator CombatLoop()
    {
        while (combatActive && activeMinions.Count > 0 && activeEnemies.Count > 0)
        {
            BuildActionQueue();
            
            yield return StartCoroutine(ProcessActionQueue());
            
            CleanupDeadUnits();
            
            if (activeMinions.Count == 0)
            {
                EndCombat(false);
                break;
            }
            else if (activeEnemies.Count == 0)
            {
                EndCombat(true);
                break;
            }
            
            yield return new WaitForSeconds(turnDuration);
        }
    }
    
    void BuildActionQueue()
    {
        actionQueue.Clear();
        
        List<Unit> allUnits = new List<Unit>();
        
        foreach (var minion in activeMinions)
        {
            if (minion != null && minion.isAlive)
                allUnits.Add(minion);
        }
        
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && enemy.isAlive)
                allUnits.Add(enemy);
        }
        
        allUnits.Sort((a, b) => b.attackSpeed.CompareTo(a.attackSpeed));
        
        foreach (var unit in allUnits)
        {
            actionQueue.Enqueue(unit);
        }
        
    }
    
    IEnumerator ProcessActionQueue()
    {
        while (actionQueue.Count > 0)
        {
            Unit currentUnit = actionQueue.Dequeue();
            
            if (currentUnit != null && currentUnit.isAlive)
            {
                yield return StartCoroutine(ProcessUnitAction(currentUnit));
            }
        }
    }
    
    IEnumerator ProcessUnitAction(Unit unit)
    {
        Unit target = FindTargetForUnit(unit);
        
        if (target != null)
        {
            Debug.Log($"[CardStyleCombatManager] {unit.name} attacks {target.name}");
            
            CardCombatAnimator animator = unit.GetComponent<CardCombatAnimator>();
            
            if (animator != null)
            {
                bool actionComplete = false;
                
                animator.PerformAttackAnimation(
                    target.transform,
                                         onImpact: () => {
                         if (IsAttackBlockedByShieldWall(unit, target))
                         {
                             ShowDamageNumber(target.transform.position + Vector3.up * 0.5f, 0);
                             return;
                         }
                         
                         float baseDamage = unit.attackPower;
                         float positionMultiplier = CalculatePositionBonus(unit, target);
                         int finalDamage = Mathf.RoundToInt(baseDamage * positionMultiplier);
                         
                         target.TakeDamage(finalDamage);
                         ShowDamageNumber(target.transform.position, finalDamage);
                     },
                    onComplete: () => {
                        actionComplete = true;
                    }
                );
                
                while (!actionComplete)
                {
                    yield return null;
                }
            }
                         else
             {
                 if (IsAttackBlockedByShieldWall(unit, target))
                 {
                     ShowDamageNumber(target.transform.position + Vector3.up * 0.5f, 0);
                 }
                 else
                 {
                     float baseDamage = unit.attackPower;
                     float positionMultiplier = CalculatePositionBonus(unit, target);
                     int finalDamage = Mathf.RoundToInt(baseDamage * positionMultiplier);
                     
                     target.TakeDamage(finalDamage);
                     ShowDamageNumber(target.transform.position, finalDamage);
                 }
                 yield return new WaitForSeconds(0.5f);
             }
        }
        else
        {
            Debug.Log($"[CardStyleCombatManager] {unit.name} has no valid target");
        }
    }
    
    Unit FindTargetForUnit(Unit attacker)
    {
        CardSlotManager.SlotType attackerSide;
        CardSlotManager.SlotType targetSide;
        
        if (attacker is MinionController)
        {
            attackerSide = CardSlotManager.SlotType.Player;
            targetSide = CardSlotManager.SlotType.Enemy;
            return FindTargetWithStrategy(attackerSide, targetSide, EnemyTargetingType.Bruiser, attacker);
        }
        else if (attacker is EnemyController enemyController)
        {
            attackerSide = CardSlotManager.SlotType.Enemy;
            targetSide = CardSlotManager.SlotType.Player;
            
            EnemyTargetingType targetingType = EnemyTargetingType.Bruiser;
            if (enemyController.enemyData != null)
            {
                targetingType = enemyController.enemyData.targetingType;
            }
            
            return FindTargetWithStrategy(attackerSide, targetSide, targetingType, attacker);
        }
        else
        {
            return null;
        }
    }
    
    Unit FindTargetWithStrategy(CardSlotManager.SlotType attackerSide, CardSlotManager.SlotType targetSide, EnemyTargetingType strategy, Unit attacker)
    {   
        var frontRow = cardSlotManager.GetOccupiedSlots(targetSide, CardSlotManager.RowType.Front);
        var backRow = cardSlotManager.GetOccupiedSlots(targetSide, CardSlotManager.RowType.Back);
        var allTargets = new List<Unit>();
        
        // Collect all valid targets
        foreach (var slot in frontRow.Concat(backRow))
        {
            if (slot.occupant != null)
            {
                Unit target = slot.occupant.GetComponent<Unit>();
                if (target != null && target.isAlive)
                {
                    allTargets.Add(target);
                }
            }
        }
        
        if (allTargets.Count == 0)
            return null;
        
        Unit selectedTarget = null;
        
        switch (strategy)
        {
            case EnemyTargetingType.Bruiser:
                selectedTarget = FindBruiserTarget(frontRow, backRow);
                break;
                
            case EnemyTargetingType.Archer:
                selectedTarget = FindArcherTarget(backRow, frontRow);
                break;
                
            case EnemyTargetingType.Assassin:
                selectedTarget = FindAssassinTarget(allTargets);
                break;
                
            case EnemyTargetingType.Sniper:
                selectedTarget = FindSniperTarget(allTargets);
                break;
                
            case EnemyTargetingType.Bomber:
                selectedTarget = FindBomberTarget(frontRow, backRow);
                break;
                
            default:
                selectedTarget = FindBruiserTarget(frontRow, backRow);
                break;
        }
        
        if (selectedTarget != null)
        {
            var targetSlot = GetSlotForUnit(selectedTarget.gameObject);
            string strategyName = strategy.ToString();
            if (targetSlot != null)
            {
                Debug.Log($"[CardStyleCombatManager] {attacker.name} ({strategyName}) targeting {selectedTarget.name} in {targetSlot.type} {targetSlot.row} row");
            }
        }
        
        return selectedTarget;
    }
    
    Unit FindBruiserTarget(List<CardSlotManager.CardSlot> frontRow, List<CardSlotManager.CardSlot> backRow)
    {
        var targetSlots = frontRow.Count > 0 ? frontRow : backRow;
        return GetLeftmostTarget(targetSlots);
    }
    
    Unit FindArcherTarget(List<CardSlotManager.CardSlot> backRow, List<CardSlotManager.CardSlot> frontRow)
    {
        var targetSlots = backRow.Count > 0 ? backRow : frontRow;
        return GetLeftmostTarget(targetSlots);
    }
    
    Unit FindAssassinTarget(List<Unit> allTargets)
    {
        Unit lowestHPTarget = null;
        float lowestHPPercent = float.MaxValue;
        
        foreach (Unit target in allTargets)
        {
            float hpPercent = (float)target.currentHP / target.maxHP;
            if (hpPercent < lowestHPPercent)
            {
                lowestHPPercent = hpPercent;
                lowestHPTarget = target;
            }
        }
        
        return lowestHPTarget;
    }
    
    Unit FindSniperTarget(List<Unit> allTargets)
    {
        Unit highestATKTarget = null;
        int highestATK = -1;
        
        foreach (Unit target in allTargets)
        {
            if (target.attackPower > highestATK)
            {
                highestATK = target.attackPower;
                highestATKTarget = target;
            }
        }
        
        return highestATKTarget;
    }
    
    Unit FindBomberTarget(List<CardSlotManager.CardSlot> frontRow, List<CardSlotManager.CardSlot> backRow)
    {
        var allSlots = frontRow.Concat(backRow).ToList();
        
        foreach (var slot in allSlots)
        {
            if (slot.slotIndex == 1 && slot.occupant != null)
            {
                Unit target = slot.occupant.GetComponent<Unit>();
                if (target != null && target.isAlive)
                    return target;
            }
        }
        
        return GetLeftmostTarget(allSlots);
    }
    
    Unit GetLeftmostTarget(List<CardSlotManager.CardSlot> slots)
    {
        Unit leftmostTarget = null;
        float leftmostX = float.MaxValue;
        
        foreach (var slot in slots)
        {
            if (slot.occupant != null)
            {
                Unit target = slot.occupant.GetComponent<Unit>();
                if (target != null && target.isAlive)
                {
                    float targetX = target.transform.position.x;
                    if (targetX < leftmostX)
                    {
                        leftmostX = targetX;
                        leftmostTarget = target;
                    }
                }
            }
        }
        
        return leftmostTarget;
    }
    
    CardSlotManager.RowType DetermineEnemyRowPlacement(int currentEnemyCount, int totalEnemies, EnemyData enemyType = null)
    {
        if (enemyType == null)
        {
            return DetermineBalancedPlacement(currentEnemyCount, totalEnemies);
        }
        
        switch (enemyType.targetingType)
        {
            case EnemyTargetingType.Bruiser:
                return CardSlotManager.RowType.Front;
                
            case EnemyTargetingType.Archer:
                return CardSlotManager.RowType.Back;
                
            case EnemyTargetingType.Sniper:
                return CardSlotManager.RowType.Back;
                
            case EnemyTargetingType.Assassin:
                return CardSlotManager.RowType.Front;
                
            case EnemyTargetingType.Bomber:
                return CardSlotManager.RowType.Front;
                
            default:
                return DetermineBalancedPlacement(currentEnemyCount, totalEnemies);
        }
    }
    
    CardSlotManager.RowType DetermineEnemyRowPlacement(int currentEnemyCount, int totalEnemies)
    {
        return DetermineBalancedPlacement(currentEnemyCount, totalEnemies);
    }
    
    CardSlotManager.RowType DetermineBalancedPlacement(int currentEnemyCount, int totalEnemies)
    {
        if (totalEnemies <= 2)
        {
            return currentEnemyCount == 0 ? CardSlotManager.RowType.Front : CardSlotManager.RowType.Back;
        }
        else if (totalEnemies <= 4)
        {
            return currentEnemyCount < 2 ? CardSlotManager.RowType.Front : CardSlotManager.RowType.Back;
        }
        else
        {
            return currentEnemyCount < 3 ? CardSlotManager.RowType.Front : CardSlotManager.RowType.Back;
        }
    }
    
    List<CardSlotManager.CardSlot> GetValidTargetsWithRange(CardSlotManager.SlotType attackerSide, CardSlotManager.SlotType targetSide, bool attackerHasRange)
    {
        var frontRow = cardSlotManager.GetOccupiedSlots(targetSide, CardSlotManager.RowType.Front);
        var backRow = cardSlotManager.GetOccupiedSlots(targetSide, CardSlotManager.RowType.Back);
        
        if (attackerHasRange && enablePositionBonuses)
        {
            var allTargets = new List<CardSlotManager.CardSlot>();
            allTargets.AddRange(frontRow);
            allTargets.AddRange(backRow);
            return allTargets;
        }
        
        if (frontRow.Count > 0)
        {
            return frontRow;
        }
        
        return backRow;
    }
    
    CardSlotManager.CardSlot GetSlotForUnit(GameObject unit)
    {
        return cardSlotManager.AllSlots.FirstOrDefault(slot => slot.occupant == unit);
    }
    
    float CalculatePositionBonus(Unit attacker, Unit target)
    {
        if (!enablePositionBonuses)
            return 1.0f;
            
        float damageMultiplier = 1.0f;
        
        var attackerSlot = GetSlotForUnit(attacker.gameObject);
        var targetSlot = GetSlotForUnit(target.gameObject);
        
        if (attackerSlot == null || targetSlot == null)
            return damageMultiplier;
        
        if (attackerSlot.row == CardSlotManager.RowType.Front)
        {
            var frontRowSlots = cardSlotManager.GetRow(attackerSlot.type, CardSlotManager.RowType.Front);
            int attackerIndex = attackerSlot.slotIndex;
            
            if (attackerIndex == 0 || attackerIndex == frontRowSlots.Length - 1)
            {
                damageMultiplier += flankingDamageBonus;
            }
        }
        
        if (attackerSlot.row == CardSlotManager.RowType.Back && targetSlot.row == CardSlotManager.RowType.Back)
        {
            damageMultiplier *= backRowRangeBonus;
        }
        
        return damageMultiplier;
    }
    
    bool IsAttackBlockedByShieldWall(Unit attacker, Unit target)
    {
        if (!enablePositionBonuses)
            return false;
            
        var attackerSlot = GetSlotForUnit(attacker.gameObject);
        var targetSlot = GetSlotForUnit(target.gameObject);
        
        if (attackerSlot == null || targetSlot == null)
            return false;

        if (targetSlot.row != CardSlotManager.RowType.Back)
            return false;
        
        var frontRowSlots = cardSlotManager.GetRow(targetSlot.type, CardSlotManager.RowType.Front);
        
        if (targetSlot.slotIndex < frontRowSlots.Length)
        {
            var frontSlot = frontRowSlots[targetSlot.slotIndex];
            if (frontSlot.isOccupied && frontSlot.occupant != null)
            {
                if (Random.Range(0f, 1f) < 0.25f)
                {
                    Debug.Log($"[CardStyleCombatManager] {frontSlot.occupant.name} blocks attack on {target.name} with Shield Wall!");
                    return true;
                }
            }
        }
        
        return false;
    }
    
    void ShowDamageNumber(Vector3 position, int damage)
    {
        if (damageNumberPrefab != null)
        {
            GameObject damageNumber = Instantiate(damageNumberPrefab, position, Quaternion.identity);
            
            TMPro.TextMeshPro textComponent = damageNumber.GetComponent<TMPro.TextMeshPro>();
            if (textComponent != null)
            {
                textComponent.text = damage.ToString();
            }
            
            if (effectsContainer != null)
                damageNumber.transform.SetParent(effectsContainer);
        }
    }
    
    void CleanupDeadUnits()
    {
        for (int i = activeMinions.Count - 1; i >= 0; i--)
        {
            if (activeMinions[i] == null || !activeMinions[i].isAlive)
            {
                if (activeMinions[i] != null)
                {
                    cardSlotManager.RemoveUnitFromSlot(activeMinions[i].gameObject);
                    Destroy(activeMinions[i].gameObject);
                }
                activeMinions.RemoveAt(i);
            }
        }
        
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null || !activeEnemies[i].isAlive)
            {
                if (activeEnemies[i] != null)
                {
                    cardSlotManager.RemoveUnitFromSlot(activeEnemies[i].gameObject);
                    Destroy(activeEnemies[i].gameObject);
                }
                activeEnemies.RemoveAt(i);
            }
        }
    }
    
    void EndCombat(bool playerWon)
    {
        combatActive = false;
        currentState = CombatState.Complete;
        
        Debug.Log($"[CardStyleCombatManager] Combat ended. Player won: {playerWon}");
        
        if (playerWon)
        {
            GameData.CompleteWave();

            if (minionMovementManager != null)
            {
                minionMovementManager.SetDragAndDropEnabled(true);
                minionMovementManager.SetClickToMoveEnabled(true);
            }
            
            Debug.Log("Wave complete! Returning to card selection for new parts...");
            Invoke(nameof(LoadCardSelection), waveCompleteDelay);
        }
        else
        {
            Debug.Log("Player defeated! Game over.");
            Invoke(nameof(LoadGameOver), waveCompleteDelay);
        }
    }
    
    void LoadNextWave()
    {
        ClearBattlefield();
        StartWavePreview();
    }
    
    void ClearBattlefield()
    {
        foreach (var minion in activeMinions)
        {
            if (minion != null)
            {
                cardSlotManager.RemoveUnitFromSlot(minion.gameObject);
                Destroy(minion.gameObject);
            }
        }
        
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                cardSlotManager.RemoveUnitFromSlot(enemy.gameObject);
                Destroy(enemy.gameObject);
            }
        }
        
        activeMinions.Clear();
        activeEnemies.Clear();
        
        currentState = CombatState.Preparing;
    }
    
    void LoadCardSelection()
    {
        SceneManager.LoadScene("MinionAssembly");
    }
    
    System.Collections.Generic.List<(string displayName, int count, float spawnDelay)> GetEnemyInfoForWave(int wave)
    {
        var enemies = new System.Collections.Generic.List<(string, int, float)>();
        
        switch (wave)
        {
            case 1:
                enemies.Add(("Shambling Corpse", 1, 0f));
                break;
            case 2:
                enemies.Add(("Rotting Zombie", 2, 0f));
                break;
            case 3:
                enemies.Add(("Basic Skeleton", 2, 0f));
                enemies.Add(("Shambling Corpse", 1, 1.5f));
                break;
            case 4:
                enemies.Add(("Undead Scout", 3, 0f));
                break;
            case 5:
                enemies.Add(("Bone Warrior", 2, 0f));
                enemies.Add(("Rotting Zombie", 2, 1f));
                break;
            case 6:
                enemies.Add(("Skeletal Archer", 3, 0f));
                enemies.Add(("Undead Brute", 1, 2f));
                break;
            case 7:
                enemies.Add(("Death Guard", 2, 0f));
                enemies.Add(("Bone Warrior", 3, 1f));
                break;
                
            case 8:
                enemies.Add(("Undead Soldier", 4, 0f));
                enemies.Add(("Skeletal Mage", 1, 1.5f));
                break;
            case 9:
                enemies.Add(("Corrupted Guardian", 2, 0f));
                enemies.Add(("Death Cultist", 3, 1f));
                break;
            case 10:
                enemies.Add(("Bone Colossus", 1, 0f));
                enemies.Add(("Skeletal Warrior", 4, 1f));
                break;
            case 11:
                enemies.Add(("Undead Champion", 3, 0f));
                enemies.Add(("Shadow Wraith", 2, 2f));
                break;
            case 12:
                enemies.Add(("Death Knight", 2, 0f));
                enemies.Add(("Bone Spearman", 4, 1f));
                break;
            case 13:
                enemies.Add(("Lich Acolyte", 1, 0f));
                enemies.Add(("Undead Horde", 4, 1f));
                break;
            case 14:
                enemies.Add(("Bone Dragon", 1, 0f));
                enemies.Add(("Death Knight", 2, 2f));
                enemies.Add(("Skeletal Archer", 2, 3f));
                break;
                
            case 15:
                enemies.Add(("Undead Titan", 2, 0f));
                enemies.Add(("Shadow Legion", 3, 1f));
                break;
            case 16:
                enemies.Add(("Lich Commander", 1, 0f));
                enemies.Add(("Death Knight", 3, 1f));
                enemies.Add(("Bone Colossus", 1, 2f));
                break;
            case 17:
                enemies.Add(("Ancient Bone Lord", 1, 0f));
                enemies.Add(("Undead Army", 4, 1f));
                break;
            case 18:
                enemies.Add(("Shadow Overlord", 2, 0f));
                enemies.Add(("Death Incarnate", 1, 2f));
                enemies.Add(("Undead Elite", 2, 3f));
                break;
            case 19:
                enemies.Add(("Bone Emperor", 1, 0f));
                enemies.Add(("Lich Supreme", 1, 1f));
                enemies.Add(("Death Legion", 3, 2f));
                break;
            case 20:
                enemies.Add(("Necro Overlord", 1, 0f));
                enemies.Add(("Ancient Death Knight", 2, 2f));
                enemies.Add(("Undead Apocalypse", 2, 4f));
                break;
                
            default:
                int endlessMultiplier = Mathf.Min(wave - 20, 3);
                enemies.Add(("Endless Horror", 1 + endlessMultiplier, 0f));
                enemies.Add(("Void Legion", Mathf.Min(2 + endlessMultiplier, 4), 1f));
                break;
        }
        
        return enemies;
    }
    
    void LoadGameOver()
    {
        SceneManager.LoadScene("GameOver");
    }
} 