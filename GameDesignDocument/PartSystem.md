# Part System

## 🧩 Core Concept

The **Part System** is the foundation of NecroDraft Arena's strategic depth. Instead of fixed units, players craft minions from **4 equipment slots** (Head, Torso, Arms, Legs), with each part providing **base stats** and **special abilities**.

## 🏗️ Part Structure

### Equipment Slots
```
🧠 Head    → Often provides: Critical abilities, mental effects
🦴 Torso   → Often provides: Defensive abilities, health bonuses  
💪 Arms    → Often provides: Offensive abilities, attack bonuses
🦵 Legs    → Often provides: Movement abilities, speed modifiers
```

### Core Properties
Every part has:
- **Part Name**: "Vampiric Zombie Head"
- **Theme**: Skeleton, Zombie, Metal, Cursed
- **Type**: Head, Torso, Arms, Legs
- **Rarity**: Common, Rare, Epic
- **Stats**: HP Bonus, ATK Bonus
- **Special Ability**: None, or one of 8+ abilities
- **Description**: Flavor text and mechanical explanation

## 🎨 Thematic Organization

### Asset Folder Structure
```
ScriptableObjects/Parts/
├── Skeleton/           # Brittle but precise
│   ├── Head/
│   │   ├── Common/     # "Bone Skull", "Cracked Skull"
│   │   ├── Rare/       # "Keen Skull", "Hunter's Skull"  
│   │   └── Epic/       # "Ancient Skull", "Lich Crown"
│   ├── Torso/
│   │   ├── Common/     # "Bone Ribs", "Hollow Torso"
│   │   ├── Rare/       # "Swift Ribcage", "Archer's Frame"
│   │   └── Epic/       # "Crystal Bones", "Deathknight Torso"
│   ├── Arms/
│   │   └── [Similar structure]
│   └── Legs/
│       └── [Similar structure]
├── Zombie/             # Decaying but resilient
│   ├── Head/
│   │   ├── Common/     # "Rotten Head", "Zombie Skull"
│   │   ├── Rare/       # "Toxic Skull", "Vampiric Head"
│   │   └── Epic/       # "Plague Lord Head", "Undying Skull"
│   └── [Similar structure for other parts]
├── Metal/              # Mechanical and armored
│   └── [Similar structure]
└── Cursed/             # Dark magic, high risk/reward
    └── [Similar structure]
```

## 🎭 Theme Characteristics

### Skeleton Parts
- **Identity**: Precise, brittle, bone-based undead
- **Visual Style**: Clean white/yellow bones, sharp edges
- **Stat Profile**: Moderate HP, High ATK
- **Signature Abilities**: CriticalStrike, Swift
- **Thematic Restrictions**:
  - ❌ **Cannot have**: Armored (bones are brittle)
  - ❌ **Cannot have**: Regeneration (no living tissue)
  - ❌ **Cannot have**: Vampiric (no blood/flesh)

### Zombie Parts  
- **Identity**: Decaying, resilient, flesh-based undead
- **Visual Style**: Green/brown rotting flesh, shambling
- **Stat Profile**: High HP, Moderate ATK
- **Signature Abilities**: Vampiric, Poison, Regeneration
- **Thematic Restrictions**:
  - ❌ **Cannot have**: Swift (too decayed for speed)
  - ❌ **Cannot have**: CriticalStrike (lacks precision)

### Metal Parts
- **Identity**: Mechanical, armored, artificial constructs
- **Visual Style**: Gray/silver metallic, industrial
- **Stat Profile**: Very High HP, Low ATK
- **Signature Abilities**: Armored, Thorns
- **Thematic Restrictions**:
  - ❌ **Cannot have**: Poison (not organic)
  - ❌ **Cannot have**: Vampiric (no biological functions)
  - ❌ **Cannot have**: Regeneration (mechanical repair different)

### Cursed Parts
- **Identity**: Dark magic, high risk/reward, forbidden arts
- **Visual Style**: Purple/black with magical effects
- **Stat Profile**: Extreme stats (very high or very low)
- **Signature Abilities**: Berserker, unique cursed effects
- **Thematic Restrictions**:
  - ⚠️ **Special Rules**: May have negative effects alongside positives
  - ⚠️ **Unstable**: Some combinations may cause penalties

## 💎 Rarity System

### Common (White) - 60% Drop Rate
- **Purpose**: Reliable foundation, balanced options
- **Stat Range**: +3-6 HP, +1-3 ATK
- **Abilities**: 50% chance for basic abilities
- **Examples**: 
  - "Bone Arm" (+4 HP, +2 ATK, no ability)
  - "Basic Zombie Torso" (+6 HP, +1 ATK, no ability)

### Rare (Blue) - 30% Drop Rate  
- **Purpose**: Good abilities, solid power upgrades
- **Stat Range**: +5-10 HP, +2-5 ATK
- **Abilities**: Always has one standard ability
- **Examples**: 
  - "Vampiric Zombie Head" (+8 HP, +6 ATK, Vampiric)
  - "Swift Bone Legs" (+3 HP, +4 ATK, Swift)

### Epic (Purple) - 10% Drop Rate
- **Purpose**: Game-changing effects, build-around cards
- **Stat Range**: +8-15 HP, +4-8 ATK (or extreme values)
- **Abilities**: Powerful abilities or multiple abilities
- **Examples**: 
  - "Berserker Skull" (+12 HP, +10 ATK, Berserker)
  - "Plate Armor Torso" (+20 HP, +0 ATK, Armored)

## 🎯 Design Principles

### Thematic Consistency
- **Visual coherence**: Parts from the same theme should look related
- **Mechanical identity**: Each theme has distinct strengths/weaknesses
- **Restriction logic**: Limitations make thematic sense

### Strategic Depth
- **Build diversity**: Multiple viable strategies within each theme
- **Cross-theme synergy**: Some abilities work across themes
- **Trade-offs**: Every powerful effect has a meaningful cost

### Scalable Content
- **Naming conventions**: [Adjective] [Material] [Type]
- **Stat formulas**: Algorithmic generation possible
- **Template system**: Easy to create new parts following established patterns

## 🧮 Stat Calculation

### Base Stats from Minion Type
```csharp
// Example: Basic Skeleton
baseHP = 15
baseATK = 8
baseMoveSpeed = 2.0f
```

### Part Bonuses Applied
```csharp
// Each equipped part adds:
totalHP = baseHP + headHP + torsoHP + armsHP + legsHP
totalATK = baseATK + headATK + torsoATK + armsATK + legsATK

// Special modifiers (like Swift) applied last:
if (hasSwiftAbility && abilityCount >= 2) {
    totalMoveSpeed *= 2.0f
    totalHP *= 0.75f  // 25% penalty
}
```

### Ability Activation
- **Set Bonus Requirement**: Need 2+ parts with same ability
- **Stacking Effects**: Some abilities get stronger with more parts
- **Threshold System**: Different power levels at 2, 3, 4 parts

## 🔮 Future Systems

### Dynamic Generation
```csharp
PartGenerator.CreatePart(
    theme: PartTheme.Skeleton,
    type: PartType.Head,
    rarity: Rarity.Rare
)
// → "Keen Bone Skull" (+7 HP, +5 ATK, CriticalStrike)
```

### Procedural Naming
- **Adjectives by ability**: Vampiric, Swift, Armored, Toxic, etc.
- **Materials by theme**: Bone (Skeleton), Flesh (Zombie), Steel (Metal)
- **Types**: Head/Skull, Torso/Chest, Arms/Limbs, Legs/Feet

### Advanced Restrictions
```csharp
public static class PartThemeValidator 
{
    public static bool IsValidCombination(PartData part, MinionData baseMinion)
    {
        // Check theme compatibility
        // Validate ability restrictions  
        // Ensure balanced combinations
    }
}
```

---

*This system provides the foundation for unlimited content expansion while maintaining thematic coherence and strategic depth.* 