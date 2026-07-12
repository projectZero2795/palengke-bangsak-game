# Phase 33: Multiplayer integrity and anti-cheat baseline

## Outcome

Phase 33 changes the Phase 32 Photon path from direct peer-authored state to a
request/confirmation model. Non-authority clients request movement, Bang/SAK,
and restart actions. The room creator validates each request, recalculates the
result from its scene state, applies it, and broadcasts confirmed state.

This is a Shared Mode baseline. It protects normal rooms from forged peer
messages and common replay/rate/geometry abuse, but the room creator is still a
player client rather than a dedicated trusted server.

## Authority boundary

- The room creator owns the timer, result, catch state, action outcomes,
  restart, and persistent round submission.
- Each roster member receives a random 32-character credential directly from
  the creator. Credentials rotate when the Photon roster changes and refresh
  every five seconds.
- Client requests carry the credential assigned to their dense roster slot.
- Creator confirmations are sent separately to each recipient with that
  recipient credential.
- Credentials and payload contents are never written to logs.
- A bounded privacy-safe audit warning records only message kind, roster slot,
  rejection reason, and aggregate rejection count.

## Rejected request classes

The authority rejects:

- invalid or out-of-roster sender slots;
- missing, incorrect, stale, or oversized credentials;
- replayed or non-increasing envelope, movement, or action sequences;
- malformed, oversized, unknown-version, or unknown-kind envelopes;
- non-finite movement values and movement input above unit magnitude;
- movement outside the map bounds or beyond the elapsed-time speed envelope;
- movement traffic above the 100 Hz abuse ceiling;
- movement or actions before/after the running round state;
- Bang from a Hider, SAK from Taya, and unknown action/facing enums;
- missing, oversized, unknown, or already-caught named-Bang targets;
- Bang/SAK faster than the authority cooldown;
- client-claimed origin, point, target, display name, or outcome;
- restart before the round finishes or restart request flooding.

For a remote Bang/SAK request, the creator uses the actor position, requested
facing direction, current colliders, walls, action range, role, called name,
and current caught state to calculate the authoritative outcome. Claimed hit
outcomes and geometry are ignored.

## State reconciliation

Confirmed round snapshots now include:

- round state, result, timer, Hider counts, and round number;
- a per-player caught-state bit mask;
- Taya counter state;
- a stable authority round ID.

These fields correct rejected local prediction and keep clients on the same
timer/result. Movement and action confirmations are accepted only from the
current room authority with the recipient credential.

## Score and reward boundary

- Only the connected Photon room creator can submit a finished multiplayer
  score.
- Local/offline and non-authority client rounds never submit production scores.
- The creator supplies one stable authority round ID per round, including after
  restart, so retries remain idempotent.
- The existing Palengke API still validates the score range, limits users to 10
  new round IDs per minute, returns the original result for duplicate round
  IDs, calculates coin rewards server-side, and caps rewards at 50 coins.

The API does not yet cryptographically verify a Photon-issued round receipt.
Therefore a modified room creator can still submit a bounded fabricated score.
Tournament-grade rewards require a trusted server or a backend-verifiable
match receipt in a later security phase.

## Automated verification

Run Unity EditMode tests:

```bash
/Applications/Unity/Hub/Editor/2022.3.50f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics \
  -projectPath unity \
  -runTests -testPlatform EditMode \
  -testResults /tmp/bang-sak-phase33-editmode.xml \
  -logFile /tmp/bang-sak-phase33-editmode.log
```

The Phase 33 tests cover credential forgery, replay, malformed values,
out-of-state requests, movement bounds/speed/rate, role violations, action
cooldowns, missing/oversized names, restart abuse, credential rotation, and
authority-side recalculation of forged and valid action outcomes.

Build production WebGL:

```bash
/Applications/Unity/Hub/Editor/2022.3.50f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics \
  -projectPath unity \
  -executeMethod Palengke.BangSak.Editor.Phase28BWebGlBuild.BuildCommandLine \
  -logFile /tmp/bang-sak-phase33-webgl.log \
  -quit
```

Expected `build-info.json`: phase `33`, version `0.33.0`.

## Verified evidence

Verified on 2026-07-11:

- `211` Unity EditMode tests passed with `0` failures;
- the Unity `2022.3.50f1` WebGL build completed at `37,436,788` bytes;
- two clean local WebGL clients created/joined room `1234`, activated the
  authority credential, loaded the same scene, kept timer/Hider state aligned,
  and produced no integrity rejection during normal movement or Bang;
- production source is commit `da6d2e2` and image
  `registry.renzlab.com/palengke/bang-sak:da6d2e2`;
- production image digest is
  `sha256:13dc6b3ac86c09b0eca8ff47788c6400f4280a7d148c3f7f02d513bc16fc46de`;
- GitOps commit `e466c5c` reconciled `Synced` and `Healthy`;
- both production replicas became Ready with zero restarts;
- `tools/verify-production.sh` passed against `https://games.palengke.es` and
  confirmed build `0.33.0`, security headers, WebGL assets, and the live
  leaderboard API.

### Photon connectivity hotfix verification

Verified on 2026-07-12 for release `0.33.1`:

- `213` Unity EditMode tests passed with `0` failures;
- the WebGL build completed at `37,442,211` bytes;
- the browser logged `direct EU secure name server` before Photon startup;
- a local WebGL client created room `1234`, and a second clean client joined as
  Player 2 with the authority credential active;
- production created room `1234` through the direct EU route and reported
  `Connected`, `1/4 players`, and `EU`;
- production verification passed build metadata, WebGL MIME/assets, request
  IDs, CORS, and the live leaderboard API;
- Argo CD reconciled GitOps revision `33eaa42` as `Synced` and `Healthy`, with
  two Ready `622a8c2` replicas and zero restarts.

Verified on 2026-07-12 for follow-up release `0.33.2`:

- the WebGL retry wait uses Unity coroutine timing instead of `Task.Delay`;
- `213` Unity EditMode tests passed with `0` failures;
- the final local WebGL artifact created room `1234` through the direct EU
  route;
- the permanent production URL `https://games.palengke.es` loaded `0.33.2`
  without a version query and created room `1234` as `Connected`, `1/4`, `EU`;
- Argo CD reconciled GitOps revision `84a3305` as `Synced` and `Healthy`, with
  two Ready `af0bfc7` replicas and zero restarts.

## Owner review in production

1. Open `https://games.palengke.es` in two separate browser tabs.
2. Tab 1: choose `ROOM`, then `CREATE`; confirm room `1234`, `1/4`, and `EU`.
3. Tab 2: choose `ROOM`, then `JOIN 1234`; confirm `2/4` and the message
   `authority credential active`.
4. Tab 1: choose `BACK`, then `PLAY`.
5. Confirm both clients enter Round 1, tab 1 is Taya, tab 2 is a Hider, and
   both timers/Hider counts stay aligned.
6. Move both players. Confirm remote movement remains smooth and no normal-play
   integrity warning appears in the browser Console.
7. Use named Bang and SAK normally. Confirm only in-range, correct-role,
   correct-name actions affect caught/result state on both clients.
8. Press action controls repeatedly during cooldown. Confirm they do not create
   duplicate outcomes.
9. Let the timer finish. Confirm both clients show the same result.
10. Request restart from tab 2. Confirm both clients enter Round 2 once.
11. If signed in to Palengke, confirm only the room creator persists the round
    and the leaderboard/coin balance does not duplicate on reload.

## Accepted baseline limitations

- The room creator is trusted because Fusion Shared Mode has no dedicated game
  server. This is not tournament-grade anti-cheat.
- Credential grants use the Shared reliable path whose callback does not expose
  a trustworthy sender. Periodic creator refresh recovers stale/forged grants,
  but sustained room denial of service remains possible.
- The authority validates movement speed and bounds but does not reproduce the
  exact physics path between snapshots.
- Manual rejoin still recreates an actor instead of restoring an exact
  pre-disconnect gameplay state.
- The Palengke API applies range, rate, idempotency, and reward caps but cannot
  yet verify a Photon-signed round receipt.

## Stop rule

After production deployment and verification, stop for project-owner review.
Do not begin Phase 34 until Phase 33 is explicitly approved.
