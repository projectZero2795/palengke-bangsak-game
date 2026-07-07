# Phase 16 TAG Removal

Phase 16 removes only the old separate TAG experiment.

The corrected Bang-Sak rule says hiders use a future safe `SAK` counter against
Taya. That counter should not be implemented as the old generic TAG button, so
Phase 16 removes the retired TAG path before roles and named Bang behavior are
added.

## What changed

- Removed TAG action scripts:
  - `TagActionController`
  - `TagHitOutcome`
  - `TagHitResult`
  - `TagHitTarget`
  - `TagActionHud`
- Removed TAG components from the default player prefab.
- Removed TAG target components from color-variant player prefabs.
- Removed TAG-specific edit-mode tests.
- Removed TAG dependency from `CaughtStateController`.
- Added `Phase16TagRemovalTests` to guard against missing scripts and retired
  TAG assets returning.

## Intentionally not changed

- Bang action.
- Bang hit detection.
- Player movement and animation.
- Caught-state foundation.
- Hiders-left counter.
- Ground, natural objects, houses, and store map pieces.

The corrected safe hider SAK counter is intentionally not implemented here. It
will come after roles and named Bang behavior.

## How to review in Unity

1. Pull the latest repository changes.
2. Open the Unity project from the `unity` folder.
3. Open `Assets/Scenes/PrototypeMap.unity`.
4. Press Play.
5. Confirm:
   - the old TAG button is gone;
   - pressing `E` no longer triggers the old TAG action;
   - movement still works;
   - Bang still shows the red cone and tsinelas marker;
   - Bang can still catch a practice target and show dizzy stars;
   - no missing-script warnings appear on the default player or color variants.

## Edit-mode tests added

- `Phase16TagRemovalTests`

## Not included in Phase 16

- Adding Taya/Hider roles.
- Adding `Bang + player name`.
- Adding the corrected hider SAK counter.
- Round rules.
- Multiplayer.
