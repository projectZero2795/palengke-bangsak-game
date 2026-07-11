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
curl --head https://games.palengke.es/ | grep -i x-request-id
ssh k8s-node-1 \
  'kubectl -n palengke-prod logs deployment/bang-sak --tail=5 --prefix'
```

Confirm the public game still loads and plays after the logging image rollout.

## Exit criteria

- Errors can be investigated from request ID through pod/backend logs.
- The public release verification script passes.
- Version, release, rollback, backup/config, and known-issue records exist.
- Releases are repeatable through immutable images and GitOps.
- No new monitoring service or secret is introduced.
