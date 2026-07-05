# Phase 2 Test Notes

## Scope

Phase 2 creates the first placeholder player design only.

Included:

- Transparent PNG player sprites in `unity/Assets/Art/Placeholders/Players`.
- Five pose frames per color:
  - `idle`
  - `walk_01`
  - `walk_02`
  - `walk_03`
  - `walk_04`
- Four color variants:
  - red
  - green
  - blue
  - yellow
- Default prefab:
  - `unity/Assets/Prefabs/PlayerPlaceholder.prefab`
- Color-variant prefabs:
  - `unity/Assets/Prefabs/Players/PlayerPlaceholder_Red.prefab`
  - `unity/Assets/Prefabs/Players/PlayerPlaceholder_Green.prefab`
  - `unity/Assets/Prefabs/Players/PlayerPlaceholder_Blue.prefab`
  - `unity/Assets/Prefabs/Players/PlayerPlaceholder_Yellow.prefab`
- Non-gameplay preview instances in `PrototypeMap`.
- Reproducible placeholder generator:
  - `tools/generate_phase2_placeholder_sprites.py`
- Review contact sheet:
  - `docs/phase-2-player-sprite-preview.png`

Not included:

- movement scripts;
- Rigidbody2D;
- Collider2D;
- input package;
- mobile joystick;
- animation controller;
- runtime animation logic;
- gameplay rules;
- Photon;
- WebGL build;
- Docker/Kubernetes/Argo files.

## Why there are multiple walk sprites

Phase 4 will need more than one walk image to make walking look acceptable. Phase 2 therefore provides four walking pose frames per color, but does not wire them into an Animator Controller yet.

The intended future loop is:

`walk_01 -> walk_02 -> walk_03 -> walk_04 -> walk_01`

Idle remains a separate still pose.

## Unity Editor checks

1. Open `unity/` in Unity 2022.3.50f1 or newer.
2. Open `Assets/Scenes/PrototypeMap.unity`.
3. Confirm the scene contains `Phase 2 Player Design Preview`.
4. Confirm red, green, blue, and yellow player variants are visible.
5. Open `Assets/Prefabs/PlayerPlaceholder.prefab`.
6. Confirm it has only:
   - `Transform`
   - `SpriteRenderer`
7. Open each color prefab under `Assets/Prefabs/Players`.
8. Confirm each prefab uses its matching idle sprite.
9. Open the Console and confirm there are no red errors.

## Exit criteria status

- Player appears in scene: ready for review.
- Player scale is correct for a 64 pixels-per-unit placeholder: ready for review.
- Player is visually readable on the map: ready for review.
- Player design approval: pending project owner review.
