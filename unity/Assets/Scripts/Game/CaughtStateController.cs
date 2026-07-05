using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Game
{
    public enum CaughtCause
    {
        None = 0,
        Bang = 1,
        Tag = 2
    }

    [DisallowMultipleComponent]
    public sealed class CaughtStateController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private PlayerMovementController movementController;

        [SerializeField]
        private BangActionController bangActionController;

        [SerializeField]
        private TagActionController tagActionController;

        [Header("Gameplay")]
        [SerializeField]
        private bool countAsHider = true;

        [SerializeField]
        private bool disableMovementWhenCaught = true;

        [SerializeField]
        private bool disableActionsWhenCaught = true;

        [Header("Caught Feedback")]
        [SerializeField]
        private Color caughtTint = new Color(1f, 0.66f, 0.35f, 1f);

        [SerializeField]
        private Color caughtPulseTint = new Color(1f, 0.94f, 0.52f, 1f);

        [SerializeField]
        [Min(0f)]
        private float pulseSpeed = 7f;

        [SerializeField]
        private string indicatorText = "CAUGHT";

        [SerializeField]
        private Vector3 indicatorOffset = new Vector3(0f, 0.58f, 0f);

        private Color originalColor = Color.white;
        private bool hasOriginalColor;
        private TextMesh indicator;
        private bool movementWasEnabled = true;
        private bool bangWasEnabled = true;
        private bool tagWasEnabled = true;
        private bool capturedControlState;

        public bool IsCaught { get; private set; }

        public CaughtCause Cause { get; private set; } = CaughtCause.None;

        public int CatchSequenceId { get; private set; } = -1;

        public Component LastCaughtSource { get; private set; }

        public bool CountAsHider => countAsHider;

        private void Awake()
        {
            ResolveReferences();
            CaptureOriginalVisualState();
            EnsureIndicator();
            SetIndicatorVisible(IsCaught);
        }

        private void Update()
        {
            if (IsCaught)
            {
                ApplyCaughtPulse(Time.time);
            }
        }

        public bool MarkCaught(Component source, CaughtCause cause, int sequenceId)
        {
            if (IsCaught)
            {
                return false;
            }

            ResolveReferences();
            CaptureOriginalVisualState();
            EnsureIndicator();
            CaptureControlState();

            IsCaught = true;
            Cause = cause;
            CatchSequenceId = sequenceId;
            LastCaughtSource = source;

            ApplyCaughtControls();
            ApplyCaughtPulse(Time.time);
            SetIndicatorVisible(true);
            return true;
        }

        public void ResetCaughtState()
        {
            ResolveReferences();

            IsCaught = false;
            Cause = CaughtCause.None;
            CatchSequenceId = -1;
            LastCaughtSource = null;

            if (spriteRenderer != null && hasOriginalColor)
            {
                spriteRenderer.color = originalColor;
            }

            RestoreControls();
            SetIndicatorVisible(false);
        }

        public void SetCountAsHider(bool value)
        {
            countAsHider = value;
        }

        private void ApplyCaughtControls()
        {
            if (disableMovementWhenCaught && movementController != null)
            {
                movementController.SetExternalInput(Vector2.zero);
                movementController.enabled = false;
            }

            if (!disableActionsWhenCaught)
            {
                return;
            }

            if (bangActionController != null)
            {
                bangActionController.enabled = false;
            }

            if (tagActionController != null)
            {
                tagActionController.enabled = false;
            }
        }

        private void RestoreControls()
        {
            if (capturedControlState && movementController != null)
            {
                movementController.enabled = movementWasEnabled;
            }

            if (capturedControlState && bangActionController != null)
            {
                bangActionController.enabled = bangWasEnabled;
            }

            if (capturedControlState && tagActionController != null)
            {
                tagActionController.enabled = tagWasEnabled;
            }

            capturedControlState = false;
        }

        private void ApplyCaughtPulse(float now)
        {
            if (spriteRenderer != null)
            {
                var pulse = pulseSpeed > 0f
                    ? Mathf.Sin(now * pulseSpeed) * 0.5f + 0.5f
                    : 0f;
                spriteRenderer.color = Color.Lerp(caughtTint, caughtPulseTint, pulse * 0.45f);
            }

            if (indicator != null)
            {
                var bob = pulseSpeed > 0f ? Mathf.Sin(now * pulseSpeed) * 0.025f : 0f;
                indicator.transform.localPosition = indicatorOffset + Vector3.up * bob;
            }
        }

        private void CaptureOriginalVisualState()
        {
            if (spriteRenderer == null || hasOriginalColor)
            {
                return;
            }

            originalColor = spriteRenderer.color;
            hasOriginalColor = true;
        }

        private void CaptureControlState()
        {
            if (capturedControlState)
            {
                return;
            }

            movementWasEnabled = movementController == null || movementController.enabled;
            bangWasEnabled = bangActionController == null || bangActionController.enabled;
            tagWasEnabled = tagActionController == null || tagActionController.enabled;
            capturedControlState = true;
        }

        private void EnsureIndicator()
        {
            if (indicator != null)
            {
                return;
            }

            var indicatorObject = new GameObject("Caught State Indicator");
            indicatorObject.transform.SetParent(transform, false);
            indicatorObject.transform.localPosition = indicatorOffset;

            indicator = indicatorObject.AddComponent<TextMesh>();
            indicator.text = indicatorText;
            indicator.anchor = TextAnchor.MiddleCenter;
            indicator.alignment = TextAlignment.Center;
            indicator.characterSize = 0.16f;
            indicator.fontSize = 46;
            indicator.color = new Color(1f, 0.88f, 0.34f, 1f);

            var meshRenderer = indicator.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = 45;
            }

            SetIndicatorVisible(false);
        }

        private void SetIndicatorVisible(bool visible)
        {
            if (indicator != null)
            {
                indicator.gameObject.SetActive(visible);
            }
        }

        private void ResolveReferences()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (movementController == null)
            {
                movementController = GetComponent<PlayerMovementController>();
            }

            if (bangActionController == null)
            {
                bangActionController = GetComponent<BangActionController>();
            }

            if (tagActionController == null)
            {
                tagActionController = GetComponent<TagActionController>();
            }
        }
    }
}
