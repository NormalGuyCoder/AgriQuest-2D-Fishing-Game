## Economy & Debt System

This folder adds a lightweight narrative economy so every minigame pays the player and chips away at their debt.

### Components

- `EconomyManager`
  - Singleton that persists wallet balance, outstanding debt, and transaction history.
  - Automatically loads/saves to `Application.persistentDataPath/economy_state.json`.
  - Awards cash via `RecordFishSale(...)` and optionally auto-pays a percentage toward the debt.
  - Emits `OnEconomyChanged(wallet, debt)` for UI hooks.
- `DebtStatusPanel`
  - Drop on any canvas panel.
  - Assign TMP labels + optional progress fill image.
  - Provides two button-ready helpers: `PayAllDebt()` and `PayFixedAmount(float)`.

### Default Story Loop

1. **Catch or Debone fish** in any minigame.
2. The minigame calls `EconomyManager.RecordFishSale`.
3. Player earns coins (higher payout for processed fish and illegal endangered catches).
4. Coins automatically pay a slice of the outstanding debt (configurable).
5. Debt reaches zero ⇒ player is “free”.

### Hooking up UI

1. Create a new panel in any scene (e.g., Achievements or main HUD).
2. Add `DebtStatusPanel`.
3. Assign:
   - `walletText`
   - `debtText`
   - `statusText` (optional)
   - `debtProgressFill` (Image with `Filled` type)
4. (Optional) Add a button and hook it to:
   - `DebtStatusPanel.PayAllDebt` or
   - `DebtStatusPanel.PayFixedAmount` (pass the value in the UnityEvent inspector).

### Tweaking Balancing

Open the `EconomyManager` instance (spawned automatically by `AchievementsSceneController` or place it in a bootstrap scene) and adjust:

- `startingDebt`: total amount the player must repay.
- `startingWallet`: seed money.
- `autoPaymentPercent`: how aggressive the mandatory payments are.
- `defaultCatchValue` / `debonedBonusValue`: fallback payouts for fish without catalog data.
- `endangeredPremiumMultiplier`: risk/reward for unsustainable sales.

### Save Reset

Delete `economy_state.json` inside `%APPDATA%/../LocalLow/<Company>/<Game>/` (the standard Unity persistent data path) to reset wallet & debt during development.

