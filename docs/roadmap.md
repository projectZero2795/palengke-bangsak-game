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
| 26 | Multiplayer Bang/SAK sync | Add compile-safe action event layer for named Bang catches and SAK counters before Photon transport is wired. | Ready for review; local/remote Bang/SAK events can be captured/applied without Fusion SDK. |
| 27 | Palengke API placeholder | Compile-safe identity, score, reward, and leaderboard adapter with mock data. | Done. |
| 27A | Visible action cooldowns | Show independent cooldown feedback inside each Hider action. | Done. |
| 28 | Real Palengke integration | Connect identity, persistent scores, server rewards, and leaderboard with safe guest fallback. | Implemented; authenticated acceptance waits for public deployment. |
| 28B | Unity WebGL build | Reproducible Unity build and local browser smoke test. | Done. |
| 28C | Docker static hosting | Non-root Nginx image for the approved WebGL files. | Container serves game locally. |
| 29 | Kubernetes deployment | Deploy to cluster and expose games.palengke.es. | Done. |
| 29A | Urgent Bang-Sak domain audit | Inventory DNS, edge TLS/proxy, GitOps route, CORS/CSP, docs/tools, smoke checks, redirect, rollback, and cleanup ownership. | Investigation recorded; no live mutation until the urgent phase is executed. |
| 29B | Urgent dual-host migration | Add `bangsak.palengke.es` TLS/edge/GitOps routing while leaving `games.palengke.es` unchanged. | Both hostnames serve the same verified healthy build. |
| 29C | Urgent canonical cutover | Make `bangsak` canonical and redirect old `games` paths to it. | New URL passes full guest/auth/Photon checks and old URLs preserve paths when redirecting. |
| 29D | Urgent old-host cleanup | Remove obsolete old-host application routing/references after the observation window and record redirect retirement policy. | No active dependency or orphaned config remains; rollback is documented. |
| 30 | Monitoring and maintenance | Structured logs, request correlation, production verification, release/version records, operations, rollback, backup/config, and known issues. | Done. |
| 31 | Polish and content expansion | Plan maps, art, skins, badges, events, sound, mobile, anti-cheat, tournaments, power-ups, and selectable components after production foundations. | Ready for review; implementation order, dependencies, gates, and owner decisions are documented. |
| 32 | Photon Fusion Shared multiplayer vertical slice | Replace the room/movement/action scaffolds with a real two-client WebGL Photon path. | Done; owner approved the two-client Photon vertical slice. |
| 33 | Multiplayer integrity and anti-cheat | Validate authority, rate limits, action outcomes, result/reward submission, and abuse cases. | Done; owner approved after the production `0.33.2` Photon room-connectivity hotfix. |
| 34A | Android release decisions | Lock the package ID, minimum Android version, orientation, reference device, Play Console account type, and privacy/support contacts. | Done and owner-approved on 2026-07-12. No build or Play Console mutation was made. |
| 34B | Android debug build | Add a reproducible Unity Android debug-APK build without signing or store work. | Done and owner-approved on 2026-07-12 after the acceptance-criteria audit. |
| 34C | Android touch layout | Make the existing HUD and controls fit touch screens and safe areas. | Done and owner-approved on 2026-07-12 after installing and reviewing Android build `0.34.1`. |
| 34D | Mobile accessibility | Add readable text/contrast, reduced-motion behavior, and visual alternatives for important audio cues. | Done and owner-approved on 2026-07-12 after reviewing Android build `0.34.2`. |
| 34E1 | Android Photon create/join | Verify room creation and joining between two Android clients only. | Done; all acceptance criteria passed the authorized self-review on build `0.34.3`. |
| 34E2 | In-game leave confirmation | Add an accessible Leave Game control and confirmation during active multiplayer play. | Ready for visual review: `0.34.4` passes Cancel/confirm behavior and 227 tests; screenshots require explicit owner approval. |
| 34E3 | Photon leave cleanup and room rules | Remove a voluntary leaver, free the slot, update counts, and apply deterministic Taya/authority/last-player rules. | Remaining clients agree on roster and round state with no ghost player. |
| 34E4 | Android pause/resume | Handle Android pause, background, and resume only. | A backgrounded client resumes into the same coherent room and round. |
| 34E5 | Android disconnect/reconnect | Handle involuntary disconnect and room-code reconnect only. | A disconnected Android client rejoins and both clients finish one agreed round. |
| 34F | Android performance | Measure and meet the agreed frame-time, memory, temperature, and download-size budgets. | The reference device completes one round within the recorded budgets. |
| 34G | Signed Play bundle | Produce a versioned, signed Android App Bundle while keeping signing secrets outside Git. | The `.aab` validates locally and its version/signing fingerprints are recorded. |
| 34H | Play internal test | Create the Play Console app and publish only to the internal testing track. | The owner installs Bang-Sak from its Play Store tester link. |
| 34I | Play listing and compliance | Add the store listing, screenshots, privacy policy, Data safety declaration, content rating, audience, and required app-content answers. | Play Console shows no unresolved required listing/app-content task. |
| 34J | Play production access | Complete the required closed test or other account-specific production-access gate. | Play Console grants production access; tester evidence is recorded when required. |
| 34K | Public Play Store release | Submit a staged production release, verify the public listing/install, and record rollback/support steps. | Bang-Sak is publicly downloadable from Google Play and a clean device completes the install/open smoke test. |

Phases 32–41, including the small Phase 34A–34K Android/Google Play track, are
ordered in the [Phase 31 expansion plan](phase-31-polish-content-expansion.md).
The detailed Android gates are in the
[Phase 34 Android and Google Play roadmap](phase-34-android-play-roadmap.md).
Phase 34E2 is the current implementation checkpoint. Phase 34E3 and later
remain blocked by the review rule.

The requested closer camera, darkness, cone-only vision, and self-only minimap
are split into future Phases 42A–42E in the
[Phase 31 expansion plan](phase-31-polish-content-expansion.md). They are
planning only and are not part of the current Android Photon phase.

The urgent hostname track is documented in the
[domain migration roadmap](urgent-domain-migration-roadmap.md). After the
current Phase 34E2 visual review, Phases 29A–29D take priority over Phase 34E3.

## Review rule

- Codex may self-review a nonvisual phase and continue only when every recorded
  acceptance criterion has objective evidence.
- Any phase that adds or modifies visible components must stop with screenshots
  and wait for explicit owner approval.
- A failed or incomplete criterion always stops the sequence.

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
