using System;
using UnityEngine;

namespace Palengke.BangSak.Audio
{
    public enum BangSakAudioChannel
    {
        Music = 0,
        Sfx = 1
    }

    public readonly struct BangSakAudioSettingsSnapshot
    {
        public BangSakAudioSettingsSnapshot(bool muted, float masterVolume, float musicVolume, float sfxVolume)
        {
            Muted = muted;
            MasterVolume = masterVolume;
            MusicVolume = musicVolume;
            SfxVolume = sfxVolume;
        }

        public bool Muted { get; }
        public float MasterVolume { get; }
        public float MusicVolume { get; }
        public float SfxVolume { get; }
    }

    public static class BangSakAudioSettings
    {
        public const string MutedKey = "bangsak.audio.muted";
        public const string MasterVolumeKey = "bangsak.audio.master_volume";
        public const string MusicVolumeKey = "bangsak.audio.music_volume";
        public const string SfxVolumeKey = "bangsak.audio.sfx_volume";

        public const float DefaultMasterVolume = 1f;
        public const float DefaultMusicVolume = 1f;
        public const float DefaultSfxVolume = 1f;

        public static event Action SettingsChanged;

        public static bool Muted => PlayerPrefs.GetInt(MutedKey, 0) != 0;
        public static float MasterVolume => ReadVolume(MasterVolumeKey, DefaultMasterVolume);
        public static float MusicVolume => ReadVolume(MusicVolumeKey, DefaultMusicVolume);
        public static float SfxVolume => ReadVolume(SfxVolumeKey, DefaultSfxVolume);

        public static BangSakAudioSettingsSnapshot Current => new BangSakAudioSettingsSnapshot(
            Muted,
            MasterVolume,
            MusicVolume,
            SfxVolume);

        public static void SetMuted(bool muted)
        {
            var value = muted ? 1 : 0;
            if (PlayerPrefs.GetInt(MutedKey, -1) == value)
            {
                return;
            }

            PlayerPrefs.SetInt(MutedKey, value);
            SaveAndNotify();
        }

        public static void SetMasterVolume(float volume) => WriteVolume(MasterVolumeKey, volume);
        public static void SetMusicVolume(float volume) => WriteVolume(MusicVolumeKey, volume);
        public static void SetSfxVolume(float volume) => WriteVolume(SfxVolumeKey, volume);

        public static float ResolveVolume(BangSakAudioChannel channel, float sourceVolume = 1f)
        {
            if (Muted)
            {
                return 0f;
            }

            var channelVolume = channel switch
            {
                BangSakAudioChannel.Music => MusicVolume,
                BangSakAudioChannel.Sfx => SfxVolume,
                _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unknown Bang-Sak audio channel.")
            };

            return Mathf.Clamp01(sourceVolume) * MasterVolume * channelVolume;
        }

        public static void ResetToDefaults()
        {
            PlayerPrefs.DeleteKey(MutedKey);
            PlayerPrefs.DeleteKey(MasterVolumeKey);
            PlayerPrefs.DeleteKey(MusicVolumeKey);
            PlayerPrefs.DeleteKey(SfxVolumeKey);
            SaveAndNotify();
        }

        private static float ReadVolume(string key, float defaultValue)
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat(key, defaultValue));
        }

        private static void WriteVolume(string key, float volume)
        {
            var normalizedVolume = Mathf.Clamp01(volume);
            if (PlayerPrefs.HasKey(key) && Mathf.Approximately(PlayerPrefs.GetFloat(key), normalizedVolume))
            {
                return;
            }

            PlayerPrefs.SetFloat(key, normalizedVolume);
            SaveAndNotify();
        }

        private static void SaveAndNotify()
        {
            PlayerPrefs.Save();
            SettingsChanged?.Invoke();
        }
    }
}
