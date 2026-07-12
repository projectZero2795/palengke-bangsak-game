# Phase 29B: Dual-host Bang-Sak routing

## Goal

Add `https://bangsak.palengke.es` alongside the unchanged
`https://games.palengke.es` endpoint. This phase does not redirect the old host
or change canonical documentation/tool defaults.

## Implemented production state

- GitOps commit `530a065` changed only the Bang-Sak Traefik rule to match both
  `games.palengke.es` and `bangsak.palengke.es`.
- Server-side Kubernetes dry-run accepted the IngressRoute before commit.
- Argo CD reconciled that exact revision as `Synced` and `Healthy`.
- A separate `/etc/nginx/sites-available/bangsak.palengke.es` edge site owns
  the target host's HTTP ACME path, HTTPS reverse proxy, and separate access and
  error logs.
- A dedicated ECDSA Let's Encrypt certificate for only
  `bangsak.palengke.es` is valid from 2026-07-12 through 2026-10-10.
- `certbot.timer` is enabled/active and a target-lineage renewal dry run passed.
- The original `games` Nginx site was backed up at
  `/root/bangsak-domain-migration-20260712T174240Z/` before the new site was
  enabled. Its live and backup SHA-256 values remain identical:
  `3f84bf397224045720d8c96c8a637577cc3b56fc8bddbbf593ff2047b89c84be`.

## Passed checks

- DNS resolves both names through `palengke.es` to `204.168.236.15`.
- Verified TLS uses `CN=bangsak.palengke.es` with a matching SAN.
- Both hosts return `200` and `ok` from `/healthz`.
- `tools/verify-production.sh` passes independently against both origins for
  build `0.33.2`, request IDs, WebAssembly/data MIME types, CORS, and the live
  leaderboard API.
- `build-info.json`, `WebGL.wasm`, and `WebGL.data` have identical SHA-256
  values through both hostnames.
- Chrome loaded the target Main Menu and one local round with no console error
  or warning.
- Two target-host Chrome clients created/joined room `1234`, loaded the shared
  game, and displayed the two-client roster without console errors. Fusion's
  normal pre-scene `NetworkRunner started with no scene` informational warning
  appeared once per client before the network scene loaded.
- The guest leaderboard displayed `No scores yet` rather than an unavailable
  error.
- Argo remains `Synced`/`Healthy`; Deployment is 2/2; both pods are Ready with
  zero restarts; the service and PDB remain healthy; no recent pod 5xx or target
  edge error-log entry was found.
- The old hostname still proxies the game directly and has not been redirected.

## Remaining acceptance gate

The selected Chrome profile is currently signed out of Palengke: its account
menu explicitly offers **Sign in**, and the game correctly reports
`Guest Player`. Therefore authenticated identity, reward, coins, persisted
score, and authenticated leaderboard have not been claimed as passed.

The owner must sign in to `https://palengke.es` in the retained Chrome tab.
Codex can then reload `https://bangsak.palengke.es`, complete one authenticated
round, and inspect the persisted result without reading or copying cookies or
tokens. Phase 29B and issue #66 remain open until that evidence passes.

## Rollback boundary

If the target host fails before acceptance:

1. disable only `/etc/nginx/sites-enabled/bangsak.palengke.es`;
2. validate and reload Nginx;
3. revert GitOps commit `530a065` and wait for Argo to reconcile;
4. rerun verification against the unchanged `games` hostname;
5. retain certificate files during diagnosis—certificate deletion is separate
   cleanup work.

The existing `games` edge site does not need restoration because Phase 29B did
not modify it.
