# Phase 11 test notes: houses, fences, and gates

Phase 11 adds the first barangay residential objects to the prototype map.

The goal is still a small, testable local prototype step. This phase proves
that houses, fences, and gates can be generated, rendered, sorted, and used as
solid wall-collision obstacles before the full map layout phase.

## What changed

- Added deterministic placeholder sprites for:
  - small warm-lit barangay house;
  - medium concrete barangay house;
  - horizontal wooden/bamboo fence;
  - vertical wooden/bamboo fence;
  - closed gate.
- Added `PrototypeResidentialObjectSpawner`.
- Added a new root object in `PrototypeMap` named
  `Phase 11 Residential Objects`.
- Residential objects are placed around the map edges so the center routes stay
  playable.
- Houses, fences, and gates use `BoxCollider2D` wall collisions.
- House colliders intentionally cover only the lower walk-blocking footprint,
  not the whole roof/visual sprite, so players do not collide with empty-looking
  space around the roof.
- Sorting order is calculated from map position so houses/fences sit above the
  ground and remain readable.
- Added edit-mode tests for imported sprites, scene wiring, object specs,
  renderers, and wall colliders.

## Component contract

- component id: `prototype_residential_object_spawner`
- component version: `1`
- component variant: `night_barangay_residential_placeholders`

This keeps residential objects scalable. Later map versions can swap the house,
fence, gate, or placement set without rewriting player movement or round rules.

## How to review in Unity

1. Open the project in Unity.
2. Open `Assets/Scenes/PrototypeMap.unity`.
3. Press Play.
4. Confirm that the map loads without Console errors.
5. Look around the outer areas of the map.
6. Confirm that houses, fences, and gates appear.
7. Move the player into a house, fence, or gate.
   - Expected: the player should not pass through them.
8. Walk close to the roof/upper visual area of a house.
   - Expected: collision should feel tight to the actual lower footprint, not
     like a large invisible box around the full sprite.
9. Confirm the central movement routes still feel open enough for chasing.
10. Confirm the player, natural objects, Bang cone, Tag range, caught-state
   stars, and nighttime ground remain readable with houses present.

## Automated checks

Run Unity edit-mode tests and confirm these pass:

- `PrototypeResidentialObjectSpawnerTests`
- `Phase11PrefabSceneTests`
- previous Phase 9 and Phase 10 tests
- movement/action/caught-state tests from earlier phases

## Not included yet

These are intentionally left for later phases:

- roof-under/behind movement rules;
- house-attached reveal lights;
- full residential neighborhood layout;
- stores/stalls;
- base point;
- role and round rules;
- Photon multiplayer;
- WebGL build/deployment.

Do not start those until their phases are approved.
