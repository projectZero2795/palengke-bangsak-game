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
- No Android reference device or Play Console account details are recorded in
  the repository.
- No verified Palengke privacy/support email is recorded in the repository.

## Decision record

| Decision | Recommended value | Status | Reason |
| --- | --- | --- | --- |
| Package ID | `es.palengke.bangsak` | Owner confirmation required | Stable, reverse-domain identifier under the Palengke domain; it becomes fixed after the first Play upload. |
| Product name | `Bang-Sak for Palengke` | Recommended | Matches the existing Unity/WebGL product name. |
| Minimum Android | Android 10 / API 29 | Owner confirmation required | Avoids carrying very old platform behavior while retaining broad device coverage. |
| Target Android API | At least API 35, then recheck the current Play requirement during every release | Policy-derived | Google currently requires new phone apps to target Android 15 / API 35 or higher; the policy may advance before publication. |
| Orientation | Landscape left and landscape right only | Owner confirmation required | Matches the existing wide game HUD and map. Portrait remains out of scope. |
| Reference phone | Exact model and Android version | Owner input required | Phase 34B–34F need one real device for repeatable install, touch, network, and performance checks. |
| Play account | Personal/organization plus creation date | Owner input required | A personal account created after 2023-11-13 needs the current closed-test production-access gate. |
| Support contact | Exact monitored email | Owner input required | Must be real and monitored; the repository contains no verified address. |
| Privacy contact | Exact monitored email, if different | Owner input required | Needed for later privacy policy and Data safety work. |

## Account-dependent production gate

If the Play Console account is a personal account created after 2023-11-13,
Google currently requires a closed test with at least 12 opted-in testers for
14 continuous days before applying for production access. An organization
account has different verification requirements, including a D-U-N-S number.
Phase 34J follows what the owner's actual Play Console displays.

## Owner review

Provide and approve these values:

1. confirm or replace package ID `es.palengke.bangsak`;
2. confirm Android 10 / API 29 minimum and landscape-only orientation;
3. provide the reference Android phone model and Android version;
4. state whether the Play Console account is personal or organization and when
   it was created;
5. provide the monitored support and privacy email address(es).

After those values are recorded and approved, close Phase 34A and stop. Do not
install Android tooling or start Phase 34B without separate approval.

## Official references

- [Google Play target API requirements](https://support.google.com/googleplay/android-developer/answer/11926878)
- [Testing requirements for new personal accounts](https://support.google.com/googleplay/android-developer/answer/14151465)
- [Choose a Play developer account type](https://support.google.com/googleplay/android-developer/answer/13634885)
