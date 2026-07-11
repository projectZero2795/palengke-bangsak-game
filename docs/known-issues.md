# Known issues

## Active limitations

- Phase 33 authenticates state-changing requests to the player-hosted room
  creator and makes that authority recalculate action outcomes. Photon Fusion
  Shared Mode still has no dedicated trusted game server: a modified room
  creator can lie about its own state, disrupt a room, or attempt a capped API
  score. Do not treat this baseline as competitive/tournament security.
- Shared reliable callbacks do not expose a trustworthy peer sender for the
  server-proxied client-to-client path. Per-player credentials protect movement,
  action, round, and restart messages; grants are refreshed every five seconds
  to recover from stale or forged grant traffic, but a hostile peer can still
  attempt room-level denial of service.
- Authority movement validation rejects non-finite, out-of-bounds, over-speed,
  replayed, and flood-rate snapshots. It does not yet server-simulate the exact
  collision path between accepted snapshots.
- Local/offline rounds remain playable but no longer submit authenticated
  production scores. Persistent score and coin submission requires the Photon
  room authority and a per-round authority ID.
- Photon room rejoin is manual. Reloaded/disconnected clients must select
  `ROOM` and re-enter the same room code.
- Fusion reports a harmless warning when connecting from the main menu before
  the room creator selects the synchronized gameplay scene.
- The Unity WebGL artifact is generated and intentionally untracked. A licensed
  Unity Editor is required before building the container.
- Authenticated game identity depends on signing in through the shared Palengke
  authentication flow so the `.palengke.es` cookie exists. Guest play remains
  available when it does not.
- The leaderboard is empty until authenticated players finish and submit rounds.
- No metrics/error-tracking platform is installed. The homelab monitoring stack
  was intentionally removed; Phase 30 uses structured logs, probes, request IDs,
  and scripted public checks.
- External DNS from cluster nodes has shown transient timeouts. Kubernetes image
  pulls retry automatically, but releases must wait for two Ready replicas.

## Resolved during release

- The registry wildcard certificate expired on 2026-07-11. A dedicated
  automatically renewable `registry.renzlab.com` certificate replaced it.
- The Palengke backend pipeline was blocked by a Go TLS vulnerability in
  `1.25.11`; production now uses `1.25.12`.

Update this file when a limitation is fixed or a new production risk is found.
