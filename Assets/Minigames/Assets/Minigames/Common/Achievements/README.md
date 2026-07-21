# Achievements & Educational Analytics System

A comprehensive system for tracking player performance, sustainable fishing practices, and educational effectiveness across all minigames.

## Overview

This system evaluates:
- **Sustainable Fishing Practices**: Tracks overfishing, endangered species catches, and diversity
- **Player Decisions**: Records good and bad choices with explanations
- **Educational Effectiveness**: Calculates learning outcomes and knowledge acquisition
- **Achievements**: Unlocks based on sustainable practices and learning milestones

## Step-by-Step Setup Guide

### Step 1: Create the Manager Objects

1. **Create Sustainable Fishing Metrics Manager**
   - In your main scene (or a persistent scene), create an empty GameObject: `SustainableFishingMetrics`
   - Add the `SustainableFishingMetrics` component
   - Configure settings:
     - `Max Catches Per Species`: 10 (default)
     - `Min Species Diversity`: 5 (default)
     - `Max Catches Per Session`: 20 (default)

2. **Create Achievement System Manager**
   - Create an empty GameObject: `AchievementSystem`
   - Add the `AchievementSystem` component
   - Leave `All Achievements` list empty for now (we'll populate it in Step 3)

3. **Create Educational Effectiveness Calculator**
   - Create an empty GameObject: `EducationalEffectivenessCalculator`
   - Add the `EducationalEffectivenessCalculator` component
   - No configuration needed

### Step 2: Create Achievement Definitions

1. **Right-click in Project window** → `Create > Achievements > Achievement Definition`

2. **Create these achievements:**

   **a. "First Catch"**
   - Achievement ID: `first_catch`
   - Title: "First Catch"
   - Description: "Caught your first fish!"
   - Type: `Total Catches`
   - Required Value: `1`

   **b. "Diversity Explorer"**
   - Achievement ID: `diversity_explorer`
   - Title: "Diversity Explorer"
   - Description: "Caught 5 different species"
   - Type: `Species Diversity`
   - Required Value: `5`

   **c. "Sustainable Angler"**
   - Achievement ID: `sustainable_angler`
   - Title: "Sustainable Angler"
   - Description: "Achieved 80+ sustainability score"
   - Type: `Sustainability Score`
   - Required Value: `80`

   **d. "No Overfishing"**
   - Achievement ID: `no_overfishing`
   - Title: "Conservationist"
   - Description: "Caught 10+ fish without overfishing any species"
   - Type: `No Overfishing`
   - Required Value: `10`

   **e. "Endangered Protector"**
   - Achievement ID: `endangered_protector`
   - Title: "Endangered Protector"
   - Description: "Never caught an endangered species"
   - Type: `No Endangered Catch`
   - Required Value: `5` (minimum catches to qualify)

   **f. "Learning Master"**
   - Achievement ID: `learning_master`
   - Title: "Learning Master"
   - Description: "Completed 5 learning activities"
   - Type: `Learning Complete`
   - Required Value: `5`

   **g. "Conservation Master"**  
   - Achievement ID: `conservation_master`
   - Title: "Conservation Master"
   - Description: "Perfect sustainability score with no violations"
   - Type: `Conservation Master`
   - Required Value: `0` (not used, but required)

3. **Assign achievements to AchievementSystem**
   - Select the `AchievementSystem` GameObject
   - In the Inspector, expand `All Achievements` list
   - Set size to match number of achievements
   - Drag each achievement asset into the list

### Step 3: Mark Endangered Species

1. **Open each Fish Catalog Entry** that represents an endangered species
2. **Check the `Is Endangered` checkbox**
3. **Save the asset**

### Step 4: Create the Achievements Scene

1. **Create a new scene**: `Assets/Minigames/Common/Achievements/Scenes/AchievementsScene.unity`

2. **Set up Canvas**:
   - Create Canvas (UI > Canvas)
   - Set to "Screen Space - Overlay"

3. **Create Main Panel Structure**:
   ```
   Canvas
   ├── MainPanel (Panel)
   │   ├── TitleText (Text): "Achievements & Analytics"
   │   ├── NavigationButtons (Panel)
   │   │   ├── SustainabilityButton (Button): "Sustainability"
   │   │   ├── AchievementsButton (Button): "Achievements"
   │   │   ├── EffectivenessButton (Button): "Effectiveness"
   │   │   └── DecisionsButton (Button): "Decisions"
   │   ├── SustainabilityPanel (Panel) - Initially Active
   │   ├── AchievementsPanel (Panel) - Initially Inactive
   │   ├── EffectivenessPanel (Panel) - Initially Inactive
   │   └── DecisionsPanel (Panel) - Initially Inactive
   └── BackButton (Button): "Back to Menu"
   ```

4. **Set up Sustainability Panel**:
   ```
   SustainabilityPanel
   ├── SustainabilityScoreText (Text)
   ├── SustainabilityScoreFill (Image, Type: Filled)
   ├── TotalCatchesText (Text)
   ├── SpeciesDiversityText (Text)
   ├── TotalSessionsText (Text)
   └── PlayTimeText (Text)
   ```

5. **Set up Decisions Panel**:
   ```
   DecisionsPanel
   ├── GoodDecisionsTitle (Text): "Good Decisions"
   ├── GoodDecisionsContainer (Vertical Layout Group)
   ├── BadDecisionsTitle (Text): "Areas for Improvement"
   └── BadDecisionsContainer (Vertical Layout Group)
   ```

6. **Set up Achievements Panel**:
   ```
   AchievementsPanel
   ├── AchievementsTitle (Text): "Achievements"
   └── AchievementsContainer (Vertical Layout Group or Grid Layout Group)
   ```

7. **Set up Effectiveness Panel**:
   ```
   EffectivenessPanel
   ├── KnowledgeScoreText (Text)
   ├── BehavioralScoreText (Text)
   ├── EngagementScoreText (Text)
   ├── OverallScoreText (Text)
   └── RecommendationsText (Text, Multi-line)
   ```

8. **Create Prefabs**:
   - **Decision Item Prefab**:
     - Create Panel with Image (for background color)
     - Add 2-3 Text children:
       - Text 0: Decision description
       - Text 1: Explanation
       - Text 2: Severity (optional)
   
   - **Achievement Item Prefab**:
     - Create Panel with Image
     - Add Image child for icon
     - Add 2-3 Text children:
       - Text 0: Achievement title
       - Text 1: Description
       - Text 2: Progress (for locked achievements)

9. **Add AchievementsSceneController**:
   - Create empty GameObject: `AchievementsSceneController`
   - Add `AchievementsSceneController` component
   - Assign all UI references in the Inspector:
     - Drag panels, buttons, text fields, containers, and prefabs to their respective fields

### Step 5: Integrate with Existing Systems

The system is already integrated! The following scripts automatically record data:

- **Deboning Minigame**: Records completion and mistakes
- **Saltwater Fishing**: Records catches with endangered status
- **Freshwater Fishing**: Records catches with endangered status

### Step 6: Test the System

1. **Play each minigame** and catch/complete fish
2. **Navigate to Achievements Scene**
3. **Check that data appears**:
   - Sustainability score updates
   - Decisions are recorded
   - Achievements unlock
   - Effectiveness scores calculate

### Step 7: Add Navigation to Achievements Scene

In your main menu or game hub:

1. **Create a button**: "View Achievements" or "Analytics"
2. **Add OnClick listener**:
   ```csharp
   UnityEngine.SceneManagement.SceneManager.LoadScene("AchievementsScene");
   ```

## Understanding the Metrics

### Sustainability Score (0-100)
- **100**: Perfect sustainable fishing
- **80-99**: Good practices
- **60-79**: Needs improvement
- **Below 60**: Poor practices

**Deductions:**
- -2 points per excess catch over limit
- -10 points per critical bad decision
- -5 points per high severity bad decision

**Bonuses:**
- +1 point per good decision
- +5 points for species diversity

### Educational Effectiveness Scores

**Knowledge Acquisition (0-100)**
- Measures learning activities completed
- Understanding of sustainability concepts
- Avoidance of bad practices

**Behavioral Change (0-100)**
- Actual fishing practices
- Overfishing avoidance
- Endangered species protection

**Engagement (0-100)**
- Play time
- Number of sessions
- Activity level

**Overall Effectiveness**
- Weighted average: 40% Knowledge + 40% Behavioral + 20% Engagement

## Customization

### Adjust Sustainability Thresholds

In `SustainableFishingMetrics`:
- `Max Catches Per Species`: How many of one species before overfishing
- `Min Species Diversity`: Minimum different species for good diversity
- `Max Catches Per Session`: Maximum catches per session before warning

### Create Custom Achievements

1. Create new `Achievement Definition` asset
2. Set type, requirements, and rewards
3. Add to `AchievementSystem.All Achievements` list

### Customize Decision Evaluation

Modify `SustainableFishingMetrics.EvaluateCatch()` to add custom decision criteria.

## Data Persistence

All data is saved to PlayerPrefs:
- Total catches, sessions, play time
- Unlocked achievements
- Sustainability metrics

Data persists between game sessions automatically.

## Research & Evaluation

The `EducationalEffectivenessCalculator` provides metrics suitable for:
- Pre/post assessments
- Learning outcome evaluation
- Game effectiveness studies
- Student progress tracking

Export data by accessing:
- `SustainabilityReport` from `SustainableFishingMetrics.Instance.GetReport()`
- `EducationalEffectivenessReport` from `EducationalEffectivenessCalculator.Instance.CalculateEffectiveness()`

## Troubleshooting

**No data showing?**
- Ensure all manager objects exist in scene
- Check that minigames are calling `RecordCatch()` methods
- Verify Fish Catalog Entries are linked

**Achievements not unlocking?**
- Check achievement requirements match player progress
- Verify achievements are added to `AchievementSystem.All Achievements`
- Check console for achievement unlock messages

**Sustainability score seems wrong?**
- Review catch limits and thresholds
- Check for endangered species flags
- Verify decision recording is working

## Next Steps

1. Customize achievement definitions for your educational goals
2. Add more decision evaluation criteria
3. Create visual feedback for achievements (particles, sounds)
4. Export data for research analysis
5. Add leaderboards or sharing features

