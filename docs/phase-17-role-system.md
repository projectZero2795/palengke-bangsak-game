# Phase 17 Role System

Phase 17 adds the first local Taya/Hider role system.

This phase is intentionally small: it makes roles explicit and role-aware, but
does not add the named Bang rule, the corrected hider SAK counter, round rules,
or multiplayer.

## What changed

- Added `PlayerRole`:
  - `Taya`
  - `Hider`
- Added `PlayerRoleController`:
  - stores the local role;
  - exposes `IsTaya`, `IsHider`, and `CanUseBang`;
  - shows a small role badge above the player;
  - marks Hiders as countable hiders for the `Hiders Left` counter;
  - keeps Bang available only for Taya.
- Updated the Bang HUD so it can be hidden by role.
- Updated Bang visuals so disabling Bang hides the range/effect visuals.
- Wired prefabs:
  - default playable player = `Taya`;
  - blue/green/red/yellow practice players = `Hider`.
- Added role-focused edit-mode tests.

## How to review in Unity

1. Pull the latest repository changes.
2. Open the Unity project from the `unity` folder.
3. Open `Assets/Scenes/PrototypeMap.unity`.
4. Press Play.
5. Confirm:
   - the playable red player shows a `TAYA` badge;
   - color-variant practice players show `HIDER` badges;
   - the playable Taya still has the Bang button;
   - Hider prefabs do not show old TAG/base UI;
   - Bang still catches Hiders and shows dizzy stars;
   - `Hiders Left` still counts the Hider prefabs;
   - there are no missing-script warnings.

## Edit-mode tests added

- `PlayerRoleControllerTests`
- `Phase17PrefabSceneTests`

## Not included in Phase 17

- `Bang + player name`.
- Corrected safe hider SAK counter.
- Round win/loss rules.
- Spawn/role selection UI.
- Multiplayer.
