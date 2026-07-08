# Phase 22: UI polish

## Goal

Make the local prototype feel usable as a small game loop instead of a loose
test scene.

This phase does not add Photon, WebGL, deployment, rewards, or new gameplay
rules.

## What changed

- Added `PrototypeMainMenuController`.
- Added runtime-generated main menu UI to `MainMenu`.
- The menu UI now previews in the editor when `MainMenu.unity` is opened; the
  preview is temporary and not saved into the scene.
- The menu creates its own Unity `EventSystem` when needed so buttons can
  receive clicks/taps in Play mode.
- Main menu now includes:
  - `PLAY LOCAL`;
  - `HOW TO PLAY`;
  - `SETTINGS`;
  - keyboard shortcuts: `P` play, `H` help, `Esc` close overlay.
- Added a How to Play overlay explaining the corrected Bang-Sak rule:
  - Taya catches by `Bang + player name`;
  - Hiders can use safe close-range `SAK`;
  - Taya wins by catching all hiders;
  - Hiders win by SAK counter or surviving the timer.
- Added a Settings placeholder panel for later volume/language/control options.
- Added a `Menu` button to the round result panel so the local loop can return
  from `PrototypeMap` to `MainMenu`.
- Kept the compact rounded Phase 21 command-bar HUD.

## Deliberately not included

- Photon/multiplayer.
- WebGL build/deployment.
- Palengke API.
- Audio/settings persistence.
- Final art assets.

## How to test in Unity

1. Pull the latest `main`.
2. Open the project in Unity Hub using the `/unity` folder.
3. Open `Assets/Scenes/MainMenu.unity`.
4. Before pressing Play, confirm the menu preview appears in the Game view or
   as `Phase 22 Main Menu UI` under `Bang-Sak Main Menu Root`.
5. Press Play. Buttons are expected to be clickable in Play mode.
6. Confirm the menu works with:
   - `PLAY LOCAL`;
   - `HOW TO PLAY`;
   - `SETTINGS`.
7. Click `HOW TO PLAY`.
8. Confirm the corrected rules are readable.
9. Click `BACK`.
10. Click `SETTINGS`.
11. Confirm the placeholder panel appears and closes.
12. Click `PLAY LOCAL`.
13. Confirm `PrototypeMap` loads.
14. Finish a round by catching all hiders or using SAK.
15. Confirm the result panel shows both:
    - `Restart`;
    - `Menu`.
16. Click `Menu`.
17. Confirm `MainMenu` loads again.
18. Run EditMode tests and confirm:
    - `Phase22UiPolishTests`
    - `Phase21PrefabSceneTests`

## Exit criteria

- Main menu is usable.
- Main menu is visible for editor review before Play.
- How-to overlay explains the corrected rules.
- Settings placeholder exists.
- Prototype scene can be launched from the menu.
- Result screen can restart or return to the menu.
- UI remains safe and cartoon/community friendly.
