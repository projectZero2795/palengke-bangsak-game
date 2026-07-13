#!/bin/bash
set -euo pipefail

ROOT_DIR=$(cd "$(dirname "$0")/.." && pwd)
UNITY_ROOT="${UNITY_ROOT:-/Applications/Unity/Hub/Editor/2022.3.50f1}"
UNITY_BIN="$UNITY_ROOT/Unity.app/Contents/MacOS/Unity"
ANDROID_ROOT="$UNITY_ROOT/PlaybackEngines/AndroidPlayer"
JAVA="$ANDROID_ROOT/OpenJDK/bin/java"
KEYTOOL="$ANDROID_ROOT/OpenJDK/bin/keytool"
JARSIGNER="$ANDROID_ROOT/OpenJDK/bin/jarsigner"
BUNDLETOOL="$ANDROID_ROOT/Tools/bundletool-all-1.16.0.jar"
AAB="$ROOT_DIR/unity/Build/Android/BangSak-0.34.9.aab"
LOG_FILE="${BANGSAK_ANDROID_BUILD_LOG:-/tmp/bang-sak-phase34g-play-bundle.log}"

export BANGSAK_ANDROID_KEYSTORE_PATH="${BANGSAK_ANDROID_KEYSTORE_PATH:-$HOME/Library/Application Support/Palengke/Signing/bangsak-upload.jks}"
export BANGSAK_ANDROID_KEY_ALIAS="${BANGSAK_ANDROID_KEY_ALIAS:-bangsak-upload}"
export BANGSAK_ANDROID_KEYSTORE_PASSWORD="${BANGSAK_ANDROID_KEYSTORE_PASSWORD:-$(security find-generic-password -a es.palengke.bangsak -s es.palengke.bangsak.upload-keystore-password -w)}"
export BANGSAK_ANDROID_KEY_PASSWORD="${BANGSAK_ANDROID_KEY_PASSWORD:-$(security find-generic-password -a es.palengke.bangsak -s es.palengke.bangsak.upload-key-password -w)}"

cleanup() {
  unset BANGSAK_ANDROID_KEYSTORE_PASSWORD BANGSAK_ANDROID_KEY_PASSWORD
}
trap cleanup EXIT

test -x "$UNITY_BIN"
test -f "$BUNDLETOOL"
test -f "$BANGSAK_ANDROID_KEYSTORE_PATH"

"$UNITY_BIN" \
  -batchmode -nographics -quit \
  -projectPath "$ROOT_DIR/unity" \
  -executeMethod Palengke.BangSak.Editor.Phase34GPlayBundle.BuildCommandLine \
  -logFile "$LOG_FILE"

"$JAVA" -jar "$BUNDLETOOL" validate --bundle="$AAB" >/dev/null
"$JARSIGNER" -verify -verbose -certs "$AAB" > /tmp/bang-sak-phase34g-jarsigner.txt
grep -q "jar verified" /tmp/bang-sak-phase34g-jarsigner.txt

BUNDLE_FINGERPRINT=$("$KEYTOOL" -printcert -jarfile "$AAB" | awk '/SHA256:/{print $2; exit}')
KEY_FINGERPRINT=$("$KEYTOOL" -list -v \
  -keystore "$BANGSAK_ANDROID_KEYSTORE_PATH" \
  -alias "$BANGSAK_ANDROID_KEY_ALIAS" \
  -storepass "$BANGSAK_ANDROID_KEYSTORE_PASSWORD" | awk '/SHA256:/{print $2; exit}')
test "$BUNDLE_FINGERPRINT" = "$KEY_FINGERPRINT"

printf 'Validated signed Play bundle: %s\n' "$AAB"
printf 'Certificate SHA-256: %s\n' "$BUNDLE_FINGERPRINT"
printf 'AAB SHA-256: '
shasum -a 256 "$AAB" | awk '{print $1}'
