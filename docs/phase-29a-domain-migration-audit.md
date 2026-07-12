# Phase 29A: Bang-Sak domain migration audit

## Goal

Approve the dependencies, ownership, execution order, acceptance checks, and
rollback boundaries for moving the canonical public game URL from
`games.palengke.es` to `bangsak.palengke.es`. This phase changes documentation
only and makes no production mutation.

## Verified starting state

Checked on 2026-07-12:

| Dependency | Owner/source of truth | Verified state |
| --- | --- | --- |
| Public DNS | Existing `*.palengke.es`/apex DNS | Both names resolve through `palengke.es` to `204.168.236.15`; no DNS change is required. |
| Public HTTP/TLS edge | Manual Nginx config on `ssh hetzner` | `games` returns HTTPS 200. `bangsak` reaches the edge but receives the default certificate and route, so verified TLS fails and insecure HTTPS returns 404. |
| Certificate renewal | Certbot webroot plus `certbot.timer` on Hetzner | The dedicated `games` ECDSA certificate is valid through 2026-10-02; the timer is enabled and active. No `bangsak` certificate exists. |
| Cluster route | `projectZero2795/palengke-gitops`, `apps/palengke-prod/ingressroute-bang-sak.yaml` | The Traefik rule matches only `Host(games.palengke.es)`. |
| Workload | Argo CD application `palengke-prod` | Synced/Healthy; Deployment 2/2; two Ready pods on separate nodes; service and PDB healthy. |
| API origin policy | `Elis-Homelab/palengke` backend configuration | `https://*.palengke.es` is allowed, so the target origin is already covered. |
| WebGL security policy | Bang-Sak container headers | Palengke subdomains are already allowed by the active CSP/frame/connect policy. |
| Active user-facing references | Bang-Sak README, operations, architecture, verification tool, and active procedures | They still point to `games` and change only during Phase 29C. Historical release evidence remains unchanged. |

The public edge file is currently outside Git. Phase 29B therefore records a
timestamped root-readable backup before editing and stores the reviewed active
shape in this runbook. GitOps remains the sole source of truth for the cluster
route; no live Kubernetes object is edited directly.

## Approved execution order

1. Phase 29B adds `bangsak` as a second GitOps host while retaining `games`.
2. Add and validate an HTTP-only Nginx ACME/proxy site for `bangsak`.
3. Issue a dedicated ECDSA certificate with Certbot webroot, enable HTTPS, and
   prove renewal with a Certbot dry run for that certificate lineage.
4. Run the dual-host automated and browser smoke matrix. Do not redirect yet.
5. Phase 29C changes active references/defaults and then turns `games` into a
   path/query-preserving `308` redirect to `bangsak`.
6. Observe the redirect and canonical endpoint for seven days. Phase 29D may
   then remove the old host from the application route, but the edge redirect
   and renewable old certificate remain for at least 90 days unless the owner
   explicitly changes that retention decision.

## Acceptance checks and commands

Automated public checks:

```bash
dig +short bangsak.palengke.es A
curl --fail https://bangsak.palengke.es/healthz
BANG_SAK_GAME_URL=https://bangsak.palengke.es \
  BANG_SAK_ORIGIN=https://bangsak.palengke.es \
  ./tools/verify-production.sh
openssl s_client -connect bangsak.palengke.es:443 \
  -servername bangsak.palengke.es </dev/null 2>/dev/null \
  | openssl x509 -noout -subject -dates -ext subjectAltName
```

Infrastructure checks:

```bash
ssh hetzner 'sudo nginx -t && systemctl is-active nginx certbot.timer'
ssh k8s-node-1 \
  'kubectl -n argocd get application palengke-prod && \
   kubectl -n palengke-prod get deployment,pods,service,pdb \
     -l app.kubernetes.io/name=bang-sak'
```

The browser matrix checks the Main Menu and local round without console errors,
two-client Photon create/join, guest leaderboard fallback, and—using the
existing signed-in Palengke browser profile—identity, server-calculated reward,
coins, persisted score, and leaderboard. Tokens must never be copied into logs,
commands, documentation, or issues.

After cutover, verify redirect preservation with a harmless path and query:

```bash
curl -sS -o /dev/null -D - \
  'https://games.palengke.es/build-info.json?migration=1'
```

The response must be `308` with
`Location: https://bangsak.palengke.es/build-info.json?migration=1`.

## Monitoring and rollback

Monitor the separate edge access/error logs, pod JSON logs, request IDs, Argo
health, Deployment availability, and both public `/healthz` endpoints during
29B. During 29C, monitor canonical health plus redirect status and location.

Phase 29B rollback removes only the new enabled Nginx site, restores its
timestamped backup if present, reloads Nginx after `nginx -t`, and reverts the
GitOps commit that added `bangsak`. The healthy `games` proxy remains untouched.

Phase 29C rollback restores the backed-up `games` proxy file and the previous
active docs/tool defaults while retaining dual-host cluster routing. Certificate
lineages are not deleted during an incident; deletion is a separate cleanup
decision after traffic is healthy.

## Acceptance result

Every production dependency has an identified owner, a verification command,
an ordered mutation step, and a component-scoped rollback. The redirect and
certificate-retention policies are explicit. No live configuration changed in
Phase 29A.
