using UnityEngine;

namespace Palengke.BangSak.Game
{
    [DisallowMultipleComponent]
    public sealed class TagHitTarget : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Color tagFlashColor = new Color(0.42f, 0.92f, 1f, 1f);

        [SerializeField]
        [Min(0.05f)]
        private float tagFlashSeconds = 0.18f;

        private Color originalColor = Color.white;
        private float flashUntil = -1f;
        private int lastRegisteredSequenceId = -1;

        public int TagHitCount { get; private set; }

        public TagActionController LastTagSource { get; private set; }

        public TagHitResult LastTagResult { get; private set; }

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

        public bool RegisterTagHit(TagActionController source, int sequenceId, TagHitResult result)
        {
            if (sequenceId == lastRegisteredSequenceId)
            {
                return false;
            }

            ResolveReferences();
            lastRegisteredSequenceId = sequenceId;
            TagHitCount += 1;
            LastTagSource = source;
            LastTagResult = result;
            FlashTagFeedback();
            return true;
        }

        private void FlashTagFeedback()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (flashUntil < 0f)
            {
                originalColor = spriteRenderer.color;
            }

            spriteRenderer.color = tagFlashColor;
            flashUntil = Time.time + tagFlashSeconds;
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
