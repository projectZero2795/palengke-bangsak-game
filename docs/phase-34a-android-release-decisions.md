# Phase 34A: Android release decisions

## Goal

Lock the minimum decisions needed before creating the first Android build.
This phase changes documentation only. It does not install Unity Android Build
Support, change Android player settings, build an APK/AAB, create a Play Console
app, or publish anything.

## Verified starting state

- Unity version: `2022.3.50f1`.
- Unity Android Build Support is not installed on the review Mac yet.
- Android application identifier is not configured.
- Current Unity minimum SDK is API `22`; this is an inherited default, not an
  approved product decision.
- Current Unity target SDK is automatic (`0`); the release pipeline must enforce
  the Google Play requirement current at build time.
- Current orientation is auto-rotation with portrait and landscape enabled.
- Play Console already contains a separate `Palengke in Spain` app with package
  ID `es.palengke.app`; Bang-Sak must not reuse that package ID.
- The signed-in developer account is a Personal account and its Bang-Sak
  production path is subject to the closed-test gate shown by Play Console.
- Play Console marks `soporte@palengke.es` as the verified public developer
  email.

## Decision record

| Decision | Recommended value | Status | Reason |
| --- | --- | --- | --- |
| Package ID | `es.palengke.bangsak` | Selected | Separate stable reverse-domain identifier under Palengke; it does not collide with `es.palengke.app`. |
| Product name | `Bang-Sak for Palengke` | Selected | Matches the existing Unity/WebGL product name. |
| Minimum Android | Android 10 / API 29 | Selected | Avoids carrying very old platform behavior while retaining broad device coverage. |
| Target Android API | API 35 minimum, upgraded whenever the current Play requirement is higher | Selected | Google currently requires new phone apps to target Android 15 / API 35 or higher; the policy may advance before publication. |
| Orientation | Landscape left and landscape right only | Selected | Matches the existing wide game HUD and map. Portrait remains out of scope. |
| Reference profile | Google Pixel 6, Android 15 / API 35, `1080 x 2400`, landscape | Selected | Stable common phone profile for repeatable emulator, touch, screenshot, and compatibility checks. Physical-device performance is checked separately in Phase 34F. |
| Play account | Personal; closed-test production gate applies | Verified in Play Console | The dashboard explicitly requires 12 opted-in testers for 14 continuous days before applying for production access. |
| Support contact | `soporte@palengke.es` | Verified in Play Console | Play Console marks it as the verified public developer email. |
| Privacy contact | `soporte@palengke.es` | Selected | Use the verified monitored role address until the owner deliberately configures a separate privacy address. |

## Account-dependent production gate

The signed-in Personal Play Console account explicitly requires a closed test
with at least 12 opted-in testers for 14 continuous days before applying for
production access. Phase 34J must collect that evidence. No legal name,
address, phone number, account identifier, or other private account detail is
copied into this repository.

## Owner review

Confirm that the selected values in the decision table are acceptable. After
approval, close Phase 34A and stop. Do not install Android tooling or start
Phase 34B without separate approval.

## Exit evidence

- Package identity, Android versions, orientation, and reference profile are
  selected.
- Play account type and its exact production-access gate were verified in the
  signed-in Play Console.
- Public support/privacy contacts use the verified developer role address.
- The existing `es.palengke.app` application remains unchanged.
- No Play Console app was created, no form was submitted, and no build was
  uploaded.

## Official references

- [Google Play target API requirements](https://support.google.com/googleplay/android-developer/answer/11926878)
- [Testing requirements for new personal accounts](https://support.google.com/googleplay/android-developer/answer/14151465)
- [Choose a Play developer account type](https://support.google.com/googleplay/android-developer/answer/13634885)
