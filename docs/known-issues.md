# Known issues

## Active limitations

- Photon Fusion Shared Mode now supports the Phase 32 two-client WebGL vertical
  slice. It is distributed client authority rather than a dedicated server;
  Phase 33 must harden sender validation, rate limits, action outcomes, and
  result/reward integrity before competitive use.
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
