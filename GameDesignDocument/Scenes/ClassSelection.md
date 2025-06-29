# Class Selection Scene

## 🎯 Scene Purpose

The **Class Selection** scene introduces players to different necromancer archetypes, each with unique starting bonuses and playstyle preferences. This adds strategic depth and replayability while providing thematic flavor.

## 🎮 Core Mechanics

### Class System


1. **Choose Archetype**: Select from 3 necromancer classes
2. **Starting Bonuses**: Each class provides different initial advantages
3. **Thematic Guidance**: Classes suggest certain part preferences
4. **Replayability**: Different classes encourage different strategies

### Class Archetypes

* **Bone Weaver**: Skeleton-focused, precision and speed
* **Flesh Sculptor**: Zombie-focused, sustain and resilience
* **Soul Binder**: Mixed approach, balanced and flexible

## 🎨 UI Layout

### Screen Structure

```
╔═══════════════════════════════════════════════════════════════════════════════╗
║                                                                               ║
║                          SELECT YOUR NECROMANCER CLASS                        ║
║                                                                               ║
║  ┌───────────────────┐  ┌───────────────────┐  ┌───────────────────┐          ║
║  │   BONE WEAVER     │  │  FLESH SCULPTOR   │  │    SOUL BINDER    │          ║
║  │                   │  │                   │  │                   │          ║
║  │    [Portrait]     │  │    [Portrait]     │  │    [Portrait]     │          ║
║  │     128x128       │  │     128x128       │  │     128x128       │          ║
║  │                   │  │                   │  │                   │          ║
║  ├───────────────────┤  ├───────────────────┤  ├───────────────────┤          ║
║  │ "Master of Bones" │  │ "Lord of Decay"   │  │ "Spirit Guide"    │          ║
║  ├───────────────────┤  ├───────────────────┤  ├───────────────────┤          ║
║  │ STARTING BONUS:   │  │ STARTING BONUS:   │  │ STARTING BONUS:   │          ║
║  │                   │  │                   │  │                   │          ║
║  │ +2 Critical Parts │  │ +2 Vampiric Parts │  │ +1 Random Rare    │          ║
║  │ +1 Swift Part     │  │ +1 Armored Part   │  │ +1 Choice of 3    │          ║
║  │                   │  │                   │  │                   │          ║
║  ├───────────────────┤  ├───────────────────┤  ├───────────────────┤          ║
║  │ PLAYSTYLE:        │  │ PLAYSTYLE:        │  │ PLAYSTYLE:        │          ║
║  │                   │  │                   │  │                   │          ║
║  │ Glass Cannon      │  │ Tank & Sustain    │  │ Flexible & Safe   │          ║
║  │ High Risk/Reward  │  │ Outlast Enemies   │  │ Adaptable Builds  │          ║
║  │                   │  │                   │  │                   │          ║
║  ├───────────────────┤  ├───────────────────┤  ├───────────────────┤          ║
║  │                   │  │                   │  │                   │          ║
║  │    [  SELECT  ]   │  │    [  SELECT  ]   │  │    [  SELECT  ]   │          ║
║  │                   │  │                   │  │                   │          ║
║  └───────────────────┘  └───────────────────┘  └───────────────────┘          ║
║                                                                               ║
║                                                                               ║
║  ┌─────────────────────────────────────────────────────────────────────────┐  ║
║  │                          DETAILED DESCRIPTION                           │  ║
║  │                                                                         │  ║
║  │  Hover over a class to see detailed information about their             │  ║
║  │  strengths, weaknesses, and recommended strategies...                   │  ║
║  │                                                                         │  ║
║  └─────────────────────────────────────────────────────────────────────────┘  ║
║                                                                               ║
║  [Back to Menu] ──────────────────────────────────── [Skip → Random Class]    ║
╚═══════════════════════════════════════════════════════════════════════════════╝
```

### Visual Hierarchy


1. **Scene Title** (Primary) - Clear purpose statement
2. **Class Cards** (Focus) - Three equal options with detailed info
3. **Description Panel** (Context) - Dynamic information on hover
4. **Navigation** (Utility) - Back option and skip for experienced players

## 🎨 Component Specifications

### Class Cards

```
Card Dimensions: 220x400px (27.5x50 grid units)
Spacing: 40px horizontal gap between cards

Border Style: Elevated panel with class-themed accents

Card Structure:
├─ Header (32px): Class name
├─ Portrait (128px): Character artwork  
├─ Subtitle (24px): Thematic title
├─ Bonus Section (80px): Starting advantages
├─ Playstyle Section (80px): Strategy description
└─ Button (32px): Selection action
```

### Portrait Area

```
Dimensions: 128x128px

Style: Ornate frame with class theming

Content: Necromancer character art

Border: 4px decorative frame per class theme

Bone Weaver: White/yellow bone motifs

Flesh Sculptor: Green/brown decay elements  
Soul Binder: Purple/blue mystical effects
```

### Class Theming

```
Bone Weaver:
- Primary Color: #F7FAFC (Bone white)
- Accent Color: #FBD38D (Aged bone yellow)
- Border Style: Sharp, angular elements
- Pattern: Subtle bone texture

Flesh Sculptor:
- Primary Color: #2F855A (Dark green)
- Accent Color: #68D391 (Sickly green)
- Border Style: Organic, rounded elements
- Pattern: Subtle flesh/decay texture

Soul Binder:
- Primary Color: #553C9A (Deep purple)
- Accent Color: #9F7AEA (Mystical purple)
- Border Style: Flowing, ethereal elements
- Pattern: Subtle magical rune texture
```

### Text Styling

```
Class Name:
- Font: 18px, bold
- Color: Class accent color
- Style: Outlined for visibility

Starting Bonus:
- Font: 14px, regular
- Color: #48BB78 (Success green)
- Format: Bullet points or numbered list

Playstyle:
- Font: 12px, regular  
- Color: #E2E8F0 (Primary text)
- Style: Descriptive, 2-3 short phrases
```

## 🎮 Interactive Elements

### Class Card States

```
Default State:
- Background: Medium panel color
- Border: Subtle class accent
- Scale: 1.0
- Opacity: 100%

Hover State:
- Background: Lighter panel color
- Border: Bright class accent, thicker
- Scale: 1.02 (subtle growth)
- Opacity: 100%
- Animation: 200ms ease-in-out

Selected State:
- Background: Class accent color (dimmed)
- Border: Bright class accent, thick
- Scale: 1.0
- Opacity: 100%
- Effect: Brief flash animation on selection
```

### Description Panel Updates

```
Default Text:
"Hover over a class to see detailed information..."

Bone Weaver Hover:
"Masters of precision and speed. Excel at critical strikes and swift 
movement, but fragile when caught. Best for players who enjoy high-risk, 
high-reward gameplay with explosive damage potential."

Flesh Sculptor Hover:
"Lords of decay and endurance. Specialize in vampiric healing and heavy 
armor, perfect for outlasting enemies. Ideal for players who prefer 
defensive, sustain-focused strategies."

Soul Binder Hover:
"Balanced practitioners of the dark arts. Offer flexibility and adaptation 
over specialization. Great for new players or those who enjoy adapting 
their strategy based on what parts they find."
```

## 🔄 Class Benefits Implementation

### Starting Inventory Modifications

```
Bone Weaver Starting Bonus:
- Add 2 random parts with CriticalStrike ability
- Add 1 random part with Swift ability
- Parts are Rare quality minimum

Flesh Sculptor Starting Bonus:  
- Add 2 random parts with Vampiric ability
- Add 1 random part with Armored ability
- Parts are Rare quality minimum

Soul Binder Starting Bonus:
- Add 1 random Rare or Epic part
- Show choice of 3 random parts (player picks 1)
- More flexible but less focused advantage
```

### Gameplay Implications

```
Bone Weaver:
- Starts closer to 2-part set bonuses
- Encourages aggressive, speed-based builds
- Higher variance due to critical strike RNG

Flesh Sculptor:
- Starts with strong sustain foundation
- Encourages tanky, defensive builds  
- More consistent, lower variance gameplay

Soul Binder:
- Flexible start, adaptable to any strategy
- Good for learning different approaches
- Balanced risk/reward profile
```

## 🎨 Pixel Art Implementation

### Portrait Creation

```
Canvas Size: 128x128px

Style Guidelines:
- Pixel art with 16-color limit per portrait
- Dark necromancy theme with class distinctions
- Clear silhouettes readable at small size
- Consistent lighting (top-left light source)

Bone Weaver:
- Skeletal features, hollow eye sockets
- Bone jewelry or accessories
- Sharp, angular design elements
- White/yellow color scheme

Flesh Sculptor:
- Decaying flesh, visible rot
- Green/brown color palette
- Organic, flowing design elements
- Shambling, powerful appearance

Soul Binder:
- Mystical robes, glowing elements
- Purple/blue magical effects
- Ethereal, flowing design
- Balanced between skeletal and fleshy
```

### Frame Design

```
Frame Dimensions: 136x136px (includes 4px border)
Border Style: 9-slice panel with class theming

Layer Structure:
1. Outer Border: 1px class accent color

2. Frame Body: 3px class-themed pattern

3. Inner Border: 1px darker accent

4. Portrait Area: 128x128px content space

Class-Specific Details:
- Bone Weaver: Angular corner decorations
- Flesh Sculptor: Organic growth patterns
- Soul Binder: Mystical rune elements
```

## 🔄 Scene Flow

### Entry Animation

```
1. Title fades in from top (300ms)
2. Class cards slide in from sides (400ms, staggered 100ms)
3. Description panel fades in (200ms)
4. Navigation buttons appear (100ms)
Total: ~800ms for full scene reveal
```

### Selection Flow

```
1. Player hovers → Description updates instantly

2. Player clicks SELECT → Card highlight + confirmation sound

3. Brief pause (500ms) for visual feedback

4. Fade out (300ms) → Load Card Selection scene

5. Apply class bonuses to starting inventory
```

### Back/Skip Options

```
Back to Menu:
- Instant transition, no class selection made
- Return to main menu with fade transition

Skip to Random:
- Randomly select one of the three classes
- Show brief "Random: [Class Name]" message
- Apply bonuses and continue to Card Selection
```

## 🎯 Balance Considerations

### Class Power Levels

```
All classes should provide equal value, just different approaches:

Bone Weaver: 3 focused parts = strong set bonus start

Flesh Sculptor: 3 focused parts = strong set bonus start  
Soul Binder: 1-2 flexible parts = adaptability advantage

Power Budget: Each class gets ~1.5x starting value compared to no class
```

### New Player Guidance

```
Visual Cues:
- Soul Binder has "Recommended for New Players" subtitle
- Tooltips explain concepts like "Glass Cannon" and "Sustain"
- Description panel uses clear, jargon-free language

Tutorial Integration:
- First playthrough highlights Soul Binder
- Explains how starting bonuses work
- Shows example of how parts translate to gameplay
```

## 📊 Analytics & Data

### Player Choice Tracking

```
Metrics to Track:
- Class selection distribution (is one overwhelming favorite?)
- Win rates by class (are they balanced?)
- New player vs. experienced player preferences
- Playtime differences between classes

Balance Adjustments:
- If one class wins too often, adjust starting bonuses
- If one class is never picked, improve bonuses or presentation
- Track which descriptions are most appealing
```


---

*This class selection system adds meaningful strategic depth while providing clear identity and replayability to the NecroDraft Arena experience.*