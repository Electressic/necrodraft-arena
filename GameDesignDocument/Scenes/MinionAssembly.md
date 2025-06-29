# Minion Assembly Scene

## 🎯 Scene Purpose

The **Minion Assembly** scene is where strategic planning becomes tactical reality. Players equip their drafted parts onto minions, optimize builds for set bonuses, and fine-tune their army before combat.

## 🎮 Core Mechanics

### Assembly Process
1. **View Minions**: See available minion slots (1-3 minions)
2. **Equip Parts**: Drag and drop parts from inventory to minion slots
3. **Optimize Builds**: Arrange parts for maximum synergy
4. **Preview Stats**: See final HP, ATK, and abilities before combat
5. **Ready Check**: Confirm army composition and proceed to battle

### Strategic Decisions
* **Part Distribution**: Spread abilities across minions vs. focus on one
* **Set Bonus Planning**: Ensure 2+ parts of same ability for activation
* **Role Assignment**: Tank, DPS, and Support minion archetypes
* **Stat Optimization**: Balance HP, ATK, and special abilities

## 🎨 UI Layout

### Screen Structure
```
╔═══════════════════════════════════════════════════════════════════════════════╗
║  Wave 2 of 5                   MINION ASSEMBLY                       ⚙️       ║
║                                                                               ║
║  ┌─────────────────────────────────────────────────────────────────────────┐  ║
║  │                          YOUR INVENTORY                              │  ║
║  │                                                                         │  ║
║  │  [Part1]  [Part2]  [Part3]  [Part4]  [Part5]  [Part6]  [Part7]  [8]   │  ║
║  │  +5HP     +8HP     +3HP     +6HP     +4HP     +7HP     +5HP     +9HP   │  ║
║  │  +2ATK    +3ATK    +4ATK    +1ATK    +5ATK    +2ATK    +3ATK    +4ATK  │  ║
║  │  Swift    Vamp     Crit     Armor    Poison   Thorns   Regen    Berserk │  ║
║  │                                                                         │  ║
║  └─────────────────────────────────────────────────────────────────────────┘  ║
║                                                                               ║
║  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐          ║
║  │   MINION  1     │    │   MINION  2     │    │   MINION  3     │          ║
║  │                 │    │                 │    │                 │          ║
║  │  ┌───────────┐  │    │  ┌───────────┐  │    │  ┌───────────┐  │          ║
║  │  │   HEAD    │  │    │  │   HEAD    │  │    │  │   HEAD    │  │          ║
║  │  │  [Slot]   │  │    │  │  [Slot]   │  │    │  │  [Slot]   │  │          ║
║  │  └───────────┘  │    │  └───────────┘  │    │  └───────────┘  │          ║
║  │  ┌───────────┐  │    │  ┌───────────┐  │    │  ┌───────────┐  │          ║
║  │  │   TORSO   │  │    │  │   TORSO   │  │    │  │   TORSO   │  │          ║
║  │  │  [Slot]   │  │    │  │  [Slot]   │  │    │  │  [Slot]   │  │          ║
║  │  └───────────┘  │    │  └───────────┘  │    │  └───────────┘  │          ║
║  │  ┌───────────┐  │    │  ┌───────────┐  │    │  ┌───────────┐  │          ║
║  │  │   ARMS    │  │    │  │   ARMS    │  │    │  │   ARMS    │  │          ║
║  │  │  [Slot]   │  │    │  │  [Slot]   │  │    │  │  [Slot]   │  │          ║
║  │  └───────────┘  │    │  └───────────┘  │    │  └───────────┘  │          ║
║  │  ┌───────────┐  │    │  ┌───────────┐  │    │  ┌───────────┐  │          ║
║  │  │   LEGS    │  │    │  │   LEGS    │  │    │  │   LEGS    │  │          ║
║  │  │  [Slot]   │  │    │  │  [Slot]   │  │    │  │  [Slot]   │  │          ║
║  │  └───────────┘  │    │  └───────────┘  │    │  └───────────┘  │          ║
║  │                 │    │                 │    │                 │          ║
║  │  HP: 15 + 0     │    │  HP: 15 + 0     │    │  HP: 15 + 0     │          ║
║  │  ATK: 8 + 0     │    │  ATK: 8 + 0     │    │  ATK: 8 + 0     │          ║
║  │  Abilities: -   │    │  Abilities: -   │    │  Abilities: -   │          ║
║  │                 │    │                 │    │                 │          ║
║  └─────────────────┘    └─────────────────┘    └─────────────────┘          ║
║                                                                               ║
║  ┌─────────────────────────────────────────────────────────────────────────┐  ║
║  │                          SET BONUS TRACKER                             │  ║
║  │                                                                         │  ║
║  │  Vampiric: 3/2 ✓  Critical: 1/2 ✗  Armored: 2/2 ✓  Swift: 0/2 ✗      │  ║
║  │  Poison: 1/2 ✗    Thorns: 1/2 ✗    Regen: 1/2 ✗    Berserker: 1/2 ✗  │  ║
║  │                                                                         │  ║
║  └─────────────────────────────────────────────────────────────────────────┘  ║
║                                                                               ║
║  [Clear All] ──────────────────────────────── [Ready for Combat!]            ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Visual Hierarchy
1. **Inventory Bar** (Primary) - Available parts to equip
2. **Minion Panels** (Focus) - Individual minion configuration
3. **Set Bonus Tracker** (Information) - Progress toward abilities
4. **Action Buttons** (Navigation) - Clear all or proceed to combat

## 🎨 Component Specifications

### Inventory Bar
```
Dimensions: Full width, 120px height
Layout: Horizontal scrolling row of part items
Item Size: 80x80px per part
Spacing: 8px gap between items
Background: Elevated panel with subtle border

Part Item Display:
├─ Icon/Sprite (48x48px): Visual representation
├─ Stats Text: "+X HP, +X ATK" (12px font)
├─ Ability Text: "Ability Name" (10px font, orange)
└─ Rarity Border: Color-coded based on rarity
```

### Minion Panels
```
Panel Dimensions: 200x320px each
Layout: Three panels side-by-side
Spacing: 20px horizontal gap
Background: Deep panel with raised border

Panel Structure:
├─ Header (24px): "MINION X" title
├─ Equipment Slots (4 × 56px): Head, Torso, Arms, Legs  
├─ Stats Display (64px): HP, ATK, abilities summary
└─ Bottom Margin (8px)

Equipment Slot:
- Size: 48x48px with 4px border
- States: Empty (dashed border) / Filled (part display)
- Drop Target: Highlight on drag-over
- Labels: Part type clearly marked
```

### Set Bonus Tracker
```
Dimensions: Full width, 80px height
Layout: Two rows of ability progress indicators
Background: Information panel (darker than main)

Progress Indicator Format:
"Ability Name: X/Y [✓/✗]"
- X = Current count of equipped parts
- Y = Required count for activation
- ✓ = Green checkmark if requirement met
- ✗ = Red X if requirement not met

Color Coding:
- Met Requirements: #48BB78 (Success green)
- Unmet Requirements: #F56565 (Warning red)
- Ability Names: #319795 (Accent teal)
```

### Drag and Drop Visual Feedback
```
Dragging State:
- Part Item: 75% opacity, follows cursor
- Valid Drop Zone: Green highlight border
- Invalid Drop Zone: Red highlight border
- Original Slot: Grayed out placeholder

Drop Feedback:
- Success: Brief green flash + satisfying sound
- Invalid: Red flash + negative sound
- Swap: Smooth animation between slots
```

## 🎮 Drag and Drop Mechanics

### Dragging from Inventory
```
Drag Start:
1. Mouse down on inventory part
2. Create dragging visual (semi-transparent copy)
3. Highlight valid drop zones (all empty minion slots)
4. Dim invalid zones

Drag Over:
1. Valid slot: Green border highlight
2. Occupied slot: Yellow border (indicates swap)
3. Invalid area: Red border or no highlight

Drop:
1. Valid empty slot: Equip part, update stats
2. Valid occupied slot: Swap parts, update both minions
3. Invalid area: Return to original position
```

### Dragging Between Minions
```
Inter-Minion Swapping:
- Drag equipped part from one minion to another
- Swap positions if target slot is occupied
- Recalculate set bonuses for both minions
- Update all affected displays instantly

Remove Parts:
- Drag equipped part back to inventory
- Remove from minion slot
- Update stats and set bonus tracker
- Return part to available inventory
```

### Visual States
```
Part Item States:
- Inventory: Full opacity, hover effects
- Equipped: Slightly dimmed, shows equipped status
- Dragging: 75% opacity, enlarged slightly
- Invalid Target: Red tint overlay

Slot States:
- Empty: Dashed border, part type label
- Filled: Solid border, part display
- Drop Target: Glowing border animation
- Invalid: Grayed out, no interaction
```

## 🧮 Stat Calculation Display

### Real-Time Updates
```
Base Stats Display:
"HP: 15 + 12 = 27"
"ATK: 8 + 9 = 17"

Breakdown on Hover:
Head: +3 HP, +2 ATK
Torso: +5 HP, +1 ATK  
Arms: +2 HP, +4 ATK
Legs: +2 HP, +2 ATK
```

### Ability Status
```
Active Abilities (Green):
"✓ Vampiric (Tier 2): Heal 35% of damage dealt"
"✓ Armored (Tier 1): Reduce damage by 1"

Inactive Abilities (Red):
"✗ Critical Strike: Need 1 more part"
"✗ Swift: Need 2 more parts"

Ability Tier Indicators:
- Tier 1 (2 parts): Standard effect
- Tier 2 (3 parts): Enhanced effect  
- Tier 3 (4 parts): Maximum effect
```

### Set Bonus Visualization
```
Progress Bars:
Vampiric: ████████████ 3/2 (Tier 2!)
Critical: ██████░░░░░░ 1/2 (Need 1 more)
Armored:  ████████████ 2/2 (Tier 1!)

Color Coding:
- Completed: Green progress bar
- In Progress: Blue progress bar  
- Not Started: Gray progress bar
```

## 🎨 Pixel Art Implementation

### Equipment Slot Design
```
Slot Dimensions: 48x48px
Border: 4px frame with slot type theming

Empty Slot Visual:
- Dashed border in slot color
- Faded icon indicating slot type
- Background: Slightly darker than panel

Filled Slot Visual:
- Solid border matching part rarity
- Part sprite/icon centered
- Background: Part's rarity color (dimmed)

Slot Type Icons:
- Head: Skull silhouette
- Torso: Ribcage silhouette
- Arms: Arm bone silhouette  
- Legs: Leg bone silhouette
```

### Part Item Cards
```
Card Dimensions: 80x80px
Layout: Icon + text overlay

Visual Layers:
1. Background: Rarity color with transparency
2. Border: Solid rarity color, 2px width
3. Icon: 48x48px part sprite, centered top
4. Text Overlay: Stats and ability, bottom area

Rarity Borders:
- Common: #E2E8F0 (Light gray)
- Rare: #4299E1 (Blue)  
- Epic: #9F7AEA (Purple)
```

### Minion Panel Design
```
Panel Dimensions: 200x320px
Background: Multi-layer panel design

Layer Structure:
1. Base Panel: Dark background color
2. Border Frame: 4px decorative border
3. Header Bar: Minion name section
4. Slot Grid: 2x2 equipment layout
5. Stats Footer: Summary information

Decorative Elements:
- Corner emblems with necromantic symbols
- Subtle texture overlay for depth
- Header divider line with mystical pattern
```

## 🔄 User Experience Flow

### First-Time Assembly
```
1. Tutorial Tooltip: "Drag parts from inventory to minion slots"
2. Guided First Drag: Highlight a part and suggested slot
3. Set Bonus Explanation: "Equip 2+ parts for abilities"
4. Stat Changes: Show how each part affects minion power
5. Ready Confirmation: "All minions equipped? Ready for battle!"
```

### Experienced User Flow
```
1. Quick Scan: Check inventory for new/better parts
2. Strategy Planning: Decide on build focus (tank/dps/hybrid)
3. Optimization: Arrange parts for maximum set bonuses
4. Final Review: Verify stats and abilities look correct
5. Combat Ready: Proceed when satisfied with builds
```

### Error Prevention
```
Visual Cues:
- Cannot drop parts on invalid slots
- Clear feedback for successful/failed drops
- Obvious undo mechanism (drag back to inventory)
- Confirmation before major changes

Accessibility:
- Keyboard navigation support
- Screen reader compatible labels
- High contrast mode option
- Large click targets for drag handles
```

## 📊 Performance Considerations

### Real-Time Calculations
```
Optimization Strategies:
- Cache stat calculations until parts change
- Update only affected minions when parts move
- Batch UI updates to avoid frame drops
- Precompute set bonus thresholds

Memory Management:
- Pool drag/drop visual objects
- Unload unused part sprites
- Efficient UI element recycling
- Smooth animations at 60fps
```

### Large Inventory Handling
```
Scalability Features:
- Virtual scrolling for 20+ parts
- Search/filter functionality
- Sort by type, rarity, or ability
- Pagination for very large collections

UI Responsiveness:
- Immediate visual feedback on interactions
- Progressive loading of part details
- Smooth scrolling animations
- Minimal input lag for drag operations
```

---

*This assembly interface transforms strategic part collection into tactical minion optimization, creating the crucial bridge between drafting and combat phases.* 