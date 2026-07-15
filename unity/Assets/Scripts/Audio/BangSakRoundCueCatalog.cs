using System;
using UnityEngine;

namespace Palengke.BangSak.Audio
{
    public enum BangSakRoundCue
    {
        RoundStarted = 0,
        RevealConfirmed = 1,
        PickupReadyConfirmed = 2,
        TayaWins = 3,
        HidersWin = 4
    }

    public enum BangSakRoundCuePattern
    {
        CountInAndGo = 0,
        RevealDoublePing = 1,
        PickupReadySparkle = 2,
        TayaVictoryCadence = 3,
        HiderEscapeCadence = 4
    }

    public readonly struct BangSakRoundCueDefinition
    {
        public BangSakRoundCueDefinition(
            BangSakRoundCue cue,
            string stableId,
            int version,
            BangSakRoundCuePattern pattern,
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

        public BangSakRoundCue Cue { get; }
        public string StableId { get; }
        public int Version { get; }
        public BangSakRoundCuePattern Pattern { get; }
        public float StartFrequency { get; }
        public float EndFrequency { get; }
        public float DurationSeconds { get; }
        public float BaseVolume { get; }
    }

    public static class BangSakRoundCueCatalog
    {
        public const string SetId = "bangsak.round_reveal_result_cues";
        public const int SetVersion = 1;
        public const int MinimumCompatibleVersion = 1;
        public const int CueCount = 5;
        public const int SampleRate = 44100;
        public const string MigrationNote =
            "Version 1 introduces round start, confirmed reveal, confirmed pickup-ready, and winner-specific result cues; unknown future cues must be ignored by older clients.";

        public static BangSakRoundCueDefinition Get(BangSakRoundCue cue)
        {
            return cue switch
            {
                BangSakRoundCue.RoundStarted => new BangSakRoundCueDefinition(
                    cue,
                    "round.started",
                    1,
                    BangSakRoundCuePattern.CountInAndGo,
                    330f,
                    880f,
                    0.32f,
                    0.2f),
                BangSakRoundCue.RevealConfirmed => new BangSakRoundCueDefinition(
                    cue,
                    "reveal.confirmed",
                    1,
                    BangSakRoundCuePattern.RevealDoublePing,
                    740f,
                    520f,
                    0.24f,
                    0.18f),
                BangSakRoundCue.PickupReadyConfirmed => new BangSakRoundCueDefinition(
                    cue,
                    "pickup.ready_confirmed",
                    1,
                    BangSakRoundCuePattern.PickupReadySparkle,
                    520f,
                    1040f,
                    0.18f,
                    0.19f),
                BangSakRoundCue.TayaWins => new BangSakRoundCueDefinition(
                    cue,
                    "result.taya_wins",
                    1,
                    BangSakRoundCuePattern.TayaVictoryCadence,
                    390f,
                    620f,
                    0.29f,
                    0.22f),
                BangSakRoundCue.HidersWin => new BangSakRoundCueDefinition(
                    cue,
                    "result.hiders_win",
                    1,
                    BangSakRoundCuePattern.HiderEscapeCadence,
                    620f,
                    1040f,
                    0.31f,
                    0.22f),
                _ => throw new ArgumentOutOfRangeException(nameof(cue), cue, "Unknown Bang-Sak round cue.")
            };
        }

        public static float[] CreateSamples(BangSakRoundCue cue, int sampleRate = SampleRate)
        {
            if (sampleRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate, "Sample rate must be positive.");
            }

            var definition = Get(cue);
            var sampleCount = Mathf.Max(2, Mathf.CeilToInt(definition.DurationSeconds * sampleRate));
            var samples = new float[sampleCount];

            for (var index = 0; index < sampleCount; index += 1)
            {
                var progress = index / (float)(sampleCount - 1);
                samples[index] = definition.Pattern switch
                {
                    BangSakRoundCuePattern.CountInAndGo => RenderCountInAndGo(definition, progress),
                    BangSakRoundCuePattern.RevealDoublePing => RenderRevealDoublePing(definition, progress),
                    BangSakRoundCuePattern.PickupReadySparkle => RenderPickupReadySparkle(definition, progress),
                    BangSakRoundCuePattern.TayaVictoryCadence => RenderTayaVictoryCadence(definition, progress),
                    BangSakRoundCuePattern.HiderEscapeCadence => RenderHiderEscapeCadence(definition, progress),
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(definition.Pattern),
                        definition.Pattern,
                        "Unknown Bang-Sak round cue pattern.")
                };
            }

            samples[0] = 0f;
            samples[sampleCount - 1] = 0f;
            return samples;
        }

        public static AudioClip CreateClip(BangSakRoundCue cue, int sampleRate = SampleRate)
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

        private static float RenderCountInAndGo(BangSakRoundCueDefinition definition, float progress)
        {
            if (progress < 0.22f)
            {
                return RenderNote(definition.StartFrequency, definition.DurationSeconds * 0.22f, progress / 0.22f, 0.72f, 0.18f);
            }

            if (progress >= 0.29f && progress < 0.51f)
            {
                return RenderNote(440f, definition.DurationSeconds * 0.22f, (progress - 0.29f) / 0.22f, 0.72f, 0.18f);
            }

            if (progress >= 0.59f)
            {
                return RenderNote(definition.EndFrequency, definition.DurationSeconds * 0.41f, (progress - 0.59f) / 0.41f, 0.78f, 0.24f);
            }

            return 0f;
        }

        private static float RenderRevealDoublePing(BangSakRoundCueDefinition definition, float progress)
        {
            if (progress < 0.38f)
            {
                return RenderBell(definition.StartFrequency, definition.DurationSeconds * 0.38f, progress / 0.38f, 0.84f);
            }

            if (progress >= 0.55f && progress < 0.93f)
            {
                return RenderBell(definition.EndFrequency, definition.DurationSeconds * 0.38f, (progress - 0.55f) / 0.38f, 0.76f);
            }

            return 0f;
        }

        private static float RenderPickupReadySparkle(BangSakRoundCueDefinition definition, float progress)
        {
            const int noteCount = 3;
            var scaled = Mathf.Min(progress * noteCount, noteCount - 0.0001f);
            var noteIndex = Mathf.FloorToInt(scaled);
            var localProgress = scaled - noteIndex;
            var frequency = Mathf.Lerp(definition.StartFrequency, definition.EndFrequency, noteIndex / 2f);
            return RenderBell(frequency, definition.DurationSeconds / noteCount, localProgress, 0.78f);
        }

        private static float RenderTayaVictoryCadence(BangSakRoundCueDefinition definition, float progress)
        {
            if (progress < 0.43f)
            {
                return RenderNote(definition.StartFrequency, definition.DurationSeconds * 0.43f, progress / 0.43f, 0.68f, 0.28f);
            }

            if (progress >= 0.5f)
            {
                return RenderNote(definition.EndFrequency, definition.DurationSeconds * 0.5f, (progress - 0.5f) / 0.5f, 0.76f, 0.3f);
            }

            return 0f;
        }

        private static float RenderHiderEscapeCadence(BangSakRoundCueDefinition definition, float progress)
        {
            const int noteCount = 4;
            var scaled = Mathf.Min(progress * noteCount, noteCount - 0.0001f);
            var noteIndex = Mathf.FloorToInt(scaled);
            var localProgress = scaled - noteIndex;
            var frequency = noteIndex switch
            {
                0 => definition.StartFrequency,
                1 => 860f,
                2 => 740f,
                _ => definition.EndFrequency
            };
            return RenderNote(
                frequency,
                definition.DurationSeconds / noteCount,
                localProgress,
                0.7f,
                noteIndex == noteCount - 1 ? 0.28f : 0.14f);
        }

        private static float RenderNote(
            float frequency,
            float durationSeconds,
            float progress,
            float fundamentalGain,
            float harmonicGain)
        {
            var phase = 2f * Mathf.PI * frequency * durationSeconds * progress;
            var envelope = Mathf.Pow(Mathf.Sin(Mathf.PI * progress), 1.25f);
            var tone = fundamentalGain * Mathf.Sin(phase) + harmonicGain * Mathf.Sin(phase * 2f);
            return Mathf.Clamp(tone * envelope * 0.9f, -1f, 1f);
        }

        private static float RenderBell(float frequency, float durationSeconds, float progress, float gain)
        {
            var phase = 2f * Mathf.PI * frequency * durationSeconds * progress;
            var envelope = Mathf.Pow(Mathf.Sin(Mathf.PI * progress), 1.05f) * Mathf.Pow(1f - progress, 0.22f);
            var bell = 0.7f * Mathf.Sin(phase) + 0.22f * Mathf.Sin(phase * 2.4142f);
            return Mathf.Clamp(bell * envelope * gain, -1f, 1f);
        }
    }
}
