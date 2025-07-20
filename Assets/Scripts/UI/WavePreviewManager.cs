using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class WavePreviewManager : MonoBehaviour
{
    [Header("UI References")]
    public TMPro.TextMeshProUGUI wavePreviewTitle;
    public TMPro.TextMeshProUGUI enemyListText;
    public TMPro.TextMeshProUGUI positioningHintText;
    public GameObject wavePreviewPanel;
    
    [Header("Wave Data")]
    [Header("Positioning Hints")]
    [TextArea(3, 5)]
    public string bruiserHint = "Bruisers target your front row (leftmost minion). Place tanky minions on the left to protect your team.";
    
    [TextArea(3, 5)]
    public string archerHint = "Archers target your back row first. Keep your squishy minions in the middle, not on the right.";
    
    [TextArea(3, 5)]
    public string assassinHint = "Assassins target your lowest HP minion. Spread damage across your team to avoid easy targets.";
    
    [TextArea(3, 5)]
    public string sniperHint = "Snipers target your highest ATK minion. Consider hiding your strongest attacker in the middle.";
    
    [TextArea(3, 5)]
    public string bomberHint = "Bombers target center positions. Keep important minions on the sides to avoid area damage.";
    
    [Header("Mixed Enemy Hints")]
    [TextArea(3, 5)]
    public string mixedHint = "Multiple enemy types detected. Balance your formation to handle different targeting patterns.";
    
    void Start()
    {
        UpdateWavePreview();
    }
    
    public void UpdateWavePreview()
    {
        int nextWave = GameData.GetCurrentWave();
        
        if (wavePreviewTitle != null)
        {
            wavePreviewTitle.text = $"Next Wave: {nextWave}";
        }
        
        if (nextWave <= 20)
        {
            DisplayEnemyInfo(nextWave);
            DisplayPositioningHint(nextWave);
        }
        else
        {
            if (enemyListText != null)
                enemyListText.text = "Final wave completed!";
            if (positioningHintText != null)
                positioningHintText.text = "Congratulations on completing all waves!";
        }
    }
    
    private void DisplayEnemyInfo(int waveNumber)
    {
        if (enemyListText == null) return;
        
        string enemyText = "Enemies:\n";
        
        var enemies = GetEnemyInfoForWave(waveNumber);
        
        foreach (var enemy in enemies)
        {
            string countText = enemy.count > 1 ? $"x{enemy.count}" : "";
            enemyText += $"• {enemy.displayName} {countText}\n";
        }
        
        enemyListText.text = enemyText;
    }
    
    private void DisplayPositioningHint(int waveNumber)
    {
        if (positioningHintText == null) return;
        
        string waveSpecificHint = GetWaveSpecificHint(waveNumber);
        
        if (!string.IsNullOrEmpty(waveSpecificHint))
        {
            positioningHintText.text = waveSpecificHint;
            return;
        }
        
        var enemies = GetEnemyInfoForWave(waveNumber);
        var enemyTypes = AnalyzeEnemyTypesFromNames(enemies);
        
        string hint = "";
        
        if (enemyTypes.Count == 1)
        {
            hint = GetHintForTargetingType(enemyTypes[0]);
        }
        else if (enemyTypes.Count > 1)
        {
            hint = mixedHint;
            hint += "\n\nEnemy types in this wave:";
            foreach (var type in enemyTypes)
            {
                hint += $"\n• {GetEnemyTypeName(type)}";
            }
        }
        else
        {
            hint = "No enemy information available.";
        }
        
        positioningHintText.text = hint;
    }
    
    private string GetWaveSpecificHint(int waveNumber)
    {
        switch (waveNumber)
        {
            // ACT 1: FOUNDATION (Waves 1-7)
            case 1:
                return "First wave! Place your minion in the front row. This simple enemy targets the leftmost position.";
                
            case 2:
                return "Two enemies this time! They'll both target your front row. Consider using a tanky minion or healing abilities.";
                
            case 3:
                return "Mixed enemy types! The Skeleton targets front row, but the Corpse will arrive later. Position to handle staggered spawns.";
                
            case 4:
                return "Three fast enemies! They'll overwhelm your front row quickly. Consider using defensive abilities or multiple minions if available.";
                
            case 5:
                return "First minion unlock wave! You can now field 2 minions. Use one as a tank in front, one as damage dealer in back.";
                
            case 6:
                return "Archer enemies! They target your back row first. Keep your squishy minions in the middle, not on the right side.";
                
            case 7:
                return "Act 1 finale! Multiple enemies with staggered spawns. Use your strongest minion in front to handle the initial rush.";
                
            // ACT 2: MASTERY (Waves 8-14)
            case 8:
                return "Four soldiers plus a mage! The mage spawns later and may have special abilities. Focus on clearing the front line first.";
                
            case 9:
                return "Second minion unlock! You can now field 3 minions. Guardians are tough - use armor penetration or high damage to break through.";
                
            case 10:
                return "Boss wave! Bone Colossus is a heavy enemy. Use your strongest minion in front, with support minions in back for sustained damage.";
                
            case 11:
                return "Shadow Wraiths target your lowest HP minion! Spread damage across your team and use healing abilities to avoid easy targets.";
                
            case 12:
                return "Death Knights are elite enemies! They have high defense. Use armor penetration abilities or focus fire to take them down quickly.";
                
            case 13:
                return "Third minion unlock! You can now field 4 minions. The Lich Acolyte may have special abilities - prioritize eliminating it first.";
                
            case 14:
                return "Act 2 finale! Dragon + Knights + Archers. The dragon spawns first - focus it down before the ranged enemies overwhelm you.";
                
            // ACT 3: ENDGAME (Waves 15-20)
            case 15:
                return "Epic parts available! Titans are massive enemies. Use your strongest minion with epic equipment to match their power.";
                
            case 16:
                return "Fourth minion unlock! You can now field all 5 minions. Lich Commander is a priority target - eliminate it before it buffs allies.";
                
            case 17:
                return "Ancient Bone Lord is a legendary enemy! Use your most powerful minion with the best equipment. This is a test of raw power.";
                
            case 18:
                return "Shadow Overlords have special abilities! Death Incarnate spawns later - save your strongest abilities for when it appears.";
                
            case 19:
                return "Bone Emperor and Lich Supreme! These are the most powerful enemies yet. Use all 5 minions with your best equipment and abilities.";
                
            case 20:
                return "FINAL BOSS WAVE! Necro Overlord is the ultimate challenge. Use perfect positioning, all 5 minions, and your strongest equipment. Good luck!";
                
            default:
                return "Endless mode! Enemies scale with wave number. Use your strongest formation and be prepared for anything.";
        }
    }
    
    private List<EnemyTargetingType> AnalyzeEnemyTypesFromNames(List<(string displayName, int count, float spawnDelay)> enemies)
    {
        var types = new List<EnemyTargetingType>();
        
        foreach (var enemy in enemies)
        {
            string name = enemy.displayName.ToLower();
            
            if (name.Contains("archer") || name.Contains("sniper") || name.Contains("marksman"))
            {
                types.Add(EnemyTargetingType.Archer);
            }
            else if (name.Contains("assassin") || name.Contains("wraith") || name.Contains("stalker"))
            {
                types.Add(EnemyTargetingType.Assassin);
            }
            else if (name.Contains("bomber") || name.Contains("explosive") || name.Contains("plague"))
            {
                types.Add(EnemyTargetingType.Bomber);
            }
            else if (name.Contains("sniper") || name.Contains("marksman") || name.Contains("hunter"))
            {
                types.Add(EnemyTargetingType.Sniper);
            }
            else
            {
                types.Add(EnemyTargetingType.Bruiser);
            }
        }
        
        return types.Distinct().ToList();
    }
    
    private string GetHintForTargetingType(EnemyTargetingType targetingType)
    {
        switch (targetingType)
        {
            case EnemyTargetingType.Bruiser:
                return bruiserHint;
            case EnemyTargetingType.Archer:
                return archerHint;
            case EnemyTargetingType.Assassin:
                return assassinHint;
            case EnemyTargetingType.Sniper:
                return sniperHint;
            case EnemyTargetingType.Bomber:
                return bomberHint;
            default:
                return "Unknown enemy type.";
        }
    }
    
    private string GetEnemyTypeName(EnemyTargetingType targetingType)
    {
        switch (targetingType)
        {
            case EnemyTargetingType.Bruiser:
                return "Bruiser (targets front row)";
            case EnemyTargetingType.Archer:
                return "Archer (targets back row)";
            case EnemyTargetingType.Assassin:
                return "Assassin (targets lowest HP)";
            case EnemyTargetingType.Sniper:
                return "Sniper (targets highest ATK)";
            case EnemyTargetingType.Bomber:
                return "Bomber (targets center)";
            default:
                return "Unknown";
        }
    }
    
    public void RefreshPreview()
    {
        UpdateWavePreview();
    }
    
    private List<(string displayName, int count, float spawnDelay)> GetEnemyInfoForWave(int wave)
    {
        var enemies = new List<(string, int, float)>();
        
        switch (wave)
        {
            // ACT 1: FOUNDATION (Waves 1-7) - Learning basic mechanics
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
            case 5: // First minion unlock wave
                enemies.Add(("Bone Warrior", 2, 0f));
                enemies.Add(("Rotting Zombie", 2, 1f));
                break;
            case 6:
                enemies.Add(("Skeletal Archer", 3, 0f));
                enemies.Add(("Undead Brute", 1, 2f));
                break;
            case 7: // Act 1 finale
                enemies.Add(("Death Guard", 2, 0f));
                enemies.Add(("Bone Warrior", 3, 1f));
                break;
                
            // ACT 2: MASTERY (Waves 8-14) - Complex formations and tactics
            case 8:
                enemies.Add(("Undead Soldier", 4, 0f));
                enemies.Add(("Skeletal Mage", 1, 1.5f));
                break;
            case 9: // Second minion unlock wave
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
            case 13: // Third minion unlock wave
                enemies.Add(("Lich Acolyte", 1, 0f));
                enemies.Add(("Undead Horde", 4, 1f));
                break;
            case 14: // Act 2 finale
                enemies.Add(("Bone Dragon", 1, 0f));
                enemies.Add(("Death Knight", 2, 2f));
                enemies.Add(("Skeletal Archer", 2, 3f));
                break;
                
            // ACT 3: ENDGAME (Waves 15-20) - Maximum difficulty
            case 15:
                enemies.Add(("Undead Titan", 2, 0f));
                enemies.Add(("Shadow Legion", 3, 1f));
                break;
            case 16:
                enemies.Add(("Lich Commander", 1, 0f));
                enemies.Add(("Death Knight", 3, 1f));
                enemies.Add(("Bone Colossus", 1, 2f));
                break;
            case 17: // Fourth minion unlock wave
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
            case 20: // Final boss wave
                enemies.Add(("Necro Overlord", 1, 0f));
                enemies.Add(("Ancient Death Knight", 2, 2f));
                enemies.Add(("Undead Apocalypse", 2, 4f));
                break;
                
            default: // Waves beyond 20 (endless mode)
                int endlessMultiplier = Mathf.Min(wave - 20, 3);
                enemies.Add(("Endless Horror", 1 + endlessMultiplier, 0f));
                enemies.Add(("Void Legion", Mathf.Min(2 + endlessMultiplier, 4), 1f));
                break;
        }
        
        return enemies;
    }
} 