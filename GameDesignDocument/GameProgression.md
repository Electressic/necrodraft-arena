# Game Progression System

## 🎯 Overview

NecroDraft Arena features a **20-wave progression system** divided into 3 acts, with strategic minion unlocks, escalating part rarities, and carefully balanced stat budgets. The system creates meaningful choice points every 4 waves while maintaining smooth difficulty curves.

## 📈 Minion Unlock Progression

### 5-Minion System Overview

| Minion Unlocked | Unlocked After Wave | Starts at Level | Act | Strategic Purpose |
|:----------------|:-------------------|:----------------|:----|:------------------|
| **Minion #1** | Game Start | 1 | Act 1 | Foundation learning |
| **Minion #2** | Wave 4 | 1 | End of Act 1 | Early power spike |
| **Minion #3** | Wave 8 | 3 | Start of Act 2 | Mid-game complexity |
| **Minion #4** | Wave 12 | 5 | Mid Act 2 | Advanced strategies |
| **Minion #5** | Wave 16 | 6 | Start of Act 3 | Endgame compositions |

### Unlock Strategy
- **Early Game (Waves 1-4)**: Master basics with single minion
- **Mid Game (Waves 5-12)**: Learn multi-minion synergies and positioning
- **Late Game (Waves 13-20)**: Complex army compositions and advanced tactics

---

## 🌊 Wave-by-Wave Progression

### Act 1: Foundation (Waves 1-7)
*Learning phase with gradual complexity increase*

| Wave | XP Reward | Part Rarities | Stat Budget Range | Special Notes |
|:-----|:----------|:--------------|:------------------|:--------------|
| 1 | 30 | 100% Common | 12 | Tutorial combat |
| 2 | 35 | 100% Common | 12 | Basic enemy variety |
| 3 | 40 | 100% Common | 12 | Pattern recognition |
| 4 | 45 | 80% Common, 20% Uncommon | 12-20 | **🔓 Unlocks Minion #2** |
| 5 | 60 | 70% Common, 30% Uncommon | 12-20 | **⚡ Spike Wave: Hulking Zombie** |
| 6 | 70 | 60% Common, 40% Uncommon | 12-20 | Uncommon meta introduction |
| 7 | 80 | 50% Common, 50% Uncommon | 12-20 | Strategic depth increases |

### Act 2: Mastery (Waves 8-14)  
*Advanced mechanics and rare part introduction*

| Wave | XP Reward | Part Rarities | Stat Budget Range | Special Notes |
|:-----|:----------|:--------------|:------------------|:--------------|
| 8 | 100 | 30% Common, 60% Uncommon, 10% Rare | 12-32 | **🔓 Unlocks Minion #3** |
| 9 | 110 | 20% Common, 60% Uncommon, 20% Rare | 12-32 | Rare part tactics |
| 10 | 150 | 10% Common, 60% Uncommon, 30% Rare | 20-32 | **👑 Boss: The Abomination** |
| 11 | 160 | 10% Common, 50% Uncommon, 40% Rare | 20-32 | Post-boss power spike |
| 12 | 175 | 100% Uncommon | 20 | **🔓 Unlocks Minion #4** |
| 13 | 190 | 40% Uncommon, 60% Rare | 20-32 | Set bonus mastery |
| 14 | 220 | 30% Uncommon, 70% Rare | 20-32 | **⚡ End of Act 2: 2x Abominations** |

### Act 3: Endgame (Waves 15-20)
*Epic parts and maximum complexity*

| Wave | XP Reward | Part Rarities | Stat Budget Range | Special Notes |
|:-----|:----------|:--------------|:------------------|:--------------|
| 15 | 250 | 10% Uncommon, 60% Rare, 30% Epic | 20-50 | Epic meta begins |
| 16 | 260 | 10% Uncommon, 50% Rare, 40% Epic | 20-50 | **🔓 Unlocks Minion #5** |
| 17 | 280 | 40% Rare, 60% Epic | 32-50 | High-power builds |
| 18 | 300 | 30% Rare, 70% Epic | 32-50 | Pre-boss optimization |
| 19 | 125 | 100% Epic | 50 | **💰 Breather: Treasure Ghoul** |
| 20 | 0 | - | - | **👑 Final Boss: The Necro-Lord** |

---

## 🎲 Part Rarity & Stat Budget System

### Stat Budget Distribution

| Rarity | Stat Budget | Stats Count | Primary Pool % | Secondary Pool % | Drop Weight |
|:--------|:------------|:------------|:---------------|:-----------------|:------------|
| **Common** | 12 points | 1-2 stats | 90% | 10% | High early, 0% late |
| **Uncommon** | 20 points | 2 stats | 75% | 25% | Consistent mid-game |
| **Rare** | 32 points | 2-3 stats | 60% | 40% | High mid to late |
| **Epic** | 50 points | 3-4 stats | 50% | 50% | Late game focus |

### Stat Pool Definitions

#### Primary Stat Pools (by Part Type)
```
Head Parts:   Crit Chance, Crit Damage, Range
Torso Parts:  Health, Defense, Dodge Chance  
Arms Parts:   Attack, Attack Speed, Crit Chance
Legs Parts:   Move Speed, Attack Speed, Dodge Chance
```

#### Secondary Stat Pools (by Part Type)
```
Head Parts:   Attack Speed, Move Speed, Health
Torso Parts:  Attack, Move Speed, Attack Speed
Arms Parts:   Health, Defense, Range, Move Speed  
Legs Parts:   Health, Defense, Range, Crit Chance
```

### Budget Allocation Rules
- **Always use full budget**: Parts must spend all available stat points
- **Primary pool priority**: Higher rarities get more secondary pool access
- **Minimum guarantees**: Each rarity has minimum stat count requirements
- **Generic part bonus**: +25% stat budget to compensate for no set bonuses

---

## 📋 Part Blueprint System

### Class-Specific Parts (60% drop rate for player's class)

#### 🦴 Skeleton Set (Speed & Precision Theme)
| Part Name | Slot | Primary Stats | Secondary Stats | Set Bonus |
|:----------|:-----|:--------------|:----------------|:----------|
| **Skeletal Skull** | Head | Crit Chance, Crit Damage, Range | Attack Speed, Move Speed | Rattling Presence |
| **Skeletal Ribcage** | Torso | Health, Defense, Dodge | Move Speed, Attack Speed | Bone Armor |
| **Skeletal Arm Bones** | Arms | Attack, Attack Speed, Crit Chance | Health, Range | Brittle Bones |
| **Skeletal Leg Bones** | Legs | Move Speed, Attack Speed, Dodge | Health, Crit Chance | Bone Rush |

#### 🧟 Zombie Set (Tank & Sustain Theme)
| Part Name | Slot | Primary Stats | Secondary Stats | Set Bonus |
|:----------|:-----|:--------------|:----------------|:----------|
| **Zombie Head** | Head | Defense, Health, Range | Attack Speed, Move Speed | Rotten Stench |
| **Zombie Torso** | Torso | Health, Defense | Attack, Move Speed | Undying Hunger |
| **Zombie Arms** | Arms | Attack, Health | Defense, Range | Heavy Blows |
| **Zombie Legs** | Legs | Health, Move Speed, Defense | Attack Speed, Range | Dead Weight |

#### 👻 Ghost Set (Balanced & Magical Theme)
| Part Name | Slot | Primary Stats | Secondary Stats | Set Bonus |
|:----------|:-----|:--------------|:----------------|:----------|
| **Ghostly Head** | Head | Range, Dodge Chance | Attack Speed, Crit Chance | Incorporeal |
| **Ghostly Torso** | Torso | Dodge Chance, Health | Move Speed, Attack Speed | Phase Shift |
| **Ghostly Arms** | Arms | Attack Speed, Range, Attack | Health, Crit Damage | Ethereal Touch |
| **Ghostly Legs** | Legs | Move Speed, Dodge, Attack Speed | Health, Range | Spectral Step |

### Generic Parts (10% drop rate, +25% stat budget)

| Part Name | Slot | Focus | Stat Budget | Set Bonus |
|:----------|:-----|:------|:------------|:----------|
| **Ancient Skull** | Head | HP/Defense focus | +25% budget | None |
| **Preserved Tissue** | Torso | HP/Defense focus | +25% budget | None |
| **Grafted Bone** | Arms | Attack focus | +25% budget | None |
| **Swift Bones** | Legs | Speed focus | +25% budget | None |

---

## 🏆 Set Bonus Activation Rules

### Cross-Set Synergy System
- **Single Set Focus**: 4 parts of same set = Tier 3 bonus
- **Dual Set Strategy**: 2+2 parts = Two Tier 1 bonuses
- **Flexible Build**: 1+1+1+1 parts = No set bonuses, maximum stat variety

### Set Bonus Tiers
```
1 Part:  No set bonus (stats only)
2 Parts: Tier 1 bonus activation
3 Parts: Tier 2 bonus activation  
4 Parts: Tier 3 bonus + completion reward
```

### Example Set Bonus Scaling

#### Bone Armor (Skeleton Set)
- **Tier 1 (2 parts)**: Gain shield for 10% of max HP
- **Tier 2 (3 parts)**: Shield increases to 15% of max HP
- **Tier 3 (4 parts)**: Shield shatters on break, dealing AoE damage

#### Undying Hunger (Zombie Set)  
- **Tier 1 (2 parts)**: Heal 5% max HP over 3 seconds after kills
- **Tier 2 (3 parts)**: Healing increased to 8% max HP
- **Tier 3 (4 parts)**: If health drops below 20%, trigger healing immediately

---

## 📊 Minion Stats & Leveling

### XP Requirements & Base Stats
*Note: All minions use the same XP curve, but start at different levels when unlocked*

| Level | XP to Next | Base HP | Base ATK | Base DEF | Base ATK Speed |
|:------|:-----------|:--------|:---------|:---------|:---------------|
| 1 | 25 | 300 | 25 | 10 | 1.20 /sec |
| 2 | 50 | 372 | 30 | 12 | 1.24 /sec |
| 3 | 90 | 516 | 39 | 16 | 1.30 /sec |
| 4 | 120 | 732 | 52 | 22 | 1.37 /sec |
| 5 | 150 | 1020 | 68 | 29 | 1.45 /sec |
| 6 | 180 | 1380 | 88 | 37 | 1.54 /sec |
| 7 | 200 | 1812 | 111 | 48 | 1.63 /sec |
| 8 | 220 | 2316 | 138 | 59 | 1.73 /sec |
| 9 | 240 | 2892 | 168 | 72 | 1.84 /sec |
| 10 | MAX | 3540 | 202 | 87 | 1.95 /sec |

### Class-Specific Modifiers

#### Skeleton Class
- **HP Modifier**: 0.85x (fragile)
- **ATK Modifier**: 1.0x (balanced)
- **SPD Modifier**: 1.5x (very fast)
- **Philosophy**: Glass cannon, hit-and-run

#### Zombie Class  
- **HP Modifier**: 1.67x (very tanky)
- **ATK Modifier**: 1.4x (strong)
- **SPD Modifier**: 0.67x (slow)
- **Philosophy**: Immovable tank, sustained damage

#### Ghost Class
- **HP Modifier**: 1.13x (moderate)
- **ATK Modifier**: 1.2x (good)
- **SPD Modifier**: 0.83x (moderate)
- **Philosophy**: Balanced, magical abilities

---

## 🎮 Loot Distribution System

### Class-Based Part Distribution
*When player selects a class, this affects part drop rates*

| Player's Class | Own Class Parts | Other Class 1 | Other Class 2 | Generic Parts |
|:---------------|:----------------|:--------------|:--------------|:--------------|
| **Skeleton** | 60% chance | 15% (Zombie) | 15% (Ghost) | 10% |
| **Zombie** | 60% chance | 15% (Skeleton) | 15% (Ghost) | 10% |
| **Ghost** | 60% chance | 15% (Skeleton) | 15% (Zombie) | 10% |

### Strategic Implications
- **Specialization Reward**: High chance of getting synergistic parts
- **Flexibility Options**: Can still build hybrid strategies with other classes
- **Generic Compensation**: Generic parts have higher stat budgets

---

## 🎯 Design Goals & Balance

### Power Progression Curve
- **Waves 1-4**: Learn fundamentals, single minion mastery
- **Waves 5-8**: Multi-minion tactics, uncommon parts
- **Waves 9-12**: Rare part strategies, complex synergies  
- **Waves 13-16**: Epic builds, army optimization
- **Waves 17-20**: Perfect builds, endgame challenges

### Strategic Diversity
- **Early Game**: Focus on basic synergies and stat accumulation
- **Mid Game**: Choose between set bonus commitment vs. flexibility
- **Late Game**: Perfect compositions with epic parts and set mastery

### Skill Expression
- **Draft Skill**: Recognize synergies and plan ahead
- **Build Skill**: Optimize part placement and minion roles
- **Meta Skill**: Adapt strategies based on enemy patterns and available parts

---

## 🔄 Future Expansion Points

### Additional Content (Post-MVP)
- **Legendary Rarity**: 75-point budget, unique mechanics
- **Set Bonus Expansions**: More complex multi-part interactions
- **Special Enemies**: Bosses that require specific counter-strategies
- **Prestige System**: Continue progression beyond Wave 20

### Balance Iteration
- **Drop Rate Tuning**: Adjust rarity distribution based on player data
- **Stat Budget Refinement**: Ensure all rarities feel meaningful
- **Set Bonus Balancing**: Rock-paper-scissors meta maintenance 