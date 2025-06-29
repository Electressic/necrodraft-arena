# Core Mechanics

## 🎮 Fundamental Gameplay Loop

NecroDraft Arena combines **deck-building strategy** with **autobattler tactics**. Players make strategic decisions during preparation phases, then watch their choices play out in automated combat.

### The Core Cycle
```
1. DRAFT PHASE    → Strategic card selection (1-2 minutes)
2. ASSEMBLY PHASE → Tactical optimization (2-3 minutes)  
3. COMBAT PHASE   → Automated execution (30-90 seconds)
4. OUTCOME PHASE  → Victory/defeat feedback (15 seconds)
5. PROGRESSION    → Stronger enemies, better rewards
```

## 🧩 Part-Based Minion System

### Equipment Slots
Every minion has **4 equipment slots** that define their capabilities:

- **Head**: Often provides mental/precision abilities (CriticalStrike, etc.)
- **Torso**: Often provides defensive/sustain abilities (Armored, Regeneration)
- **Arms**: Often provides offensive abilities (Vampiric, Thorns)
- **Legs**: Often provides mobility abilities (Swift, etc.)

### Stat Calculation
```
Final HP = Base HP + Head HP + Torso HP + Arms HP + Legs HP
Final ATK = Base ATK + Head ATK + Torso ATK + Arms ATK + Legs ATK
Final Speed = Base Speed × Speed Multipliers (from abilities)
```

### Modular Flexibility
- **Mix & Match**: Any part can go in any slot (no type restrictions)
- **Theme Freedom**: Can combine Skeleton + Zombie + Metal parts
- **Strategic Choices**: Sacrifice one stat type for another based on strategy

## ⚔️ Set Bonus Strategy

### Activation Requirements
**Key Mechanic**: Abilities only activate with **2 or more parts** of the same type.

```
1 Vampiric Part   → Only base stats (HP/ATK bonus)
2 Vampiric Parts  → Ability activates at Tier 1
3 Vampiric Parts  → Ability activates at Tier 2  
4 Vampiric Parts  → Ability activates at Tier 3 (maximum power)
```

### Strategic Implications
- **Commitment vs. Flexibility**: Dedicate slots for power, or spread for versatility
- **Build Archetypes**: Tank (4 Armored), Vampire (4 Vampiric), Hybrid (2+2)
- **Counter-Play**: Every focused build has specific weaknesses

### Power Scaling
```
Single Parts: 1.0x power (just stats)
2-Part Set:   1.5x power (ability activation)
3-Part Set:   2.0x power (enhanced effect)
4-Part Set:   3.0x power (maximum effect, often with trade-offs)
```

## 🎲 Draft Strategy

### Selection Pressure
Players must choose **1 of 3** random parts each turn, creating meaningful decisions:

- **Rarity Tension**: Take Epic part now, or wait for better synergy?
- **Set Building**: Commit to a strategy or stay flexible?
- **Opportunity Cost**: Each choice eliminates other possibilities

### Information Management
- **Visible Inventory**: See what you currently have
- **Ability Descriptions**: Understand what each part does
- **Set Progress**: Track how close you are to bonuses
- **Future Planning**: Consider how current choice affects later picks

### Risk Assessment
```
Safe Strategy: Pick highest rarity regardless of synergy
Greedy Strategy: Commit early to specific set bonuses
Flexible Strategy: Adapt based on what's offered
Counter Strategy: Draft to counter expected enemy types
```

## ⚔️ Combat System

### Autobattler Mechanics
Combat is **fully automated** - players influence through preparation, not execution:

- **Unit AI**: Simple targeting and movement rules
- **Positioning**: Units spawn in formation, move to engage
- **Turn Order**: Based on speed, modified by abilities
- **Victory Condition**: Last army standing wins

### Ability Triggers
Different abilities activate at different times:

```
Passive Abilities:
- Armored: Reduces incoming damage constantly
- Swift: Modifies movement speed permanently

Combat Abilities:
- Vampiric: Triggers on dealing damage
- CriticalStrike: Chance-based on attacks
- Thorns: Triggers when taking damage

Time-Based Abilities:
- Regeneration: Heals over time
- Poison: Damage over time after infliction
- Berserker: Activates when health drops below threshold
```

### Visual Feedback
- **Attack Effects**: Yellow flash on attacker
- **Damage Effects**: White flash on victim
- **Critical Hits**: Enhanced yellow flash + larger damage numbers
- **Healing**: Green numbers floating upward
- **Death**: Screen shake + particle explosion

## 🎯 Player Agency

### Strategic Decision Points

#### Draft Phase Decisions
1. **Rarity vs. Synergy**: Epic part or Common that fits your build?
2. **Commitment Level**: All-in on one set or hedge bets?
3. **Counter-Drafting**: Anticipate enemy strategies
4. **Flexibility Preservation**: Keep options open vs. focus early

#### Assembly Phase Decisions
1. **Slot Optimization**: Which parts on which minions?
2. **Minion Specialization**: One super unit or balanced team?
3. **Ability Distribution**: Concentrate effects or spread them?
4. **Stat Balancing**: Prioritize HP, ATK, or Speed?

#### Meta-Game Decisions
1. **Build Learning**: Understand which strategies work when
2. **Adaptation**: Adjust tactics based on enemy patterns
3. **Risk Management**: When to play safe vs. go for power

### Skill Expression

#### Beginner Level
- **Part Recognition**: Learn what each ability does
- **Basic Synergy**: Understand set bonuses
- **Simple Builds**: Focus on one strategy at a time

#### Intermediate Level
- **Build Comparison**: Evaluate different strategic paths
- **Adaptation**: Pivot strategies based on draft offerings
- **Optimization**: Fine-tune stat distribution and slot usage

#### Advanced Level
- **Meta Reading**: Anticipate and counter common strategies
- **Risk Calculation**: Mathematical evaluation of trade-offs
- **Edge Cases**: Exploit unusual combinations and interactions

## 🔄 Feedback Loops

### Positive Feedback
- **Set Completion**: More parts = exponentially more power
- **Synergy Discovery**: Finding effective combinations feels rewarding
- **Victory Momentum**: Winning provides better rewards for next round

### Negative Feedback
- **Over-Commitment Risk**: All-in strategies can backfire completely
- **Adaptation Pressure**: Pure strategies lose to good counters
- **Resource Constraints**: Limited slots force difficult choices

### Learning Loops
```
Try Strategy → See Results → Understand Outcome → Adjust Approach
│                                                              │
└──────────────── Knowledge Accumulation ←───────────────────┘
```

## ⚖️ Balance Philosophy

### Core Principles

#### No Dominant Strategy
- Every build type should have counter-play
- High-risk strategies should have high-reward potential
- Safe strategies should be viable but not optimal

#### Meaningful Choices
- Each draft pick should matter
- Multiple viable paths to victory
- Clear trade-offs between options

#### Emergent Complexity
- Simple rules create complex interactions
- New discoveries possible even after many games
- Depth emerges from part combinations

### Balance Levers

#### Power Level Adjustments
```
Part Stats: Adjust HP/ATK bonuses
Ability Effects: Modify healing amounts, damage reduction, etc.
Set Thresholds: Change how many parts needed for activation
Rarity Distribution: Control how often powerful parts appear
```

#### Strategic Balance
```
Counter-Play: Ensure every strategy has weaknesses
Variety: Multiple build paths should be viable
Adaptation: Flexible strategies should compete with focused ones
Skill Ceiling: Advanced play should offer meaningful advantages
```

## 🎲 Randomness Management

### Controlled Randomness
- **Draft Pools**: Curated to ensure viable options
- **Rarity Distribution**: Weighted to provide progression
- **Combat RNG**: Limited to specific abilities (crits, etc.)

### Player Agency Preservation
- **Always Options**: Never completely random outcomes
- **Information Available**: Players can see and evaluate choices
- **Skill Matters**: Better players should win more consistently

### Variance Sweet Spot
```
Too Little RNG: Game becomes solved, predictable
Sweet Spot: Enough variety for replayability, skill still dominates
Too Much RNG: Player choices don't matter, luck decides games
```

---

*These core mechanics create the strategic depth that makes NecroDraft Arena engaging while maintaining the clarity that makes it accessible.* 