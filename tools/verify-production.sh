#!/usr/bin/env bash
set -euo pipefail

game_url="${BANG_SAK_GAME_URL:-https://games.palengke.es}"
api_url="${PALENGKE_API_URL:-https://palengke.es/api/backend}"
expected_version="${BANG_SAK_EXPECTED_VERSION:-0.33.3}"
origin="${BANG_SAK_ORIGIN:-https://games.palengke.es}"

tmp_dir="$(mktemp -d)"
trap 'rm -rf "$tmp_dir"' EXIT

health="$(curl --fail --silent --show-error --max-time 20 "$game_url/healthz")"
test "$health" = "ok"

curl --fail --silent --show-error --max-time 20 \
  "$game_url/build-info.json" > "$tmp_dir/build-info.json"
python3 - "$tmp_dir/build-info.json" "$expected_version" <<'PY'
import json
import sys

path, expected = sys.argv[1:]
with open(path, encoding="utf-8") as handle:
    payload = json.load(handle)
if payload.get("version") != expected:
    raise SystemExit(f"expected version {expected}, got {payload.get('version')}")
if payload.get("buildTarget") != "WebGL":
    raise SystemExit(f"expected WebGL target, got {payload.get('buildTarget')}")
print(f"build version={payload['version']} unity={payload.get('unityVersion')}")
PY

curl --fail --silent --show-error --head --max-time 20 \
  "$game_url/Build/WebGL.wasm" > "$tmp_dir/wasm.headers"
tr -d '\r' < "$tmp_dir/wasm.headers" | grep -Eiq '^content-type: application/wasm$'
tr -d '\r' < "$tmp_dir/wasm.headers" | grep -Eiq '^x-request-id: .+$'

curl --fail --silent --show-error --head --max-time 20 \
  "$game_url/Build/WebGL.data" > "$tmp_dir/data.headers"
tr -d '\r' < "$tmp_dir/data.headers" | grep -Eiq '^content-type: application/octet-stream$'

curl --fail --silent --show-error --max-time 20 \
  --dump-header "$tmp_dir/api.headers" \
  --header "Origin: $origin" \
  "$api_url/games/bang-sak/leaderboard" > "$tmp_dir/leaderboard.json"
tr -d '\r' < "$tmp_dir/api.headers" | grep -Fqi "access-control-allow-origin: $origin"
python3 - "$tmp_dir/leaderboard.json" <<'PY'
import json
import sys

with open(sys.argv[1], encoding="utf-8") as handle:
    payload = json.load(handle)
if not isinstance(payload.get("entries"), list):
    raise SystemExit("leaderboard response does not contain an entries list")
print(f"leaderboard entries={len(payload['entries'])}")
PY

echo "Bang-Sak production verification passed for $game_url"
