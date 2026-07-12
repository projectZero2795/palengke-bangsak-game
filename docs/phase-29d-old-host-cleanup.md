# Phase 29D: Old-host application cleanup

## Decision

The owner explicitly instructed Codex on 2026-07-12 to run the checks and
continue, overriding the previously planned seven-day wait. Cleanup proceeded
only after the live edge log showed no post-cutover WebGL asset, API, Photon,
or user-session request on `games.palengke.es`.

The public edge redirect and renewable certificate are not removed. Retain
both through at least 2026-10-10, then review real redirect traffic before any
separate retirement decision.

## Application cleanup

- GitOps commit `a066cf1ba66fab8d51bf63bd7e7270b26d818e67` removed
  `games.palengke.es` from the Traefik application rule.
- Palengke commit `de584ed5265f022938c781efd19bb65eac8cb0a7` removed the
  old origin from the auth bridge allowlist and frame policy.
- Palengke deployment workflow `29209919095` passed dependency audit,
  typecheck, backend tests, vulnerability scans, database backup/migrations,
  listing-preservation checks, rollouts, and frontend/backend smoke tests.
- Its normal image update produced final GitOps revision
  `09ee4d16ba7146685f99cffe155caac103c2e61b` without reintroducing the
  old host.

Historical phase and release records retain `games.palengke.es` where it was
true at the time. Operations documentation retains it only as the intentional
edge redirect.

## Acceptance evidence

- Post-cutover old-host traffic contained the deliberate verification calls
  and two automated root-page probes only. No old-host WebGL asset or
  application-session request occurred. One probe followed the redirect to the
  canonical host in the same second.
- The public old host returns a path/query-preserving `308`.
- A direct cluster request with the old Host returns `404`; the canonical Host
  returns `200`.
- Argo CD revision `09ee4d16ba7146685f99cffe155caac103c2e61b` is
  `Synced` and `Healthy`; the IngressRoute is exactly
  ``Host(`bangsak.palengke.es`)``.
- Bang-Sak deployment `2/2` is available, with one Ready zero-restart pod on
  each node.
- The canonical production verifier passes build `0.33.3`, request IDs,
  WebGL MIME/CORS checks, and the two-entry leaderboard.
- The live auth document is no-store, names only
  `https://bangsak.palengke.es` in its allowlist and `frame-ancestors`, and
  contains no old-host reference.
- Nginx configuration is valid; Nginx and `certbot.timer` are active; edge
  errors are empty for the cutover period.
- Simulated renewal succeeded independently for the old redirect certificate
  and the canonical certificate.
- No GitOps monitoring/dashboard/log configuration contains the retired
  application host. All local Markdown links resolve.

## Retained edge ownership

`/etc/nginx/sites-available/games.palengke.es` remains the intentional 308
redirect and keeps its separate `games-palengke` access/error logs for
retention review. Its HTTP ACME location remains local. The original proxy is
backed up at:

`/root/bangsak-domain-migration-20260712T211500Z/games.palengke.es.before-29c`

Backup SHA-256:

`3f84bf397224045720d8c96c8a637577cc3b56fc8bddbbf593ff2047b89c84be`

## Rollback

For application rollback, restore the dual-host Traefik rule by reverting
GitOps commit `a066cf1`, restore the old auth origin by reverting Palengke
commit `de584ed`, and wait for both normal deployment workflows to pass. To
restore direct old-host service, also copy the recorded edge backup into the
active Nginx site, run `nginx -t`, reload Nginx, and rerun both host verifiers.
Never remove origin validation or expose the access token in a URL.
