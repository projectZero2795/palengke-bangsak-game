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
| 7 | Close tag action | Harmless close-range Tag/Close Tap mechanic. | Done. |
| 8 | Caught state | isCaught state, animation, HUD indicator. | Done. |
| 9 | Soil / ground tiles | Soil, road, grass, concrete tilemap. | Done. |
| 10 | Trees and natural objects | Trees, bushes, plants, collision/occlusion, optional future `RevealSource` light/animal placeholders. | Done. |
| 11 | Houses | Houses, fences, gates, wall collisions, optional future `RevealSource` house-light placeholders. | Done. |
| 12 | Stores | Sari-sari store, stalls, signboards, crates, optional future `RevealSource` environment props. | Done. |
| 13 | Base point | Sak base trigger and hider-only interaction. | Ready for local SAK-base review. |
| 14 | Map layout v1 | Place objects into playable map with spawn points and map component defaults. | First playable map approved. |
| 15 | Role system | Taya/Hider roles, UI, markers, role component variants. | Roles work locally. |
| 16 | Round rules | Timer, hider count, stealth/reveal rules, win conditions, restart, result screen. | Local prototype playable. |
| 17 | UI polish | Main menu, how-to, HUD, result, settings placeholder. | Local prototype usable. |
| 18 | Photon setup | Install/configure Photon Fusion, room lifecycle, and room component selection/vote placeholder. | Two clients join same room. |
| 19 | Multiplayer player spawning | Network player prefab, ownership, spawn points. | Multiplayer movement foundation works. |
| 20 | Multiplayer movement sync | Smooth remote movement and basic lag notes. | Movement sync stable. |
| 21 | Multiplayer Bang/Tag sync | Networked catch mechanics, stealth/reveal sync, and validation. | Online catching works. |
| 22 | Multiplayer Sak sync | Networked base interaction and round result. | Full online round works. |
| 23 | WebGL build | Unity WebGL build and browser test. | WebGL works locally. |
| 24 | Docker static hosting | Nginx Docker image for WebGL files. | Container serves game. |
| 25 | Kubernetes deployment | Deploy to cluster and expose games.palengke.es. | Public game reachable. |
| 26 | Argo CD integration | GitOps-managed deployment. | Argo sync and rollback work. |
| 27 | Palengke API placeholder | Mock API client, mock user, mock leaderboard/coins. | Game works without real API. |
| 28 | Real Palengke integration | Login, score submit, leaderboard, rewards. | Palengke-connected safely. |
| 29 | Monitoring and maintenance | Logs, error tracking plan, versioning, release notes. | Production maintenance documented. |
| 30 | Polish and content expansion | More maps, sprites, skins, badges, events, sounds. | Content expansion plan approved. |

## Mandatory stop rule

After each phase, stop and wait for review before continuing.

## Future gameplay rules to respect

- Stealth/reveal rules are documented in [Gameplay Rules](gameplay-rules.md).
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
