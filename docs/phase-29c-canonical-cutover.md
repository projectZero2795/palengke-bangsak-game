# Phase 29C: Canonical Bang-Sak hostname cutover

## Goal

Make `https://bangsak.palengke.es` the canonical public game URL while keeping
the accepted dual-host cluster route available for rollback. The legacy edge
host must preserve each path and query in a permanent redirect.

## Production change

- Active README, architecture, operations, monitoring, release procedure, and
  production-verifier defaults now use `bangsak.palengke.es`.
- The `games.palengke.es` HTTP and HTTPS edge servers return `308` to
  `https://bangsak.palengke.es$request_uri`.
- The HTTP ACME challenge location remains local and does not redirect.
- The old ECDSA certificate and renewal lineage remain installed and valid
  through 2026-10-02.
- The original proxy configuration is backed up at
  `/root/bangsak-domain-migration-20260712T211500Z/games.palengke.es.before-29c`
  with SHA-256
  `3f84bf397224045720d8c96c8a637577cc3b56fc8bddbbf593ff2047b89c84be`.
- The cluster IngressRoute still accepts both hostnames throughout Phase 29D's
  observation window, so rollback requires only the edge backup and active
  documentation/defaults.

## Acceptance evidence

- Nginx configuration validation passed and both Nginx and `certbot.timer`
  are active.
- HTTP and HTTPS old-root requests return `308` to the canonical root.
- `/build-info.json?migration=1&keep=%2Fpath` redirects to the same path and
  byte-for-byte query on the canonical host.
- `/deep/path/?a=1&b=two` preserves its trailing slash and query.
- A missing old-host ACME challenge returns local `404`, not a redirect.
- The default `tools/verify-production.sh` passes canonical build `0.33.3`,
  request IDs, WebAssembly/data MIME types, CORS, and a two-entry leaderboard.
- Canonical signed-in identity remained `Palengke Spain` with `5` coins and
  persisted score `500` at leaderboard rank `#2`.
- Two canonical clients joined Photon room `1234` as `JuanP` and `Maria`; the
  authoritative score/reward round completed before cutover and the session
  remained healthy through the edge change.
- Both final browser clients reported zero warnings or errors.
- Argo CD revision `b1efcddef22bfae30c7c05fcc07e2a1d4a9ddb63` remains
  `Synced` and `Healthy`; deployment `2/2` is available.

## Observation and rollback

Phase 29D observation begins 2026-07-12. Do not remove `games.palengke.es`
from the cluster application route before 2026-07-19. Keep the edge redirect
and its renewable certificate for at least 90 days unless the owner explicitly
changes that policy.

To roll back during observation, restore the recorded edge backup to
`/etc/nginx/sites-available/games.palengke.es`, run `nginx -t`, reload Nginx,
restore active docs/tool defaults, and rerun verification. No cluster change is
required because the dual-host IngressRoute remains live.
