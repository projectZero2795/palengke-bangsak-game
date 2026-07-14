# Phase 34G: Signed Play bundle

## Scope

Produce and validate one versioned release Android App Bundle for Google Play.
This phase does not create a Play Console app, enable a testing track, upload
the bundle, invite testers, or publish anything.

## Signing decision

| Decision | Selected handling |
| --- | --- |
| Key owner | Palengke project owner |
| Local key role | Google Play upload key only; it is not the Play app-signing key |
| Play app-signing policy | Use Google Play App Signing when the Play Console app is created in Phase 34H |
| Keystore | `~/Library/Application Support/Palengke/Signing/bangsak-upload.jks` |
| Public certificate | `~/Library/Application Support/Palengke/Signing/bangsak-upload-certificate.pem` |
| Password storage | macOS login Keychain generic-password entries; no password file or repository value |
| Alias | `bangsak-upload` |

The signing directory is mode `700` and the JKS is mode `600`. The RSA 4096
upload key is valid until 2053-11-28. The certificate is public; the JKS and
both passwords are private. The upload key remains recoverable through
Google's upload-key reset process after Play App Signing is enabled, but the
owner should still keep a separate secure backup of the JKS.

The Keychain service names are:

- `es.palengke.bangsak.upload-keystore-password`
- `es.palengke.bangsak.upload-key-password`

## Reproducible build

`tools/build-android-play-bundle.sh` reads the two passwords from Keychain,
exports them only to the Unity build process, unsets them on exit, builds the
release AAB, validates it with Bundletool and `jarsigner`, and verifies that the
bundle certificate matches the external upload keystore.

Run from the repository root:

```bash
./tools/build-android-play-bundle.sh
```

Unity's editor build entry point is
`Palengke.BangSak.Editor.Phase34GPlayBundle.BuildCommandLine`. It fails closed
unless all four signing environment values are present and the keystore is an
existing absolute path. Passwords and the keystore path are not written to the
build metadata.

## Artifact record

> **Historical artifact:** Google rejected version code `1` on 2026-07-14
> because Unity `2022.3.50f1` is affected by CVE-2025-59489. Do not upload this
> artifact again. Phase 34J records the officially patched replacement, version
> code `2`.

| Field | Verified value |
| --- | --- |
| AAB | `unity/Build/Android/BangSak-0.34.9.aab` |
| Package | `es.palengke.bangsak` |
| Version name | `0.34.9` |
| Version code | `1` |
| Minimum / target API | `29` / `35` |
| Native ABI | `arm64-v8a` only |
| Development build | No |
| Artifact bytes | `21,678,248` |
| AAB SHA-256 | `ad658543bf5a673f055d3a298ef21dc1697d79a6a5c1c047a918d10168725d43` |
| Upload certificate SHA-256 | `3F:9B:13:89:72:4F:8F:8B:3F:2F:43:93:68:B1:DA:F8:6A:D0:9F:98:F2:F0:31:47:64:B0:0B:74:DD:7B:26:78` |

## Validation evidence

- Unity EditMode tests: `236/236` passed.
- Unity produced a non-development, signed AAB with no build errors.
- Bundletool `1.16.0` validated the AAB successfully.
- `jarsigner` reported `jar verified`.
- The certificate extracted from the AAB exactly matched the external upload
  keystore SHA-256 fingerprint.
- The base manifest reported package `es.palengke.bangsak`, version `0.34.9`,
  version code `1`, minimum API `29`, and target API `35`.
- Bundle contents contained only the approved `arm64-v8a` native ABI.
- A source-tree scan found neither Keychain password and no signing-key file in
  the repository.
- `.gitignore` rejects JKS, keystore, PKCS#12, and PFX signing files as defense
  in depth.

## Acceptance review

| Acceptance criterion | Evidence | Result |
| --- | --- | --- |
| Versioned signed AAB exists | `BangSak-0.34.9.aab`, version code `1`, is release-signed. | Pass |
| AAB validates locally | Bundletool and `jarsigner` both passed. | Pass |
| Signing identity is recorded | AAB and upload-keystore fingerprints match the recorded SHA-256 value. | Pass |
| Secrets remain outside Git | JKS is in Application Support; passwords are in Keychain; source scan is clean. | Pass |
| Play signing handling is defined | Local key is upload-only; Google Play App Signing is reserved for Phase 34H. | Pass |
| No later phase leaked in | No Play app, upload, tester track, listing, or publication was created. | Pass |

Phase 34G passed its original authorized nonvisual self-review, but the later
Unity security advisory supersedes the artifact's upload approval. Keep it only
as historical build evidence; use the remediated Phase 34J artifact or rebuild
with Unity `2022.3.67f2` or newer.
