# Phase 18 Corrected Bang-name Rule

Phase 18 adds the first local implementation of the corrected Bang-Sak catch
rule:

> Taya must call `Bang + player name`.

This phase is still local-only. It does not add the hider SAK counter, round
win/loss rules, multiplayer, or voice recognition.

## What changed

- Added `PlayerNameIdentity`:
  - default Taya player = `JuanP`;
  - hider variants = `Maria`, `Pedro`, `Ana`, and `Luis`.
- Added `BangNameCallController`:
  - stores the currently called hider name;
  - can cycle available hider names with `Q` / `E`;
  - validates the name when Bang hits a target.
- Added `BangNameCallHud`:
  - compact Taya-only `Call: name` panel;
  - prev/next buttons for review in Play Mode;
  - feedback text for correct or wrong names.
- Updated Bang hit flow:
  - correct called name + physical hit = hider is caught;
  - wrong called name + physical hit = `NameMismatch`;
  - `NameMismatch` flashes feedback but does not mark the hider caught.
- Added edit-mode tests for:
  - correct named Bang catching;
  - wrong named Bang not catching;
  - name normalization;
  - prefab name-call wiring.

## How to review in Unity

1. Pull the latest repository changes.
2. Open the Unity project from the `unity` folder.
3. Open `Assets/Scenes/PrototypeMap.unity`.
4. Press Play.
5. Confirm:
   - the Taya player has a compact `Call: Maria` style HUD;
   - press `Q` or `E`, or click the small arrows, to change the called name;
   - if the called name matches the hider you hit, the hider is caught;
   - if the called name does not match the hider you hit, the hider is not
     caught and feedback says the name was wrong;
   - there are still no floating `TAYA` / `HIDER` labels above players;
   - no old TAG button and no old SAK base returns.

## Not included in Phase 18

- Safe hider SAK counter.
- Round timer or final win/loss rules.
- Voice recognition.
- Multiplayer or Photon.
- Palengke account/leaderboard integration.
