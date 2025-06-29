# Game Over Scene

## 🎯 Scene Purpose

The **Game Over** scene provides resolution and reflection after each run. Whether celebrating victory or analyzing defeat, it offers meaningful feedback, progression rewards, and clear paths forward.

## 🎮 Core Functions

### Outcome Resolution

1. **Display Results**: Show victory/defeat with context
2. **Performance Summary**: Statistics and achievements from the run
3. **Progression Rewards**: Experience, unlocks, and advancement
4. **Reflection Tools**: Analysis of key decisions and performance
5. **Next Steps**: Clear options for continuing play

### Dual Purpose Design

* **Victory**: Celebration, rewards, progression to harder content
* **Defeat**: Encouragement, learning opportunities, retry options

## 🎨 UI Layout

### Victory Screen Structure

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                                                                               ║
║                              🏆 VICTORY! 🏆                                   ║
║                          You conquered the arena!                            ║
║                                                                               ║
║                                                                               ║
║  ┌─────────────────────────────────────────────────────────────────────────┐  ║
║  │                          VICTORY SUMMARY                                │  ║
║  │                                                                         │  ║
║  │   ⭐ Waves Completed: 5/5            💀 Total Enemies Defeated: 47     │  ║
║  │   ⏱️ Total Time: 12:34               🧩 Final Minion Count: 3          │  ║
║  │   💥 Total Damage Dealt: 2,847      🏅 Perfect Battles: 3              │  ║
║  │   💚 Total Healing Done: 892         🎯 Abilities Used: 156            │  ║
║  │                                                                         │  ║
║  └─────────────────────────────────────────────────────────────────────────┘  ║
║                                                                               ║
║  ┌─────────────────────────────────────────────────────────────────────────┐  ║
║  │                           REWARDS EARNED                               │  ║
║  │                                                                         │  ║
║  │   🌟 Experience Gained: +250 XP                                         │  ║
║  │   🔓 New Parts Unlocked: 3                                              │  ║
║  │      • Epic Lich Crown (Head)                                           │  ║
║  │      • Rare Bone Gauntlets (Arms)                                       │  ║
║  │      • Common Spirit Legs (Legs)                                        │  ║
║  │                                                                         │  ║
║  │   🏆 Achievement Unlocked: "Undead Mastery"                             │  ║
║  │      Complete a run using only Skeleton parts                           │  ║
║  │                                                                         │  ║
║  └─────────────────────────────────────────────────────────────────────────┘  ║
║                                                                               ║
║  ┌─────────────────────────────────────────────────────────────────────────┐  ║
║  │                        FINAL ARMY SHOWCASE                             │  ║
║  │                                                                         │  ║
║  │    CHAMPION 1         CHAMPION 2         CHAMPION 3                    │  ║
║  │    🦴 Bone Lord       🧟 Decay Master    🤖 Steel Guardian             │  ║
║  │    HP: 45 ATK: 32     HP: 68 ATK: 21     HP: 78 ATK: 18               │  ║
║  │    Swift + Critical   Vampiric + Regen   Armored + Thorns             │  ║
║  │                                                                         │  ║
║  └─────────────────────────────────────────────────────────────────────────┘  ║
║                                                                               ║
║  [Play Again] ─── [Harder Difficulty] ─── [Main Menu] ─── [Quit]             ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Defeat Screen Structure

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                                                                               ║
║                               💀 DEFEAT 💀                                   ║
║                          The arena claims another...                         ║
║                                                                               ║
║                                                                               ║
║  ┌─────────────────────────────────────────────────────────────────────────┐  ║
║  │                           RUN SUMMARY                                   │  ║
║  │                                                                         │  ║
║  │   📊 Waves Completed: 3/5            💀 Enemies Defeated: 23           │  ║
║  │   ⏱️ Survival Time: 8:47              🏆 Best Performance: Wave 2       │  ║
║  │   💥 Damage Dealt: 1,456             💔 Cause of Death: Overwhelmed     │  ║
║  │   🛡️ Damage Taken: 1,892             📈 Progress: 60%                  │  ║
║  │                                                                         │  ║
║  └─────────────────────────────────────────────────────────────────────────┘  ║
║                                                                               ║
║  ┌─────────────────────────────────────────────────────────────────────────┐  ║
║  │                          LESSONS LEARNED                               │  ║
║  │                                                                         │  ║
║  │   💡 Your army lacked sustain abilities                                 │  ║
║  │      Consider drafting Vampiric or Regeneration parts                   │  ║
║  │                                                                         │  ║
║  │   💡 Set bonuses were underutilized                                     │  ║
║  │      You had 1 Vampiric part - need 2+ for ability activation          │  ║
║  │                                                                         │  ║
║  │   💡 Enemy type: Heavy Armor Battalion                                  │  ║
║  │      Critical Strike or high ATK parts would counter this better        │  ║
║  │                                                                         │  ║
║  └─────────────────────────────────────────────────────────────────────────┘  ║
║                                                                               ║
║  ┌─────────────────────────────────────────────────────────────────────────┐  ║
║  │                          YOUR FALLEN ARMY                              │  ║
║  │                                                                         │  ║
║  │    FALLEN HERO 1      FALLEN HERO 2      FALLEN HERO 3                 │  ║
║  │    💀 Lost Warrior    💀 Weak Assassin   💀 Glass Tank                 │  ║
║  │    HP: 24 ATK: 28     HP: 18 ATK: 35     HP: 52 ATK: 12               │  ║
║  │    No Set Bonuses     Swift (Tier 1)     Armored (Tier 1)             │  ║
║  │                                                                         │  ║
║  └─────────────────────────────────────────────────────────────────────────┘  ║
║  ┌─────────────────────────────────────────────────────────────────────────┐  ║
║  │                         RESTART OPTIONS                                │  ║
║  │                                                                         │  ║
║  │  [Retry Same Run] - Start over with same class & conditions            │  ║
║  │  [Try New Class] - Select different necromancer archetype              │  ║
║  │  [Easier Mode] - Reduce difficulty for learning                        │  ║
║  │                                                                         │  ║
║  └─────────────────────────────────────────────────────────────────────────┘  ║
║                                                                               ║
║  [Retry] ──── [Change Class] ──── [Main Menu] ──── [Quit]                    ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Visual Hierarchy


1. **Outcome Header** (Primary) - Clear victory/defeat statement
2. **Summary Statistics** (Information) - Performance data and metrics
3. **Learning/Rewards** (Value) - Progression or improvement guidance
4. **Army Showcase** (Reflection) - Final minion configurations
5. **Next Actions** (Navigation) - Clear paths forward

## 🎨 Component Specifications

### Outcome Header

```
Victory Header:
- Text: "🏆 VICTORY! 🏆" (48px, gold color)
- Subtitle: "You conquered the arena!" (24px, light text)
- Animation: Fade in with triumphant particle effects
- Background: Subtle golden glow

Defeat Header:
- Text: "💀 DEFEAT 💀" (48px, red color)
- Subtitle: "The arena claims another..." (24px, gray text)
- Animation: Fade in with somber atmosphere
- Background: Subtle red tint overlay
```

### Statistics Panel

```
Panel Dimensions: Full width, 120px height

Layout: 2 columns × 4 rows of key metrics

Background: Elevated panel with appropriate theming

Metric Format:
"[Icon] Label: Value"

Key Metrics:
- Waves Completed: Progress indicator
- Total Time: Duration of run
- Damage Dealt/Taken: Combat effectiveness
- Enemies Defeated: Overall impact
- Best Performance: Highlight moment
- Abilities Used: Engagement with systems
```

### Learning/Rewards Panel

```
Victory - Rewards Panel:
- Experience gained with animated counter
- New parts unlocked with rarity indicators
- Achievements earned with descriptions
- Progression markers (level up, etc.)

Defeat - Lessons Panel:
- Strategic advice based on army composition
- Enemy type counters and matchup info
- Set bonus optimization suggestions
- General tactical improvements

Text Style:
- Headers: 16px, bold, appropriate color
- Content: 14px, regular, readable contrast
- Icons: 24x24px, thematically appropriate
```

### Army Showcase

```
Minion Display Cards: 200x140px each

Layout: Horizontal row of final army state

Background: Slightly recessed panel

Victory Cards:
- Title: "CHAMPION X" (success green)
- Final stats with bonuses applied
- Active abilities and tier levels
- Heroic presentation style

Defeat Cards:
- Title: "FALLEN HERO X" (muted red)
- Stats showing weaknesses/gaps
- Missed set bonus opportunities
- Learning-focused presentation

Visual Enhancement:
- Victory: Subtle golden particle effects
- Defeat: Faded, ghostly appearance
```

## 🎮 Emotional Design

### Victory Celebration

```
Visual Effects:
- Golden particle shower from top of screen
- Screen flash on entry (brief, triumphant)
- Gentle pulsing glow around achievements
- Smooth number counting animations

Audio Design:
- Triumphant fanfare on scene entry
- Satisfying "ding" sounds for each reward
- Positive UI feedback sounds
- Victory music loop (not overwhelming)

Color Psychology:
- Gold accents for achievement and success
- Bright greens for positive metrics
- Clean whites for clarity and celebration
```

### Defeat Encouragement

```
Visual Effects:
- Gentle fade-in (no harsh transitions)
- Subtle red tint for context without punishment
- Soft lighting to maintain readability
- No jarring or discouraging effects

Audio Design:
- Somber but not depressing background track
- Gentle UI sounds (avoid harsh feedback)
- Encouraging "you can do it" tone
- Hope for next attempt

Color Psychology:
- Muted reds for defeat context
- Blues and purples for reflection
- Warm oranges for learning opportunities
- Avoid pure black/white extremes
```

## 🔄 Player Motivation

### Victory Path Forward

```
Difficulty Progression:
- "Harder Difficulty" option for challenge seekers
- New unlocked content preview
- Achievement progress toward long-term goals
- Mastery challenges or special modes

Immediate Options:
- Play Again: Same difficulty, new run
- Harder Mode: Increased challenge
- Collection: Review unlocked parts
- Social: Share achievement (future feature)
```

### Defeat Recovery

```
Learning Support:
- Specific, actionable advice (not vague)
- Clear explanation of what went wrong
- Positive framing ("try this" vs "you failed")
- Progress acknowledgment (how far you got)

Retry Encouragement:
- "Retry Same Run": Learn from specific failure
- "Try New Class": Fresh approach/strategy
- "Easier Mode": Reduce barriers to learning
- "Study Mode": Detailed post-mortem analysis
```

### Progress Persistence

```
Always Show Growth:
- Parts discovered (even in defeat)
- Personal best tracking
- Knowledge gained about enemy types
- Skill development metrics

Future Goal Teasing:
- "Next unlock at: X more victories"
- "Coming soon: New enemy types"
- "Master all classes: 2/3 complete"
- Preview of advanced features
```

## 🎨 Pixel Art Implementation

### Celebration Effects

```
Victory Particles:
- Golden sparkles (3-5 pixels each)
- Gentle upward drift animation
- Varying opacity for depth
- 20-30 particles maximum for performance

Achievement Icons:
- 32x32px achievement badges
- Metallic gold/silver coloring
- Simple, clear symbolic designs
- Consistent pixel art style

Progress Bars:
- 8px height, smooth fill animation
- Color-coded by context (gold/blue/green)
- Pixel-perfect edges and corners
- Satisfying fill-up animation
```

### Army Showcase Sprites

```
Final Minion Portraits:
- 64x64px detailed character sprites
- Show equipped parts where possible
- Victory: Bright, heroic lighting
- Defeat: Dimmed, respectful presentation

Background Elements:
- Subtle pedestal or platform base
- Appropriate mood lighting
- Non-distracting decorative elements
- Consistent with overall theme
```

## 📊 Data Tracking and Analytics

### Performance Metrics

```
Victory Data:
- Time to completion
- Efficiency ratings (damage per second, etc.)
- Strategy diversity (part types used)
- Perfect battle streaks

Defeat Analysis:
- Failure point identification
- Army composition analysis
- Decision point tracking
- Common failure patterns
```

### Learning System

```
Adaptive Advice:
- Track repeated failure patterns
- Suggest specific counter-strategies
- Highlight successful past decisions
- Personalize tips based on playstyle

Progress Tracking:
- Skill development over time
- Strategy experimentation rewards
- Knowledge base expansion
- Achievement completion rates
```

## 🔄 Scene Transitions

### Entry Animation

```
Victory Sequence:
1. Screen flash (white, 100ms)
2. Victory header drops from top (500ms)
3. Statistics panel slides in from left (400ms)
4. Rewards panel slides in from right (400ms)
5. Army showcase fades in (300ms)
6. Action buttons appear (200ms)

Defeat Sequence:
1. Gentle fade from black (800ms)
2. Defeat header fades in (600ms)
3. Summary panel appears (400ms)
4. Lessons panel fades in (400ms)
5. Army showcase appears with respect (300ms)
6. Options buttons fade in (200ms)
```

### Exit Transitions

```
To Main Menu:
- Gentle fade out (500ms)
- Preserve any celebration particles
- Smooth audio transition

To New Game:
- Quick fade (300ms) to maintain momentum
- Carry forward any unlocked content
- Reset appropriate game state

To Retry:
- Brief flash transition (200ms)
- Show "Preparing new run..." message
- Quick transition to maintain engagement
```


---

*This game over scene transforms both victory and defeat into meaningful experiences that drive continued engagement while respecting the emotional state of the player.*