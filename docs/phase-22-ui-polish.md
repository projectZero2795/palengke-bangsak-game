# Phase 22: UI polish

## Goal

Make the local prototype feel usable as a small game loop instead of a loose
test scene.

This phase does not add Photon, WebGL, deployment, rewards, or new gameplay
rules.

## What changed

- Added `PrototypeMainMenuController`.
- Added runtime-generated main menu UI to `MainMenu`.
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
4. Press Play.
5. Confirm the menu appears with:
   - `PLAY LOCAL`;
   - `HOW TO PLAY`;
   - `SETTINGS`.
6. Click `HOW TO PLAY`.
7. Confirm the corrected rules are readable.
8. Click `BACK`.
9. Click `SETTINGS`.
10. Confirm the placeholder panel appears and closes.
11. Click `PLAY LOCAL`.
12. Confirm `PrototypeMap` loads.
13. Finish a round by catching all hiders or using SAK.
14. Confirm the result panel shows both:
    - `Restart`;
    - `Menu`.
15. Click `Menu`.
16. Confirm `MainMenu` loads again.
17. Run EditMode tests and confirm:
    - `Phase22UiPolishTests`
    - `Phase21PrefabSceneTests`

## Exit criteria

- Main menu is usable.
- How-to overlay explains the corrected rules.
- Settings placeholder exists.
- Prototype scene can be launched from the menu.
- Result screen can restart or return to the menu.
- UI remains safe and cartoon/community friendly.
