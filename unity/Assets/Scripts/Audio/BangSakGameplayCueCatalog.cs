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

    public readonly struct BangSakGameplayCueDefinition
    {
        public BangSakGameplayCueDefinition(
            BangSakGameplayCue cue,
            string stableId,
            int version,
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
        public const int SetVersion = 1;
        public const int MinimumCompatibleVersion = 1;
        public const int CueCount = 4;
        public const int SampleRate = 44100;
        public const string MigrationNote =
            "Version 1 introduces Bang/SAK request and confirmed-outcome cues; unknown future cues must be ignored by older clients.";

        public static BangSakGameplayCueDefinition Get(BangSakGameplayCue cue)
        {
            return cue switch
            {
                BangSakGameplayCue.BangRequest => new BangSakGameplayCueDefinition(
                    cue,
                    "gameplay.bang_request",
                    1,
                    285f,
                    420f,
                    0.115f,
                    0.21f,
                    2f,
                    0.16f,
                    0f),
                BangSakGameplayCue.BangCaughtConfirmed => new BangSakGameplayCueDefinition(
                    cue,
                    "gameplay.bang_caught_confirmed",
                    1,
                    510f,
                    780f,
                    0.18f,
                    0.24f,
                    1.5f,
                    0.12f,
                    1f),
                BangSakGameplayCue.SakRequest => new BangSakGameplayCueDefinition(
                    cue,
                    "gameplay.sak_request",
                    1,
                    390f,
                    610f,
                    0.13f,
                    0.2f,
                    2.5f,
                    0.09f,
                    1.5f),
                BangSakGameplayCue.SakCounteredConfirmed => new BangSakGameplayCueDefinition(
                    cue,
                    "gameplay.sak_countered_confirmed",
                    1,
                    470f,
                    740f,
                    0.19f,
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
                var flutter = definition.FlutterCycles <= 0f
                    ? 1f
                    : 1f + 0.035f * Mathf.Sin(2f * Mathf.PI * definition.FlutterCycles * progress);
                var frequency = Mathf.Lerp(definition.StartFrequency, definition.EndFrequency, progress) * flutter;
                phase += 2f * Mathf.PI * frequency / sampleRate;

                // The zero-to-zero envelope keeps these short, nonverbal cues click-free.
                var envelope = Mathf.Pow(Mathf.Sin(Mathf.PI * progress), 1.45f);
                var fundamental = Mathf.Sin(phase);
                var softHarmonic = definition.HarmonicGain * Mathf.Sin(phase * definition.HarmonicRatio);
                samples[index] = Mathf.Clamp((fundamental + softHarmonic) * envelope * 0.78f, -1f, 1f);
            }

            samples[0] = 0f;
            samples[sampleCount - 1] = 0f;
            return samples;
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
