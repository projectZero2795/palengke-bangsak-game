# Bang-Sak Object Catalog

This catalog is derived from the latest reference image provided by the project owner. It is the source of truth for what future phases should create.

Do not create these assets in Phase 0. This file defines the target objects for later phases.

Reference image: [bang-sak-roadmap-reference.jpg](reference/bang-sak-roadmap-reference.jpg).

## Characters

| Object | Phase | Notes |
| --- | ---: | --- |
| Taya player | 2-4, 15 | Red shirt/marker, readable as the catcher. |
| Hider player | 2-4, 15 | Green/alternate shirt, readable as runner/hider. |
| Player color variants | 2 | Different colors for multiple users. |
| Avatar portraits | 17, 19 | Used in HUD, lobby, result, leaderboard. |
| Caught player state | 8 | Dizzy stars or playful frozen/waiting visual. |

## Safe action objects

| Object | Phase | Safe visual rule |
| --- | ---: | --- |
| Bang marker | 5 | Finger-gun pose, toy dart, light beam, foam tag, or tsinelas. No realistic gun. |
| Bang projectile/effect | 6 | Cartoon sparkle, toy dart, or tsinelas projectile. No bullets/blood. |
| Close Tag / Close Tap | 7 | Harmless touch/foam tag visual. Do not call it knife. |
| Sak base button | 13, 22 | Green base action for hiders near base. |

## Ground and map foundation

| Object | Phase | Notes |
| --- | ---: | --- |
| Soil tile | 9 | Barangay/palengke floor. |
| Road/path tile | 9 | Main movement routes. |
| Grass tile | 9 | Soft natural areas. |
| Concrete tile | 9 | Plaza/storefront areas. |
| Tilemap sorting layers | 9 | Ground below players and objects. |

## Natural objects and hiding props

| Object | Phase | Notes |
| --- | ---: | --- |
| Tree | 10 | Solid trunk collision, canopy can provide partial hiding. |
| Bush | 10 | Hiding/occlusion object. |
| Plant pot | 10 | Small obstacle and theme detail. |
| Large plant cluster | 10, 14 | Map variety and chase routes. |

## Houses and residential objects

| Object | Phase | Notes |
| --- | ---: | --- |
| Small house | 11 | Solid wall collision. |
| Medium house | 11 | Larger obstacle and visual anchor. |
| Fence pieces | 11 | Maze/chase path shaping. |
| Gates | 11 | Visual variety; can be blocker or passage. |
| Roof sorting | 11 | Optional, if player passes behind/under edges. |

## Stores and marketplace objects

| Object | Phase | Notes |
| --- | ---: | --- |
| Sari-sari store | 12 | Core Filipino theme object. |
| Palengke stall | 12 | Market rows and hiding routes. |
| Food stall | 12 | Visual variety. |
| Signboards | 12 | Readable labels, e.g. SARI, GULAY. |
| Crates/baskets | 12 | Small blockers/details. |
| Barrels/buckets | 12, 14 | Optional obstacle/detail objects. |

## Base and round objects

| Object | Phase | Notes |
| --- | ---: | --- |
| Sak base | 13 | Green circular base marker with flag. |
| Base trigger collider | 13 | Enables Sak only for hiders nearby. |
| Spawn points | 14, 19 | Taya/hider spawn positions. |
| Camera boundary | 14 | Prevents camera showing outside map. |
| Hiding routes | 14 | Paths around houses, trees, and stores. |

## RevealSource objects

RevealSource objects are scalable reveal providers. The core stealth system
should depend on the shared `reveal_source` category, not hardcoded object IDs.
This lets us add new reveal objects in future maps without rewriting the round
rules.

| RevealSource type | Example objects | Phase | Notes |
| --- | --- | ---: | --- |
| `light` | Streetlight, market lamp, string light | 10, 14, 16 | Flicker/sweep/turn-on clue after hide timeout. |
| `house_light` | Window light, balcony light, door light | 11, 14, 16 | House-attached warm clue near hidden hider. |
| `animal_sound` | Dog bark, rooster noise | 10, 14, 16 | Directional sound clue. Friendly only; no attack/chase behavior. |
| `movement_noise` | Running footsteps, dust ripple, sound wave | 16, 21 | Running reveals direction. Walking stays quiet. |
| `environment` | Loose can, hanging bell, bucket, plastic chair | 12, 16+ | Optional future noisy prop category after core stealth works. |

## UI objects

| Object | Phase | Notes |
| --- | ---: | --- |
| Main menu | 17 | Bang-Sak logo, Play, Custom Room, How to Play, Settings. |
| Lobby panel | 18 | Room code, players, roles, ready state, map preview. |
| HUD | 15-17 | Role badge, timer, hiders-left, buttons. |
| Mobile joystick | 3, 17 | Bottom-left movement control. |
| Bang/Tag/Sak buttons | 5, 7, 13, 17 | Role-aware action buttons. |
| Result screen | 16-17 | Taya/Hider win, caught list, rewards. |
| Leaderboard | 27-28 | Mock first, real later. |

## Visual direction

- Use polished 2D top-down / slight 2.5D composition similar to the reference image.
- Make objects chunky, readable, warm, Filipino-themed, and friendly.
- Prioritize clear gameplay readability over visual noise.
- Mobile UI must stay legible and thumb-friendly.
- Bang-Sak is played at night, so objects need readable silhouettes, warm local
  light, and cool shadow separation.
- Every object/component that may appear in maps or gameplay should follow
  [Object Design, Implementation, and Versioning](object-design-versioning.md):
  design first, implementation contract second, version/variant metadata third.
- This includes visual objects, gameplay systems, UI widgets, map generators,
  reveal sources, networking adapters, and API adapters.
