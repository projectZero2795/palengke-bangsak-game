# Phase 31 — Polish and content expansion plan

## Goal

Turn the post-release ideas into small, ordered, reviewable phases without
changing gameplay during this planning phase.

Phase 31 does not implement new maps, art, skins, badges, events, audio,
Android, Google Play distribution, power-ups, tournaments, Photon transport,
or anti-cheat behavior. Each approved follow-up phase must still be
implemented, tested, pushed, and reviewed separately.

## Planning rules

- Preserve the corrected core rule: Taya catches with `Bang + player name` and
  Hiders counter with safe close-range `SAK`.
- Keep every action playful and community-safe: no realistic weapons, knives,
  blood, gore, death, or lethal framing.
- Never sell or persistently award competitive power. Skins, badges, and event
  rewards are cosmetic/status-only; gameplay power-ups are temporary match
  pickups available under the same rules to every eligible player.
- The server or multiplayer state authority decides movement, hits, catches,
  pickups, rewards, and results. The client only requests actions and presents
  confirmed state.
- Every map, cosmetic, sound set, event, and gameplay component uses a stable
  ID, version, compatibility metadata, and migration/retirement notes.
- Audio is never the only gameplay signal. Important sounds also have readable
  visual feedback.
- Desktop WebGL and mobile layouts remain usable after every phase.
- One phase is active at a time, followed by project-owner review.

## Dependency order

```text
Authoritative multiplayer
  -> anti-cheat baseline
  -> mobile and accessibility baseline
  -> audio and art polish
  -> maps and selectable component registry
  -> cosmetics and badges
  -> role power-ups
  -> seasonal events
  -> tournaments
```

Content that affects a match cannot be released before authoritative
multiplayer validation. Tournaments cannot be released before the anti-cheat,
result-integrity, moderation, and operational gates pass.

## Proposed follow-up phases

| Phase | Deliverable | Included | Review gate |
| ---: | --- | --- | --- |
| 32 | Authoritative Photon vertical slice | Import/configure the approved Photon Fusion SDK; real create/join/leave; two-client spawn, movement, Bang/SAK, timer, and result sync; reconnect/leave behavior. | Two separate clients complete the same round and agree on roles, actions, timer, and result. Guest/local fallback still works. |
| 33 | Multiplayer integrity and anti-cheat baseline | Validate role, range, cooldown, target name, SAK proximity, movement envelope, round state, duplicate events, score submission, and reward idempotency at the authority/API boundary; add rate limits and privacy-safe audit events. | Forged, replayed, impossible, duplicate, or out-of-state actions are rejected without breaking valid play. |
| 34A | Android release decisions | Lock package ID, current Play requirements, minimum Android version, orientation, reference device, account type, and privacy/support contacts. | Owner approves the decision record; no Android build is made. |
| 34B | Android debug build | Reproducible Unity debug APK. | APK installs and opens the main menu on the reference device. |
| 34C | Android touch layout | Touch controls, safe areas, notches, and screen-size layout only. | A local touch round completes with no critical overlap. |
| 34D | Mobile accessibility | Readable text/contrast, reduced motion, and non-audio cues only. | Each option has one visible owner-checkable result. |
| 34E1–34E5 | Android Photon lifecycle and voluntary leave | Split create/join, leave confirmation, remaining-room cleanup/rules, pause/resume, and reconnect into separate checkpoints. | Each checkpoint has one focused two-device checklist and stops for approval. |
| 34F | Android performance | Reference-device FPS/frame time, memory, temperature, and size budgets only. | One measured round meets the recorded budgets. |
| 34G | Signed Play bundle | Versioned signed AAB with external signing-secret handling. | Bundle and certificate/version metadata validate locally. |
| 34H | Play internal test | Play Console app plus internal-testing release only. | Owner downloads and installs from the Play Store tester link. |
| 34I | Play listing and compliance | Listing, privacy policy, Data safety, content rating, audience, and required app-content declarations. | Play Console has no unresolved required listing/app-content task. |
| 34J | Play production access | Account-required closed testing or equivalent production-access gate. | Play Console grants production access. |
| 34K | Public Play Store release | Staged production rollout, public listing/install verification, support, and rollback. | Bang-Sak is publicly downloadable and opens on a clean Android device. |
| 35 | Sound and settings | Versioned friendly SFX for menu, movement, Bang, SAK, reveal clues, pickup, result, and ambient barangay/palengke sound; master/music/SFX controls and persistence. | Sound can be muted, never provides an exclusive clue, respects settings, and remains safe/non-startling. |
| 36 | Character and environment art polish | Replace selected placeholders with approved top-down Filipino barangay/palengke sprites, animation variants, readable role/action effects, and an asset import/performance budget. | Approved reference scenes remain readable at gameplay scale on desktop and mobile; colliders/game rules do not change accidentally. |
| 37 | Map expansion and selectable components | Add one new map first; validated spawn/route/reveal cells; map metadata/versioning; minimum selectable component registry; admin defaults and compatible room map voting. | Both maps pass route, spawn, camera, collision, reveal, mobile readability, and two-client synchronization tests. |
| 38 | Cosmetic skins and badges | Cosmetic-only character variants, badge definitions, ownership/equip API flow, fallback assets, contrast rules, and moderation-safe names. | Cosmetics never alter hitboxes, speed, cooldown, range, concealment, rewards, or map visibility; missing assets fall back safely. |
| 39 | Random role power-ups | Implement the existing power-up proposal with authoritative randomized valid spawn cells, role validation, rarity/duration/respawn configuration, synchronized pickup state, and visible HUD feedback. | Every pickup is fair, bounded, synchronized, tested, and available without purchase; core play remains viable without a pickup. |
| 40 | Seasonal events | Server-configured start/end times, seasonal map/object/cosmetic variants, opt-safe fallback content, version compatibility, and rollback/expiry behavior. | An event can start and end without a client rebuild, expired content falls back safely, and core queues remain playable. |
| 41 | Tournaments | Server-owned eligibility, bracket/round lifecycle, result verification, disconnect policy, moderation/admin tools, privacy limits, and operational runbook. | A staged tournament completes without accepting client-authored results, duplicate rewards, or unresolved disconnect outcomes. |
| 42A | Closer perspective camera framing | Move the 2D gameplay camera slightly closer and tune framing only. | One local route remains readable and playable while the scene has a stronger depth impression. |
| 42B | Dark ambient visibility | Dim the map and every player while preserving accessible HUD/action readability. | All actors feel placed in darkness without losing essential role/action feedback. |
| 42C | Local cone-only visibility | Add a local-facing vision mask with a small safe near-player area. | Each client sees actors/world only inside its own cone; HUD stays visible. |
| 42D | Safe-area minimap shell | Add only the top-right minimap frame, map bounds, and static landmarks. | The empty marker-free map fits mobile safe areas without HUD overlap. |
| 42E | Self-only minimap marker | Add only the local player position/facing marker. | Each client sees itself and never sees another player on its minimap. |

Phase numbers become implementation commitments only after this Phase 31 plan
is approved. Phase 34A–34K is expanded in the
[Android and Google Play roadmap](phase-34-android-play-roadmap.md). The existing
selectable-component registry issue is consumed by Phase 37, and the existing
random power-up issue is consumed by Phase 39.

## Random power-up decisions required before Phase 39

Taya candidates:

- reduced named-Bang cooldown across Hider targets;
- longer Bang range;
- a temporary no-name-required hit effect;
- a safe recovery/rescue state (the previous “resurrection” wording must be
  replaced with a non-death rule before implementation);
- teleport to a validated destination.

Hider candidates:

- reduced SAK cooldown—the earlier “knife” wording is not allowed;
- longer concealment before reveal clues;
- cooldown-limited dash;
- teleport to a validated destination.

The no-name-required effect changes the signature Bang-Sak rule, so it requires
a dedicated owner-approved playtest decision. Teleports must never enter solid,
hidden, unsafe, out-of-bounds, or opponent-overlap cells.

## Common acceptance matrix

Every implementation phase after Phase 31 must document and pass the applicable
rows before owner review.

| Area | Required evidence |
| --- | --- |
| Core gameplay | Existing movement, roles, named Bang, SAK, timer, win/loss, restart, menu, identity, score, coins, and leaderboard behavior still passes. |
| Multiplayer | Two-client state agrees under normal play, reconnect/leave, duplicate messages, and simulated latency; no client decides authoritative results. |
| Desktop WebGL | Production verification script passes; supported desktop browser smoke test has no new critical console error or layout obstruction. |
| Mobile/Android | Agreed reference device completes a round; touch targets, notches/safe areas, orientation policy, text, performance, pause/background, and reconnect are checked. |
| Safety | No realistic weapon/knife, blood, gore, death, lethal framing, hostile animal behavior, unsafe chat/content, or culturally disrespectful asset. |
| Fairness | No cosmetic or paid competitive advantage; role/map/pickup selection follows published, synchronized rules. |
| Accessibility | Essential information has visual feedback, readable contrast/text, and does not rely only on color, sound, or rapid motion. |
| Security | Invalid/replayed/rate-limited actions fail closed; secrets and trusted calculations remain outside the client; logs avoid tokens and unnecessary personal data. |
| Operations | Immutable release, release note, rollback path, known-issue update, two Ready production replicas, and public smoke verification. |

## Performance budgets

Each implementation phase records its before/after measurements. Phase 34 will
lock the reference devices and exact budgets; until then, the minimum gates are:

- no new sustained frame-time regression above 10% in the same reference scene;
- 30 FPS minimum during normal play on the agreed mobile reference device;
- no unbounded network event, log, object, texture, or audio growth;
- no additional production request failure in the public verification suite;
- new content must define memory/download impact and a removal/rollback path.

## Owner decisions by phase

- Phase 32: Photon plan/account, region policy, room size, reconnect rule.
- Phase 34A: package ID, current Play requirements, Android minimum version,
  reference device, orientation, Play Console account type, and privacy/support
  contacts.
- Phase 34G: signing/upload-key owner and secure storage path.
- Phase 34H: internal tester list.
- Phase 34I: listing copy/assets, countries, audience, privacy, Data safety, and
  content-rating answers.
- Phase 34J: the production-access test shown by the owner's Play Console.
- Phase 34K: rollout percentage and final public-release approval.
- Phase 35: audio direction, language needs, and volume defaults.
- Phase 36: first approved sprite/environment reference set.
- Phase 37: first additional map theme and map-voting rule.
- Phase 38: first skins/badges and whether coins can unlock cosmetics.
- Phase 39: approved power-up subset, durations/rarity, and decisions on
  no-name Bang and safe recovery.
- Phase 40: first event theme, schedule/time zone, and reward set.
- Phase 41: tournament size, eligibility, disconnect policy, moderation owner,
  and reward rules.
- Phase 42A: exact closer camera framing approved on the reference phone.
- Phase 42B: darkness strength and minimum accessible contrast.
- Phase 42C: cone angle/range and the near-player safe visibility radius.
- Phase 42D: minimap size and top-right safe-area placement.
- Phase 42E: self-marker appearance; other-player markers remain prohibited.

## Phase 31 review

Confirm that:

1. all requested expansion areas are represented;
2. the dependency order is acceptable;
3. cosmetics remain non-competitive and power-ups remain free match pickups;
4. the proposed phases are small enough for the stop-and-review rule;
5. the owner-decision list is sufficient before implementation begins.

After approval, close Phase 31, create/update the approved follow-up issues, and
begin Phase 32 only.

## Exit criteria

- Maps, art, skins, badges, events, sound, Android/Google Play release,
  anti-cheat, tournaments, power-ups, and component selection have an ordered
  implementation path.
- Dependencies, safety/fairness rules, acceptance evidence, performance gates,
  and owner decisions are explicit.
- No future-phase gameplay or content implementation is included.
- The project owner approves the plan.
