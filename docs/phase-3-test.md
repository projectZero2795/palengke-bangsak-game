# Phase 3 Test Notes

## Scope

Phase 3 makes the default player prefab move and collide locally.

Included:

- `PlayerMovementController`
  - keyboard input with Unity's built-in legacy axes;
  - configurable movement speed;
  - normalized diagonal movement;
  - Rigidbody2D `MovePosition` movement in `FixedUpdate`.
- `MobileJoystickPlaceholder`
  - visual on-screen joystick placeholder;
  - drag input forwarded to the player controller;
  - centered compact handle, with the handle anchored to the joystick base center;
  - no new Input System package required.
- Default playable prefab updates:
  - `Rigidbody2D`;
  - `CircleCollider2D`;
  - `PlayerMovementController`;
  - `SpriteRenderer`.
- `PrototypeMap` movement test area:
  - one playable player;
  - boundary walls;
  - one center obstacle;
  - mobile joystick placeholder canvas.
- EditMode tests for:
  - keyboard-vs-joystick input priority;
  - diagonal input clamping;
  - movement speed delta;
  - playable prefab wiring;
  - wall collider presence;
  - mobile joystick presence.
  - compact centered joystick handle.
- Reproducible helper asset generator:
  - `tools/generate_phase3_placeholder_assets.py`

Not included:

- animation controller;
- sprite switching while walking;
- Bang action;
- Tag action;
- caught state;
- role system;
- round rules;
- Photon;
- WebGL build;
- Docker/Kubernetes/Argo files.

## Why the Input System package is still not used

Phase 1 review found repeated Unity editor errors from `com.unity.inputsystem@1.7.0` on macOS. Phase 3 uses Unity's built-in legacy input axes for keyboard movement and `UnityEngine.EventSystems` for the joystick placeholder, so the project remains free of the problematic package for now.

If we need the newer Input System later, it should be reintroduced deliberately in a small reviewed phase with editor validation.

The project explicitly sets Unity input handling to **Both** so Unity's legacy `Input` API remains available even if an editor remembers or imports newer Input System preferences. If the Console shows:

`You are trying to read Input using the UnityEngine.Input class...`

pull the latest repo and restart Unity so it reloads `ProjectSettings/ProjectSettings.asset`.

## Automated checks completed

- Unity 2022.3.50f1 imported the project successfully.
- EditMode tests passed: `7/7`.
- Confirmed no `com.unity.inputsystem` dependency in `Packages`.
- Confirmed project input handling is set to `Both` for Phase 3 compatibility.
- Confirmed the default prefab has movement physics components.
- Confirmed `PrototypeMap` contains movement walls and mobile joystick placeholder.
- Added regression coverage for the compact centered joystick handle.

## Manual Unity Editor checks

1. Pull latest `main`.
2. Open `unity/` in Unity 2022.3.50f1 or newer.
3. Open `Assets/Scenes/PrototypeMap.unity`.
4. Press Play.
5. Move with keyboard:
   - `W` / up arrow;
   - `A` / left arrow;
   - `S` / down arrow;
   - `D` / right arrow.
6. Confirm the player cannot pass through the boundary walls or center obstacle.
7. Confirm movement feels smooth and does not jitter.
8. In Game view, drag the lower-left joystick placeholder and confirm the player moves.
9. Confirm the joystick handle starts centered inside the base and returns to center after release.
10. Confirm the joystick is compact and no longer dominates the lower-left corner.
11. Confirm Console has no red errors.

## Exit criteria status

- Player moves up/down/left/right: ready for review.
- Player cannot pass through walls: ready for review.
- Movement speed is configurable: ready for review.
- Mobile joystick placeholder exists: ready for review.
- Movement/collision approval: pending project owner review.
