# Phase 30 — Monitoring and maintenance

## Goal

Make the public release diagnosable, repeatable, versioned, and recoverable
without recreating the monitoring platform intentionally removed from the
homelab.

## Deliverables

- JSON Nginx access logs on stdout and warning/error logs on stderr;
- per-response/request `X-Request-ID` correlation;
- automated public production verification;
- production operations and incident/rollback runbook;
- explicit version/release record;
- backup and configuration ownership;
- known-issues register and privacy-aware error-tracking plan.

## Review

```bash
./tools/verify-production.sh
curl --head https://bangsak.palengke.es/ | grep -i x-request-id
ssh k8s-node-1 \
  'kubectl -n palengke-prod logs deployment/bang-sak --tail=5 --prefix'
```

Confirm the public game still loads and plays after the logging image rollout.

## Production verification

Verified on 2026-07-11:

- source commit: `bc078b5`;
- immutable image: `registry.renzlab.com/palengke/bang-sak:bc078b5`;
- image digest: `sha256:c4fd4e388f0c1e90b5375ba51b103f8b6cad3922c2862a343c3dd6a01986a569`;
- GitOps commit: `fd4cb4c`;
- Argo CD application `palengke-prod`: `Synced` and `Healthy`;
- two Phase 30 game replicas: Ready;
- `./tools/verify-production.sh`: passed against `https://bangsak.palengke.es`;
- public `X-Request-ID` correlated with the same ID in the JSON pod log;
- production WebGL menu rendered successfully with no browser warnings or
  errors.

## Exit criteria

- Errors can be investigated from request ID through pod/backend logs.
- The public release verification script passes.
- Version, release, rollback, backup/config, and known-issue records exist.
- Releases are repeatable through immutable images and GitOps.
- No new monitoring service or secret is introduced.
