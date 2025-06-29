# Card Selection Scene

## 🎯 Scene Purpose

The **Card Selection** scene is where players draft parts for their minions. This is the primary "deck building" phase where strategic decisions are made about which abilities to pursue and which parts to prioritize.

## 🎮 Core Mechanics

### Draft Process


1. **Present Options**: Show 3 random parts with different rarities/abilities
2. **Player Choice**: Player selects 1 of the 3 parts
3. **Inventory Update**: Selected part goes to player inventory
4. **Repeat**: Continue for predetermined number of picks (usually 8-12)
5. **Transition**: Move to Minion Assembly scene

### Selection Strategy

* **Rarity Evaluation**: Higher rarity parts offer better stats/abilities
* **Set Planning**: Look for parts that work together for bonuses
* **Flexibility**: Balance commitment to specific abilities vs. adaptability

## 🎨 UI Layout

### Screen Structure

```
╔═══════════════════════════════════════════════════════════╗
║  Wave Progress + Gold Display                  [Settings] ║
║                                                           ║
║  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        ║
║  │   CARD 1    │  │   CARD 2    │  │   CARD 3    │        ║
║  │             │  │             │  │             │        ║
║  │  [Sprite]   │  │  [Sprite]   │  │  [Sprite]   │        ║
║  │   Name      │  │   Name      │  │   Name      │        ║
║  │  +X HP      │  │  +X HP      │  │  +X HP      │        ║
║  │  +X ATK     │  │  +X ATK     │  │  +X ATK     │        ║
║  │  Ability    │  │  Ability    │  │  Ability    │        ║
║  │             │  │             │  │             │        ║
║  │  [SELECT]   │  │  [SELECT]   │  │  [SELECT]   │        ║
║  └─────────────┘  └─────────────┘  └─────────────┘        ║
║                                                           ║
║  Current Inventory: [Part1] [Part2] [Part3]...            ║
║                                                           ║
║  [Skip Draft] ────────────────────── [Continue to Build]  ║
╚═══════════════════════════════════════════════════════════╝
```

### Visual Hierarchy


1. **Cards** (Primary focus) - Large, centered, equal spacing
2. **Progress** (Secondary) - Top of screen, shows advancement
3. **Inventory** (Context) - Bottom, shows what you already have
4. **Navigation** (Utility) - Bottom corners, always accessible

## 🎨 Card Visual Design

### Rarity Indication

* **Background Color**: Card background matches rarity
  * White/Gray for Common
  * Blue for Rare
  * Purple for Epic
* **Border Effects**: Subtle glow or border thickness
* **Text Color**: Rarity-appropriate text colors

### Information Display

```
┌─────────────────┐
│   Part Sprite   │ ← Visual representation
├─────────────────┤
│ "Vampiric Head" │ ← Part name, rarity colored
│   Zombie Head   │ ← Type and theme
├─────────────────┤
│     +8 HP       │ ← Stat bonuses, green text
│     +3 ATK      │
├─────────────────┤
│    Vampiric     │ ← Ability name, orange text
│ Heal from kills │ ← Ability description, smaller
├─────────────────┤
│   [SELECT]      │ ← Action button
└─────────────────┘
```

### Interactive States

* **Default**: Neutral colors, clear readability
* **Hover**: Slight scale increase, highlight border
* **Selected**: Green highlight, confirmation feedback
* **Disabled**: Grayed out (if already have max parts)

## 🎯 Player Information Needs

### Decision-Making Data


1. **Current Build Direction**: What abilities do I already have?
2. **Set Bonus Progress**: How many of each ability type?
3. **Stat Comparison**: Is this an upgrade over existing parts?
4. **Synergy Potential**: Does this work with my strategy?
5. **Rarity Assessment**: Is this worth taking over other options?

### UI Solutions

* **Inventory Display**: Shows current parts with ability counts
* **Stat Tooltips**: Compare new part stats with equipped parts
* **Set Progress**: Visual indication of how close to set bonuses
* **Ability Descriptions**: Clear explanation of what each ability does

## 🔄 Flow States

### Early Draft (Picks 1-3)

* **Goal**: Establish strategy direction
* **UI Focus**: Part abilities and synergy potential
* **Player Mindset**: "What kind of build am I going for?"

### Mid Draft (Picks 4-7)

* **Goal**: Commit to sets or stay flexible
* **UI Focus**: Set bonus progress, opportunity cost
* **Player Mindset**: "Do I double down or pivot?"

### Late Draft (Picks 8-12)

* **Goal**: Optimize final build
* **UI Focus**: Direct upgrades, filling gaps
* **Player Mindset**: "What perfects my strategy?"

## ⚡ Polish & Feel

### Animation & Feedback

* **Card Reveal**: Cards slide in from top with slight delay
* **Selection**: Smooth transition with satisfying click
* **Inventory Update**: New part slides into inventory bar
* **Progress**: Numbers count up, progress bars fill smoothly

### Audio Cues

* **Card Appearance**: Soft mystical sound
* **Selection**: Satisfying "click" or "thunk"
* **Rarity Reveals**: Different pitch/tone for Common/Rare/Epic
* **Progress**: Subtle confirmation sound for each selection

### Visual Polish

* **Particle Effects**: Subtle sparkles on Epic cards
* **Color Temperature**: Cool blues/purples for dark necromancy theme
* **Typography**: Clean, readable fonts with good contrast
* **Spacing**: Generous whitespace for clarity

## 🚨 Edge Cases & Error States

### No Valid Parts Available

* **Cause**: RNG gives 3 parts player already has at max
* **Solution**: At least one option is always a stat upgrade
* **Fallback**: "Reroll" option if all three are duplicates

### Player Takes Too Long

* **Timer**: Optional 60-second timer for draft picks
* **Warning**: Visual countdown in last 10 seconds
* **Auto-select**: Choose highest rarity if timer expires
* **Disable**: Option to turn off timer for casual play

### Invalid Selections

* **Prevention**: Disable unavailable options rather than error
* **Feedback**: Clear visual indication why option is disabled
* **Recovery**: Always provide at least one valid option

## 🎮 Accessibility Features

### Visual Accessibility

* **Color Blind Support**: Shapes/patterns in addition to colors
* **High Contrast**: Strong contrast between text and backgrounds
* **Font Scaling**: Support for larger text sizes
* **Clear Icons**: Distinctive part type and ability symbols

### Input Accessibility

* **Keyboard Navigation**: Tab through cards, Enter to select
* **Mouse Alternative**: Keyboard shortcuts for all actions
* **Click Areas**: Large, clear targets for mouse interaction
* **Confirmation**: Optional "confirm selection" step

## 📊 Metrics & Analytics

### Player Behavior Tracking

* **Selection Time**: How long players consider each choice
* **Rarity Preference**: Do players always pick highest rarity?
* **Set Commitment**: At what point do players commit to a set?
* **Strategy Distribution**: Which builds are most popular?

### Balance Implications

* **Part Win Rates**: Which parts lead to victories?
* **Rarity Balance**: Are Epic parts too powerful?
* **Draft Position**: Are early vs. late picks balanced?
* **Player Skill**: Do better players show different patterns?


---

*This scene is crucial for player engagement - it's where strategy begins and excitement builds for the assembly and combat phases.*


