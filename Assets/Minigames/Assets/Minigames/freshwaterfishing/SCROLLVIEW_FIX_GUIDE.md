# Quick Fix: Items Spawning Outside ScrollView

If your fish statistics or recent catches are spawning outside the ScrollView box, follow these steps:

## Step-by-Step Fix

### 1. Fix the Content GameObject

**Select:** `ScrollView → Viewport → Content` (the Content inside your ScrollView)

**RectTransform Settings:**
- Click the **Anchor Preset** box (top-left of RectTransform)
- Choose **Top-Left** preset
- **Pivot:** Should be at (0, 1) - Top-Left
- **Position:** X: `0`, Y: `0`
- **Width:** Match your ScrollView width (or use Stretch for width)
- **Height:** Start with `100` (will auto-expand)

### 2. Add Vertical Layout Group

**On the Content GameObject:**
- Add Component → **UI → Vertical Layout Group**

**Settings:**
- ✅ **Child Force Expand:** Width checked, Height unchecked
- ✅ **Child Control Size:** Both checked
- **Spacing:** `10`
- **Padding:** Left `5`, Right `5`, Top `5`, Bottom `5`
- **Child Alignment:** Upper Center

### 3. Add Content Size Fitter

**On the Content GameObject:**
- Add Component → **UI → Content Size Fitter**

**Settings:**
- **Horizontal Fit:** Unfit
- **Vertical Fit:** Preferred Size ✅

### 4. Fix Your Prefab

**Open your prefab** (Fish Stat Item or Recent Catch Item)

**Root GameObject RectTransform:**
- **Anchor Preset:** **Top-Left**
- **Pivot:** (0, 1) - Top-Left
- **Width:** Match ScrollView width (e.g., `490` for 500px ScrollView)
- **Height:** `60-80` pixels (adjust as needed)

### 5. Verify Controller Assignment

**In FreshwaterFishingScorePanelController:**
- **Fish Stats Container:** Should be the **Content** GameObject (not ScrollView!)
- **Recent Catches Container:** Should be the **Content** GameObject (not ScrollView!)

**Path should be:**
```
ScrollView
  └── Viewport
      └── Content ← Assign THIS one!
```

## Quick Checklist

- [ ] Content has **Top-Left** anchor (not Stretch-Stretch)
- [ ] Content has **Vertical Layout Group** component
- [ ] Content has **Content Size Fitter** component
- [ ] Prefab has **Top-Left** anchor
- [ ] Prefab has correct width (matches ScrollView)
- [ ] Controller is assigned to **Content**, not ScrollView
- [ ] Viewport has **Mask** component (should be automatic)

## Still Not Working?

1. **Check Console for errors**
2. **Verify prefab is instantiated as child of Content:**
   - In Hierarchy during play, check if items appear under `ScrollView → Viewport → Content`
3. **Try manually positioning one item:**
   - Drag an item into Content manually
   - If it appears correctly, the issue is with instantiation
   - If it's still outside, the Content settings are wrong

## Example Prefab Setup

**Fish Stat Item Prefab Structure:**
```
FishStatItem (RectTransform: Top-Left, Width: 490, Height: 70)
  ├── TextMeshProUGUI (Fish Name) - Font Size: 18, Bold
  └── TextMeshProUGUI (Stats) - Font Size: 14
```

**Recent Catch Item Prefab Structure:**
```
RecentCatchItem (RectTransform: Top-Left, Width: 490, Height: 60)
  ├── TextMeshProUGUI (Fish Name) - Font Size: 18, Bold
  └── TextMeshProUGUI (Details) - Font Size: 14
```

Both should have **Top-Left** anchors and fixed widths matching your ScrollView!

