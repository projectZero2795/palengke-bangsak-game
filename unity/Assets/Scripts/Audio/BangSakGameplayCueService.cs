using System;
using UnityEngine;

namespace Palengke.BangSak.Audio
{
    public static class BangSakGameplayCueService
    {
        public static event Action<BangSakGameplayCue> CuePublished;

        public static void PublishBangRequest() => Publish(BangSakGameplayCue.BangRequest);

        public static void PublishBangCaughtConfirmed() => Publish(BangSakGameplayCue.BangCaughtConfirmed);

        public static void PublishSakRequest() => Publish(BangSakGameplayCue.SakRequest);

        public static void PublishSakCounteredConfirmed() => Publish(BangSakGameplayCue.SakCounteredConfirmed);

        private static void Publish(BangSakGameplayCue cue)
        {
            try
            {
                BangSakGameplayCuePlayer.PlayShared(cue);
                CuePublished?.Invoke(cue);
            }
            catch (Exception exception)
            {
                // Audio must never prevent or roll back an accepted gameplay state.
                Debug.LogWarning($"Bang-Sak gameplay cue {cue} could not play: {exception.Message}");
            }
        }
    }
}
