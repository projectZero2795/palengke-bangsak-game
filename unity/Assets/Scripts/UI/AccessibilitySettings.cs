using System;
using Palengke.BangSak.Game;
using UnityEngine;

namespace Palengke.BangSak.UI
{
    public static class AccessibilitySettings
    {
        public const string ReadableTextKey = "bangsak.accessibility.readable_text";
        public const string HighContrastKey = "bangsak.accessibility.high_contrast";
        public const string ReducedMotionKey = "bangsak.accessibility.reduced_motion";
        public const string VisualCuesKey = "bangsak.accessibility.visual_cues";

        public static event Action SettingsChanged;

        public static bool ReadableTextEnabled => ReadBool(ReadableTextKey, true);
        public static bool HighContrastEnabled => ReadBool(HighContrastKey, false);
        public static bool ReducedMotionEnabled => ReadBool(ReducedMotionKey, false);
        public static bool VisualCuesEnabled => ReadBool(VisualCuesKey, true);

        public static void SetReadableText(bool enabled) => WriteBool(ReadableTextKey, enabled);
        public static void SetHighContrast(bool enabled) => WriteBool(HighContrastKey, enabled);
        public static void SetReducedMotion(bool enabled) => WriteBool(ReducedMotionKey, enabled);
        public static void SetVisualCues(bool enabled) => WriteBool(VisualCuesKey, enabled);

        public static int ResolveFontSize(int baseFontSize)
        {
            return ReadableTextEnabled
                ? Mathf.Max(baseFontSize, Mathf.CeilToInt(baseFontSize * 1.16f))
                : baseFontSize;
        }

        public static float ResolvePulse(float animatedPulse)
        {
            return ReducedMotionEnabled ? 0.5f : animatedPulse;
        }

        public static string FormatToggle(bool enabled) => enabled ? "ON" : "OFF";

        public static void ResetToDefaults()
        {
            PlayerPrefs.DeleteKey(ReadableTextKey);
            PlayerPrefs.DeleteKey(HighContrastKey);
            PlayerPrefs.DeleteKey(ReducedMotionKey);
            PlayerPrefs.DeleteKey(VisualCuesKey);
            PlayerPrefs.Save();
            SettingsChanged?.Invoke();
        }

        private static bool ReadBool(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) != 0;
        }

        private static void WriteBool(string key, bool enabled)
        {
            var value = enabled ? 1 : 0;
            if (PlayerPrefs.GetInt(key, -1) == value)
            {
                return;
            }

            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
            SettingsChanged?.Invoke();
        }
    }

    public readonly struct AccessibilityVisualCue
    {
        public AccessibilityVisualCue(string message, Color color)
        {
            Message = message ?? string.Empty;
            Color = color;
        }

        public string Message { get; }
        public Color Color { get; }
    }

    public static class AccessibilityCueService
    {
        public static event Action<AccessibilityVisualCue> CueRequested;

        public static bool PublishBang(BangHitOutcome outcome)
        {
            switch (outcome)
            {
                case BangHitOutcome.HitTarget:
                    return Publish("BANG! CAUGHT", new Color(0.55f, 1f, 0.58f, 1f));
                case BangHitOutcome.NameMismatch:
                    return Publish("BANG! WRONG NAME", new Color(1f, 0.82f, 0.25f, 1f));
                case BangHitOutcome.Blocked:
                    return Publish("BANG! BLOCKED", new Color(0.68f, 0.84f, 1f, 1f));
                default:
                    return Publish("BANG! MISS", new Color(1f, 0.78f, 0.3f, 1f));
            }
        }

        public static bool PublishSak(SakCounterOutcome outcome)
        {
            switch (outcome)
            {
                case SakCounterOutcome.CounteredTaya:
                    return Publish("SAK! COUNTER", new Color(0.55f, 1f, 0.58f, 1f));
                case SakCounterOutcome.Blocked:
                    return Publish("SAK! BLOCKED", new Color(0.68f, 0.84f, 1f, 1f));
                default:
                    return Publish("SAK! MISS", new Color(1f, 0.78f, 0.3f, 1f));
            }
        }

        public static bool Publish(string message, Color color)
        {
            if (!AccessibilitySettings.VisualCuesEnabled || string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            CueRequested?.Invoke(new AccessibilityVisualCue(message.Trim(), color));
            return true;
        }
    }
}
