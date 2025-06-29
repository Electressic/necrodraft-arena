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

### **Rarity Tiers**

#### **Common (White)**
- **Stats Generated**: 1-2 stats per part
- **Value Ranges**: Low (HP: 3-8, ATK: 2-5, percentages: 5-15%)
- **Frequency**: ~60% of drops
- **Purpose**: Reliable power progression, building blocks

#### **Rare (Blue)**
- **Stats Generated**: 2-3 stats per part  
- **Value Ranges**: Medium (HP: 6-12, ATK: 4-8, percentages: 10-25%)
- **Frequency**: ~30% of drops
- **Purpose**: Meaningful upgrades, synergy pieces

#### **Epic (Purple)**
- **Stats Generated**: 3-4 stats per part
- **Value Ranges**: High (HP: 10-18, ATK: 7-12, percentages: 20-40%)
- **Frequency**: ~10% of drops
- **Purpose**: Build-defining pieces, high-risk choices

## 📊 Dynamic Stat System

### **Core Stats** (Flat Bonuses)
```
Health: Flat HP increase (3-18 based on rarity)
Attack: Flat damage increase (2-12 based on rarity)  
Defense: Flat damage reduction (1-6 based on rarity)
```

### **Combat Stats** (Percentage Bonuses)
```
Attack Speed: Multiplier to attack rate (5-40% based on rarity)
Crit Chance: Chance for critical hits (5-40% based on rarity)
Crit Damage: Multiplier for critical damage (10-50% based on rarity)
```

### **Movement Stats** (Percentage Bonuses)
```
Move Speed: Multiplier to movement rate (5-50% based on rarity)
Range: Multiplier to attack range (5-40% based on rarity)
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

### **Generation Algorithm**
```
1. Select Theme → Determines stat affinity chances
2. Select Rarity → Determines value ranges and stat count
3. Roll Each Stat → Based on theme chances and rarity ranges
4. Ensure Minimums → Guarantee minimum stats for rarity tier
5. Assign Set Bonus → Independent of generated stats
```

### **Theme-Based Generation Examples**

#### **Skeleton Epic Head**
```
Generated Stats (high chance):
- +15 Attack (good attack affinity)
- +25% Crit Chance (very high crit affinity) 
- +30% Move Speed (very high speed affinity)
- +20% Range (high range affinity)

Set Bonus: Critical Strike
Result: Perfect glass cannon piece
```

#### **Zombie Epic Torso**
```
Generated Stats (high chance):
- +16 Health (very high health affinity)
- +5 Defense (high defense affinity)
- +8 Attack (medium attack affinity)

Set Bonus: Armored  
Result: Perfect tank piece
```

#### **Ghost Rare Arms**
```
Generated Stats (balanced):
- +6 Attack (medium affinity)
- +15% Attack Speed (good affinity)
- +20% Crit Damage (high affinity)

Set Bonus: Vampiric
Result: Versatile sustain piece
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