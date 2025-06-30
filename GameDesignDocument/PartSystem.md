# Dynamic Part System

## 🎯 Core Philosophy

The **Dynamic Part System** is the heart of NecroDraft Arena's strategic depth. Unlike traditional games with fixed items, every part is **procedurally generated** with random stats based on **thematic identity** and **rarity tier**. This creates endless variety while maintaining thematic consistency and strategic meaning.

## 🧩 Part Structure

### **Equipment Slots**
Every minion has **4 equipment slots** that can accept any part type:
- **Head**: Often mental/precision focused (crits, range)
- **Torso**: Often defensive/sustain focused (health, defense) 
- **Arms**: Often offensive focused (attack, attack speed)
- **Legs**: Often mobility focused (move speed, range)

*Note: Any part can go in any slot - these are tendencies, not restrictions*

### **Part Themes**
Parts belong to one of **3 undead themes**, each with distinct stat affinities:

#### **🦴 Skeleton Theme**
*"Fast, fragile, precise"*
- **High Affinity**: Move Speed (90%), Crit Chance (80%), Range (70%)
- **Medium Affinity**: Attack (60%), Crit Damage (60%), Attack Speed (50%)
- **Low Affinity**: Health (20%), Defense (10%)
- **Playstyle**: Glass cannon builds, hit-and-run tactics, precision strikes

#### **🧟 Zombie Theme** 
*"Tanky, slow, sustaining"*
- **High Affinity**: Health (90%), Defense (80%)
- **Medium Affinity**: Attack (50%), Crit Damage (40%)
- **Low Affinity**: Move Speed (20%), Crit Chance (20%), Attack Speed (20%), Range (30%)
- **Playstyle**: Tank builds, sustained combat, outlasting enemies

#### **👻 Ghost Theme**
*"Ethereal, magical, balanced"*
- **High Affinity**: Crit Damage (70%), Attack Speed (60%), Move Speed (60%)
- **Medium Affinity**: All other stats (40-50%)
- **Low Affinity**: None (balanced theme)
- **Playstyle**: Versatile builds, magical damage focus, hybrid strategies

### **Rarity Tiers & Stat Budget System**

#### **Common (White) - 12 Stat Budget**
- **Stats Generated**: 1-2 stats per part
- **Stat Distribution**: 90% primary pool, 10% secondary pool
- **Frequency**: High early game, 0% late game
- **Purpose**: Foundation pieces, learning basic synergies

#### **Uncommon (Green) - 20 Stat Budget**
- **Stats Generated**: 2 stats per part (always full budget)
- **Stat Distribution**: 75% primary pool, 25% secondary pool
- **Frequency**: Consistent mid-game backbone
- **Purpose**: Reliable upgrades, early set bonus enablers

#### **Rare (Blue) - 32 Stat Budget**
- **Stats Generated**: 2-3 stats per part (always full budget)
- **Stat Distribution**: 60% primary pool, 40% secondary pool
- **Frequency**: Mid to late game focus
- **Purpose**: Build-defining pieces, strong synergies

#### **Epic (Purple) - 50 Stat Budget**
- **Stats Generated**: 3-4 stats per part (always full budget)
- **Stat Distribution**: 50% primary pool, 50% secondary pool
- **Frequency**: Late game specialization
- **Purpose**: Endgame optimization, complex builds

#### **Generic Parts - +25% Stat Budget**
- **Compensation**: Higher stat budgets to offset lack of set bonuses
- **Budget Examples**: Common = 15, Uncommon = 25, Rare = 40, Epic = 62.5
- **Purpose**: Alternative builds, stat-focused strategies

## 📊 Dynamic Stat System

### **Stat Budget Allocation**
The new system uses **stat budgets** instead of fixed ranges, ensuring consistent power scaling:

```
Common (12 points):   Focused builds, 1-2 stats
Uncommon (20 points): Balanced builds, 2 stats  
Rare (32 points):     Strong builds, 2-3 stats
Epic (50 points):     Complex builds, 3-4 stats
```

### **Primary vs Secondary Stat Pools**

#### **Primary Stat Pools** (by part slot)
```
Head Parts:   Crit Chance, Crit Damage, Range
Torso Parts:  Health, Defense, Dodge Chance
Arms Parts:   Attack, Attack Speed, Crit Chance  
Legs Parts:   Move Speed, Attack Speed, Dodge Chance
```

#### **Secondary Stat Pools** (by part slot)
```
Head Parts:   Attack Speed, Move Speed, Health
Torso Parts:  Attack, Move Speed, Attack Speed
Arms Parts:   Health, Defense, Range, Move Speed
Legs Parts:   Health, Defense, Range, Crit Chance
```

### **Stat Point Values**
```
Health: 1 point per +1 HP
Attack: 1 point per +1 Attack
Defense: 2 points per +1 Defense (powerful)
Attack Speed: 50 points per +100% (percentage stats more expensive)
Crit Chance: 100 points per +100% 
Crit Damage: 50 points per +100%
Move Speed: 50 points per +100%
Range: 75 points per +100%
Dodge Chance: 100 points per +100% (rare stat)
```

## ⚔️ Set Bonus System

### **Activation Requirements**
Set bonuses require **2 or more parts** with the same special ability:
```
1 Part:  Only stat bonuses apply (no set bonus)
2 Parts: Set bonus activates at Tier I  
3 Parts: Set bonus activates at Tier II (enhanced)
4 Parts: Set bonus activates at Tier III (maximum power)
```

### **Available Set Bonuses**

#### **Berserker** 🔥
- **Tier I (2 parts)**: +50% attack speed when below 50% HP
- **Tier II (3 parts)**: +75% attack speed when below 50% HP  
- **Tier III (4 parts)**: +100% attack speed when below 50% HP
- **Theme Synergy**: Works well with Skeleton (speed) or Zombie (survivability)

#### **Armored** 🛡️
- **Tier I (2 parts)**: Reduce incoming damage by 1 (minimum 1)
- **Tier II (3 parts)**: Reduce incoming damage by 2 (minimum 1)
- **Tier III (4 parts)**: Reduce incoming damage by 3 (minimum 1)
- **Theme Synergy**: Perfect for Zombie theme builds

#### **Swift** 💨
- **Tier I (2 parts)**: +100% move speed, -25% max HP
- **Tier II (3 parts)**: +150% move speed, -25% max HP
- **Tier III (4 parts)**: +200% move speed, -25% max HP
- **Theme Synergy**: Natural fit for Skeleton theme

#### **Poison** ☠️
- **Tier I (2 parts)**: Attacks inflict 2 poison damage over 3 seconds
- **Tier II (3 parts)**: Attacks inflict 3 poison damage over 3 seconds
- **Tier III (4 parts)**: Attacks inflict 4 poison damage over 3 seconds
- **Theme Synergy**: Works with any theme, good with Ghost

#### **Regeneration** 💚
- **Tier I (2 parts)**: Heal 1 HP every 3 seconds
- **Tier II (3 parts)**: Heal 2 HP every 3 seconds
- **Tier III (4 parts)**: Heal 3 HP every 3 seconds
- **Theme Synergy**: Excellent for Zombie tank builds

#### **Critical Strike** ⚡
- **Tier I (2 parts)**: 25% chance for double damage
- **Tier II (3 parts)**: 35% chance for double damage
- **Tier III (4 parts)**: 50% chance for double damage
- **Theme Synergy**: Perfect for Skeleton precision builds

#### **Thorns** 🌵
- **Tier I (2 parts)**: Reflect 1 damage to attackers
- **Tier II (3 parts)**: Reflect 2 damage to attackers
- **Tier III (4 parts)**: Reflect 3 damage to attackers
- **Theme Synergy**: Great for Zombie defensive builds

#### **Vampiric** 🩸
- **Tier I (2 parts)**: Heal for 25% of damage dealt
- **Tier II (3 parts)**: Heal for 40% of damage dealt
- **Tier III (4 parts)**: Heal for 60% of damage dealt
- **Theme Synergy**: Works well with Ghost balanced builds

## 🎲 Procedural Generation

### **Budget-Based Generation Algorithm**
```
1. Select Theme → Determines primary/secondary pool preferences
2. Select Rarity → Sets total stat budget (12/20/32/50 points)
3. Select Part Type → Defines primary and secondary stat pools
4. Allocate Budget → Distribute points between primary (preferred) and secondary pools
5. Assign Set Bonus → Independent of generated stats
6. Generate Names → Based on theme, rarity, and abilities
```

### **Budget Allocation Examples**

#### **Skeleton Epic Head (50 budget)**
```
Primary Pool (25 points): Crit Chance, Crit Damage, Range
Secondary Pool (25 points): Attack Speed, Move Speed, Health

Generated Stats:
- +25% Crit Chance (25 points from primary)
- +20% Attack Speed (10 points from secondary)
- +15 Health (15 points from secondary)

Set Bonus: Rattling Presence
Result: 50/50 budget split, precision focus
```

#### **Zombie Uncommon Torso (20 budget)**
```
Primary Pool (15 points): Health, Defense, Dodge Chance  
Secondary Pool (5 points): Attack, Move Speed, Attack Speed

Generated Stats:
- +12 Health (12 points from primary)
- +1 Defense (2 points from primary - defense costs 2x)
- +3 Attack (3 points from secondary - rounds to use full budget)

Set Bonus: Undying Hunger
Result: Tank-focused with minor offense
```

#### **Ghost Rare Arms (32 budget)**
```
Primary Pool (19 points): Attack, Attack Speed, Crit Chance
Secondary Pool (13 points): Health, Defense, Range, Move Speed

Generated Stats:
- +12 Attack (12 points from primary)
- +14% Attack Speed (7 points from primary)
- +8 Health (8 points from secondary)
- +6% Move Speed (3 points from secondary)
- +3% Range (2 points from secondary)

Set Bonus: Ethereal Touch
Result: Balanced offensive piece with utility
```

### **Generic Part Examples**

#### **Grafted Bone Epic Arms (62.5 budget = 63 rounded)**
```
No primary/secondary restrictions - optimized stat distribution

Generated Stats:
- +25 Attack (25 points)
- +30% Attack Speed (15 points)
- +15% Crit Chance (15 points) 
- +5 Health (5 points)
- +1 Defense (2 points)

Set Bonus: None
Result: Higher total stats but no set synergy
```

## 🎮 Strategic Implications

### **Build Diversity**
- **Pure Theme Builds**: 4 Skeleton parts for maximum speed/precision
- **Hybrid Builds**: 2+2 split for multiple set bonuses
- **Stat-Focused Builds**: Ignore themes, focus on specific stat types
- **Balanced Builds**: Mix themes for well-rounded stats

### **Decision Points**
- **Rarity vs. Synergy**: Take Epic with wrong theme, or Rare with perfect theme?
- **Stats vs. Set Bonus**: High-stat parts vs. set bonus completion?
- **Theme Commitment**: Go all-in on theme, or stay flexible?
- **Slot Optimization**: Which parts go in which slots for maximum effect?

### **Counter-Play**
- **Anti-Speed**: High defense beats glass cannon builds
- **Anti-Tank**: High crit damage bypasses defense
- **Anti-Sustain**: Burst damage overwhelms healing
- **Adaptation**: Flexible builds adapt to enemy strategies

## 📈 Progression & Rewards

### **Early Game (Waves 1-2)**
- **Focus**: Learn set bonus system with simple 2-part combos
- **Parts**: Mostly Common/Rare with clear themes
- **Strategy**: Single set bonus mastery

### **Mid Game (Waves 3-4)**
- **Focus**: Complex builds with multiple set bonuses
- **Parts**: More Epic parts, hybrid opportunities
- **Strategy**: Army composition, role specialization

### **Late Game (Waves 5+)**
- **Focus**: Optimization and counter-play
- **Parts**: High-roll Epic parts, perfect stat combinations
- **Strategy**: Meta-gaming, adaptation, edge cases

## 🔬 Balance Philosophy

### **Theme Balance**
- **Skeleton**: High reward, high risk (glass cannon)
- **Zombie**: Medium reward, low risk (reliable tank)
- **Ghost**: Medium reward, medium risk (flexible)

### **Rarity Balance**
- **Common**: Always useful, never amazing
- **Rare**: Meaningful upgrades, strategic options
- **Epic**: Game-changing, but opportunity cost

### **Set Bonus Balance**
- **All viable**: Every set bonus has optimal situations
- **Clear trade-offs**: Power comes with drawbacks
- **Tier scaling**: More parts = more power, more commitment

---

*This dynamic system creates endless replayability while maintaining clear strategic choices and thematic identity. Every part feels unique, every build has potential, and every playthrough offers new discoveries.* 