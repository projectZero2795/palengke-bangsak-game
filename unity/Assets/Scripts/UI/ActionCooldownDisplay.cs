using UnityEngine;

namespace Palengke.BangSak.UI
{
    public static class ActionCooldownDisplay
    {
        public static float RemainingFraction(float remainingSeconds, float cooldownSeconds)
        {
            if (cooldownSeconds <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01(remainingSeconds / cooldownSeconds);
        }

        public static string FormatSeconds(float remainingSeconds)
        {
            return remainingSeconds > 0f ? $"{remainingSeconds:0.0}s" : "READY";
        }
    }
}
