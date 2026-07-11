# Phase 29 — Kubernetes deployment and public release

## Goal

Release the approved Bang-Sak WebGL container through the existing Palengke
GitOps platform at `https://games.palengke.es` and activate the real public
leaderboard API.

## Release inventory

- game source commit: `9069ee6`
- game image: `registry.renzlab.com/palengke/bang-sak:9069ee6`
- game image digest: `sha256:431c5a0c4a6d417845f904b28470a190725f241c9447bd936084a217576be9e0`
- GitOps deployment commit: `3a3fda2`
- Palengke Bang-Sak API commit: `3b38423`
- Palengke Go security update: `0077f94`
- WebGL build version: `0.28.2`

## Production architecture

`games.palengke.es` terminates TLS at the existing public Nginx/Traefik path.
Traefik routes the request to the `bang-sak` ClusterIP service in
`palengke-prod`. Two hardened, non-root Nginx pods serve the immutable WebGL
image. Argo CD owns the desired Kubernetes state in `palengke-gitops`.

The Deployment includes:

- two replicas with rolling updates and topology spreading;
- startup, readiness, and liveness probes at `/healthz`;
- CPU/memory requests and limits;
- UID/GID `101`, dropped capabilities, no privilege escalation, read-only root
  filesystem, and `RuntimeDefault` seccomp;
- a PodDisruptionBudget with one replica always available;
- no service-account token mounted in the game pods.

## Public review

1. Open `https://games.palengke.es`.
2. Confirm MainMenu renders over HTTPS.
3. Open `HOW`, `SET`, `ROOM`, and `SCORES`.
4. Confirm `SCORES` says `No scores yet` instead of `Leaderboard unavailable`.
5. Select `PLAY` and confirm `PrototypeMap` loads with independent cooldown
   bars for Ana, Pedro, and Maria.
6. Confirm the browser Console has no blocking warnings or errors.

Authenticated owner check:

1. Sign in at `https://palengke.es` in the same browser.
2. Open `https://games.palengke.es` in a new tab.
3. Confirm the footer shows the Palengke identity instead of `Guest Player`.
4. Complete a round and confirm the result grants server-calculated coins.
5. Reopen `SCORES` and confirm the persisted score appears.

## Operational verification

```bash
ssh k8s-node-1
kubectl -n argocd get application palengke-prod
kubectl -n palengke-prod get deployment,pods,service,pdb -l app.kubernetes.io/name=bang-sak
kubectl -n palengke-prod rollout status deployment/bang-sak
kubectl -n networking get ingressroute bang-sak
```

Public endpoints:

```bash
curl --fail https://games.palengke.es/healthz
curl --fail https://games.palengke.es/build-info.json
curl --fail https://palengke.es/api/backend/games/bang-sak/leaderboard
```

## Rollback

For a bad game image, change the image in
`apps/palengke-prod/deployment-bang-sak.yaml` to the previous immutable tag,
commit, push, and wait for Argo CD to report `Synced` and `Healthy`.

For a complete public withdrawal:

1. revert GitOps commit `3a3fda2` and push;
2. wait for Argo CD to reconcile the reverted desired state;
3. because the current Argo application has pruning disabled, explicitly delete
   the `bang-sak` IngressRoute, Deployment, Service, and PodDisruptionBudget;
4. leave the immutable registry image available for investigation.

No production secret is stored in the game or GitOps repositories.

## Verification result

Verified on 2026-07-11:

- the immutable game image was pushed after renewing the expired registry TLS
  certificate with a dedicated automatically renewable certificate;
- all Kubernetes manifests passed server-side dry-run validation;
- Argo CD reported `Synced` and `Healthy`;
- both game replicas became Ready on separate nodes;
- public TLS, health, build metadata, WebAssembly MIME, data MIME, caching, and
  browser security headers passed;
- public MainMenu, guest leaderboard, and PrototypeMap rendered without browser
  Console warnings or errors;
- the Palengke backend security pipeline passed on Go `1.25.12`, backed up the
  database, ran migrations, deployed the API, and passed rollout smoke tests;
- public leaderboard and CORS/preflight requests succeeded;
- authenticated identity, reward, and persisted-score acceptance remains the
  owner review step because no active Palengke login was available in the
  controlled browser session.
