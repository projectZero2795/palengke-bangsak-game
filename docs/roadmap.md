# Bang-Sak Development Roadmap

Work on one phase at a time. Do not start a later phase until the current phase is reviewed and approved.

| Phase | Title | Goal | Exit criteria |
| ---: | --- | --- | --- |
| 0 | Repository and documentation | Create repo, docs, roadmap, maintenance plan, issues only. | Done. |
| 1 | Unity base project | Create clean Unity 2D foundation. | Done. |
| 2 | Player design | Create first player sprite/prefab and color variants. | Done. |
| 3 | Player physics and movement | Rigidbody2D, Collider2D, keyboard movement, joystick placeholder. | Done. |
| 4 | Player animation | Idle/walk animations and direction-facing. | Done. |
| 5 | Bang action design | Safe animated tsinelas marker/effect, compact circular button, cooldown, forward-cone range indicator. | Done. |
| 6 | Bang projectile / hit physics | Projectile/raycast hit detection and feedback. | Done. |
| 7 | Experimental TAG action | Separate close-range TAG action prototype. | Superseded; remove in Phase 16. |
| 8 | Caught state | isCaught state, animation, HUD indicator. | Done. |
| 9 | Soil / ground tiles | Soil, road, grass, concrete tilemap. | Done. |
| 10 | Trees and natural objects | Trees, bushes, plants, collision/occlusion, optional future `RevealSource` light/animal placeholders. | Done. |
| 11 | Houses | Houses, fences, gates, wall collisions, optional future `RevealSource` house-light placeholders. | Done. |
| 12 | Stores | Sari-sari store, stalls, signboards, crates, optional future `RevealSource` environment props. | Done. |
| 13 | Incorrect base prototype | Sak base trigger was implemented from the wrong rule assumption. | Superseded; remove in Phase 15. |
| 14 | Rules documentation correction | Document the corrected rule and adapt the phase plan without changing Unity code. | Done. |
| 15 | Remove incorrect SAK base | Remove only base object/scripts/HUD/sprite/tests. | Done. |
| 16 | Remove separate TAG mechanic | Remove only old TAG action/button/components/tests. | Done. |
| 17 | Role system | Taya/Hider roles, UI, markers, role component variants. | Done. |
| 18 | Corrected Bang-name rule | Taya uses `Bang + player name` to catch hiders. | Ready for review; local named catch rule works. |
| 19 | Safe SAK counter | Hiders use safe close-range SAK counter against Taya. | Local SAK counter works safely. |
| 20 | Map layout v1 | Place objects into playable map with spawn points and map component defaults. | Ready for review; map layout component and playable boundary are configured. |
| 21 | Round rules | Timer, hider count, Bang/SAK win conditions, restart, result screen. | Ready for review; local round loop works. |
| 22 | UI polish | Main menu, how-to, HUD, result, settings placeholder. | Ready for review; local prototype has a menu/how-to/settings loop. |
| 23 | Photon setup | Add Photon-ready room lifecycle scaffold, room menu, and document SDK import path without committing the App ID. | Scaffold ready for review; real two-client Photon join remains pending until Fusion SDK import/adapter. |
| 24 | Multiplayer player spawning | Add local network-style roster spawning, ownership markers, spawn slots, and camera/joystick local-owner targeting. | Ready for review; local network-spawn preview works, real Photon spawning remains pending SDK adapter. |
| 25 | Multiplayer movement sync | Add compile-safe snapshot/smoothing layer before Photon transport is wired. | Ready for review; local authority and remote replica movement sync scaffold works without Fusion SDK. |
| 26 | Multiplayer Bang/SAK sync | Networked named catch, SAK counter, stealth/reveal sync, and validation. | Online catching/countering works. |
| 27 | WebGL build | Unity WebGL build and browser test. | WebGL works locally. |
| 28 | Docker static hosting | Nginx Docker image for WebGL files. | Container serves game. |
| 29 | Kubernetes deployment | Deploy to cluster and expose games.palengke.es. | Public game reachable. |
| 30 | Argo CD, Palengke API, monitoring, polish | GitOps, real Palengke rewards/leaderboard, monitoring, and content expansion after WebGL/deploy foundations. | Production expansion plan approved. |

## Mandatory stop rule

After each phase, stop and wait for review before continuing.

## Future gameplay rules to respect

- Stealth/reveal rules are documented in [Gameplay Rules](gameplay-rules.md).
- The corrected Bang-Sak core rule is documented in [Gameplay Rules](gameplay-rules.md):
  Taya catches by `Bang + player name`; hiders counter with close-range SAK;
  there is no base.
- Object/component design/versioning is documented in
  [Object Design, Implementation, and Versioning](object-design-versioning.md).
- Every reusable piece of the game must be scalable, configurable, modifiable,
  and versioned: objects, player movement, actions, UI, map generation, reveal
  sources, network adapters, and API adapters.
- The long-term selection model is documented in
  [Object Design, Implementation, and Versioning](object-design-versioning.md):
  admins can set defaults, rooms can select compatible component versions, and
  players can vote for allowed maps/component sets.
- Hiders can only hide safely for `5` seconds before a streetlight, house light,
  or dog-bark clue reveals them.
- Walking should stay quiet; running should reveal the hider direction through
  short directional clues.
- Reveal objects must use the scalable `RevealSource` category so future reveal
  providers can be added by object type/version/variant rather than hardcoding
  each provider into round rules.
