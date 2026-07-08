using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Game
{
    [DisallowMultipleComponent]
    public sealed class TayaCounteredStateController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private PlayerMovementController movementController;

        [SerializeField]
        private BangActionController bangActionController;

        [SerializeField]
        private PlayerRoleController roleController;

        [Header("Feedback")]
        [SerializeField]
        [Min(0.1f)]
        private float counteredSeconds = 1.15f;

        [SerializeField]
        private Color counteredTint = new Color(0.72f, 1f, 0.68f, 1f);

        [SerializeField]
        private Color counteredPulseTint = new Color(1f, 0.95f, 0.45f, 1f);

        [SerializeField]
        private Vector3 indicatorOffset = new Vector3(0f, 0.62f, 0f);

        [SerializeField]
        [Min(0.01f)]
        private float indicatorSize = 0.28f;

        [SerializeField]
        [Min(0f)]
        private float indicatorSpinSpeed = 4.5f;

        private static Sprite sharedSakBurstSprite;

        private Transform indicatorRoot;
        private SpriteRenderer indicatorRenderer;
        private Color originalColor = Color.white;
        private bool hasOriginalColor;
        private bool movementWasEnabled = true;
        private bool bangWasEnabled = true;
        private bool capturedControls;
        private float counteredUntil = -999f;
        private int lastSequenceId = -1;

        public bool IsCountered { get; private set; }

        public int CounteredCount { get; private set; }

        public Component LastCounterSource { get; private set; }

        public float CounteredSeconds => counteredSeconds;

        private void Awake()
        {
            ResolveReferences();
            CaptureOriginalVisualState();
            EnsureIndicator();
            SetIndicatorVisible(false);
        }

        private void Update()
        {
            if (!IsCountered)
            {
                return;
            }

            if (Time.time >= counteredUntil)
            {
                ResetCounteredState();
                return;
            }

            ApplyCounteredPulse(Time.time);
        }

        public bool CanBeCounteredBySak()
        {
            ResolveReferences();
            return roleController == null || roleController.IsTaya;
        }

        public bool MarkCountered(Component source, int sequenceId)
        {
            if (sequenceId == lastSequenceId)
            {
                return false;
            }

            ResolveReferences();
            if (!CanBeCounteredBySak())
            {
                return false;
            }

            CaptureOriginalVisualState();
            EnsureIndicator();
            CaptureControlState();

            lastSequenceId = sequenceId;
            CounteredCount += 1;
            LastCounterSource = source;
            IsCountered = true;
            counteredUntil = Time.time + counteredSeconds;

            ApplyCounteredControls();
            ApplyCounteredPulse(Time.time);
            SetIndicatorVisible(true);
            return true;
        }

        public void ResetCounteredState()
        {
            ResolveReferences();
            IsCountered = false;
            LastCounterSource = null;

            if (spriteRenderer != null && hasOriginalColor)
            {
                spriteRenderer.color = originalColor;
            }

            RestoreControls();
            SetIndicatorVisible(false);
        }

        private void ApplyCounteredControls()
        {
            if (movementController != null)
            {
                movementController.SetExternalInput(Vector2.zero);
                movementController.enabled = false;
            }

            if (bangActionController != null)
            {
                bangActionController.enabled = false;
            }
        }

        private void RestoreControls()
        {
            if (capturedControls && movementController != null)
            {
                movementController.enabled = movementWasEnabled;
            }

            if (capturedControls && bangActionController != null)
            {
                bangActionController.enabled = bangWasEnabled && (roleController == null || roleController.CanUseBang);
            }

            capturedControls = false;
        }

        private void ApplyCounteredPulse(float now)
        {
            var pulse = Mathf.Sin(now * 9f) * 0.5f + 0.5f;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(counteredTint, counteredPulseTint, pulse * 0.5f);
            }

            if (indicatorRenderer != null)
            {
                indicatorRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, now * indicatorSpinSpeed * 90f);
                indicatorRenderer.transform.localScale = Vector3.one * Mathf.Lerp(indicatorSize * 0.88f, indicatorSize * 1.12f, pulse);
                indicatorRenderer.color = Color.Lerp(Color.white, counteredPulseTint, pulse * 0.45f);
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
            if (capturedControls)
            {
                return;
            }

            movementWasEnabled = movementController == null || movementController.enabled;
            bangWasEnabled = bangActionController == null || bangActionController.enabled;
            capturedControls = true;
        }

        private void EnsureIndicator()
        {
            if (indicatorRoot != null)
            {
                return;
            }

            var indicatorObject = new GameObject("SAK Counter Burst");
            indicatorObject.transform.SetParent(transform, false);
            indicatorObject.transform.localPosition = indicatorOffset;
            indicatorRoot = indicatorObject.transform;

            indicatorRenderer = indicatorObject.AddComponent<SpriteRenderer>();
            indicatorRenderer.sprite = GetOrCreateSakBurstSprite();
            indicatorRenderer.sortingOrder = 48;
            indicatorRenderer.color = Color.white;
        }

        private void SetIndicatorVisible(bool visible)
        {
            if (indicatorRoot != null)
            {
                indicatorRoot.gameObject.SetActive(visible);
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

            if (roleController == null)
            {
                roleController = GetComponent<PlayerRoleController>();
            }
        }

        private static Sprite GetOrCreateSakBurstSprite()
        {
            if (sharedSakBurstSprite != null)
            {
                return sharedSakBurstSprite;
            }

            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "SAK Counter Burst Runtime Sprite",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[size * size];
            var center = new Vector2(size * 0.5f, size * 0.5f);
            for (var y = 0; y < size; y += 1)
            {
                for (var x = 0; x < size; x += 1)
                {
                    var point = new Vector2(x + 0.5f, y + 0.5f);
                    var offset = point - center;
                    var distance = offset.magnitude;
                    var angle = Mathf.Atan2(offset.y, offset.x);
                    var ray = Mathf.Abs(Mathf.Sin(angle * 6f));
                    var rayRadius = Mathf.Lerp(16f, 42f, ray);
                    var circleAlpha = Mathf.Clamp01(1f - Mathf.Abs(distance - 20f) / 8f);
                    var rayAlpha = distance < rayRadius && distance > 9f ? Mathf.Clamp01(1f - distance / 46f) : 0f;
                    var alpha = Mathf.Max(circleAlpha * 0.75f, rayAlpha * 0.9f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);

            sharedSakBurstSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size);
            sharedSakBurstSprite.name = "SAK Counter Burst";
            return sharedSakBurstSprite;
        }
    }
}
