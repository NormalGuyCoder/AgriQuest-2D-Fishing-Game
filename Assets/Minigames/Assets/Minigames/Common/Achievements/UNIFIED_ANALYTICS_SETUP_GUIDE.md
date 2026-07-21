# Unified Analytics System Setup Guide

This guide explains how to set up the unified analytics system that tracks and evaluates player performance across all three minigames:
1. **Deboning Minigame** (ScoreManager)
2. **Saltwater Fishing** (FishingMinigame)
3. **Freshwater Fishing** (FreshwaterFishingScoreManager)

---

## Overview

The unified analytics system:
- ✅ Tracks separate score databases for each minigame
- ✅ Aggregates data from all three minigames
- ✅ Evaluates overall player performance
- ✅ Determines if player did a "good job" based on multiple criteria
- ✅ Provides recommendations and feedback
- ✅ Contributes to achievements and analytics

---

## System Components

### 1. UnifiedAnalyticsManager
- Aggregates data from all three minigames
- Calculates overall performance scores
- Generates comprehensive reports

### 2. PlayerPerformanceEvaluator
- Evaluates player performance across all minigames
- Determines if player did a "good job"
- Identifies strengths, weaknesses, and improvement areas
- Calculates letter grades

### 3. UnifiedAnalyticsDashboard
- UI controller for displaying comprehensive analytics
- Shows breakdown by minigame
- Displays performance evaluation

---

## Setup Steps

### Step 1: Create Manager GameObjects

In your main scene or a persistent scene:

1. **Create UnifiedAnalyticsManager:**
   - Create Empty GameObject: `UnifiedAnalyticsManager`
   - Add Component → `UnifiedAnalyticsManager`
   - Configure weights (default: 0.33 each for balanced evaluation)
   - Set `goodJobScoreThreshold` (default: 70)

2. **Create PlayerPerformanceEvaluator:**
   - Create Empty GameObject: `PlayerPerformanceEvaluator`
   - Add Component → `PlayerPerformanceEvaluator`
   - Configure thresholds:
     - `goodJobThreshold`: 70 (minimum overall score)
     - `minSustainabilityScore`: 60 (minimum sustainability)
     - `maxBadDecisions`: 5 (maximum allowed bad decisions)
     - `minGoodDecisions`: 10 (minimum required good decisions)

### Step 2: Verify Minigame Integration

**All three minigames should already be reporting to the system:**

1. **Deboning Minigame:**
   - ✅ Already reports to `ScoreManager` (separate database)
   - ✅ Already reports to `SustainableFishingMetrics` via `GameManager.cs`
   - ✅ Already reports to `FishInventoryManager`

2. **Saltwater Fishing:**
   - ✅ Already reports to `SustainableFishingMetrics` via `FishingLineController.cs`
   - ✅ Already reports to `FishInventoryManager`

3. **Freshwater Fishing:**
   - ✅ Reports to `FreshwaterFishingScoreManager` (separate database)
   - ✅ Now reports to `SustainableFishingMetrics` (updated in `FishingMinigame.cs`)
   - ✅ Reports to `FishInventoryManager`

**Verify Integration:**
- Play each minigame and catch fish
- Check Console for messages like:
  - `"Freshwater Fishing: Caught [FishName]..."`
  - `"Score added: [Score] points from [FishName]..."`

### Step 3: Create Analytics Dashboard UI

1. **Create Main Dashboard Panel:**
   - In your Canvas, create a Panel: `AnalyticsDashboardPanel`
   - Set to inactive initially

2. **Create Three Sub-Panels:**
   - `OverviewPanel` - Overall performance summary
   - `MinigameBreakdownPanel` - Performance by minigame
   - `PerformanceEvaluationPanel` - Detailed evaluation

3. **Overview Panel UI Elements:**
   - `OverallScoreText` - TextMeshProUGUI: "Overall Performance: XX%"
   - `OverallGradeText` - TextMeshProUGUI: "Grade: A+"
   - `DidGoodJobText` - TextMeshProUGUI: "✓ Good Job!" or "⚠ Needs Improvement"
   - `OverallScoreFill` - Image (fill type) for progress bar
   - `SummaryText` - TextMeshProUGUI for feedback summary

4. **Minigame Breakdown Panel UI Elements:**
   - `DeboningScoreText` - TextMeshProUGUI
   - `SaltwaterScoreText` - TextMeshProUGUI
   - `FreshwaterScoreText` - TextMeshProUGUI
   - `DeboningScoreFill` - Image (fill type)
   - `SaltwaterScoreFill` - Image (fill type)
   - `FreshwaterScoreFill` - Image (fill type)

5. **Performance Evaluation Panel UI Elements:**
   - `SkillScoreText` - TextMeshProUGUI
   - `DecisionScoreText` - TextMeshProUGUI
   - `DiversityScoreText` - TextMeshProUGUI
   - `SustainabilityScoreText` - TextMeshProUGUI
   - `FeedbackText` - TextMeshProUGUI (multi-line)
   - `StrengthsContainer` - Transform (ScrollView Content)
   - `WeaknessesContainer` - Transform (ScrollView Content)
   - `ImprovementAreasContainer` - Transform (ScrollView Content)
   - `RecommendationsContainer` - Transform (ScrollView Content)
   - `RecommendationsTitleText` - TextMeshProUGUI
   - `ListItemPrefab` - Prefab for list items (simple TextMeshProUGUI)

6. **Navigation Buttons:**
   - `OverviewButton` - Button to show overview
   - `BreakdownButton` - Button to show minigame breakdown
   - `EvaluationButton` - Button to show performance evaluation
   - `RefreshButton` - Button to refresh data

### Step 4: Set Up UnifiedAnalyticsDashboard Controller

1. **Add Controller:**
   - Select `AnalyticsDashboardPanel`
   - Add Component → `UnifiedAnalyticsDashboard`

2. **Assign All References:**
   - **Main Panels:** Assign the three sub-panels
   - **Overview UI:** Assign all overview text and image elements
   - **Minigame Breakdown UI:** Assign all breakdown text and fill images
   - **Performance Evaluation UI:** Assign all evaluation text elements
   - **Containers:** Assign all container transforms
   - **Navigation:** Assign all buttons
   - **Prefab:** Assign `ListItemPrefab`

3. **Test:**
   - Play the game and catch fish in all three minigames
   - Open the analytics dashboard
   - Click "Refresh" button
   - Verify data displays correctly

---

## How It Works

### Score Databases (Separate)

Each minigame maintains its own score database:

1. **Deboning:**
   - Database: `ScoreManager` (saves to `score_data.json`)
   - Tracks: Total score, games played, mistakes, fish statistics

2. **Saltwater Fishing:**
   - Database: `FishInventoryManager` (tracks catches)
   - Tracks: Total catches, species diversity

3. **Freshwater Fishing:**
   - Database: `FreshwaterFishingScoreManager` (saves to `freshwater_fishing_score_data.json`)
   - Tracks: Total score, games played, fish caught, catch times

### Unified Analytics (Combined)

The `UnifiedAnalyticsManager` aggregates data from all three:

- **Performance Scores:** Calculates 0-100 score for each minigame
- **Overall Performance:** Weighted average across all minigames
- **Sustainability:** Uses `SustainableFishingMetrics` (shared across all)

### Performance Evaluation

The `PlayerPerformanceEvaluator` determines if player did "good job" based on:

1. **Overall Score:** Must be ≥ 70% (configurable)
2. **Sustainability Score:** Must be ≥ 60% (configurable)
3. **Good Decisions:** Must have ≥ 10 good decisions
4. **Bad Decisions:** Must have ≤ 5 bad decisions
5. **Critical Issues:** No critical bad decisions allowed

### Achievement Integration

All three minigames contribute to achievements:

- **Total Catches:** Sum of catches from all minigames
- **Species Diversity:** Combined unique species across all minigames
- **Sustainability Score:** Shared across all minigames
- **Good/Bad Decisions:** Tracked across all minigames

---

## Usage in Code

### Get Comprehensive Report

```csharp
if (UnifiedAnalyticsManager.Instance != null)
{
    UnifiedAnalyticsReport report = UnifiedAnalyticsManager.Instance.GetComprehensiveReport();
    
    Debug.Log($"Overall Performance: {report.overallPerformance}%");
    Debug.Log($"Did Good Job: {report.didGoodJob}");
    Debug.Log($"Deboning Score: {report.deboningData.performanceScore}%");
    Debug.Log($"Saltwater Score: {report.saltwaterData.performanceScore}%");
    Debug.Log($"Freshwater Score: {report.freshwaterData.performanceScore}%");
}
```

### Evaluate Player Performance

```csharp
if (PlayerPerformanceEvaluator.Instance != null)
{
    PlayerPerformanceEvaluation evaluation = 
        PlayerPerformanceEvaluator.Instance.EvaluatePlayer();
    
    Debug.Log($"Overall Score: {evaluation.overallScore}%");
    Debug.Log($"Grade: {evaluation.grade}");
    Debug.Log($"Did Good Job: {evaluation.didGoodJob}");
    Debug.Log($"Feedback: {evaluation.feedback}");
    
    foreach (string strength in evaluation.strengths)
    {
        Debug.Log($"Strength: {strength}");
    }
}
```

### Get Performance Breakdown

```csharp
if (UnifiedAnalyticsManager.Instance != null)
{
    Dictionary<string, float> breakdown = 
        UnifiedAnalyticsManager.Instance.GetPerformanceBreakdown();
    
    foreach (var kvp in breakdown)
    {
        Debug.Log($"{kvp.Key}: {kvp.Value}%");
    }
}
```

---

## Data Flow

```
┌─────────────────┐
│ Deboning Game   │ → ScoreManager (separate DB)
│                 │ → SustainableFishingMetrics (shared)
│                 │ → FishInventoryManager (shared)
└─────────────────┘
         │
         ├─────────────────────────────────┐
         │                                 │
┌─────────────────┐              ┌─────────────────┐
│ Saltwater Game  │              │ Freshwater Game │
│                 │              │                 │
│                 │ → FishInventoryManager         │ → FreshwaterFishingScoreManager (separate DB)
│                 │ → SustainableFishingMetrics    │ → SustainableFishingMetrics (shared)
│                 │                                │ → FishInventoryManager (shared)
└─────────────────┘              └─────────────────┘
         │                                 │
         └─────────────────────────────────┘
                        │
                        ▼
         ┌──────────────────────────────┐
         │ UnifiedAnalyticsManager      │
         │ - Aggregates all data        │
         │ - Calculates overall scores  │
         └──────────────────────────────┘
                        │
                        ▼
         ┌──────────────────────────────┐
         │ PlayerPerformanceEvaluator   │
         │ - Evaluates performance      │
         │ - Determines "good job"      │
         │ - Generates feedback         │
         └──────────────────────────────┘
                        │
                        ▼
         ┌──────────────────────────────┐
         │ AchievementSystem            │
         │ - Unlocks achievements       │
         │ - Tracks progress            │
         └──────────────────────────────┘
```

---

## Troubleshooting

### No Data Showing:
- ✅ Verify all three manager GameObjects exist in scene
- ✅ Check that minigames are recording catches
- ✅ Click "Refresh" button in dashboard
- ✅ Check Console for errors

### Scores Are Zero:
- ✅ Play each minigame and catch fish
- ✅ Verify ScoreManager, FreshwaterFishingScoreManager exist
- ✅ Check that SustainableFishingMetrics is recording

### "Did Good Job" Always False:
- ✅ Check thresholds in PlayerPerformanceEvaluator
- ✅ Verify sustainability score is above minimum
- ✅ Check that good decisions > bad decisions
- ✅ Ensure no critical bad decisions

### Missing Minigame Data:
- ✅ Verify the minigame is reporting to SustainableFishingMetrics
- ✅ Check FishInventoryManager for catch records
- ✅ Ensure the minigame's score manager exists

---

## File Locations

**New Scripts:**
- `Assets/Minigames/Common/Achievements/UnifiedAnalyticsManager.cs`
- `Assets/Minigames/Common/Achievements/PlayerPerformanceEvaluator.cs`
- `Assets/Minigames/Common/Achievements/UnifiedAnalyticsDashboard.cs`

**Updated Scripts:**
- `Assets/Minigames/freshwaterfishing/Scripts/FishingMinigame.cs` (added SustainableFishingMetrics reporting)

**Save Files:**
- Deboning: `score_data.json`
- Freshwater: `freshwater_fishing_score_data.json`
- Achievements: `achievements_analytics.json` (via AchievementsDataStore)

---

## Next Steps

1. **Customize Thresholds:** Adjust evaluation criteria to match your game's difficulty
2. **Add More Metrics:** Extend the system to track additional performance indicators
3. **Create Visualizations:** Add charts/graphs to show performance trends
4. **Export Reports:** Add functionality to export analytics data
5. **Link to Rewards:** Connect performance evaluation to in-game rewards

Good luck! 🎣📊

