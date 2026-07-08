# Phase 23: Photon setup scaffold

## Goal

Prepare Bang-Sak for Photon Fusion rooms without changing local gameplay yet.

This phase intentionally stays small:

- Add a room lifecycle component.
- Add a compact **ROOM** entry to the main menu.
- Keep the Photon App ID out of Git.
- Document the remaining SDK import step.

## What changed

- Added `PrototypeNetworkRoomController`.
- Added `PrototypeNetworkRoomState`.
- Added a **ROOM** tile to the main menu.
- Added a room panel with:
  - Create room preview.
  - Join default room `1234`.
  - Leave room.
  - SDK status.
  - Provider/status text.
- Added edit-mode tests for room-code validation and lifecycle state.
- Added `.gitignore` protection for local Photon App ID assets.

## Important limitation

The Photon Fusion SDK is not imported in this commit.

The official Photon Fusion docs distribute the SDK as a Unity package import flow, so the real SDK import must happen in Unity Editor. This commit therefore implements a safe offline room preview and detects whether Fusion exists, but it does not connect to Photon Cloud yet.

Do not commit the Photon App ID.

## How to review in Unity

1. Pull the latest repo.
2. Open `unity/Assets/Scenes/MainMenu.unity`.
3. Press Play.
4. Click **ROOM**.
5. Confirm the panel opens.
6. Click **CREATE**.
7. Confirm it shows an offline room code.
8. Click **JOIN 1234**.
9. Confirm the state changes to joined room `1234`.
10. Click **LEAVE**.
11. Confirm the state returns to disconnected.

Keyboard shortcut:

- `R` opens the room panel.

## How to complete the real Photon step later

1. Import Photon Fusion 2 into Unity Editor.
2. Configure the Photon App ID locally using the Photon settings workflow.
3. Do not commit generated App ID secrets.
4. Replace the offline lifecycle calls with a dedicated Fusion adapter component.
5. Test with two local/editor clients or two browser clients joining the same room.

## Test result expected for this phase

- Unity compiles.
- Main menu opens.
- ROOM panel buttons work.
- No Photon App ID is committed.
- Edit-mode room lifecycle tests pass.

## Exit status

Scaffold ready for review.

The original full exit criterion, “two clients join same room,” remains pending until the Photon Fusion SDK is imported through Unity Editor and the real Fusion adapter is implemented.
