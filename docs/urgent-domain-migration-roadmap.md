# Urgent domain migration: `games` to `bangsak`

## Goal

Move the canonical public game URL from `https://games.palengke.es` to
`https://bangsak.palengke.es` through small reversible phases, without an
unplanned outage or breaking identity, scores, rewards, leaderboard, Photon,
WebGL assets, or historical release evidence.

This is an urgent planning record only. It does not authorize a live routing,
certificate, redirect, DNS, or cleanup mutation by itself.

## Investigation snapshot

Checked on 2026-07-12:

- `games.palengke.es` resolves through `palengke.es` to `204.168.236.15` and
  returns a healthy HTTPS response;
- `bangsak.palengke.es` already resolves to the same IP through the existing
  DNS alias, so an additional DNS record is not currently required;
- `bangsak.palengke.es` fails certificate hostname verification because the
  public edge only has a dedicated `games.palengke.es` certificate;
- ignoring that TLS error reaches the edge default and returns `404`, because
  neither public Nginx nor Traefik currently matches the new host;
- Hetzner Nginx owns public HTTP challenge handling, TLS, request logs, and the
  reverse proxy to the cluster;
- its current `games.palengke.es` site is a manually managed file outside the
  game and GitOps repositories;
- its Let’s Encrypt certificate uses the webroot authenticator, is renewed by
  `certbot.timer`, and expires on 2026-10-02 if renewal fails;
- `projectZero2795/palengke-gitops` owns the in-cluster `bang-sak`
  `IngressRoute`, which currently matches only `games.palengke.es`;
- Argo CD is `Synced`/`Healthy`, both Bang-Sak pods are Ready, and the service
  and PodDisruptionBudget are healthy;
- backend CORS already permits `https://*.palengke.es`;
- the game CSP already permits Palengke subdomains for connections and frame
  ancestry;
- current README, operations, architecture, production verification defaults,
  and active release instructions still use `games.palengke.es`;
- historical release records should retain the hostname that was true when
  those releases were verified.

## Urgent baby-step phases

| Phase | Only this work | Acceptance check | Rollback boundary |
| ---: | --- | --- | --- |
| 29A | Approve this dependency inventory, source-of-truth gap, order, smoke matrix, redirect policy, and rollback plan. | Every production dependency has an owner and command/check; no live mutation. | Documentation revert only. |
| 29B | Add the new Nginx HTTP/TLS proxy and renewable certificate plus dual-host GitOps routing. Keep `games` unchanged. | Both hostnames serve the same healthy build; new TLS, WebGL headers/MIME, Photon, guest/auth API flows pass. | Remove only the new edge site/certificate renewal entry and revert the new GitOps host. |
| 29C | Make `bangsak` canonical in active docs/tools/links and turn `games` into a path/query-preserving permanent redirect. | Canonical checks pass; old URLs redirect to the same new path; authenticated score/reward and leaderboard pass. | Restore the old proxy and defaults while dual-host cluster routing remains. |
| 29D | After the approved observation window, remove obsolete old-host application routing/references and document redirect retention or retirement. | No active application dependency uses `games`; no orphaned config; Certbot, Nginx, GitOps, logs, and rollback runbook are coherent. | Restore old GitOps host and edge proxy from the recorded configuration. |

## Required migration smoke matrix

Each live phase runs only the checks it owns:

- DNS A/AAAA/CNAME resolution and TLS hostname/expiry;
- `/healthz`, `/build-info.json`, HTML, WebAssembly and data MIME/cache headers;
- CSP, CORS preflight, frame policy, and request ID;
- main menu and local round browser console;
- Photon room create/join in two clients;
- guest leaderboard fallback;
- authenticated Palengke identity, score/reward, coins, and leaderboard;
- Argo `Synced`/`Healthy`, two Ready pods, service/PDB, rollout, and logs;
- old-host same-path redirect after cutover;
- explicit rollback verification.

## Priority

Finish the current visual Phase 34E2 review first. Then execute urgent Phases
29A–29D before starting Phase 34E3 or another feature phase.
