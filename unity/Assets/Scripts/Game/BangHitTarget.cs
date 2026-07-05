using UnityEngine;

namespace Palengke.BangSak.Game
{
    [DisallowMultipleComponent]
    public sealed class BangHitTarget : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Color hitFlashColor = new Color(0.65f, 1f, 0.55f, 1f);

        [SerializeField]
        [Min(0.05f)]
        private float hitFlashSeconds = 0.18f;

        private Color originalColor = Color.white;
        private float flashUntil = -1f;
        private int lastRegisteredSequenceId = -1;

        public int HitCount { get; private set; }

        public BangActionController LastHitSource { get; private set; }

        public BangHitResult LastHitResult { get; private set; }

        private void Awake()
        {
            ResolveReferences();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        private void Update()
        {
            if (spriteRenderer != null && flashUntil > 0f && Time.time >= flashUntil)
            {
                spriteRenderer.color = originalColor;
                flashUntil = -1f;
            }
        }

        public bool RegisterBangHit(BangActionController source, int sequenceId, BangHitResult result)
        {
            if (sequenceId == lastRegisteredSequenceId)
            {
                return false;
            }

            ResolveReferences();
            lastRegisteredSequenceId = sequenceId;
            HitCount += 1;
            LastHitSource = source;
            LastHitResult = result;
            FlashHitFeedback();
            return true;
        }

        private void FlashHitFeedback()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (flashUntil < 0f)
            {
                originalColor = spriteRenderer.color;
            }

            spriteRenderer.color = hitFlashColor;
            flashUntil = Time.time + hitFlashSeconds;
        }

        private void ResolveReferences()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }
    }
}
