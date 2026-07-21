# Achievements Scene UI Setup Guide
## Step-by-Step Visual Layout Instructions

This guide will walk you through creating the Achievements Scene UI with exact positioning and arrangement.

---

## Step 1: Create the Scene and Canvas

1. **Create New Scene**
   - Right-click in Project: `Assets/Minigames/Common/Achievements/Scenes/`
   - Create > Scene
   - Name it: `AchievementsScene`

2. **Set up Canvas**
   - In Hierarchy: Right-click > UI > Canvas
   - Select Canvas in Inspector:
     - Render Mode: **Screen Space - Overlay**
     - Canvas Scaler:
       - UI Scale Mode: **Scale With Screen Size**
       - Reference Resolution: **1920 x 1080**
       - Match: **0.5** (width/height)

---

## Step 2: Main Panel Structure

### Create the Root Main Panel

1. **Right-click Canvas** > UI > Panel
   - Name it: **MainPanel**
   - Inspector settings:
     - Anchor: **Stretch-Stretch** (hold Alt + Shift, click bottom-right anchor preset)
     - Left: **0**, Right: **0**, Top: **0**, Bottom: **0**
     - Color: **Dark Grey** (R:40, G:40, B:40, A:255) or leave default

---

## Step 3: Title Bar (Top Section)

1. **Right-click MainPanel** > UI > Panel
   - Name: **TitleBar**
   - Inspector:
     - Anchor: **Top-Stretch** (top anchor preset)
     - Left: **0**, Right: **0**
     - Top: **0**, Height: **100**
     - Color: **Darker Grey** (R:30, G:30, B:30)

2. **Right-click TitleBar** > UI > Text - TextMeshPro (or Text)
   - Name: **TitleText**
   - Inspector:
     - Text: **"Achievements & Analytics"**
     - Font Size: **48**
     - Alignment: **Center, Middle**
     - Color: **White**
     - Anchor: **Stretch-Stretch**
     - Left: **0**, Right: **0**, Top: **0**, Bottom: **0**

---

## Step 4: Navigation Buttons (Left Sidebar)

1. **Right-click MainPanel** > UI > Panel
   - Name: **NavigationPanel**
   - Inspector:
     - Anchor: **Left-Stretch**
     - Left: **0**, Width: **250**
     - Top: **100** (below title bar), Bottom: **60** (above back button)
     - Color: **Medium Grey** (R:50, G:50, B:50)

2. **Create Navigation Buttons** (inside NavigationPanel):

   **a. Sustainability Button**
   - Right-click NavigationPanel > UI > Button - TextMeshPro
   - Name: **SustainabilityButton**
   - Inspector:
     - Anchor: **Top-Stretch**
     - Left: **10**, Right: **10**
     - Top: **10**, Height: **60**
     - Text: **"Sustainability"**
     - Font Size: **24**

   **b. Achievements Button**
   - Right-click NavigationPanel > UI > Button - TextMeshPro
   - Name: **AchievementsButton**
   - Inspector:
     - Anchor: **Top-Stretch**
     - Left: **10**, Right: **10**
     - Top: **80** (10px gap from previous)
     - Height: **60**
     - Text: **"Achievements"**

   **c. Effectiveness Button**
   - Right-click NavigationPanel > UI > Button - TextMeshPro
   - Name: **EffectivenessButton**
   - Inspector:
     - Anchor: **Top-Stretch**
     - Left: **10**, Right: **10**
     - Top: **150** (10px gap)
     - Height: **60**
     - Text: **"Effectiveness"**

   **d. Decisions Button**
   - Right-click NavigationPanel > UI > Button - TextMeshPro
   - Name: **DecisionsButton**
   - Inspector:
     - Anchor: **Top-Stretch**
     - Left: **10**, Right: **10**
     - Top: **220** (10px gap)
     - Height: **60**
     - Text: **"Decisions"**

---

## Step 5: Content Area (Right Side - Main Content)

1. **Right-click MainPanel** > UI > Panel
   - Name: **ContentArea**
   - Inspector:
     - Anchor: **Stretch-Stretch**
     - Left: **260** (right of navigation), Right: **20**
     - Top: **100** (below title), Bottom: **60** (above back button)
     - Color: **Light Grey** (R:60, G:60, B:60)

---

## Step 6: Sustainability Panel (Default Active Panel)

1. **Right-click ContentArea** > UI > Panel
   - Name: **SustainabilityPanel**
   - Inspector:
     - Anchor: **Stretch-Stretch**
     - Left: **20**, Right: **20**, Top: **20**, Bottom: **20**
     - Color: **Slightly Lighter Grey** (R:70, G:70, B:70)
     - **Set Active: TRUE** (this is the default panel)

2. **Inside SustainabilityPanel, create:**

   **a. Title Text**
   - Right-click SustainabilityPanel > UI > Text
   - Name: **SustainabilityTitleText**
   - Text: **"Sustainability Metrics"**
   - Font Size: **36**
   - Anchor: **Top-Left**
   - Pos X: **20**, Pos Y: **-20**, Width: **400**, Height: **50**

   **b. Score Display (Top Left)**
   - Right-click SustainabilityPanel > UI > Panel
   - Name: **ScorePanel**
   - Anchor: **Top-Left**
   - Pos X: **20**, Pos Y: **-80**
   - Width: **300**, Height: **150**
   - Color: **Dark Blue** (R:30, G:50, B:80)

   - Inside ScorePanel:
     - **Right-click ScorePanel** > UI > Text
     - Name: **SustainabilityScoreText**
     - Text: **"Sustainability Score: 0/100"**
     - Font Size: **28**, Bold
     - Anchor: **Top-Left**
     - Pos X: **10**, Pos Y: **-10**, Width: **280**, Height: **40**

     - **Right-click ScorePanel** > UI > Image
     - Name: **SustainabilityScoreFill**
     - Image Type: **Filled**
     - Fill Method: **Horizontal**
     - Fill Amount: **0**
     - Color: **Green** (R:50, G:200, B:50)
     - Anchor: **Stretch-Stretch**
     - Left: **10**, Right: **10**, Top: **60**, Bottom: **10**

   **c. Stats Grid (Right of Score)**
   - Right-click SustainabilityPanel > UI > Panel
   - Name: **StatsGridPanel**
   - Anchor: **Top-Left**
   - Pos X: **340** (right of score panel), Pos Y: **-80**
   - Width: **400**, Height: **150**

   - Inside StatsGridPanel, create 4 Text elements (stacked vertically):
     
     **Total Catches Text**
     - Right-click StatsGridPanel > UI > Text
     - Name: **TotalCatchesText**
     - Text: **"Total Catches: 0"**
     - Font Size: **24**
     - Anchor: **Top-Left**
     - Pos X: **10**, Pos Y: **-10**, Width: **380**, Height: **30**

     **Species Diversity Text**
     - Right-click StatsGridPanel > UI > Text
     - Name: **SpeciesDiversityText**
     - Text: **"Species Diversity: 0"**
     - Font Size: **24**
     - Anchor: **Top-Left**
     - Pos X: **10**, Pos Y: **-50**, Width: **380**, Height: **30**

     **Total Sessions Text**
     - Right-click StatsGridPanel > UI > Text
     - Name: **TotalSessionsText**
     - Text: **"Total Sessions: 0"**
     - Font Size: **24**
     - Anchor: **Top-Left**
     - Pos X: **10**, Pos Y: **-90**, Width: **380**, Height: **30**

     **Play Time Text**
     - Right-click StatsGridPanel > UI > Text
     - Name: **PlayTimeText**
     - Text: **"Play Time: 0.0 hours"**
     - Font Size: **24**
     - Anchor: **Top-Left**
     - Pos X: **10**, Pos Y: **-130**, Width: **380**, Height: **30**

---

## Step 7: Achievements Panel

1. **Right-click ContentArea** > UI > Panel
   - Name: **AchievementsPanel**
   - Inspector:
     - Anchor: **Stretch-Stretch**
     - Left: **20**, Right: **20**, Top: **20**, Bottom: **20**
     - Color: **Slightly Lighter Grey** (R:70, G:70, B:70)
     - **Set Active: FALSE**

2. **Inside AchievementsPanel:**

   **a. Title**
   - Right-click AchievementsPanel > UI > Text
   - Name: **AchievementsTitleText**
   - Text: **"Achievements"**
   - Font Size: **36**
   - Anchor: **Top-Left**
   - Pos X: **20**, Pos Y: **-20**, Width: **400**, Height: **50**

   **b. Scroll View for Achievement List**
   - Right-click AchievementsPanel > UI > Scroll View
   - Name: **AchievementsScrollView**
   - Anchor: **Stretch-Stretch**
   - Left: **20**, Right: **20**, Top: **80**, Bottom: **20**

   - **Select the Content GameObject** inside ScrollView:
     - Add Component: **Vertical Layout Group**
     - Spacing: **10**
     - Padding: Left/Right: **10**, Top/Bottom: **10**
     - Child Force Expand: **Width: TRUE, Height: FALSE**
     - Child Control Size: **Height: TRUE**

   - **Rename Content to: AchievementsContainer**

---

## Step 8: Effectiveness Panel

1. **Right-click ContentArea** > UI > Panel
   - Name: **EffectivenessPanel**
   - Inspector:
     - Anchor: **Stretch-Stretch**
     - Left: **20**, Right: **20**, Top: **20**, Bottom: **20**
     - Color: **Slightly Lighter Grey**
     - **Set Active: FALSE**

2. **Inside EffectivenessPanel:**

   **a. Title**
   - Right-click EffectivenessPanel > UI > Text
   - Name: **EffectivenessTitleText**
   - Text: **"Educational Effectiveness"**
   - Font Size: **36**
   - Anchor: **Top-Left**
   - Pos X: **20**, Pos Y: **-20**, Width: **500**, Height: **50**

   **b. Scores Section (Left Side)**
   - Right-click EffectivenessPanel > UI > Panel
   - Name: **ScoresSection**
   - Anchor: **Top-Left**
   - Pos X: **20**, Pos Y: **-80**
   - Width: **500**, Height: **300**

   - Inside ScoresSection, create 4 Text elements (stacked):
     
     **Knowledge Score Text**
     - Right-click ScoresSection > UI > Text
     - Name: **KnowledgeScoreText**
     - Text: **"Knowledge Acquisition: 0/100"**
     - Font Size: **28**
     - Anchor: **Top-Left**
     - Pos X: **10**, Pos Y: **-10**, Width: **480**, Height: **50**

     **Behavioral Score Text**
     - Right-click ScoresSection > UI > Text
     - Name: **BehavioralScoreText**
     - Text: **"Behavioral Change: 0/100"**
     - Font Size: **28**
     - Anchor: **Top-Left**
     - Pos X: **10**, Pos Y: **-70**, Width: **480**, Height: **50**

     **Engagement Score Text**
     - Right-click ScoresSection > UI > Text
     - Name: **EngagementScoreText**
     - Text: **"Engagement: 0/100"**
     - Font Size: **28**
     - Anchor: **Top-Left**
     - Pos X: **10**, Pos Y: **-130**, Width: **480**, Height: **50**

     **Overall Score Text**
     - Right-click ScoresSection > UI > Text
     - Name: **OverallScoreText**
     - Text: **"Overall Effectiveness: 0/100"**
     - Font Size: **32**, Bold
     - Anchor: **Top-Left**
     - Pos X: **10**, Pos Y: **-200**, Width: **480**, Height: **60**

   **c. Recommendations Section (Right Side)**
   - Right-click EffectivenessPanel > UI > Panel
   - Name: **RecommendationsSection**
   - Anchor: **Top-Left**
   - Pos X: **540** (right of scores), Pos Y: **-80**
   - Width: **600**, Height: **400**

   - Inside RecommendationsSection:
     - **Right-click RecommendationsSection** > UI > Text
     - Name: **RecommendationsTitleText**
     - Text: **"Recommendations:"**
     - Font Size: **28**, Bold
     - Anchor: **Top-Left**
     - Pos X: **10**, Pos Y: **-10**, Width: **580**, Height: **40**

     - **Right-click RecommendationsSection** > UI > Text
     - Name: **RecommendationsText**
     - Text: **"No recommendations yet."**
     - Font Size: **20**
     - Alignment: **Upper Left**
     - Anchor: **Stretch-Stretch**
     - Left: **10**, Right: **10**, Top: **50**, Bottom: **10**

---

## Step 9: Decisions Panel

1. **Right-click ContentArea** > UI > Panel
   - Name: **DecisionsPanel**
   - Inspector:
     - Anchor: **Stretch-Stretch**
     - Left: **20**, Right: **20**, Top: **20**, Bottom: **20**
     - Color: **Slightly Lighter Grey**
     - **Set Active: FALSE**

2. **Inside DecisionsPanel:**

   **a. Good Decisions Section (Left Side)**
   - Right-click DecisionsPanel > UI > Panel
   - Name: **GoodDecisionsSection**
   - Anchor: **Top-Left**
   - Pos X: **20**, Pos Y: **-20**
   - Width: **50%** (use Right: **50%** anchor), Height: **Stretch** (Top: **-60**, Bottom: **20**)

   - Inside GoodDecisionsSection:
     - **Right-click GoodDecisionsSection** > UI > Text
     - Name: **GoodDecisionsTitle**
     - Text: **"Good Decisions ✓"**
     - Font Size: **32**, Bold, Color: **Green**
     - Anchor: **Top-Stretch**
     - Left: **10**, Right: **10**, Top: **10**, Height: **50**

     - **Right-click GoodDecisionsSection** > UI > Scroll View
     - Name: **GoodDecisionsScrollView**
     - Anchor: **Stretch-Stretch**
     - Left: **10**, Right: **10**, Top: **70**, Bottom: **10**

     - **Select Content inside ScrollView**, rename to: **GoodDecisionsContainer**
     - Add Component: **Vertical Layout Group**
     - Spacing: **5**
     - Padding: **10**

   **b. Bad Decisions Section (Right Side)**
   - Right-click DecisionsPanel > UI > Panel
   - Name: **BadDecisionsSection**
   - Anchor: **Top-Left**
   - Pos X: **50%** (use Left: **50%** anchor), Pos Y: **-20**
   - Width: **50%** (Right: **20**), Height: **Stretch** (Top: **-60**, Bottom: **20**)

   - Inside BadDecisionsSection:
     - **Right-click BadDecisionsSection** > UI > Text
     - Name: **BadDecisionsTitle**
     - Text: **"Areas for Improvement ✗"**
     - Font Size: **32**, Bold, Color: **Red**
     - Anchor: **Top-Stretch**
     - Left: **10**, Right: **10**, Top: **10**, Height: **50**

     - **Right-click BadDecisionsSection** > UI > Scroll View
     - Name: **BadDecisionsScrollView**
     - Anchor: **Stretch-Stretch**
     - Left: **10**, Right: **10**, Top: **70**, Bottom: **10**

     - **Select Content inside ScrollView**, rename to: **BadDecisionsContainer**
     - Add Component: **Vertical Layout Group**
     - Spacing: **5**
     - Padding: **10**

---

## Step 10: Back Button (Bottom)

1. **Right-click MainPanel** > UI > Button - TextMeshPro
   - Name: **BackButton**
   - Inspector:
     - Anchor: **Bottom-Stretch**
     - Left: **20**, Right: **20**
     - Bottom: **10**, Height: **50**
     - Text: **"Back to Menu"**
     - Font Size: **24**

---

## Step 11: Create Prefabs

### Decision Item Prefab

1. **Right-click in Project** > Create > UI > Panel
   - Name: **DecisionItemPrefab**
   - Inspector:
     - Width: **400**, Height: **100**
     - Color: **Light Grey** (R:80, G:80, B:80)

2. **Inside DecisionItemPrefab:**
   - **Right-click DecisionItemPrefab** > UI > Text
   - Name: **DescriptionText**
   - Text: **"Decision Description"**
   - Font Size: **20**, Bold
   - Anchor: **Top-Stretch**
   - Left: **10**, Right: **10**, Top: **5**, Height: **30**

   - **Right-click DecisionItemPrefab** > UI > Text
   - Name: **ExplanationText**
   - Text: **"Explanation text here"**
   - Font Size: **16**
   - Alignment: **Upper Left**
   - Anchor: **Stretch-Stretch**
   - Left: **10**, Right: **10**, Top: **35**, Bottom: **5**

3. **Drag DecisionItemPrefab to Prefabs folder**
   - Delete from Hierarchy (keep prefab)

### Achievement Item Prefab

1. **Right-click in Project** > Create > UI > Panel
   - Name: **AchievementItemPrefab**
   - Inspector:
     - Width: **600**, Height: **120**

2. **Inside AchievementItemPrefab:**
   - **Right-click AchievementItemPrefab** > UI > Image
   - Name: **IconImage**
   - Width: **100**, Height: **100**
   - Anchor: **Left-Stretch**
   - Pos X: **10**, Top: **10**, Bottom: **10**

   - **Right-click AchievementItemPrefab** > UI > Text
   - Name: **TitleText**
   - Text: **"Achievement Title"**
   - Font Size: **24**, Bold
   - Anchor: **Top-Left**
   - Pos X: **120**, Pos Y: **-10**, Width: **470**, Height: **40**

    - **Right-click AchievementItemPrefab** > UI > Text
    - Name: **DescriptionText**
    - Text: **"Achievement description"**
    - Font Size: **18**
    - Anchor: **Top-Left**
    - Pos X: **120**, Pos Y: **-50**, Width: **470**, Height: **40**

   - **Right-click AchievementItemPrefab** > UI > Text
   - Name: **ProgressText**
   - Text: **"Progress: 0%"**
   - Font Size: **16**
   - Anchor: **Top-Left**
   - Pos X: **120**, Pos Y: **-90**, Width: **470**, Height: **30**

3. **Drag AchievementItemPrefab to Prefabs folder**
   - Delete from Hierarchy (keep prefab)

---

## Step 12: Connect Script to UI

1. **Create Empty GameObject** in Hierarchy
   - Name: **AchievementsSceneController**

2. **Add Component**: `AchievementsSceneController`

3. **Assign all references in Inspector:**
   - **Main Panel**: Drag `MainPanel`
   - **Sustainability Panel**: Drag `SustainabilityPanel`
   - **Achievements Panel**: Drag `AchievementsPanel`
   - **Effectiveness Panel**: Drag `EffectivenessPanel`
   - **Decisions Panel**: Drag `DecisionsPanel`
   - **Sustainability Score Text**: Drag `SustainabilityScoreText`
   - **Total Catches Text**: Drag `TotalCatchesText`
   - **Species Diversity Text**: Drag `SpeciesDiversityText`
   - **Total Sessions Text**: Drag `TotalSessionsText`
   - **Play Time Text**: Drag `PlayTimeText`
   - **Sustainability Score Fill**: Drag `SustainabilityScoreFill`
   - **Good Decisions Container**: Drag `GoodDecisionsContainer`
   - **Bad Decisions Container**: Drag `BadDecisionsContainer`
   - **Decision Item Prefab**: Drag `DecisionItemPrefab` from Prefabs folder
   - **Achievements Container**: Drag `AchievementsContainer`
   - **Achievement Item Prefab**: Drag `AchievementItemPrefab` from Prefabs folder
   - **Knowledge Score Text**: Drag `KnowledgeScoreText`
   - **Behavioral Score Text**: Drag `BehavioralScoreText`
   - **Engagement Score Text**: Drag `EngagementScoreText`
   - **Overall Score Text**: Drag `OverallScoreText`
   - **Recommendations Text**: Drag `RecommendationsText`
   - **All Buttons**: Drag respective buttons
   - **Back Button**: Drag `BackButton`

---

## Visual Layout Summary

```
┌─────────────────────────────────────────────────────────┐
│  Title Bar (Top, Full Width, Height: 100)              │
│  "Achievements & Analytics"                            │
├──────────┬──────────────────────────────────────────────┤
│          │                                              │
│  Nav     │  Content Area (Main Panels Here)            │
│  Panel   │                                              │
│  (Left)  │  ┌──────────────────────────────────────┐   │
│  Width:  │  │ Sustainability Panel (Default)      │   │
│  250     │  │ - Score Display (Top Left)           │   │
│          │  │ - Stats Grid (Top Right)             │   │
│  Buttons:│  └──────────────────────────────────────┘   │
│  - Sust  │                                              │
│  - Achiev│  ┌──────────────────────────────────────┐   │
│  - Effect│  │ Achievements Panel                   │   │
│  - Decis │  │ - Scroll View with Achievement List │   │
│          │  └──────────────────────────────────────┘   │
│          │                                              │
│          │  ┌──────────────────────────────────────┐   │
│          │  │ Effectiveness Panel                  │   │
│          │  │ - Scores (Left)                      │   │
│          │  │ - Recommendations (Right)            │   │
│          │  └──────────────────────────────────────┘   │
│          │                                              │
│          │  ┌──────────────┬──────────────┐            │
│          │  │ Good         │ Bad          │            │
│          │  │ Decisions    │ Decisions    │            │
│          │  │ (Left 50%)   │ (Right 50%)  │            │
│          │  └──────────────┴──────────────┘            │
│          │                                              │
├──────────┴──────────────────────────────────────────────┤
│  Back Button (Bottom, Full Width, Height: 50)          │
└─────────────────────────────────────────────────────────┘
```

---

## Color Scheme Suggestions

- **Main Background**: Dark Grey (R:40, G:40, B:40)
- **Navigation Panel**: Medium Grey (R:50, G:50, B:50)
- **Content Panels**: Light Grey (R:70, G:70, B:70)
- **Good Decisions**: Green tint (R:50, G:150, B:50, A:100)
- **Bad Decisions**: Red tint (R:150, G:50, B:50, A:100)
- **Score Fill**: Green (R:50, G:200, B:50)
- **Text**: White or Light Grey

---

## Tips

1. **Use Anchors**: Always set anchors properly for responsive design
2. **Spacing**: Keep consistent spacing (10-20px between elements)
3. **Font Sizes**: 
   - Titles: 32-36
   - Headers: 24-28
   - Body: 18-20
   - Small: 14-16
4. **Test Responsiveness**: Change Game view resolution to test different screen sizes
5. **Scroll Views**: Always add Content Size Fitter to content for proper scrolling

---

## Final Checklist

- [ ] Canvas created with proper scaling
- [ ] Main panel covers full screen
- [ ] Title bar at top
- [ ] Navigation panel on left with 4 buttons
- [ ] Content area on right
- [ ] All 4 content panels created (only Sustainability active)
- [ ] All UI elements assigned to script
- [ ] Prefabs created and assigned
- [ ] Back button at bottom
- [ ] Script component added and all references assigned

Once complete, your Achievements Scene should be fully functional!

