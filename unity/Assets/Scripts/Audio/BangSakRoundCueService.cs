using System;
using Palengke.BangSak.Game;
using UnityEngine;

namespace Palengke.BangSak.Audio
{
    public static class BangSakRoundCueService
    {
        public static event Action<BangSakRoundCue> CuePublished;

        public static void PublishRoundStartedConfirmed() => Publish(BangSakRoundCue.RoundStarted);

        public static void PublishRevealConfirmed() => Publish(BangSakRoundCue.RevealConfirmed);

        public static void PublishPickupReadyConfirmed() => Publish(BangSakRoundCue.PickupReadyConfirmed);

        public static void PublishResultConfirmed(PrototypeRoundResult result)
        {
            switch (result)
            {
                case PrototypeRoundResult.TayaWins:
                    Publish(BangSakRoundCue.TayaWins);
                    break;
                case PrototypeRoundResult.HidersWin:
                    Publish(BangSakRoundCue.HidersWin);
                    break;
            }
        }

        private static void Publish(BangSakRoundCue cue)
        {
            try
            {
                BangSakRoundCuePlayer.PlayShared(cue);
                CuePublished?.Invoke(cue);
            }
            catch (Exception exception)
            {
                // Presentation must never prevent or roll back a confirmed state transition.
                Debug.LogWarning($"Bang-Sak round cue {cue} could not play: {exception.Message}");
            }
        }
    }
}
