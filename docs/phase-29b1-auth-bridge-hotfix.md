# Phase 29B1: Signed-in Palengke auth bridge hotfix

## Problem

Palengke stored its access token in `palengke.es` local storage. Browser origin
isolation correctly prevented `bangsak.palengke.es` and `games.palengke.es`
from reading that storage, so the game remained in Guest mode even when the
owner was signed in.

## Implementation

- Palengke commit `919df92a069848d1a55400d0df440cc1217f6b31` exposes a
  no-store `/api/game-auth/bang-sak` bridge document.
- The bridge accepts messages only from the exact `games` and `bangsak`
  origins, validates the parent window and a random 128-bit request ID, and
  responds only to the validated caller.
- Game commit `60a8e77d985664ed16389c21851b352128d2f484` requests the token
  through a hidden iframe and keeps it only in Unity runtime memory.
- Game commit `92fa8078435466d70ccaeb6cee80d39bb11f3522` permits only
  `https://palengke.es` as a framed auth source in the game CSP.
- Game commit `784cecce2df9323be3bc43eeda5edabb01e12d7f` versions every
  WebGL loader/data/framework/WASM URL so a returning browser cannot combine
  cached files from different releases.

No cookie or access token was printed, copied into a command, stored on a game
origin, documented, or added to an issue.

## Verification

- Palengke CI run `29207624448` passed dependency audit, typecheck, backend
  tests, vulnerability scan, database backup/migrations, listing-preservation
  checks, rollout, and frontend/backend smoke tests.
- The auth bridge returns `200`, `Cache-Control: private, no-store`, exact
  `frame-ancestors` for both game hosts, `nosniff`, and no permissive frame
  header.
- Unity EditMode: `228` passed, `0` failed.
- Both public production verifiers passed build `0.33.3`.
- Argo CD revision `b1efcddef22bfae30c7c05fcc07e2a1d4a9ddb63` is `Synced`
  and `Healthy`; both replicas are Ready on separate nodes.
- The signed-in game displayed `Palengke Spain`, two clients joined Photon room
  `1234`, and the authoritative round saved score `500`, awarded `5` coins,
  and persisted the account at leaderboard rank `#2`.
- Both Chrome clients had zero console warnings or errors.

## Rollback

Revert the Bang-Sak image to `92fa807`, wait for Argo CD to become
`Synced`/`Healthy`, and rerun both public verifiers. If the identity bridge is
the incident source, also revert Palengke commit `919df92` through its normal
CI workflow. Never disable origin validation or expose the token in a URL.
