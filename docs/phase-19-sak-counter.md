# Phase 19: Safe SAK Counter

## Scope

Phase 19 adds the first local hider `SAK` counter against Taya.

Included:

- `SakCounterController`
  - hider-only action;
  - short-range `CircleCast` detection;
  - wall blocking before Taya;
  - cooldown;
  - safe cartoon burst feedback;
  - no realistic weapon visuals.
- `TayaCounteredStateController`
  - temporary local playful countered state;
  - color pulse;
  - spinning cartoon burst above Taya;
  - temporary movement/Bang disable during the feedback window.
- `SakCounterHud`
  - compact SAK button for local playable hider use;
  - hidden when the role is not Hider.
- `PlayerRoleController`
  - Taya can use Bang, not SAK;
  - Hider can use SAK, not Bang.
- `CaughtStateController`
  - caught hiders also lose SAK while caught.
- Prefab wiring:
  - default playable prefab has SAK components but starts as Taya, so SAK is disabled;
  - default playable prefab has Taya counter feedback;
  - hider color variants have the SAK controller but no HUD, avoiding duplicate practice buttons.

Not included:

- round timer;
- Taya/Hider win or loss result;
- score/rewards;
- multiplayer;
- Photon;
- realistic weapons;
- blood/gore/lethal combat.

## Safety rule

SAK is a harmless close-range counter. In this prototype it is represented as a friendly `SAK` button and cartoon burst/tint. It must not become a knife, killing animation, blood, gore, or realistic combat.

## Automated checks added

- `SakCounterControllerTests`
  - close-range hider SAK counters Taya;
  - far SAK misses;
  - wall before Taya blocks SAK;
  - SAK cooldown works;
  - Taya cannot use SAK.
- `PlayerRoleControllerTests`
  - Taya enables Bang and disables SAK;
  - Hider disables Bang and enables SAK.
- `Phase19PrefabSceneTests`
  - default player prefab has Taya counter feedback and SAK components;
  - hider prefabs have SAK controller;
  - old retired `SakActionHud.cs` does not return.

## Manual Unity review

1. Pull latest `main`.
2. Open `unity/` in Unity 2022.3.50f1 or newer.
3. Run EditMode tests and confirm the Phase 19 tests pass.
4. Open `Assets/Scenes/PrototypeMap.unity`.
5. Press Play and verify the normal Taya flow still works:
   - Taya sees the compact named Bang buttons;
   - Taya does not see a SAK button;
   - Bang still catches only the called hider name.
6. For a quick SAK review, temporarily set the playable player role to `Hider` in the Inspector and place/keep a Taya object close in front of the hider:
   - press the SAK button;
   - confirm close SAK gives Taya a playful burst/tint;
   - confirm SAK goes on cooldown;
   - confirm SAK does not work when Taya is far away or behind a wall.
7. Confirm Console has no red errors.

## Exit criteria status

- Hider-only SAK action: ready for review.
- Safe visuals only: ready for review.
- Close-range Taya counter: ready for review.
- Cooldown: ready for review.
- Wall blocking: ready for review.
- Taya playful feedback state: ready for review.
- Round rules intentionally deferred.
