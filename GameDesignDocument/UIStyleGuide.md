# UI Style Guide

## 🎨 Visual Identity

NecroDraft Arena's UI should feel **dark fantasy meets tactical strategy** - professional and readable while maintaining the necromantic theme. Think "spellbook interface" rather than "horror game".

## 🎯 Design Principles

### Clarity First
- **Information hierarchy**: Most important info is largest/highest contrast
- **Consistent layouts**: Similar functions use similar visual patterns
- **Readable typography**: Clear fonts, appropriate sizing, good contrast

### Thematic Integration
- **Dark palette**: Deep blues, purples, grays with selective accent colors
- **Medieval touches**: Subtle borders, parchment textures, mystical elements
- **Modern UX**: Despite theme, follows contemporary usability standards

### Scalable System
- **Component library**: Reusable UI elements across all screens
- **Pixel-perfect**: Crisp edges, consistent pixel grid alignment
- **Modular design**: Easy to add new screens using existing components

## 🎨 Creating Pixel Art UI in Aseprite

### Setting Up Your Canvas

#### Project Setup
```
1. File → New
   - Width: 1920px (or your target resolution)
   - Height: 1080px
   - Color Mode: RGB
   - Background: Transparent

2. View → Grid → Grid Settings
   - Width: 8px
   - Height: 8px
   - (This helps maintain consistent spacing)

3. View → Show Grid (Ctrl+')
   - Keep grid visible while working
```

#### Color Palette Creation
Based on the beautiful UI you showed me, create a cohesive palette:

```
Primary Colors:
- Dark Background: #2D3748 (Dark gray-blue)
- Medium Background: #4A5568 (Medium gray-blue)  
- Light Background: #718096 (Light gray-blue)
- Accent Teal: #319795 (Bright teal for highlights)
- Text Light: #E2E8F0 (Off-white for primary text)
- Text Dark: #2D3748 (Dark for light backgrounds)

Supporting Colors:
- Success Green: #48BB78
- Warning Orange: #ED8936
- Error Red: #F56565
- Rare Blue: #4299E1
- Epic Purple: #9F7AEA
```

### 9-Slice Panel Creation

The key to that professional UI look is **9-slice panels** - create once, scale infinitely:

#### Step 1: Create Base Panel
```
1. New Layer: "Panel Border"
2. Rectangle Select Tool (M)
3. Draw a 32x32 pixel rectangle
4. Use Pencil Tool (B) with 1-pixel brush
5. Create border pattern:
   ┌─────────────────────────────────┐
   │ 8px │   16px content   │ 8px   │
   ├─────┼─────────────────────┼─────┤
   │ 8px │                     │ 8px │
   │     │    Fill Area        │     │
   │ 8px │                     │ 8px │
   ├─────┼─────────────────────┼─────┤
   │ 8px │       16px         │ 8px │
   └─────────────────────────────────┘
```

#### Step 2: Add Visual Details
```
Border Decoration:
- Outer edge: 1px darker outline
- Inner edge: 1px lighter highlight
- Corner details: Small decorative elements
- Subtle texture: Noise or pattern in fill area

Color Gradients:
- Use Dithering for smooth gradients
- Top: Slightly lighter than base
- Bottom: Slightly darker than base
- Gives depth and dimension
```

#### Step 3: Create Variants
```
Panel States:
- Default: Normal background color
- Hover: Slightly lighter/highlighted border
- Selected: Accent color border or background
- Disabled: Desaturated/grayed out
- Error: Red accent elements
- Success: Green accent elements
```

### Button Component System

#### Basic Button Template (32x12 pixels)
```
Layer Structure:
1. "Button Background" - Base shape and fill
2. "Button Border" - Outline and depth
3. "Button Highlight" - Top edge highlight
4. "Button Shadow" - Bottom edge shadow
5. "Button Icon" - Optional icon space (8x8)
6. "Button Text" - Text overlay area
```

#### Size Variants
```
Small Button: 24x8 pixels (icons, compact actions)
Medium Button: 32x12 pixels (standard actions)
Large Button: 48x16 pixels (primary actions)
Wide Button: 64x12 pixels (text-heavy actions)
```

### Icon System

#### Creating Consistent Icons (8x8 or 16x16)
```
Design Rules:
- Stick to pixel grid (no sub-pixel positioning)
- Use consistent line weights (usually 1px)
- Limited color palette (3-4 colors max per icon)
- Clear silhouettes (recognizable at small sizes)
- Thematic consistency (medieval/fantasy elements)

Common Icons Needed:
- Part Types: Head (skull), Torso (chest), Arms (arm), Legs (leg)
- Abilities: Sword (attack), Shield (defense), Heart (health)
- Actions: Gear (settings), X (close), Arrow (navigation)
- Rarity: Stars, gems, crowns for different tiers
```

### Text and Typography

#### Pixel Font Guidelines
```
Recommended Approach:
1. Use built-in bitmap fonts or create custom
2. Stick to multiples of 8px (8, 16, 24, 32)
3. Ensure good contrast against backgrounds
4. Test readability at actual game resolution

Font Hierarchy:
- Headers: 16px, bold/outlined
- Body text: 12px, regular weight  
- Captions: 8px, often with reduced opacity
- UI labels: 10px, often all-caps
```

#### Text Color System
```
Primary Text: #E2E8F0 (light gray, main content)
Secondary Text: #A0AEC0 (medium gray, supporting info)
Accent Text: #319795 (teal, highlights and links)
Success Text: #48BB78 (green, positive feedback)
Warning Text: #ED8936 (orange, cautions)
Error Text: #F56565 (red, errors and alerts)
```

### Creating the Card UI Component

Following the style you showed me:

#### Card Template (128x180 pixels)
```
Layer Structure:
1. "Card Background" - Main panel with 9-slice borders
2. "Rarity Background" - Color coding for rarity
3. "Content Area" - Where sprite/icon goes
4. "Text Sections" - Divided areas for different info
5. "Button Area" - Action button at bottom
6. "Border Effects" - Optional glow/highlights for rarity

Layout Grid:
┌─── 128px ───┐
├─────────────┤ ← 8px margin
│   [ICON]    │ ← 48x48 icon area
├─────────────┤ ← 8px spacing
│   Title     │ ← 16px text height
│   Subtitle  │ ← 12px text height  
├─────────────┤ ← 8px spacing
│   Stats     │ ← 24px stats area
├─────────────┤ ← 8px spacing
│  Abilities  │ ← 32px ability area
├─────────────┤ ← 8px spacing
│  [BUTTON]   │ ← 16px button height
└─────────────┘ ← 8px margin
```

### Advanced Techniques

#### Dithering for Gradients
```
Pattern Examples:
Light Dither (25%):    Medium Dither (50%):    Heavy Dither (75%):
█ █ █ █                █ █ █ █                █ █ █ █
 █ █ █                █ █ █ █                █ █ █ █
█ █ █ █                █ █ █ █                █ █ █ █
 █ █ █                █ █ █ █                █ █ █ █

Use for smooth color transitions without actual gradients
```

#### Animation-Ready Assets
```
Create sprite sheets for animated elements:
- Button hover states (frame 1: normal, frame 2: hover)
- Loading spinners (8-12 frames for rotation)
- Progress bar fills (animated growth)
- Particle effects (sparkles, glows)

Export Settings:
- PNG with transparency
- No anti-aliasing
- Maintain pixel perfect edges
```

### Unity Integration

#### Import Settings for UI Sprites
```
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Pixels Per Unit: 1 (for UI elements)
Filter Mode: Point (no filter)
Compression: None
Max Size: Next power of 2 that fits
```

#### 9-Slice Setup in Unity
```
1. Select your panel sprite
2. Click "Sprite Editor"
3. Set Border values:
   - L: 8 (left border width)
   - R: 8 (right border width)  
   - T: 8 (top border height)
   - B: 8 (bottom border height)
4. Apply changes
5. Use Image component with Type: Sliced
```

## 📱 Component Library

### Button Variants
```
Primary Button: Teal background, white text
Secondary Button: Gray background, light text  
Danger Button: Red background, white text
Ghost Button: Transparent background, colored border
Icon Button: Square, icon-only, minimal styling
```

### Panel Types
```
Content Panel: Main background color, subtle border
Card Panel: Elevated appearance, stronger borders
Dialog Panel: High contrast, modal overlay feeling
Tooltip Panel: Dark background, light border
Status Panel: Color-coded by status (success/warning/error)
```

### Input Elements
```
Text Field: Inset appearance, clear input area
Dropdown: Arrow indicator, expandable list
Checkbox: Square with checkmark, themed colors
Radio Button: Circle with dot, grouped options
Slider: Track with handle, shows current value
```

## 🎮 Responsive Design

### Screen Size Considerations
```
Minimum: 1280x720 (720p)
Target: 1920x1080 (1080p)  
Maximum: 2560x1440 (1440p)

UI Scaling:
- Use Unity's Canvas Scaler
- Scale with Screen Size mode
- Reference Resolution: 1920x1080
- Match Width or Height: 0.5 (balanced)
```

### Layout Adaptations
```
Large Screens: More spacing, larger UI elements
Small Screens: Compact layouts, smaller margins
Ultra-wide: Maintain aspect ratio, add side padding
Portrait: Stack elements vertically, adjust proportions
```

---

*This style guide provides the foundation for creating cohesive, professional-looking UI while maintaining the pixel art aesthetic that makes the game visually distinctive.* 