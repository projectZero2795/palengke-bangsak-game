# Phase 34 Android and Google Play roadmap

## Goal

Make Bang-Sak publicly downloadable from Google Play through very small,
owner-reviewable checkpoints. Phase 34A records decisions and Phase 34B adds a
local debug APK; later phases remain gated and this roadmap never authorizes a
Play Console upload or publication by itself.

Phase 33, Phase 34A, and Phase 34B are approved. Every 34-series phase stops for
owner review before the next one begins.

## Small review phases

| Phase | Only this work | Owner verification | Explicitly not included |
| ---: | --- | --- | --- |
| 34A | Record the immutable package ID, current Play target requirements, minimum supported Android version, portrait/landscape policy, reference phone, Play Console account type, and privacy/support contacts. | Read and approve the decision record. | Android build, UI changes, Play upload. |
| 34B | Install/verify Unity Android build support and add one reproducible debug-APK build command. | Install the APK on the reference phone and confirm the main menu opens. | Touch polish, multiplayer testing, signing, Play Console. |
| 34C | Fix only touch controls, safe areas, notches, and Android screen-size layout. | Complete one local round using touch; confirm no critical overlap. | Accessibility options, reconnect work, performance tuning. |
| 34D | Add only readable contrast/text, reduced-motion behavior, and visual alternatives for essential audio cues. | Toggle each option and confirm its visible effect. | Networking, performance tuning, store work. |
| 34E | Fix only Photon create/join and Android pause/background/resume/reconnect behavior. | Two Android clients create/join, background/resume, reconnect, and finish one round. | Performance tuning, signing, Play upload. |
| 34F | Measure and tune only the agreed reference-device FPS/frame time, memory, temperature, and package/download-size budgets. | Play one measured round and compare the recorded result with the budget. | Signing, listing, publishing. |
| 34G | Produce one versioned signed `.aab`; configure Play App Signing/upload-key handling without committing keys or passwords. | Validate the bundle and compare the recorded certificate fingerprint/version. | Play upload or tester distribution. |
| 34H | Create the Play Console app and publish the approved bundle to internal testing only. | Use the Play tester link to install the game from Google Play. | Public listing, closed test, production release. |
| 34I | Complete only listing and compliance material: title/description, icon/screenshots, privacy policy, Data safety, content rating, target audience, countries, and other required app-content answers. | Confirm Play Console has no unresolved required listing/app-content task. | Production submission. |
| 34J | Complete only the production-access test required for the owner's account, including a closed test when Play Console requires it. | Confirm Play Console grants production access and review the tester evidence. | Public rollout. |
| 34K | Submit a staged production rollout and verify install, launch, support, and rollback paths. | Find Bang-Sak publicly on Google Play, install it on a clean device, and open the main menu. | Later content phases. |

## Store constraints captured by the plan

- The package ID becomes difficult to change after the first Play upload, so it
  is approved in Phase 34A before any Console app is created.
- Google Play distributes Android App Bundles and generates optimized APKs for
  devices; the release artifact is therefore an `.aab`, not a manually shared
  release APK.
- Signing material and passwords remain outside Git, build logs, Unity assets,
  and release documentation.
- Internal testing is the first Play Store download checkpoint. It is not a
  public release.
- The Data safety declaration must cover Bang-Sak code plus Unity, Photon,
  Palengke, and any later third-party SDK behavior.
- Production-access testing varies by Play developer account. Phase 34A records
  what the owner's Play Console actually requires, and Phase 34J follows that
  displayed requirement instead of assuming it.
- Store policies and target API requirements are rechecked from official Google
  documentation during Phase 34A and immediately before Phase 34K.

## Official references

- [Android App Bundles](https://developer.android.com/guide/app-bundle)
- [Create and set up a Play Console app](https://support.google.com/googleplay/android-developer/answer/9859152)
- [Internal, closed, and open testing](https://support.google.com/googleplay/android-developer/answer/9845334)
- [Data safety declaration](https://support.google.com/googleplay/android-developer/answer/10787469)
- [Content rating requirements](https://support.google.com/googleplay/android-developer/answer/9859655)

## Stop rule

At the end of every phase:

1. commit and push only that phase;
2. record its build/version and test evidence;
3. explain the short owner verification;
4. stop;
5. continue only after explicit owner approval.

## Final exit criterion

Phase 34 is complete only after Phase 34K is approved and Bang-Sak is publicly
downloadable from Google Play. An APK, an AAB, an internal tester link, or a
submitted review does not count as the public release.
