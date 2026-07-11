# syntax=docker/dockerfile:1.7

ARG NGINX_IMAGE=nginxinc/nginx-unprivileged:1.28-alpine@sha256:7377697a821c131a924a7105fafbe7414db4e9fcc77a6f08f776f33f141ec3f8
FROM ${NGINX_IMAGE}

ARG BANG_SAK_VERSION=dev
ARG VCS_REF=unknown

LABEL org.opencontainers.image.title="Bang-Sak for Palengke" \
      org.opencontainers.image.description="Unity WebGL static game container" \
      org.opencontainers.image.version="${BANG_SAK_VERSION}" \
      org.opencontainers.image.revision="${VCS_REF}" \
      org.opencontainers.image.source="https://github.com/projectZero2795/palengke-bangsak-game"

COPY --chown=101:101 docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --chown=101:101 unity/Build/WebGL/ /usr/share/nginx/html/

USER 101:101
EXPOSE 8080

HEALTHCHECK --interval=10s --timeout=3s --start-period=10s --retries=3 \
    CMD wget -q -O /dev/null http://127.0.0.1:8080/healthz || exit 1
