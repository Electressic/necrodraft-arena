# Gameplay Scene

## 🎯 Scene Purpose

The **Gameplay** scene is where preparation meets execution. Players watch their carefully crafted minions battle autonomously while UI elements provide clear information about combat progress, unit status, and battle outcomes.

## 🎮 Core Mechanics

### Autobattler Combat
1. **Autonomous Battle**: Units fight automatically based on AI rules
2. **Real-Time Feedback**: Visual and audio cues for all combat events
3. **Strategic Observation**: Players analyze performance for future decisions
4. **Dynamic Information**: Live health, ability triggers, and battle state
5. **Victory Conditions**: Last army standing wins the round

### Combat Phases
* **Deployment**: Units spawn in formation
* **Engagement**: Units move toward enemies and begin fighting
* **Resolution**: Battle continues until one side is eliminated
* **Outcome**: Victory/defeat determination and rewards

## 🎨 UI Layout

### Screen Structure
```
╔═══════════════════════════════════════════════════════════════════════════════╗
║  Wave 3 of 5                    COMBAT                         ⚙️  📊       ║
║                                                                               ║
║  ┌─────────────────────────────────────────────────────────────────────────┐  ║
║  │                          ENEMY ARMY                                    │  ║
║  │                                                                         │  ║
║  │    🧟‍♂️         💀         🧟‍♂️                                              │  ║
║  │   HP: 25      HP: 18      HP: 32                                        │  ║
║  │   ████████    ██████░░    █████████                                     │  ║
║  │                                                                         │  ║
║  └─────────────────────────────────────────────────────────────────────────┘  ║
║                                                                               ║
║                               BATTLEFIELD                                    ║
║                                                                               ║
║                     💥    🗡️    ⚡    💔    ✨                               ║
║                   Combat Effects & Particles                                 ║
║                                                                               ║
║  ┌─────────────────────────────────────────────────────────────────────────┐  ║
║  │                           YOUR ARMY                                    │  ║
║  │                                                                         │  ║
║  │   SKELETON      ZOMBIE       METAL                                      │  ║
║  │    🦴️           🧟           🤖                                          │  ║
║  │   HP: 22        HP: 35       HP: 40                                     │  ║
║  │   ████████░     ████████████  ████████████                             │  ║
║  │   Swift+Crit    Vamp+Armor   Armor+Thorns                              │  ║
║  │                                                                         │  ║
║  └─────────────────────────────────────────────────────────────────────────┘  ║
║                                                                               ║
║  ┌─────────────────────────────────────────────────────────────────────────┐  ║
║  │                          COMBAT LOG                                    │  ║
║  │                                                                         │  ║
║  │  💀 Skeleton CRITS Zombie for 16 damage!                                │  ║
║  │  🧟 Zombie heals for 4 HP (Vampiric)                                    │  ║
║  │  🤖 Metal takes 8 damage, reflects 2 (Thorns)                           │  ║
║  │  ⚡ Swift Skeleton moves to flank                                       │  ║
║  │  💔 Enemy Zombie dies! Screen shake effect                              │  ║
║  │                                                                         │  ║
║  └─────────────────────────────────────────────────────────────────────────┘  ║
║                                                                               ║
║  [Speed: 1x] [Speed: 2x] [Speed: 4x] ──── [Skip to End] [Pause Battle]      ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Visual Hierarchy
1. **Battlefield** (Primary) - The main action area with units and effects
2. **Army Status** (Information) - Health and ability status for all units
3. **Combat Log** (Context) - Detailed play-by-play of combat events
4. **Battle Controls** (Utility) - Speed adjustment and battle flow controls

## 🎨 Component Specifications

### Unit Display Cards
```
Card Dimensions: 180x120px per unit
Layout: Side-by-side in army rows
Spacing: 16px gap between units
Background: Transparent overlay on battlefield

Card Structure:
├─ Unit Icon (48x48px): Visual representation of minion
├─ Health Bar (160x12px): Color-coded HP display
├─ Unit Name (16px font): "Skeleton Warrior", etc.
├─ Abilities (12px font): Active ability list
└─ Status Effects: Temporary buffs/debuffs

Health Bar Colors:
- 80-100%: #48BB78 (Healthy green)
- 50-79%: #ED8936 (Warning orange)  
- 25-49%: #F56565 (Danger red)
- 1-24%: #E53E3E (Critical red, pulsing)
```

### Battlefield Area
```
Dimensions: Central area, ~60% of screen
Background: Arena environment with subtle texture
Layout: Units positioned in formation

Visual Elements:
- Unit Sprites: 64x64px animated minions
- Movement Trails: Subtle particle effects for movement
- Attack Effects: Flash effects, projectiles, impacts
- Death Effects: Explosion particles, screen shake
- Ability Visual: Special effects for crits, healing, etc.

Positioning:
- Player Army: Bottom third of battlefield
- Enemy Army: Top third of battlefield  
- Center Area: Combat engagement zone
```

### Combat Log Panel
```
Dimensions: Full width, 120px height
Layout: Scrolling text with icon indicators
Background: Semi-transparent dark panel
Font: 14px monospace for consistent alignment

Message Format:
"[Icon] [Actor] [Action] [Target] for [Amount] [Effect]!"

Message Types:
- Attack: "⚔️ Skeleton attacks Zombie for 12 damage!"
- Critical: "💥 CRITICAL HIT! Double damage!"
- Healing: "💚 Vampiric healing for 5 HP!"
- Death: "💀 Enemy Zombie has died!"
- Ability: "⚡ Swift activated! +100% speed!"

Color Coding:
- Your units: #319795 (Teal)
- Enemy units: #F56565 (Red)
- Special events: #FBD38D (Yellow)
- System messages: #A0AEC0 (Gray)
```

### Battle Control Panel
```
Dimensions: Full width, 48px height
Layout: Left-aligned speed controls, right-aligned actions
Background: Semi-transparent panel

Speed Controls:
├─ Button "1x": Normal speed (default)
├─ Button "2x": Double speed  
├─ Button "4x": Quadruple speed
└─ Indicator: Current speed highlighted

Action Controls:
├─ Button "Skip to End": Jump to battle conclusion
├─ Button "Pause": Freeze battle (toggle)
└─ Button "Settings": Combat preferences

Button Style:
- Size: 64x32px each
- Style: Raised panel design
- States: Normal, hover, active, disabled
```

## ⚔️ Combat Visual Effects

### Attack Feedback
```
Attacker Effects:
- Flash Effect: Brief yellow overlay (100ms)
- Attack Animation: Strike motion toward target
- Weapon Trail: Subtle particle effect
- Sound: Attack sound based on unit type

Target Effects:
- Flash Effect: Brief white overlay (150ms)
- Damage Numbers: Floating red numbers
- Knockback: Slight position offset
- Health Bar: Immediate update with smooth animation
```

### Special Ability Effects
```
Critical Strike:
- Enhanced yellow flash on attacker
- Larger damage numbers (1.5x size)
- Screen flash effect
- Distinctive "CRIT!" text overlay

Vampiric Healing:
- Green healing numbers floating up
- Brief green glow around healer
- Health bar fills with green tint
- Healing sound effect

Poison/DoT:
- Purple damage numbers over time
- Sick status icon above unit
- Pulsing purple outline on affected unit
- Subtle poison particle effects

Armored Reduction:
- "BLOCKED" text for reduced damage
- Shield shimmer effect on impact
- Damage numbers shown with reduction modifier
- Metallic deflection sound
```

### Death and Destruction
```
Unit Death Sequence:
1. Death cry sound effect
2. Unit sprite flashes red (200ms)
3. Explosion particle effect
4. Screen shake (mild, 300ms)
5. Unit fades out over 500ms
6. Remove from battlefield

Victory/Defeat Effects:
- Victory: Bright flash, triumphant sound
- Defeat: Red overlay, defeated sound
- Pause: 2-second pause for impact
- Transition: Fade to results screen
```

## 🎮 Interactive Elements

### Unit Selection (Optional)
```
Click on Unit:
- Highlight: Blue outline around selected unit
- Info Panel: Detailed stats and ability descriptions
- History: Show recent actions in combat log
- Status: Current buffs, debuffs, conditions

Unit Info Overlay:
┌─────────────────────┐
│   Skeleton Warrior  │
├─────────────────────┤
│ HP: 22/35 (63%)     │
│ ATK: 15 (8+7 bonus) │
│ Speed: 4.0 (Swift)  │
├─────────────────────┤
│ Active Abilities:   │
│ ✓ Swift (Tier 1)    │
│ ✓ Critical (Tier 2) │
│ ✗ Poison (disabled) │
└─────────────────────┘
```

### Speed Control Logic
```
Speed Settings:
- 1x: Normal speed, full effects
- 2x: Double speed, reduced particle effects  
- 4x: Quadruple speed, minimal effects
- Skip: Instant resolution with summary

Player Preferences:
- Default Speed: Remember last setting
- Auto-Skip: Option to skip repeated battles
- Effects Level: Full/Reduced/Minimal for performance
```

### Pause/Resume System
```
Pause State:
- All animations freeze
- "PAUSED" overlay appears
- Battle timer stops
- Allow examination of current state

Resume Options:
- Click Pause again to resume
- Press spacebar for quick toggle
- Auto-resume after settings closed
- Maintain speed setting through pause
```

## 📊 Information Display

### Battle Statistics
```
Real-Time Stats Panel (Optional):
┌─────────────────────────────┐
│       BATTLE STATS          │
├─────────────────────────────┤
│ Duration: 00:32             │
│ Your Damage: 127            │
│ Enemy Damage: 89            │
│ Abilities Used: 8           │
│ Critical Hits: 3            │
└─────────────────────────────┘

End-of-Battle Summary:
- Total damage dealt/taken
- Abilities triggered count
- MVP unit (most damage/healing)
- Battle duration
- Experience/rewards earned
```

### Status Effect Indicators
```
Icon System:
🛡️ Armored: Damage reduction active
⚡ Swift: Movement speed increased
💪 Berserker: Attack speed boosted
☠️ Poisoned: Taking damage over time
💚 Regenerating: Healing over time
⭐ Critical Ready: Next attack may crit

Status Duration:
- Permanent: Ability-based effects
- Temporary: Combat-triggered effects
- Countdown: Show remaining duration
- Visual: Pulsing/fading to indicate expiration
```

## 🎨 Pixel Art Implementation

### Unit Sprites
```
Sprite Dimensions: 64x64px
Animation Frames: 4-8 frames per action
Color Palette: Consistent with part themes

Animation Types:
- Idle: Subtle breathing/floating (2-4 frames)
- Walk: Movement animation (4-6 frames)
- Attack: Strike motion (3-4 frames)
- Hit: Damage reaction (2 frames)
- Death: Collapse/explosion (4-6 frames)

Visual Consistency:
- Match equipped parts when possible
- Skeleton theme: White/yellow bones
- Zombie theme: Green/brown decay
- Metal theme: Gray/silver mechanical
```

### Particle Effects
```
Attack Sparks:
- Small orange/yellow particles
- Brief duration (300-500ms)
- Directional based on attack angle
- 2-4 pixels per particle

Death Explosion:
- Red/orange particles radiating outward
- Larger size (4-8 pixels per particle)
- Longer duration (800-1200ms)
- Gravity effect for realistic motion

Healing Effects:
- Green sparkles rising upward
- Soft glow around healed unit
- Gentle floating motion
- 1-3 pixels per particle
```

### Background Environment
```
Arena Design:
- Dark stone/metal floor texture
- Subtle grid pattern for positioning
- Atmospheric lighting from torches
- Mystical fog/mist effects around edges

Visual Layers:
1. Base Floor: Tiled stone texture
2. Grid Lines: Subtle positioning guides
3. Atmospheric: Particle effects, lighting
4. UI Overlays: Health bars, status icons

Color Scheme:
- Base: Dark grays (#2D3748, #4A5568)
- Accents: Mystical blues/purples
- Lighting: Warm oranges from torches
- Effects: Bright colors for contrast
```

## 🔄 Battle Flow and Pacing

### Combat Timing
```
Turn Structure:
- Initiative Order: Based on unit speed stats
- Action Resolution: Attacks, abilities, movement
- Status Updates: DoT, regeneration, buffs
- Victory Check: After each unit death

Pacing Control:
- Natural Pauses: After major events (deaths)
- Player Control: Speed adjustment anytime
- Auto-Progression: Battle advances automatically
- Skip Option: Jump to conclusion if desired
```

### Dramatic Moments
```
Close Calls:
- Slow motion effect when unit drops to 1 HP
- Tension-building music changes
- Enhanced visual effects for final blows
- Pause before showing battle outcome

Comeback Victories:
- Special effects when odds are overcome
- Triumphant music cues
- Enhanced victory animations
- Celebration particles/screen effects
```

---

*This gameplay scene transforms strategic preparation into thrilling spectacle, providing clear information while maintaining the excitement of watching plans unfold in real-time combat.* 