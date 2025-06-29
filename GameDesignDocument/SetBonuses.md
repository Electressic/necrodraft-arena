# Set Bonuses

## 🎯 Core Philosophy

**Set Bonuses** are the heart of NecroDraft Arena's strategic depth. Unlike traditional autobattlers where items provide immediate effects, our abilities **only activate when you have 2 or more parts of the same type equipped**.

This creates meaningful decisions: **Do you commit multiple slots for powerful effects, or spread across different abilities for versatility?**

## ⚖️ Risk vs. Reward Balance

### The Commitment System
- **Single Part**: Provides only base stats (HP/ATK bonuses)
- **2+ Parts**: Ability activates, creating powerful synergies
- **Opportunity Cost**: Each slot dedicated to a set is one less for other strategies

### Strategic Implications
- **All-in builds**: 4 Vampiric parts = maximum healing but no other abilities
- **Hybrid builds**: 2 Vampiric + 2 Armored = moderate effects from both
- **Flexible builds**: Mix of singles = more stats, fewer special effects

## 🔢 Set Bonus Tiers

### Tier 1 - Activation at 2 Parts

#### Vampiric
- **2 Parts**: Heal for 25% of damage dealt
- **3 Parts**: Heal for 35% of damage dealt  
- **4 Parts**: Heal for 50% of damage dealt
- **Strategy**: Sustain fighter, good against chip damage
- **Counter**: Burst damage, healing prevention

#### Armored  
- **2 Parts**: Reduce incoming damage by 1 (minimum 1)
- **3 Parts**: Reduce incoming damage by 2 (minimum 1)
- **4 Parts**: Reduce incoming damage by 3 (minimum 1)
- **Strategy**: Tank, counters low-damage rapid attacks
- **Counter**: High single-hit damage

#### Poison
- **2 Parts**: Attacks deal 2 damage over 3 seconds
- **3 Parts**: Attacks deal 3 damage over 4 seconds
- **4 Parts**: Attacks deal 4 damage over 5 seconds
- **Strategy**: Damage over time, good vs. high HP enemies
- **Counter**: Fast kills, healing effects

#### Thorns
- **2 Parts**: Reflect 1 damage to attackers
- **3 Parts**: Reflect 2 damage to attackers
- **4 Parts**: Reflect 3 damage + 10% of incoming damage
- **Strategy**: Passive retaliation, punishes attackers
- **Counter**: Ranged attacks, high HP pools

#### Critical Strike
- **2 Parts**: 25% chance for double damage
- **3 Parts**: 35% chance for double damage
- **4 Parts**: 25% chance for **triple** damage
- **Strategy**: Burst damage, high variance
- **Counter**: Consistent damage reduction, high HP

### Tier 2 - Activation at 3 Parts

#### Swift
- **3 Parts**: +100% move speed, -25% HP
- **4 Parts**: +150% move speed, -20% HP (reduced penalty)
- **Strategy**: Hit and run, flanking maneuvers
- **Counter**: Area effects, crowd control

#### Berserker
- **3 Parts**: +50% attack speed when below 50% HP
- **4 Parts**: +75% attack speed when below 60% HP
- **Strategy**: Comeback mechanic, high risk/reward
- **Counter**: Burst damage to prevent low HP trigger

#### Regeneration
- **3 Parts**: Heal 1 HP every 3 seconds
- **4 Parts**: Heal 2 HP every 3 seconds
- **Strategy**: Long-term sustain, attrition warfare
- **Counter**: Burst damage, healing prevention

### Tier 3 - Epic Sets (4 Parts Required)

#### Undying (Future)
- **4 Parts**: Revive once at 25% HP when killed
- **Strategy**: Ultimate safety net, insurance policy
- **Counter**: Multi-hit kills, area damage

#### Cursed Power (Future)
- **4 Parts**: All abilities trigger twice, but minion takes 1 damage per turn
- **Strategy**: Maximum power with significant risk
- **Counter**: Patience, let self-damage accumulate

## 🧮 Mathematical Balance

### Power Level Guidelines
```
Single Part Value = Base Stats Only
2-Part Set Value = 1.5x Single Part Value  
3-Part Set Value = 2.0x Single Part Value
4-Part Set Value = 3.0x Single Part Value (with drawbacks)
```

### Example Power Calculation
```
4 Random Parts: 4 × 1.0 = 4.0 power units
2 Vampiric + 2 Armored: 2 × 1.5 + 2 × 1.5 = 6.0 power units
4 Vampiric Parts: 1 × 3.0 = 3.0 power units (specialized)
```

**Note**: Specialized builds trade raw power for focused effectiveness against specific strategies.

## 🎮 Implementation Details

### Code Structure
```csharp
public class Minion 
{
    public bool HasSetBonus(SpecialAbility ability, int requiredCount = 2)
    {
        return GetAbilityCount(ability) >= requiredCount;
    }
    
    public int GetSetBonusTier(SpecialAbility ability)
    {
        int count = GetAbilityCount(ability);
        if (count >= 4) return 4;
        if (count >= 3) return 3; 
        if (count >= 2) return 2;
        return 0; // No bonus
    }
}
```

### Ability Trigger Logic
```csharp
// In MinionController.ProcessSpecialAbilities()
if (minionData.HasSetBonus(SpecialAbility.Vampiric, 2))
{
    int tier = minionData.GetSetBonusTier(SpecialAbility.Vampiric);
    float healPercent = 0.25f + (tier - 2) * 0.1f; // 25%, 35%, 50%
    
    // Apply vampiric healing based on tier
    int healAmount = Mathf.RoundToInt(finalDamage * healPercent);
    TakeDamage(-healAmount); // Negative damage = healing
}
```

## 📊 Strategic Meta Game

### Build Archetypes

#### The Tank
- **4 Armored Parts**: Maximum damage reduction
- **Strengths**: Survives low-damage sustained attacks
- **Weaknesses**: Vulnerable to high single-hit damage
- **Counter-play**: Bring high-damage abilities like CriticalStrike

#### The Vampire
- **4 Vampiric Parts**: Maximum sustain
- **Strengths**: Outlasts enemies in prolonged fights
- **Weaknesses**: Vulnerable to burst damage
- **Counter-play**: Kill quickly before healing accumulates

#### The Glass Cannon
- **4 Critical Strike Parts**: Maximum burst potential
- **Strengths**: Can one-shot enemies with lucky crits
- **Weaknesses**: Inconsistent, vulnerable when crits don't proc
- **Counter-play**: Consistent damage reduction, high HP pools

#### The Hybrid
- **2 Vampiric + 2 Armored**: Balanced offense and defense
- **Strengths**: Good against multiple strategies
- **Weaknesses**: Not exceptional against any specific build
- **Counter-play**: Hard to counter, but outscaled by specialists

### Rock-Paper-Scissors Balance
```
Armored (Rock) beats Critical Strike (Scissors)
Critical Strike (Scissors) beats Vampiric (Paper)  
Vampiric (Paper) beats Armored (Rock)

Swift disrupts all three by changing engagement rules
Poison/Thorns provide alternative win conditions
```

## 🎯 Design Goals Achieved

### Strategic Depth
- **Multiple viable paths**: Tank, DPS, Sustain, Hybrid
- **Clear trade-offs**: Power vs. versatility
- **Counter-play options**: Every strategy has weaknesses

### Player Agency  
- **Meaningful choices**: Each part selection impacts build direction
- **Flexible adaptation**: Can pivot strategies mid-game
- **Risk management**: Decide when to commit to a set

### Skill Expression
- **Draft optimization**: Identify synergies across multiple parts
- **Build reading**: Adapt to opponent strategies
- **Resource management**: Balance immediate power vs. set completion

## 🔮 Future Expansions

### Cross-Theme Sets
```
Undead Mastery: 2 Skeleton + 2 Zombie parts
→ Special bonus for mixing classic undead types

Arcane Fusion: 2 Metal + 2 Cursed parts  
→ Risk/reward for combining opposing forces
```

### Dynamic Sets
```
Battle Evolution: Sets that change based on combat duration
Turn 1-3: Basic effect
Turn 4-6: Enhanced effect  
Turn 7+: Maximum power
```

### Negative Synergies
```
Chaos Penalty: 4 different single abilities
→ -10% to all stats (encourages set building)

Theme Conflict: Skeleton + Zombie parts together
→ Slight penalty to encourage thematic consistency
```

---

*This set bonus system creates the strategic depth that separates NecroDraft Arena from simpler autobattlers, rewarding both planning and adaptation.* 