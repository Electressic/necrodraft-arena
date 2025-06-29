# Main Menu Scene

## 🎯 Scene Purpose

The **Main Menu** is the player's first impression and gateway to NecroDraft Arena. It should feel atmospheric and professional while providing clear navigation to all game modes and options.

## 🎮 Core Functions

### Primary Actions
1. **Start New Game** - Begin the campaign/arcade mode
2. **Continue** - Resume existing save (if available)
3. **Settings** - Audio, graphics, and gameplay options
4. **Credits** - Development team and acknowledgments
5. **Quit** - Exit application

### Atmospheric Elements
* **Background**: Dark arena or necromantic laboratory
* **Particle Effects**: Subtle magical elements, floating embers
* **Audio**: Atmospheric background music, UI sound effects

## 🎨 UI Layout

### Screen Structure
```
╔═══════════════════════════════════════════════════════════════════╗
║                                                                   ║
║                     NECRODRAFT ARENA                              ║
║                      [Subtitle/Tagline]                          ║
║                                                                   ║
║                                                                   ║
║                  ┌─────────────────────┐                         ║
║                  │                     │                         ║
║                  │   [LOGO/ARTWORK]    │                         ║
║                  │                     │                         ║
║                  │     192x192px       │                         ║
║                  │                     │                         ║
║                  └─────────────────────┘                         ║
║                                                                   ║
║                                                                   ║
║               ┌─────────────────────────────┐                    ║
║               │        START GAME           │                    ║
║               └─────────────────────────────┘                    ║
║                                                                   ║
║               ┌─────────────────────────────┐                    ║
║               │         CONTINUE            │                    ║
║               └─────────────────────────────┘                    ║
║                                                                   ║
║               ┌─────────────────────────────┐                    ║
║               │         SETTINGS            │                    ║
║               └─────────────────────────────┘                    ║
║                                                                   ║
║               ┌─────────────────────────────┐                    ║
║               │         CREDITS             │                    ║
║               └─────────────────────────────┘                    ║
║                                                                   ║
║               ┌─────────────────────────────┐                    ║
║               │          QUIT               │                    ║
║               └─────────────────────────────┘                    ║
║                                                                   ║
║  v1.0.0                                              [Sound] 🔊  ║
╚═══════════════════════════════════════════════════════════════════╝
```

### Visual Hierarchy
1. **Game Title** (Primary) - Large, prominent, themed font
2. **Main Logo/Art** (Secondary) - Central focal point, thematic imagery
3. **Menu Buttons** (Navigation) - Uniform size, clear labels, proper spacing
4. **Version/Options** (Utility) - Small, unobtrusive corner elements

## 🎨 Component Specifications

### Title Text
```
Font Size: 48px (3x standard 16px)
Style: Bold, outlined for readability
Color: #E2E8F0 (light) with #319795 (teal) accent
Position: Top center, 80px from top
Shadow: Dark outline for depth
```

### Logo/Artwork Panel
```
Dimensions: 192x192px (12x12 grid units)
Style: Elevated panel with border
Content: Game logo, thematic skull/necromancy imagery
Border: 8px decorative frame
Background: Slightly darker than main background
```

### Menu Buttons
```
Button Size: 240x32px (30x4 grid units)
Spacing: 16px vertical gap between buttons
Style: Raised panel appearance
States: Default, Hover, Pressed, Disabled

Text Style:
- Font: 16px, bold
- Color: #E2E8F0 (default), #FFFFFF (hover)
- Alignment: Center

Button Colors:
- Default: Medium background (#4A5568)
- Hover: Lighter background (#718096) 
- Pressed: Darker background (#2D3748)
- Disabled: Desaturated (#A0AEC0)
```

### Corner Elements
```
Version Text:
- Position: Bottom left, 16px margin
- Font: 12px, regular weight
- Color: #A0AEC0 (secondary text)

Sound Toggle:
- Position: Bottom right, 16px margin
- Size: 24x24px icon button
- States: On/Off with visual indicator
```

## 🎮 User Experience Flow

### First Time User
1. **Visual Impact**: Title and artwork establish theme
2. **Clear Call-to-Action**: "START GAME" is most prominent
3. **Exploration**: Settings accessible but not overwhelming
4. **Smooth Transition**: Button feedback leads naturally to next scene

### Returning User
1. **Quick Access**: "CONTINUE" prominently placed
2. **Familiar Layout**: Consistent with previous sessions
3. **Options Available**: Easy access to settings if needed

## ⚡ Interactive Elements

### Button Hover Effects
```
Animation: 150ms ease-in-out
Scale: 1.0 → 1.02 (subtle growth)
Color: Background lightens
Border: Highlight color appears
Sound: Soft hover sound effect
```

### Button Press Effects
```
Animation: 100ms ease-in
Scale: 1.02 → 0.98 (quick press feedback)
Color: Background darkens
Sound: Satisfying click sound
```

### Background Elements
```
Particle System:
- Floating embers: 5-10 slow-moving particles
- Color: Orange/red glow
- Movement: Gentle upward drift
- Opacity: 30-60% for atmosphere

Ambient Animation:
- Subtle background elements
- Flickering candles or magical effects
- Very slow, non-distracting movement
```

## 🎨 Pixel Art Implementation

### Background Creation
```
Base Layer:
- Dark gradient: #1A202C → #2D3748
- Texture overlay: Subtle noise or parchment pattern
- Vignette effect: Darker edges, lighter center

Decorative Elements:
- Gothic border elements in corners
- Mystical symbols or runes (very subtle)
- Stone texture or aged metal appearance
```

### Button Component Template
```
9-Slice Panel (240x32px):
├─ Left Border (8px)
├─ Center Fill (224px) - stretches horizontally
└─ Right Border (8px)

Visual Details:
- Top highlight: 1px lighter color
- Bottom shadow: 1px darker color
- Corner decorations: Small mystical elements
- Inner glow: Subtle internal highlight
```

### Icon Design
```
Sound Icon (24x24px):
- Speaker symbol with sound waves
- On state: Full opacity, normal colors
- Off state: Reduced opacity, strike-through
- Style: Simple, pixel-perfect lines
- Colors: Match button text colors
```

## 🔄 Scene Transitions

### To Game Flow
```
START GAME → Class Selection Scene
- Fade out: 300ms
- Load time: Show subtle loading indicator
- Fade in: 300ms to new scene
```

### To Settings
```
SETTINGS → Settings Overlay/Modal
- Slide down: Settings panel from top
- Background: Darken main menu (50% opacity)
- Close: Slide up animation back to main menu
```

### To Credits
```
CREDITS → Credits Overlay/Scene
- Similar to settings approach
- Scrolling text area with development info
- Background music continues
```

## 🎵 Audio Design

### Background Music
```
Style: Dark ambient/orchestral
Tempo: Slow, atmospheric
Volume: 60% of max, loops seamlessly
Mood: Mysterious but not threatening
```

### UI Sound Effects
```
Button Hover: Soft magical "whoosh" (50ms)
Button Click: Satisfying "thunk" or stone click (100ms)
Scene Transition: Deeper magical sound (500ms)
Error/Disabled: Subtle negative feedback sound
```

## 📱 Responsive Considerations

### Different Screen Sizes
```
1280x720 (Minimum):
- Reduce button spacing to 12px
- Smaller logo: 128x128px
- Maintain button sizes for usability

1920x1080 (Target):
- Standard layout as specified
- Optimal spacing and proportions

2560x1440 (Large):
- Increase margins around content
- Larger decorative elements
- Maintain central layout focus
```

### Ultra-wide Support
```
21:9 Aspect Ratios:
- Center main content column
- Add decorative side panels
- Extend background elements
- Maintain button functionality
```

## 🎯 Success Metrics

### User Engagement
* **Time to First Click**: Should be under 3 seconds
* **Navigation Clarity**: Users find intended buttons immediately
* **Visual Appeal**: Creates excitement for the game experience
* **Accessibility**: All users can navigate regardless of input method

### Technical Performance
* **Load Time**: Menu appears in under 1 second
* **Smooth Animations**: 60fps for all hover/press effects
* **Audio Sync**: Sound effects trigger without delay
* **Memory Usage**: Minimal impact, fast scene transitions

---

*This main menu sets the tone for the entire NecroDraft Arena experience, balancing atmospheric immersion with functional clarity.* 