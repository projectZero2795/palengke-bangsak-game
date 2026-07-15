using System;
using UnityEngine;

namespace Palengke.BangSak.Audio
{
    public enum BangSakGameplayCue
    {
        BangRequest = 0,
        BangCaughtConfirmed = 1,
        SakRequest = 2,
        SakCounteredConfirmed = 3
    }

    public enum BangSakGameplayCuePattern
    {
        BangPercussivePop = 0,
        BangCaughtSparkle = 1,
        SakElasticBoing = 2,
        SakCounteredDeflect = 3
    }

    public readonly struct BangSakGameplayCueDefinition
    {
        public BangSakGameplayCueDefinition(
            BangSakGameplayCue cue,
            string stableId,
            int version,
            BangSakGameplayCuePattern pattern,
            float startFrequency,
            float endFrequency,
            float durationSeconds,
            float baseVolume,
            float harmonicRatio,
            float harmonicGain,
            float flutterCycles)
        {
            Cue = cue;
            StableId = stableId;
            Version = version;
            Pattern = pattern;
            StartFrequency = startFrequency;
            EndFrequency = endFrequency;
            DurationSeconds = durationSeconds;
            BaseVolume = baseVolume;
            HarmonicRatio = harmonicRatio;
            HarmonicGain = harmonicGain;
            FlutterCycles = flutterCycles;
        }

        public BangSakGameplayCue Cue { get; }
        public string StableId { get; }
        public int Version { get; }
        public BangSakGameplayCuePattern Pattern { get; }
        public float StartFrequency { get; }
        public float EndFrequency { get; }
        public float DurationSeconds { get; }
        public float BaseVolume { get; }
        public float HarmonicRatio { get; }
        public float HarmonicGain { get; }
        public float FlutterCycles { get; }
    }

    public static class BangSakGameplayCueCatalog
    {
        public const string SetId = "bangsak.gameplay_action_cues";
        public const int SetVersion = 2;
        public const int MinimumCompatibleVersion = 1;
        public const int CueCount = 4;
        public const int SampleRate = 44100;
        public const string MigrationNote =
            "Version 2 gives every Bang/SAK request and confirmed outcome a distinct rhythm and timbre; unknown future cues must be ignored by older clients.";

        public static BangSakGameplayCueDefinition Get(BangSakGameplayCue cue)
        {
            return cue switch
            {
                BangSakGameplayCue.BangRequest => new BangSakGameplayCueDefinition(
                    cue,
                    "gameplay.bang_request",
                    2,
                    BangSakGameplayCuePattern.BangPercussivePop,
                    210f,
                    90f,
                    0.125f,
                    0.21f,
                    2f,
                    0.16f,
                    0f),
                BangSakGameplayCue.BangCaughtConfirmed => new BangSakGameplayCueDefinition(
                    cue,
                    "gameplay.bang_caught_confirmed",
                    2,
                    BangSakGameplayCuePattern.BangCaughtSparkle,
                    620f,
                    1040f,
                    0.22f,
                    0.24f,
                    1.5f,
                    0.12f,
                    1f),
                BangSakGameplayCue.SakRequest => new BangSakGameplayCueDefinition(
                    cue,
                    "gameplay.sak_request",
                    2,
                    BangSakGameplayCuePattern.SakElasticBoing,
                    520f,
                    240f,
                    0.18f,
                    0.2f,
                    2.5f,
                    0.09f,
                    1.5f),
                BangSakGameplayCue.SakCounteredConfirmed => new BangSakGameplayCueDefinition(
                    cue,
                    "gameplay.sak_countered_confirmed",
                    2,
                    BangSakGameplayCuePattern.SakCounteredDeflect,
                    760f,
                    540f,
                    0.2f,
                    0.23f,
                    2f,
                    0.08f,
                    2.5f),
                _ => throw new ArgumentOutOfRangeException(nameof(cue), cue, "Unknown Bang-Sak gameplay cue.")
            };
        }

        public static float[] CreateSamples(BangSakGameplayCue cue, int sampleRate = SampleRate)
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
                    BangSakGameplayCuePattern.BangPercussivePop => RenderBangPercussivePop(
                        definition,
                        progress,
                        index,
                        sampleRate,
                        ref phase),
                    BangSakGameplayCuePattern.BangCaughtSparkle => RenderBangCaughtSparkle(
                        definition,
                        progress),
                    BangSakGameplayCuePattern.SakElasticBoing => RenderSakElasticBoing(
                        definition,
                        progress,
                        sampleRate,
                        ref phase),
                    BangSakGameplayCuePattern.SakCounteredDeflect => RenderSakCounteredDeflect(
                        definition,
                        progress,
                        index,
                        sampleRate,
                        ref phase),
                    _ => throw new ArgumentOutOfRangeException(
                        nameof(definition.Pattern),
                        definition.Pattern,
                        "Unknown Bang-Sak gameplay cue pattern.")
                };
            }

            samples[0] = 0f;
            samples[sampleCount - 1] = 0f;
            return samples;
        }

        private static float RenderBangPercussivePop(
            BangSakGameplayCueDefinition definition,
            float progress,
            int sampleIndex,
            int sampleRate,
            ref float phase)
        {
            var frequency = Mathf.Lerp(definition.StartFrequency, definition.EndFrequency, progress);
            phase += 2f * Mathf.PI * frequency / sampleRate;
            var attack = Mathf.Clamp01(progress / 0.035f);
            var decay = Mathf.Pow(1f - progress, 2.25f);
            var body = 0.72f * Mathf.Sin(phase) + 0.16f * Mathf.Sin(phase * 2f);
            var transient = DeterministicNoise(sampleIndex) * 0.42f * Mathf.Pow(1f - progress, 6f);
            return Mathf.Clamp((body + transient) * attack * decay, -1f, 1f);
        }

        private static float RenderBangCaughtSparkle(
            BangSakGameplayCueDefinition definition,
            float progress)
        {
            const int noteCount = 3;
            var scaledProgress = Mathf.Min(progress * noteCount, noteCount - 0.0001f);
            var noteIndex = Mathf.FloorToInt(scaledProgress);
            var noteProgress = scaledProgress - noteIndex;
            var frequency = Mathf.Lerp(
                definition.StartFrequency,
                definition.EndFrequency,
                noteIndex / (float)(noteCount - 1));
            var noteDuration = definition.DurationSeconds / noteCount;
            var localPhase = 2f * Mathf.PI * frequency * noteDuration * noteProgress;
            var envelope = Mathf.Pow(Mathf.Sin(Mathf.PI * noteProgress), 1.3f);
            var sparkle = 0.74f * Mathf.Sin(localPhase) +
                          definition.HarmonicGain * Mathf.Sin(localPhase * definition.HarmonicRatio);
            return Mathf.Clamp(sparkle * envelope * 0.9f, -1f, 1f);
        }

        private static float RenderSakElasticBoing(
            BangSakGameplayCueDefinition definition,
            float progress,
            int sampleRate,
            ref float phase)
        {
            var springWobble = 1f + 0.13f * Mathf.Sin(5f * Mathf.PI * progress) * (1f - progress);
            var frequency = Mathf.Lerp(definition.StartFrequency, definition.EndFrequency, progress) * springWobble;
            phase += 2f * Mathf.PI * frequency / sampleRate;
            var envelope = Mathf.Pow(Mathf.Sin(Mathf.PI * progress), 0.72f) * Mathf.Pow(1f - progress, 0.28f);
            var elasticBody = 0.66f * Mathf.Sin(phase) + 0.24f * Mathf.Sin(phase * 0.5f);
            return Mathf.Clamp(elasticBody * envelope * 0.9f, -1f, 1f);
        }

        private static float RenderSakCounteredDeflect(
            BangSakGameplayCueDefinition definition,
            float progress,
            int sampleIndex,
            int sampleRate,
            ref float phase)
        {
            var frequency = Mathf.Lerp(definition.StartFrequency, definition.EndFrequency, progress);
            phase += 2f * Mathf.PI * frequency / sampleRate;
            var attack = Mathf.Clamp01(progress / 0.018f);
            var decay = Mathf.Pow(1f - progress, 1.75f);
            var metallicRing = 0.52f * Mathf.Sin(phase) +
                               0.25f * Mathf.Sin(phase * 1.4142f) +
                               0.14f * Mathf.Sin(phase * 2.4142f);
            var contact = DeterministicNoise(sampleIndex) * 0.3f * Mathf.Pow(1f - progress, 12f);
            return Mathf.Clamp((metallicRing + contact) * attack * decay * 0.92f, -1f, 1f);
        }

        private static float DeterministicNoise(int sampleIndex)
        {
            var value = unchecked((uint)sampleIndex * 747796405u + 2891336453u);
            value = ((value >> ((int)(value >> 28) + 4)) ^ value) * 277803737u;
            value = (value >> 22) ^ value;
            return value / (float)uint.MaxValue * 2f - 1f;
        }

        public static AudioClip CreateClip(BangSakGameplayCue cue, int sampleRate = SampleRate)
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
