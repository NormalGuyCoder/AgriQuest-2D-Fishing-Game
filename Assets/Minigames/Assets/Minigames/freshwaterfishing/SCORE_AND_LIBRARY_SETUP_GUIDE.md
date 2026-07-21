# Freshwater Fishing Score System & Shared Fish Library Setup Guide

This guide will help you set up:
1. **Score Viewing System** - Similar to the deboning minigame
2. **Shared Fish Library** - Both minigames use the same fish database

---

## Part 1: Setting Up the Score System

### Step 1: Create the ScoreManager GameObject

1. In your freshwater fishing scene, create an empty GameObject:
   - Right-click in Hierarchy → Create Empty
   - Name it: `FreshwaterFishingScoreManager`
   
2. Add the `FreshwaterFishingScoreManager` component:
   - Select the GameObject
   - In Inspector, click "Add Component"
   - Search for "FreshwaterFishingScoreManager" and add it
   
3. Configure settings:
   - Check "Persist Across Sessions" if you want scores to save between game restarts

### Step 2: Create the Score Panel UI

1. **Create the Score Panel:**
   - In your Canvas, create a new Panel (right-click Canvas → UI → Panel)
   - Name it: `ScorePanel`
   - Set it to inactive initially (uncheck the checkbox in Inspector)

2. **Add Score Display Text Elements:**
   Inside the ScorePanel, create TextMeshProUGUI elements for:
   - `TotalScoreText` - Shows total score
   - `SessionScoreText` - Shows current session score
   - `GamesPlayedText` - Shows number of games played
   - `HighestScoreText` - Shows best catch score
   - `AverageScoreText` - Shows average score
   - `TotalFishCaughtText` - Shows total fish caught
   - `UniqueFishCountText` - Shows unique fish species caught
   - `MostCaughtFishText` - Shows most frequently caught fish
   - `LastUpdatedText` - Shows last update time

3. **Create Fish Statistics Section:**
   - Create a ScrollView inside ScorePanel
   - Name it: `FishStatsContainer`
   - Create a TextMeshProUGUI for the title: `FishStatsTitleText`
   - Create a prefab for individual fish stat items (or use a simple Text element)

4. **Create Recent Catches Section:**
   - Create another ScrollView inside ScorePanel
   - Name it: `RecentCatchesContainer`
   - Create a TextMeshProUGUI for the title: `RecentCatchesTitleText`
   - Create a prefab for individual catch items

5. **Add Buttons:**
   - Create a "Close" button inside ScorePanel
   - Create an "Open Score Panel" button in your main menu/UI (wherever you want to access scores)

### Step 2.5: UI Positioning and Layout for Score Panel

**Score Panel Layout (Recommended):**

1. **ScorePanel (Main Container):**
   - **RectTransform Settings:**
     - Anchor: **Stretch-Stretch** (hold Alt+Shift and click the bottom-right anchor preset)
     - Left: `50`, Right: `50`, Top: `50`, Bottom: `50` (margins from screen edges)
     - This creates a centered panel with padding
   - **Image Component:**
     - Color: Semi-transparent dark (e.g., RGBA: 0, 0, 0, 200) for background overlay
     - Or use a solid panel color if preferred

2. **Header Section (Top of Panel):**
   - Create an empty GameObject: `HeaderSection`
   - **Position:** Anchor to Top-Left
   - **RectTransform:**
     - Anchor Preset: **Top-Stretch**
     - Top: `0`, Left: `20`, Right: `20`, Height: `80`
   - Add elements inside:
     - **Title Text** (e.g., "Fishing Statistics"): 
       - Position: Center, Top: `-10`
       - Font Size: `32-36`
     - **Close Button:**
       - Position: Top-Right corner
       - Anchor: **Top-Right**
       - Position: X: `-20`, Y: `-20`
       - Size: `60x60` or `80x40`

3. **Score Display Section (Left Side):**
   - Create empty GameObject: `ScoreDisplaySection`
   - **Position:** Left side of panel
   - **RectTransform:**
     - Anchor Preset: **Top-Left**
     - Left: `30`, Top: `-100`, Width: `400`, Height: `500`
   - **Layout:** Use Vertical Layout Group component for automatic spacing
   - **Text Elements (arrange vertically, top to bottom):**
     - `TotalScoreText`: Font Size `28`, Bold
     - `SessionScoreText`: Font Size `24`
     - `GamesPlayedText`: Font Size `20`
     - `HighestScoreText`: Font Size `20`
     - `AverageScoreText`: Font Size `20`
     - `TotalFishCaughtText`: Font Size `20`
     - `UniqueFishCountText`: Font Size `20`
     - `MostCaughtFishText`: Font Size `20`
     - Spacing: `15-20` pixels between each text

4. **Fish Statistics Section (Right Side, Top):**
   - Create empty GameObject: `FishStatsSection`
   - **Position:** Right side, top half
   - **RectTransform:**
     - Anchor Preset: **Top-Right**
     - Right: `30`, Top: `-100`, Width: `500`, Height: `350`
   - **Inside this section:**
     - **Title Text** (`FishStatsTitleText`):
       - Position: Top-Left
       - Font Size: `24`, Bold
       - Margin: Top `0`, Left `10`
     - **ScrollView** (`FishStatsContainer`):
       - Position: Below title
       - Anchor: **Stretch-Stretch** (relative to parent)
       - Top: `40`, Left: `0`, Right: `0`, Bottom: `0`
       - **ScrollView Settings:**
         - Horizontal: Unchecked
         - Vertical: Checked
         - Scroll Sensitivity: `20`
       - **Content (inside ScrollView):**
         - Add Vertical Layout Group
         - Spacing: `10`
         - Child Force Expand: Width checked, Height unchecked
         - Child Alignment: Upper Center

5. **Recent Catches Section (Right Side, Bottom):**
   - Create empty GameObject: `RecentCatchesSection`
   - **Position:** Right side, bottom half
   - **RectTransform:**
     - Anchor Preset: **Bottom-Right**
     - Right: `30`, Bottom: `50`, Width: `500`, Height: `250`
   - **Inside this section:**
     - **Title Text** (`RecentCatchesTitleText`):
       - Position: Top-Left
       - Font Size: `24`, Bold
       - Margin: Top `0`, Left `10`
     - **ScrollView** (`RecentCatchesContainer`):
       - Position: Below title
       - Anchor: **Stretch-Stretch** (relative to parent)
       - Top: `40`, Left: `0`, Right: `0`, Bottom: `0`
       - Same ScrollView settings as above

6. **Last Updated Text (Bottom of Panel):**
   - **Position:** Bottom-Center
   - **RectTransform:**
     - Anchor Preset: **Bottom-Stretch**
     - Bottom: `20`, Left: `20`, Right: `20`, Height: `30`
   - Font Size: `16`, Italic, Gray color

**Visual Layout Summary:**
```
┌─────────────────────────────────────────────────────────┐
│ [Title]                                    [Close] [X]  │
├──────────────────┬──────────────────────────────────────┤
│                  │  Fish Statistics                     │
│  Total Score     │  ┌──────────────────────────────┐   │
│  Session Score   │  │ Fish 1 - Stats                │   │
│  Games Played    │  │ Fish 2 - Stats                │   │
│  Highest Score   │  │ Fish 3 - Stats                │   │
│  Average Score   │  │ ... (scrollable)               │   │
│  Total Caught    │  └──────────────────────────────┘   │
│  Unique Fish     │                                      │
│  Most Caught     │  Recent Catches                      │
│                  │  ┌──────────────────────────────┐   │
│                  │  │ Catch 1 - Details            │   │
│                  │  │ Catch 2 - Details            │   │
│                  │  │ ... (scrollable)             │   │
│                  │  └──────────────────────────────┘   │
│                  │                                      │
├──────────────────┴──────────────────────────────────────┤
│         Last Updated: 12:34:56                          │
└─────────────────────────────────────────────────────────┘
```

### Step 3: Set Up the ScorePanelController

1. **Add the Controller:**
   - Select the ScorePanel GameObject
   - Add Component → `FreshwaterFishingScorePanelController`

2. **Assign References in Inspector:**
   - **Panel References:**
     - Score Panel: Drag the `ScorePanel` GameObject
     - Open Score Panel Button: Drag your "Open Score Panel" button
     - Close Score Panel Button: Drag your "Close" button
   
   - **Score Display:**
     - Assign all the TextMeshProUGUI elements you created
   
   - **Fish Statistics:**
     - Fish Stats Container: Drag the `FishStatsContainer` ScrollView's Content
     - Fish Stat Item Prefab: Drag your fish stat item prefab
     - Fish Stats Title Text: Drag the title text
   
   - **Recent Catches:**
     - Recent Catches Container: Drag the `RecentCatchesContainer` ScrollView's Content
     - Recent Catch Item Prefab: Drag your catch item prefab
     - Recent Catches Title Text: Drag the title text
   
   - **Additional Info:**
     - Last Updated Text: Drag the last updated text element

3. **Test the Score Panel:**
   - Play the game and catch some fish
   - Click the "Open Score Panel" button
   - Verify that scores are displayed correctly

---

## Part 2: Setting Up the Shared Fish Library

### Step 1: Create or Use Existing FishCatalogDatabase

1. **Check if you already have a FishCatalogDatabase:**
   - In Project window, search for "FishCatalogDatabase"
   - If it exists, note its location
   - If not, create one:
     - Right-click in `Assets/Minigames/Common/FishCatalog/`
     - Create → Fish Catalog → Database
     - Name it: `FishCatalogDatabase`

2. **Populate the Database:**
   - Select the FishCatalogDatabase asset
   - In Inspector, add all your fish entries to the "Entries" list
   - Make sure each fish has:
     - `fishId` (unique identifier)
     - `displayName`
     - `scientificName`
     - `description`
     - `mainSprite` and `iconSprite`
     - `isEndangered` flag (if applicable)

### Step 2: Create Fish Library UI for Freshwater Fishing

1. **Create the Library Panel:**
   - In your Canvas, create a new Panel
   - Name it: `FishLibraryPanel`
   - Set it to inactive initially

2. **Add UI Elements:**
   - **Title Text:** "Freshwater Fish Library" or "Fish Database"
   - **ScrollView:** For displaying fish cards
     - Name the Content: `FishCardsContainer`
   - **Pagination Buttons:**
     - Previous Page button
     - Next Page button
     - Page Indicator text (e.g., "Page 1 / 3")
   - **Back Button:** To close the library

3. **Create Fish Card Prefab:**
   - Create a UI element (Panel or Image) for a single fish card
   - Add:
     - Image component for fish icon
     - TextMeshProUGUI for fish name
     - Optional: TextMeshProUGUI for scientific name
   - Make it a prefab: Drag to `Assets/Minigames/freshwaterfishing/Prefabs/`
   - Name it: `FishCardPrefab`

### Step 2.5: UI Positioning and Layout for Fish Library

**Fish Library Panel Layout (Recommended):**

1. **FishLibraryPanel (Main Container):**
   - **RectTransform Settings:**
     - Anchor: **Stretch-Stretch** (full screen overlay)
     - Left: `0`, Right: `0`, Top: `0`, Bottom: `0`
   - **Image Component:**
     - Color: Semi-transparent dark background (RGBA: 0, 0, 0, 220)

2. **Header Section:**
   - Create empty GameObject: `LibraryHeader`
   - **RectTransform:**
     - Anchor Preset: **Top-Stretch**
     - Top: `0`, Left: `50`, Right: `50`, Height: `100`
   - **Inside Header:**
     - **Title Text** (`LibraryTitleText`):
       - Position: Center
       - Font Size: `36-40`, Bold
       - Color: White or your theme color
     - **Back Button:**
       - Position: Top-Left corner
       - Anchor: **Top-Left**
       - Position: X: `20`, Y: `-20`
       - Size: `100x50` or `120x40`
       - Text: "Back" or "← Back"

3. **Main Content Area (Center):**
   - Create empty GameObject: `LibraryContent`
   - **RectTransform:**
     - Anchor Preset: **Stretch-Stretch**
     - Top: `120`, Left: `50`, Right: `50`, Bottom: `150`
   - **Inside Content:**
     - **ScrollView** for fish cards:
       - Name: `FishCardsScrollView`
       - **RectTransform:**
         - Anchor: **Stretch-Stretch** (fills parent)
         - All margins: `0`
       - **ScrollView Settings:**
         - Horizontal: Unchecked
         - Vertical: Checked
         - Movement Type: Elastic
         - Scroll Sensitivity: `20`
       - **Viewport:**
         - Image component: Transparent or subtle background
       - **Content** (`FishCardsContainer`):
         - **RectTransform:**
           - Anchor: **Top-Left**
           - Width: Match parent width
           - Use Content Size Fitter: Vertical Fit = Preferred Size
         - **Add Grid Layout Group:**
           - Cell Size: `150x200` (adjust based on card size)
           - Spacing: `20x20` (horizontal and vertical spacing)
           - Start Corner: Upper Left
           - Start Axis: Horizontal
           - Child Alignment: Upper Left
           - Constraint: Fixed Column Count = `4` (or 3-5 depending on screen size)
           - Padding: Left `10`, Right `10`, Top `10`, Bottom `10`

4. **Pagination Section (Bottom):**
   - Create empty GameObject: `PaginationSection`
   - **RectTransform:**
     - Anchor Preset: **Bottom-Stretch**
     - Bottom: `20`, Left: `50`, Right: `50`, Height: `80`
   - **Inside Pagination:**
     - **Previous Button:**
       - Position: Left side
       - Anchor: **Middle-Left**
       - Position: X: `50`, Y: `0`
       - Size: `120x50`
       - Text: "← Previous"
     - **Page Indicator Text:**
       - Position: Center
       - Anchor: **Middle-Center**
       - Position: X: `0`, Y: `0`
       - Font Size: `20-24`
       - Text: "Page 1 / 3" (will be updated by script)
     - **Next Button:**
       - Position: Right side
       - Anchor: **Middle-Right**
       - Position: X: `-50`, Y: `0`
       - Size: `120x50`
       - Text: "Next →"

5. **Fish Card Prefab Layout:**
   - **Card Container (Panel or Image):**
     - Size: `140x190` (slightly smaller than grid cell for spacing)
     - **RectTransform:**
       - Anchor: **Middle-Center**
     - **Image Component:**
       - Color: White or light background
       - Optional: Add a border/sprite for card frame
   - **Inside Card:**
     - **Fish Icon Image:**
       - Position: Top-Center
       - Anchor: **Top-Stretch**
       - Top: `10`, Left: `15`, Right: `15`, Height: `100`
       - Image Type: Simple
       - Preserve Aspect: Checked
     - **Fish Name Text:**
       - Position: Below icon
       - Anchor: **Top-Stretch**
       - Top: `120`, Left: `5`, Right: `5`, Height: `30`
       - Font Size: `16-18`, Bold
       - Alignment: Center, Middle
       - Auto Size: Checked (optional)
     - **Scientific Name Text (Optional):**
       - Position: Below name
       - Anchor: **Top-Stretch**
       - Top: `150`, Left: `5`, Right: `5`, Height: `25`
       - Font Size: `12-14`, Italic
       - Alignment: Center, Middle
       - Color: Gray

**Visual Layout Summary:**
```
┌─────────────────────────────────────────────────────────┐
│ [← Back]         Freshwater Fish Library                │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  ┌────┐  ┌────┐  ┌────┐  ┌────┐                        │
│  │ 🐟 │  │ 🐟 │  │ 🐟 │  │ 🐟 │                        │
│  │Name│  │Name│  │Name│  │Name│                        │
│  └────┘  └────┘  └────┘  └────┘                        │
│                                                          │
│  ┌────┐  ┌────┐  ┌────┐  ┌────┐                        │
│  │ 🐟 │  │ 🐟 │  │ 🐟 │  │ 🐟 │                        │
│  │Name│  │Name│  │Name│  │Name│                        │
│  └────┘  └────┘  └────┘  └────┘                        │
│                                                          │
│  (Scrollable area with grid layout)                     │
│                                                          │
├─────────────────────────────────────────────────────────┤
│ [← Previous]        Page 1 / 3        [Next →]         │
└─────────────────────────────────────────────────────────┘
```

**Responsive Design Tips:**
- For smaller screens: Use 3 columns instead of 4
- For larger screens: Use 5-6 columns
- Adjust card size proportionally: `(Screen Width - Padding) / Columns - Spacing`
- Use Content Size Fitter on ScrollView Content to auto-adjust height

### Step 3: Set Up SharedFishLibraryController

1. **Add the Controller:**
   - Select the FishLibraryPanel GameObject
   - Add Component → `SharedFishLibraryController`

2. **Assign References:**
   - **UI References:**
     - Fish Cards Container: Drag the `FishCardsContainer` (ScrollView Content)
     - Fish Card Prefab: Drag your `FishCardPrefab`
     - Back To Menu Button: Drag your back button
     - Library Title Text: Drag your title text
   
   - **Pagination:**
     - Items Per Page: Set to 8 (or your preferred number)
     - Next Page Button: Drag your next button
     - Previous Page Button: Drag your previous button
     - Page Indicator Text: Drag your page indicator
   
   - **Fish Database:**
     - Fish Catalog Database: Drag your `FishCatalogDatabase` asset
     - (Optional) Available Fish: Leave empty if using database
   
   - **Learning Tips (Optional):**
     - Assign tip text elements if you have them

3. **Add Button to Open Library:**
   - In your main menu/UI, create a "View Fish Library" button
   - In the button's OnClick event:
     - Add the FishLibraryPanel GameObject
     - Select `GameObject.SetActive(bool)`
     - Set to `true`

### Step 4: Update Deboning Minigame to Use Shared Library (Optional)

If you want the deboning minigame to also use the shared library:

1. **In the Deboning Scene:**
   - Find the existing `FishLibraryController` component
   - You can either:
     - **Option A:** Replace it with `SharedFishLibraryController`
     - **Option B:** Keep both (one for deboning-specific, one shared)

2. **If using Option A:**
   - Remove the old `FishLibraryController` component
   - Add `SharedFishLibraryController` component
   - Assign the same `FishCatalogDatabase` asset
   - Update UI references to match the new controller

---

## Part 3: Integration with Existing Systems

### Step 1: Verify Score Recording

The `FishingMinigame.cs` script has been updated to automatically record scores when fish are caught. Verify:

1. Play the game and catch a fish
2. Check the Console for: `"Freshwater Fishing: Caught [FishName] - Score: [Score]..."`
3. Open the Score Panel and verify the catch appears

### Step 2: Link Fish to FishCatalogEntry (Optional but Recommended)

To fully integrate with the shared catalog system:

1. **Update FishManager or CSV:**
   - Add a column for `fishId` that matches `FishCatalogEntry.fishId`
   - Or create a mapping dictionary in code

2. **Update FishingMinigame.cs:**
   - When recording scores, try to find the matching `FishCatalogEntry`
   - Use the catalog entry's ID instead of just the fish name
   - This ensures consistency across minigames

Example code to add to `FishCaught()`:
```csharp
// Try to find matching catalog entry
string fishId = currentFishOnLine.name.ToLower().Replace(" ", "_");
if (FishCatalogDatabase.Instance != null)
{
    FishCatalogEntry catalogEntry = FishCatalogDatabase.Instance.GetEntry(fishId);
    if (catalogEntry != null)
    {
        fishId = catalogEntry.GetFishId();
    }
}
```

---

## Part 4: Testing

### Test Score System:
1. ✅ Catch multiple fish
2. ✅ Check that scores accumulate
3. ✅ Verify session score resets on scene reload (if configured)
4. ✅ Check that total score persists after game restart
5. ✅ Verify fish statistics are tracked correctly
6. ✅ Check recent catches list

### Test Fish Library:
1. ✅ Open the fish library panel
2. ✅ Verify all fish from catalog are displayed
3. ✅ Test pagination (next/previous buttons)
4. ✅ Click on a fish card to see details (if detail panel is set up)
5. ✅ Verify fish sprites and names display correctly

### Test Shared Database:
1. ✅ Verify both minigames show the same fish
2. ✅ Check that fish caught in one minigame appear in the other's library
3. ✅ Verify endangered status and other metadata are consistent

---

## Troubleshooting

### Score Panel Not Showing:
- ✅ Check that ScorePanel GameObject is assigned in controller
- ✅ Verify button OnClick events are set up correctly
- ✅ Check Console for errors

### Scores Not Recording:
- ✅ Verify FreshwaterFishingScoreManager exists in scene
- ✅ Check Console for score recording messages
- ✅ Ensure FishingMinigame.cs has the updated code

### Fish Library Empty:
- ✅ Verify FishCatalogDatabase asset is assigned
- ✅ Check that database has entries in the "Entries" list
- ✅ Verify fish entries have valid data (name, sprites, etc.)

### Fish Cards Not Displaying:
- ✅ Check that FishCardPrefab is assigned
- ✅ Verify prefab has Image and Text components
- ✅ Check that SharedFishCardUI component can find UI elements
- ✅ Verify ScrollView Content is assigned correctly

### Items Spawning Outside ScrollView Box (Fish Stats / Recent Catches):
**This is a common issue! Follow these steps to fix:**

1. **Fix the ScrollView Content (Most Important!):**
   - Select the **Content** GameObject inside your ScrollView (not the ScrollView itself)
   - In Inspector, check RectTransform:
     - **Anchor Preset:** Should be **Top-Left** (not Stretch-Stretch)
     - **Pivot:** Should be at **Top-Left** (0, 1)
     - **Position:** X: `0`, Y: `0` (top-left corner)
     - **Width:** Should match ScrollView width (or use Stretch for width only)
     - **Height:** Start with a small value like `100` (will auto-expand with Content Size Fitter)

2. **Add Layout Components to Content:**
   - Select the **Content** GameObject
   - Add Component → **Vertical Layout Group**
   - Configure:
     - **Child Force Expand:** 
       - ✅ Width: Checked
       - ❌ Height: Unchecked
     - **Child Control Size:**
       - ✅ Width: Checked
       - ✅ Height: Checked
     - **Spacing:** `10` (or your preferred spacing)
     - **Padding:** Left `5`, Right `5`, Top `5`, Bottom `5`
     - **Child Alignment:** Upper Center

3. **Add Content Size Fitter:**
   - Still on the **Content** GameObject
   - Add Component → **Content Size Fitter**
   - Configure:
     - **Horizontal Fit:** Unfit
     - **Vertical Fit:** Preferred Size (this makes it grow with content)

4. **Fix Your Prefab Settings:**
   - Open your prefab (Fish Stat Item or Recent Catch Item)
   - Select the root GameObject of the prefab
   - **RectTransform Settings:**
     - **Anchor Preset:** **Top-Left** (click anchor preset, choose top-left)
     - **Pivot:** **Top-Left** (0, 1)
     - **Position:** X: `0`, Y: `0` (will be set automatically by layout)
     - **Width:** Set to match ScrollView width (e.g., `490` if ScrollView is 500px wide)
     - **Height:** Set appropriate height (e.g., `60-80` pixels)
   - **Important:** Make sure the prefab has a **RectTransform** (not Transform)

5. **Verify ScrollView Viewport:**
   - Select the **Viewport** GameObject (inside ScrollView)
   - Check that it has:
     - **Mask Component** (should be added automatically)
     - **Image Component** (can be transparent, but should exist)
   - **RectTransform:**
     - Should be **Stretch-Stretch** to fill ScrollView

6. **Quick Test:**
   - After making these changes, play the game
   - Items should now appear inside the ScrollView box
   - If items still appear outside, check:
     - Is the Content GameObject assigned correctly in the controller?
     - Are you assigning the Content (inside ScrollView) or the ScrollView itself?
     - The controller should use: `ScrollView → Viewport → Content`

**Visual Guide:**
```
ScrollView (Stretch-Stretch)
  └── Viewport (Stretch-Stretch, has Mask)
      └── Content (Top-Left anchor, has Vertical Layout Group + Content Size Fitter) ← ASSIGN THIS!
          ├── Item 1 (Top-Left anchor, fixed width)
          ├── Item 2 (Top-Left anchor, fixed width)
          └── Item 3 (Top-Left anchor, fixed width)
```

**Common Mistakes:**
- ❌ Assigning the ScrollView instead of the Content
- ❌ Content has Stretch-Stretch anchor (should be Top-Left)
- ❌ Prefab has wrong anchor (should be Top-Left)
- ❌ Missing Vertical Layout Group on Content
- ❌ Missing Content Size Fitter on Content
- ❌ Prefab size is too large or has wrong dimensions

---

## File Locations Summary

**New Scripts Created:**
- `Assets/Minigames/freshwaterfishing/Scripts/FreshwaterFishingScoreManager.cs`
- `Assets/Minigames/freshwaterfishing/Scripts/FreshwaterFishingScorePanelController.cs`
- `Assets/Minigames/Common/FishCatalog/SharedFishLibraryController.cs`

**Updated Scripts:**
- `Assets/Minigames/freshwaterfishing/Scripts/FishingMinigame.cs` (added score recording)

**Save File Location:**
- Scores are saved to: `%USERPROFILE%\AppData\LocalLow\[CompanyName]\[GameName]\freshwater_fishing_score_data.json`

---

## Next Steps

1. **Customize UI:** Style the score panel and fish library to match your game's aesthetic
2. **Add More Stats:** Add additional statistics tracking if needed
3. **Link to Achievements:** Connect score milestones to your achievement system
4. **Add Filters:** Add filters to the fish library (by rarity, habitat, etc.)
5. **Add Search:** Implement search functionality in the fish library

---

## UI Positioning Quick Reference

### Common Anchor Presets (Unity UI)

**How to Use Anchor Presets:**
1. Select your UI element
2. In RectTransform, click the Anchor Preset box (top-left)
3. Hold **Alt** to also set position, **Shift** to set size

**Most Common Presets:**
- **Top-Left**: Element stays at top-left corner
- **Top-Stretch**: Element spans width, anchored to top
- **Stretch-Stretch**: Element fills parent (for panels)
- **Middle-Center**: Element centered in parent
- **Bottom-Stretch**: Element spans width, anchored to bottom

### Positioning Tips

1. **Use Margins Instead of Fixed Positions:**
   - For responsive design, use Left/Right/Top/Bottom values
   - Example: Left `50`, Right `50` creates 50px margins on both sides

2. **Layout Groups for Automatic Spacing:**
   - **Vertical Layout Group**: Stacks elements vertically with spacing
   - **Horizontal Layout Group**: Stacks elements horizontally
   - **Grid Layout Group**: Creates a grid (perfect for fish cards)
   - Set "Spacing" for gaps between elements

3. **Content Size Fitter:**
   - Use when you want UI to auto-resize based on content
   - Example: ScrollView Content should use "Preferred Size" for height

4. **Recommended Spacing:**
   - Between text elements: `15-20` pixels
   - Between cards: `15-20` pixels
   - Panel margins: `20-50` pixels from screen edges
   - Button size: Minimum `100x40` for easy clicking

5. **Font Sizes (Recommended):**
   - Main Title: `32-40`
   - Section Titles: `24-28`
   - Body Text: `18-22`
   - Small Text/Details: `14-16`
   - Labels: `16-18`

6. **Color Scheme Suggestions:**
   - Background Panel: Dark with transparency (RGBA: 0, 0, 0, 200-240)
   - Text: White or light color for contrast
   - Buttons: Your theme color with hover states
   - Cards: White or light background with subtle shadow

### Screen Size Considerations

**For 1920x1080 (Full HD):**
- Score Panel: Width `900-1000`, Height `700-800`
- Fish Cards: `150x200` per card, 4 columns
- Text sizes as recommended above

**For 1280x720 (HD):**
- Score Panel: Width `700-800`, Height `550-650`
- Fish Cards: `120x170` per card, 3 columns
- Reduce font sizes by 2-4 points

**For Mobile/Tablet:**
- Use 2-3 columns for fish cards
- Increase button sizes to `120x50` minimum
- Use larger fonts for readability
- Add more padding/margins

---

## Questions or Issues?

If you encounter any problems:
1. Check the Console for error messages
2. Verify all references are assigned in Inspector
3. Ensure scripts compile without errors
4. Test each component individually before integrating
5. Check RectTransform anchor settings if elements don't position correctly

**Common Positioning Issues:**
- **Element not visible**: Check if it's outside the parent's bounds
- **Element moves when resizing**: Wrong anchor preset - use Stretch presets for flexible layouts
- **Text cut off**: Increase RectTransform height or enable "Auto Size" in TextMeshPro
- **Cards overlapping**: Check Grid Layout Group spacing and cell size settings

Good luck! 🎣

