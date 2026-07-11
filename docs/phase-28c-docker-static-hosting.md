# Phase 28C — Docker static hosting for Unity WebGL

## Goal

Package the approved Phase 28B WebGL artifact in a reproducible, non-root
Nginx container. This phase validates local container hosting only. It does not
push an image or add Kubernetes, DNS, TLS, or a public release.

## Prerequisite

Build Phase 28B from Unity first:

1. Open `unity/Assets/Scenes/MainMenu.unity`.
2. Select `Bang-Sak > Build > Phase 28B WebGL`.
3. Confirm `unity/Build/WebGL/build-info.json` exists.

The WebGL artifact remains untracked. The Docker build fails intentionally when
that prerequisite is missing.

## Build the image

From the repository root:

```bash
docker build \
  --build-arg BANG_SAK_VERSION=0.28.2 \
  --build-arg VCS_REF="$(git rev-parse --short HEAD)" \
  --tag bang-sak:0.28.2 .
```

The minimal Docker context contains only the `Dockerfile`, Nginx configuration,
and generated WebGL output. Unity source, local configuration, and secrets are
excluded. The non-root Nginx base image is pinned to the digest verified during
this phase so rebuilding does not silently select a different base image.

## Local review

```bash
docker run --detach --rm \
  --name bang-sak-phase28c \
  --publish 8080:8080 \
  bang-sak:0.28.2

docker exec bang-sak-phase28c nginx -t
docker exec bang-sak-phase28c id
curl --fail http://127.0.0.1:8080/healthz
curl --fail http://127.0.0.1:8080/build-info.json
```

Open `http://127.0.0.1:8080` and confirm:

- MainMenu loads;
- `HOW`, `SET`, `ROOM`, and `SCORES` open;
- `PLAY` reaches `PrototypeMap`;
- each Hider keeps an independent cooldown bar;
- the browser Console has no blocking errors.

Stop the review container:

```bash
docker stop bang-sak-phase28c
```

### Review through the Codex VM

When Docker is running on `codex-github`, bind the container only to the VM
loopback interface:

```bash
ssh codex-github
cd /home/codex/personal/apps/palengke-bangsak-game
docker run --detach --rm \
  --name bang-sak-phase28c \
  --publish 127.0.0.1:18080:8080 \
  bang-sak:0.28.2-phase28c
```

In a second Mac terminal, create the review tunnel:

```bash
ssh -N -L 127.0.0.1:18080:127.0.0.1:18080 codex-github
```

Then open `http://127.0.0.1:18080`. Stop the tunnel with `Ctrl+C` after review.

## Container contract

- listens on unprivileged port `8080` as UID/GID `101`;
- exposes `/healthz` for the later Kubernetes probes;
- exposes Phase 28B metadata at `/build-info.json`;
- serves WebAssembly as `application/wasm` and Unity data as
  `application/octet-stream`;
- disables dynamic compression because the local Unity build is uncompressed;
- applies short revalidation caching to stable Unity build filenames;
- applies browser security headers compatible with the Palengke API and future
  Photon WebSocket traffic.

## Exit criteria

- The image builds from the approved WebGL output.
- Nginx configuration validation and the image health check pass.
- The container runs without root privileges.
- MainMenu and guest gameplay work through the container.
- No registry push, Kubernetes resource, DNS, TLS, or public release is added.

## Verification result

Verified on 2026-07-11 with Docker `29.6.1` on the Codex VM:

- the build context was limited to the 24 MB approved WebGL artifact and hosting
  configuration;
- the pinned non-root Nginx image built successfully;
- `nginx -t` passed and the container reported healthy;
- the running process used UID/GID `101`;
- `/healthz` and `/build-info.json` returned successfully;
- `.wasm` used `application/wasm` and `.data` used
  `application/octet-stream`;
- MainMenu and `PrototypeMap` rendered through the container with independent
  Hider cooldown bars;
- the browser Console had no warnings or errors.
