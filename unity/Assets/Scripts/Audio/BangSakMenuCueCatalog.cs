using System;
using UnityEngine;

namespace Palengke.BangSak.Audio
{
    public enum BangSakMenuCue
    {
        Navigate = 0,
        Confirm = 1,
        Back = 2
    }

    public enum BangSakMenuCuePattern
    {
        NavigateTick = 0,
        ConfirmDoubleChime = 1,
        BackDescendingBubble = 2
    }

    public readonly struct BangSakMenuCueDefinition
    {
        public BangSakMenuCueDefinition(
            BangSakMenuCue cue,
            string stableId,
            int version,
            BangSakMenuCuePattern pattern,
            float startFrequency,
            float endFrequency,
            float durationSeconds,
            float baseVolume)
        {
            Cue = cue;
            StableId = stableId;
            Version = version;
            Pattern = pattern;
            StartFrequency = startFrequency;
            EndFrequency = endFrequency;
            DurationSeconds = durationSeconds;
            BaseVolume = baseVolume;
        }

        public BangSakMenuCue Cue { get; }
        public string StableId { get; }
        public int Version { get; }
        public BangSakMenuCuePattern Pattern { get; }
        public float StartFrequency { get; }
        public float EndFrequency { get; }
        public float DurationSeconds { get; }
        public float BaseVolume { get; }
    }

    public static class BangSakMenuCueCatalog
    {
        public const string SetId = "bangsak.menu_interface_cues";
        public const int SetVersion = 2;
        public const int MinimumCompatibleVersion = 1;
        public const int CueCount = 3;
        public const string MigrationNote =
            "Version 2 gives navigate, confirm, and back distinct rhythmic and timbral patterns; unknown future cues must be ignored by older clients.";
        public const int SampleRate = 44100;

        public static BangSakMenuCueDefinition Get(BangSakMenuCue cue)
        {
            return cue switch
            {
                BangSakMenuCue.Navigate => new BangSakMenuCueDefinition(
                    cue,
                    "menu.navigate",
                    2,
                    BangSakMenuCuePattern.NavigateTick,
                    960f,
                    1160f,
                    0.055f,
                    0.16f),
                BangSakMenuCue.Confirm => new BangSakMenuCueDefinition(
                    cue,
                    "menu.confirm",
                    2,
                    BangSakMenuCuePattern.ConfirmDoubleChime,
                    520f,
                    850f,
                    0.15f,
                    0.2f),
                BangSakMenuCue.Back => new BangSakMenuCueDefinition(
                    cue,
                    "menu.back",
                    2,
                    BangSakMenuCuePattern.BackDescendingBubble,
                    620f,
                    280f,
                    0.12f,
                    0.15f),
                _ => throw new ArgumentOutOfRangeException(nameof(cue), cue, "Unknown Bang-Sak menu cue.")
            };
        }

        public static float[] CreateSamples(BangSakMenuCue cue, int sampleRate = SampleRate)
        {
            if (sampleRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate, "Sample rate must be positive.");
            }

            var definition = Get(cue);
            var sampleCount = Mathf.Max(2, Mathf.CeilToInt(definition.DurationSeconds * sampleRate));
            var samples = new float[sampleCount];
            var phase = 0f;

            for (var index = 0; index < sampleCount; index += 1)
            {
                var progress = index / (float)(sampleCount - 1);
                samples[index] = definition.Pattern switch
                {
                    BangSakMenuCuePattern.NavigateTick => RenderNavigateTick(
                        definition,
                        progress,
                        sampleRate,
                        ref phase),
                    BangSakMenuCuePattern.ConfirmDoubleChime => RenderConfirmDoubleChime(
                        definition,
                        progress),
                    BangSakMenuCuePattern.BackDescendingBubble => RenderBackDescendingBubble(
                        definition,
                        progress,
                        sampleRate,
                        ref phase),
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(definition.Pattern),
                        definition.Pattern,
                        "Unknown Bang-Sak menu cue pattern.")
                };
            }

            samples[0] = 0f;
            samples[sampleCount - 1] = 0f;
            return samples;
        }

        private static float RenderNavigateTick(
            BangSakMenuCueDefinition definition,
            float progress,
            int sampleRate,
            ref float phase)
        {
            var frequency = Mathf.Lerp(definition.StartFrequency, definition.EndFrequency, progress);
            phase += 2f * Mathf.PI * frequency / sampleRate;
            var envelope = Mathf.Pow(Mathf.Sin(Mathf.PI * progress), 0.72f) * Mathf.Pow(1f - progress, 0.45f);
            var brightTick = 0.72f * Mathf.Sin(phase) + 0.2f * Mathf.Sin(phase * 3f);
            return Mathf.Clamp(brightTick * envelope * 0.88f, -1f, 1f);
        }

        private static float RenderConfirmDoubleChime(
            BangSakMenuCueDefinition definition,
            float progress)
        {
            const float firstNoteEnd = 0.42f;
            const float secondNoteStart = 0.52f;

            float noteProgress;
            float noteFrequency;
            float noteDuration;
            if (progress < firstNoteEnd)
            {
                noteProgress = progress / firstNoteEnd;
                noteFrequency = definition.StartFrequency;
                noteDuration = definition.DurationSeconds * firstNoteEnd;
            }
            else if (progress > secondNoteStart)
            {
                noteProgress = (progress - secondNoteStart) / (1f - secondNoteStart);
                noteFrequency = definition.EndFrequency;
                noteDuration = definition.DurationSeconds * (1f - secondNoteStart);
            }
            else
            {
                return 0f;
            }

            var localPhase = 2f * Mathf.PI * noteFrequency * noteDuration * noteProgress;
            var envelope = Mathf.Pow(Mathf.Sin(Mathf.PI * noteProgress), 1.35f);
            var chime = 0.78f * Mathf.Sin(localPhase) + 0.16f * Mathf.Sin(localPhase * 2.5f);
            return Mathf.Clamp(chime * envelope * 0.86f, -1f, 1f);
        }

        private static float RenderBackDescendingBubble(
            BangSakMenuCueDefinition definition,
            float progress,
            int sampleRate,
            ref float phase)
        {
            var wobble = 1f + 0.025f * Mathf.Sin(4f * Mathf.PI * progress);
            var frequency = Mathf.Lerp(definition.StartFrequency, definition.EndFrequency, progress) * wobble;
            phase += 2f * Mathf.PI * frequency / sampleRate;
            var envelope = Mathf.Pow(Mathf.Sin(Mathf.PI * progress), 1.2f);
            var bubble = 0.72f * Mathf.Sin(phase) + 0.22f * Mathf.Sin(phase * 0.5f);
            return Mathf.Clamp(bubble * envelope * 0.82f, -1f, 1f);
        }

        public static AudioClip CreateClip(BangSakMenuCue cue, int sampleRate = SampleRate)
        {
            var definition = Get(cue);
            var samples = CreateSamples(cue, sampleRate);
            var clip = AudioClip.Create(
                $"{SetId}.{definition.StableId}.v{definition.Version}",
                samples.Length,
                1,
                sampleRate,
                false);
            clip.hideFlags = HideFlags.DontSave;
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
