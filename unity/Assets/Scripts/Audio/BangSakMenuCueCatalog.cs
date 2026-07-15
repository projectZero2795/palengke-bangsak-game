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

    public readonly struct BangSakMenuCueDefinition
    {
        public BangSakMenuCueDefinition(
            BangSakMenuCue cue,
            string stableId,
            int version,
            float startFrequency,
            float endFrequency,
            float durationSeconds,
            float baseVolume)
        {
            Cue = cue;
            StableId = stableId;
            Version = version;
            StartFrequency = startFrequency;
            EndFrequency = endFrequency;
            DurationSeconds = durationSeconds;
            BaseVolume = baseVolume;
        }

        public BangSakMenuCue Cue { get; }
        public string StableId { get; }
        public int Version { get; }
        public float StartFrequency { get; }
        public float EndFrequency { get; }
        public float DurationSeconds { get; }
        public float BaseVolume { get; }
    }

    public static class BangSakMenuCueCatalog
    {
        public const string SetId = "bangsak.menu_interface_cues";
        public const int SetVersion = 1;
        public const int MinimumCompatibleVersion = 1;
        public const int CueCount = 3;
        public const string MigrationNote = "Version 1 introduces navigate, confirm, and back; unknown future cues must be ignored by older clients.";
        public const int SampleRate = 44100;

        public static BangSakMenuCueDefinition Get(BangSakMenuCue cue)
        {
            return cue switch
            {
                BangSakMenuCue.Navigate => new BangSakMenuCueDefinition(
                    cue,
                    "menu.navigate",
                    1,
                    460f,
                    560f,
                    0.065f,
                    0.16f),
                BangSakMenuCue.Confirm => new BangSakMenuCueDefinition(
                    cue,
                    "menu.confirm",
                    1,
                    540f,
                    720f,
                    0.105f,
                    0.2f),
                BangSakMenuCue.Back => new BangSakMenuCueDefinition(
                    cue,
                    "menu.back",
                    1,
                    520f,
                    400f,
                    0.085f,
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
                var frequency = Mathf.Lerp(definition.StartFrequency, definition.EndFrequency, progress);
                phase += 2f * Mathf.PI * frequency / sampleRate;

                // A smooth zero-to-zero envelope avoids clicks and keeps every cue gentle.
                var envelope = Mathf.Pow(Mathf.Sin(Mathf.PI * progress), 1.6f);
                var fundamental = Mathf.Sin(phase);
                var softHarmonic = 0.1f * Mathf.Sin(phase * 2f);
                samples[index] = Mathf.Clamp((fundamental + softHarmonic) * envelope * 0.82f, -1f, 1f);
            }

            samples[0] = 0f;
            samples[sampleCount - 1] = 0f;
            return samples;
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
