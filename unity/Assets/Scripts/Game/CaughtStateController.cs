using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Game
{
    public enum CaughtCause
    {
        None = 0,
        Bang = 1
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
        private Vector3 indicatorOffset = new Vector3(0f, 0.58f, 0f);

        [SerializeField]
        [Min(0.01f)]
        private float starOrbitRadius = 0.18f;

        [SerializeField]
        [Min(0.01f)]
        private float starVerticalSpread = 0.08f;

        [SerializeField]
        [Min(0.01f)]
        private float starSize = 0.18f;

        [SerializeField]
        [Min(0f)]
        private float starSpinSpeed = 5f;

        [SerializeField]
        private Color starColor = new Color(1f, 0.8f, 0.18f, 1f);

        [SerializeField]
        private Color starPulseColor = new Color(1f, 0.96f, 0.5f, 1f);

        private const int StarCount = 3;
        private static Sprite sharedStarSprite;

        private Color originalColor = Color.white;
        private bool hasOriginalColor;
        private Transform indicatorRoot;
        private readonly SpriteRenderer[] starRenderers = new SpriteRenderer[StarCount];
        private bool movementWasEnabled = true;
        private bool bangWasEnabled = true;
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

            AnimateDizzyStars(now);
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
            capturedControlState = true;
        }

        private void EnsureIndicator()
        {
            if (indicatorRoot != null)
            {
                return;
            }

            var indicatorObject = new GameObject("Caught Dizzy Stars");
            indicatorObject.transform.SetParent(transform, false);
            indicatorObject.transform.localPosition = indicatorOffset;
            indicatorRoot = indicatorObject.transform;

            for (var index = 0; index < starRenderers.Length; index += 1)
            {
                var starObject = new GameObject($"Dizzy Star {index + 1}");
                starObject.transform.SetParent(indicatorRoot, false);
                var starRenderer = starObject.AddComponent<SpriteRenderer>();
                starRenderer.sprite = GetOrCreateStarSprite();
                starRenderer.color = starColor;
                starRenderer.sortingOrder = 45 + index;
                starRenderers[index] = starRenderer;
            }

            SetIndicatorVisible(false);
        }

        private void AnimateDizzyStars(float now)
        {
            if (indicatorRoot == null)
            {
                return;
            }

            var bob = pulseSpeed > 0f ? Mathf.Sin(now * pulseSpeed) * 0.025f : 0f;
            indicatorRoot.localPosition = indicatorOffset + Vector3.up * bob;

            for (var index = 0; index < starRenderers.Length; index += 1)
            {
                var starRenderer = starRenderers[index];
                if (starRenderer == null)
                {
                    continue;
                }

                var phase = now * starSpinSpeed + index * Mathf.PI * 2f / StarCount;
                var localPulse = Mathf.Sin(phase * 1.7f) * 0.5f + 0.5f;
                starRenderer.transform.localPosition = new Vector3(
                    Mathf.Cos(phase) * starOrbitRadius,
                    Mathf.Sin(phase) * starVerticalSpread,
                    0f);
                starRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, -phase * Mathf.Rad2Deg);
                starRenderer.transform.localScale = Vector3.one * Mathf.Lerp(starSize * 0.82f, starSize * 1.16f, localPulse);
                starRenderer.color = Color.Lerp(starColor, starPulseColor, localPulse);
            }
        }

        private void SetIndicatorVisible(bool visible)
        {
            if (indicatorRoot != null)
            {
                indicatorRoot.gameObject.SetActive(visible);
            }
        }

        private static Sprite GetOrCreateStarSprite()
        {
            if (sharedStarSprite != null)
            {
                return sharedStarSprite;
            }

            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Caught Dizzy Star Runtime Sprite",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[size * size];
            var clear = new Color(0f, 0f, 0f, 0f);
            for (var index = 0; index < pixels.Length; index += 1)
            {
                pixels[index] = clear;
            }

            var vertices = CreateStarVertices(new Vector2(size * 0.5f, size * 0.5f), 28f, 12f, 5, Mathf.PI * 0.5f);
            for (var y = 0; y < size; y += 1)
            {
                for (var x = 0; x < size; x += 1)
                {
                    var point = new Vector2(x + 0.5f, y + 0.5f);
                    if (IsPointInPolygon(point, vertices))
                    {
                        pixels[y * size + x] = Color.white;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);

            sharedStarSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size);
            sharedStarSprite.name = "Caught Dizzy Star";
            return sharedStarSprite;
        }

        private static Vector2[] CreateStarVertices(Vector2 center, float outerRadius, float innerRadius, int points, float rotation)
        {
            var vertices = new Vector2[points * 2];
            for (var index = 0; index < vertices.Length; index += 1)
            {
                var radius = index % 2 == 0 ? outerRadius : innerRadius;
                var angle = rotation + index * Mathf.PI / points;
                vertices[index] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            return vertices;
        }

        private static bool IsPointInPolygon(Vector2 point, Vector2[] vertices)
        {
            var inside = false;
            for (int current = 0, previous = vertices.Length - 1; current < vertices.Length; previous = current, current += 1)
            {
                var currentVertex = vertices[current];
                var previousVertex = vertices[previous];
                var intersects =
                    (currentVertex.y > point.y) != (previousVertex.y > point.y)
                    && point.x < (previousVertex.x - currentVertex.x) * (point.y - currentVertex.y)
                    / (previousVertex.y - currentVertex.y)
                    + currentVertex.x;

                if (intersects)
                {
                    inside = !inside;
                }
            }

            return inside;
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

        }
    }
}
