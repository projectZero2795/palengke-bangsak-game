# Phase 4 Test Notes

## Scope

Phase 4 makes the placeholder player visibly animate while moving.

Included:

- `PlayerAnimationController`
  - swaps between the idle sprite and the four Phase 2 walk sprites;
  - uses configurable `framesPerSecond`;
  - tracks `PlayerFacingDirection`;
  - flips the sprite when moving left and unflips when moving right;
  - keeps the last facing direction when idle.
- `PlayerFacingDirection`
  - `Down`;
  - `Up`;
  - `Left`;
  - `Right`.
- Default player prefab updates:
  - `PlayerAnimationController`;
  - red idle sprite reference;
  - four red walk frame references.
- EditMode tests for:
  - horizontal direction resolution;
  - vertical direction resolution;
  - stationary fallback direction;
  - left/right flip behavior;
  - walk frame looping;
  - walking-vs-idle threshold;
  - prefab animation wiring;
  - `PrototypeMap` playable-player animation wiring.

Not included:

- final production art;
- directional north/south sprite sheets;
- Unity Animator graph;
- Bang action;
- Tag action;
- caught state;
- role system;
- round rules;
- Photon;
- WebGL build;
- Docker/Kubernetes/Argo files.

## Why this uses a script-based animation controller

The current Phase 2 placeholder sprites are simple front-facing characters. A script-based controller is easier to review now because it only proves the animation rules:

- idle when not moving;
- walk frame cycling when moving;
- facing state changes from input direction;
- left/right flip behavior.

A Unity Animator graph can still be introduced later if the final sprite sheet needs more complex animation states.

## Manual Unity Editor checks

1. Pull latest `main`.
2. Fully close and reopen Unity so project settings and prefab changes reload.
3. Open `unity/` in Unity 2022.3.50f1 or newer.
4. Open `Assets/Scenes/PrototypeMap.unity`.
5. Press Play.
6. Move with keyboard:
   - `W` / up arrow;
   - `A` / left arrow;
   - `S` / down arrow;
   - `D` / right arrow.
7. Confirm the player cycles through walk frames while moving.
8. Confirm the player returns to the idle sprite when stopped.
9. Confirm moving left flips the placeholder player and moving right unflips it.
10. Drag the lower-left joystick placeholder and confirm it also drives the walk animation.
11. Confirm Console has no red errors.

## Exit criteria status

- Idle animation: ready for review.
- Walk animation: ready for review.
- Direction-facing logic: ready for review.
- Basic animation controller: ready for review.
- Animation approval: pending project owner review.
