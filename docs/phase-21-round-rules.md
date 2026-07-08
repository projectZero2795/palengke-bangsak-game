# Phase 21: Round rules

## Goal

Make the local single-device prototype play as a complete round.

This phase connects the existing Taya/Hider roles, Bang catch state, safe SAK
counter, Phase 20 map layout, and a small round HUD.

## What changed

- Added `PrototypeRoundRulesController`.
- Added `PrototypeRoundRulesHud`.
- Added `Phase 21 Round Rules` to `PrototypeMap`.
- The round starts automatically in Play mode.
- Round duration is `150` seconds (`02:30`).
- The HUD shows:
  - timer;
  - hiders remaining;
  - result panel;
  - Restart button.
- Taya wins when all Hiders are caught.
- Hiders win when Taya is countered by SAK.
- Hiders win by default if the timer expires.
- Pressing `R` or clicking `Restart` resets caught/countered state and starts
  a new round.
- Round start places Taya and Hiders on the Phase 20 map spawn points.
- The old per-player Phase 8 hiders counter HUD was removed from the default
  player prefab, because Phase 21 now owns the round HUD.

## Deliberately not included

- Photon/multiplayer.
- WebGL build/deployment.
- Palengke API.
- Scoring/coins/leaderboard.
- Stealth/reveal dog/light rules.

The stealth/reveal rules need a dedicated hide/run/noise state first. Adding
them inside this phase would make the round controller too large and harder to
review.

## How to test in Unity

1. Pull the latest `main`.
2. Open the project in Unity Hub using the `/unity` folder.
3. Open `Assets/Scenes/PrototypeMap.unity`.
4. Press Play.
5. Confirm the round HUD appears at the top:
   - timer starts at `02:30`;
   - hiders count appears under it.
6. Confirm the actors start on Phase 20 spawn points.
7. As Taya, catch every Hider with the named Bang buttons.
8. Confirm the result panel says `Taya wins!`.
9. Click `Restart` or press `R`.
10. Confirm Hiders reset and the timer starts again.
11. As a Hider/Sak test setup, use SAK against Taya.
12. Confirm the result panel says `Hiders win!`.
13. Run EditMode tests and confirm:
    - `PrototypeRoundRulesControllerTests`
    - `Phase21PrefabSceneTests`

## Exit criteria

- Local round starts.
- Timer counts down.
- Hiders-left count is visible.
- Taya can win by catching all Hiders.
- Hiders can win by SAK countering Taya.
- Timer expiry has a configured result.
- Restart resets the round.
- Old duplicate hiders counter HUD is no longer attached to the default player.
