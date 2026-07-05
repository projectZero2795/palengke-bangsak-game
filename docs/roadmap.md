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
| 7 | Close tag action | Harmless close-range Tag/Close Tap mechanic. | Ready for local close-tag review. |
| 8 | Caught state | isCaught state, animation, HUD indicator. | Caught state reliable. |
| 9 | Soil / ground tiles | Soil, road, grass, concrete tilemap. | Basic map floor ready. |
| 10 | Trees and natural objects | Trees, bushes, plants, collision/occlusion. | Natural obstacles work. |
| 11 | Houses | Houses, fences, gates, wall collisions. | Residential area works. |
| 12 | Stores | Sari-sari store, stalls, signboards, crates. | Marketplace area works. |
| 13 | Base point | Sak base trigger and hider-only interaction. | Base mechanic works locally. |
| 14 | Map layout v1 | Place objects into playable map with spawn points. | First playable map approved. |
| 15 | Role system | Taya/Hider roles, UI, markers. | Roles work locally. |
| 16 | Round rules | Timer, hider count, win conditions, restart, result screen. | Local prototype playable. |
| 17 | UI polish | Main menu, how-to, HUD, result, settings placeholder. | Local prototype usable. |
| 18 | Photon setup | Install/configure Photon Fusion and room lifecycle. | Two clients join same room. |
| 19 | Multiplayer player spawning | Network player prefab, ownership, spawn points. | Multiplayer movement foundation works. |
| 20 | Multiplayer movement sync | Smooth remote movement and basic lag notes. | Movement sync stable. |
| 21 | Multiplayer Bang/Tag sync | Networked catch mechanics and validation. | Online catching works. |
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
