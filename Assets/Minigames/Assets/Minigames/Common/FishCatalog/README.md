# Shared Fish Catalog & Inventory

This folder contains the shared data layer that lets every minigame talk about the same fish and aggregate catch stats.

## Components

- `FishCatalogEntry`: one ScriptableObject per fish (unique `fishId`, sprites, lore, aliases).
- `FishCatalogDatabase`: list of entries + lookup by id/alias.
- `FishInventoryManager`: runtime singleton that keeps totals per fish and per minigame source, persisted via `PlayerPrefs`.

## Setup Steps

1. **Create catalog entries**
   - Right–click in Project panel → `Create > Fish Catalog > Entry`.
   - Fill `fishId` (lowercase, underscores), display/scientific names, description, sprites.
   - Add aliases (e.g., `"sardine"`, `"sardinella"`) so older assets resolve automatically.

2. **Create the database**
   - Right–click → `Create > Fish Catalog > Database`.
   - Add every `FishCatalogEntry` to the list (order is not important).

3. **Place the inventory manager in a scene**
   - Add an empty GameObject (e.g., `FishInventory`), attach `FishInventoryManager`.
   - Assign the catalog database.
   - Mark the GameObject as part of your bootstrap/persistent scene so it loads before any minigame (the script auto `DontDestroyOnLoad`).

4. **Link existing fish assets**
   - `FishDefinition` (deboning), `FishData` (FishingMinigame), and `FreshwaterFishData` now expose `sharedCatalogEntry`.
   - Open each ScriptableObject asset and assign the correct catalog entry.
   - The helper fields auto-generate a consistent `sharedFishId`; aliases handle mismatched old names.

5. **Verify**
   - Play each minigame, catch/finish with the same fish name.
   - Use `FishInventoryManager.GetAllTotals()` (or add a temporary debug UI) to confirm totals increment across minigames.

## Extending

- Subscribe to `FishInventoryManager.OnInventoryChanged` to update UI dashboards.
- Call `FishInventoryManager.Instance.GetTotals("sardine")` to display aggregated counts.
- If you move to a custom save system, replace the PlayerPrefs calls inside `FishInventoryManager`.

