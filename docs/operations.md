# Bang-Sak production operations

## Ownership and sources of truth

- Game code and container definition: `projectZero2795/palengke-bangsak-game`.
- Kubernetes desired state: `projectZero2795/palengke-gitops`.
- Identity, scores, rewards, and leaderboard: `Elis-Homelab/palengke`.
- Immutable images: `registry.renzlab.com/palengke/bang-sak:<git-commit>`.
- Canonical public endpoint: `https://bangsak.palengke.es`.
- Legacy redirect: `https://games.palengke.es` preserves the path and query
  through at least 2026-10-10. Its certificate remains in the automatic
  renewal inventory during that retention period.

Do not edit a running Deployment as a release mechanism. Update GitOps and let
Argo CD reconcile it.

The application IngressRoute accepts only `bangsak.palengke.es`. The old host
exists only at the public edge as a redirect; a direct old-host cluster request
should return `404`.

## Fast health check

```bash
./tools/verify-production.sh

ssh k8s-node-1 \
  'kubectl -n argocd get application palengke-prod && \
   kubectl -n palengke-prod get deployment,pods,service,pdb \
     -l app.kubernetes.io/name=bang-sak'
```

Expected state is `Synced`, `Healthy`, Deployment `2/2`, and two Ready pods.

## Logs and request correlation

Nginx writes JSON access events to stdout and warnings/errors to stderr. Health
probe traffic is excluded. Every served response includes `X-Request-ID`; the
same value is present as `request_id` in the access event.

```bash
ssh k8s-node-1
kubectl -n palengke-prod logs deployment/bang-sak --since=15m --prefix
kubectl -n palengke-prod logs deployment/bang-sak --since=15m \
  | python3 -m json.tool
kubectl -n palengke-prod logs deployment/bang-sak --since=1h \
  | grep '"status":500'
```

Investigate a report by recording UTC time, URL, request ID, browser/version,
game build metadata, pod, and backend response. Never request or paste access
tokens into an issue.

## Version identification

```bash
curl --fail https://bangsak.palengke.es/build-info.json
curl --head https://bangsak.palengke.es/ | grep -i x-request-id
ssh k8s-node-1 \
  'kubectl -n palengke-prod get deployment bang-sak \
   -o jsonpath="{.spec.template.spec.containers[0].image}{\"\\n\"}"'
```

A release record contains the Unity version, game source commit, immutable image
tag/digest, GitOps commit, Palengke backend commit, and verification date.

## Release procedure

1. Build WebGL using `Bang-Sak > Build > Phase 28B WebGL`.
2. Run the local HTTP and Docker browser checks from the Phase 28B/28C docs.
3. Build an image tagged with the game commit and push it to the registry.
4. Update only the Bang-Sak image in `palengke-gitops`.
5. Validate manifests with `kubectl apply --dry-run=server`.
6. Push GitOps and wait for Argo CD `Synced`/`Healthy` and rollout completion.
7. Run `tools/verify-production.sh` and the browser review.
8. Record the release and keep the phase issue open until owner approval.

## Rollback and incident response

For a bad image, point GitOps to the previous immutable image tag, push, wait for
Argo, and rerun production verification. Do not use mutable `latest` tags.

For an outage:

1. verify public TLS and `/healthz`;
2. check Argo, Deployment, pods, events, and JSON logs;
3. verify the registry certificate and image availability;
4. verify `palengke.es/api/backend/games/bang-sak/leaderboard` separately;
5. roll back only the failing component;
6. document UTC timeline, impact, root cause, recovery, and prevention.

## Backup and configuration

The game pods are stateless and require no volume backup. Restore sources are the
Git repositories and immutable registry images. Scores and coins live in the
Palengke MySQL database and follow its encrypted backup/restore workflow. GitOps
stores no secret values; it references the existing registry pull secret.

The registry and public TLS certificates are operational dependencies. The
dedicated `registry.renzlab.com` certificate is managed by Certbot and must be
included in certificate-expiry review.

## Error tracking plan

Current investigation uses browser Console output, JSON container logs,
Kubernetes events, Argo status, backend logs, and request IDs. A hosted error
tracker is intentionally not added without a privacy/data-retention decision.
If added later, scrub tokens and personal data, document retention and access,
and obtain owner approval in its own phase.
