using UnityEngine;

namespace Palengke.BangSak.Game
{
    [DisallowMultipleComponent]
    public sealed class SakBaseActor : MonoBehaviour
    {
        [SerializeField]
        private bool canUseSak = true;

        private SakBaseController currentBase;
        private SakAttemptResult lastAttemptResult;
        private int successfulSakCount;

        public bool CanUseSak => canUseSak;

        public SakBaseController CurrentBase => currentBase;

        public bool IsNearBase => currentBase != null;

        public SakAttemptResult LastAttemptResult => lastAttemptResult;

        public int SuccessfulSakCount => successfulSakCount;

        public void SetCanUseSak(bool value)
        {
            canUseSak = value;
        }

        public bool CanPressSak()
        {
            return canUseSak && currentBase != null && currentBase.IsBaseActive;
        }

        public SakAttemptResult TryPressSakNow()
        {
            return TryPressSak(Time.time);
        }

        public SakAttemptResult TryPressSak(float now)
        {
            if (currentBase == null)
            {
                lastAttemptResult = SakAttemptResult.NoBase(this, now);
                return lastAttemptResult;
            }

            lastAttemptResult = currentBase.TryPressSak(this, now);
            if (lastAttemptResult.Succeeded)
            {
                successfulSakCount += 1;
            }

            return lastAttemptResult;
        }

        public void RegisterBase(SakBaseController baseController)
        {
            if (baseController != null)
            {
                currentBase = baseController;
            }
        }

        public void UnregisterBase(SakBaseController baseController)
        {
            if (currentBase == baseController)
            {
                currentBase = null;
            }
        }
    }
}
