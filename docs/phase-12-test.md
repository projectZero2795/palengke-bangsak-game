# Phase 12 test notes: stores and marketplace props

Phase 12 adds the first Filipino marketplace props to the prototype map.

The goal is to make the map feel more like a barangay/palengke space while
keeping this phase small and reviewable. This phase does not add the Sak base,
roles, round rules, or multiplayer.

## What changed

- Added deterministic placeholder sprites for:
  - sari-sari store;
  - palengke fruit/vegetable stall;
  - small food stall;
  - SARI signboard;
  - crates and baskets.
- Added `PrototypeStoreObjectSpawner`.
- Added a new root object in `PrototypeMap` named `Phase 12 Store Objects`.
- Store props are placed around existing market/concrete/soil areas and avoid
  Phase 9 road cells.
- Store props use `BoxCollider2D` wall collisions with tight lower footprints.
- Added edit-mode tests for imported sprites, scene wiring, object specs,
  non-road placement, renderers, and colliders.

## Component contract

- component id: `prototype_store_object_spawner`
- component version: `1`
- component variant: `night_palengke_store_placeholders`

This keeps store props scalable. Later map versions can swap the store/stall
visuals, object counts, or placement rules without rewriting player movement or
round logic.

## How to review in Unity

1. Open the project in Unity.
2. Open `Assets/Scenes/PrototypeMap.unity`.
3. Press Play.
4. Confirm that the map loads without Console errors.
5. Look around the marketplace areas near the center/side concrete pads.
6. Confirm that sari-sari stores, palengke stalls, food stalls, signs, and
   crates/baskets appear.
7. Move the player into the lower/solid parts of store props.
   - Expected: the player should not pass through them.
8. Walk around the sides of store props.
   - Expected: collisions should feel tight to the visible lower footprint, not
     like a huge invisible box.
9. Confirm the main movement routes still feel open enough for chasing.
10. Confirm the player, houses, natural objects, Bang cone,
    caught-state stars, and nighttime ground remain readable.

## Automated checks

Run Unity edit-mode tests and confirm these pass:

- `PrototypeStoreObjectSpawnerTests`
- `Phase12PrefabSceneTests`
- previous Phase 9, 10, and 11 tests
- movement/action/caught-state tests from earlier phases

## Not included yet

These are intentionally left for later phases:

- noisy prop interactions;
- reveal-source environment props;
- Sak base;
- spawn points and full map layout;
- role and round rules;
- Photon multiplayer;
- WebGL build/deployment.

Do not start those until their phases are approved.
