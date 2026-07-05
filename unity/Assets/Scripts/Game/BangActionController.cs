using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Game
{
    [RequireComponent(typeof(PlayerAnimationController))]
    public sealed class BangActionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PlayerAnimationController animationController;

        [Header("Safe Bang Design")]
        [SerializeField]
        private BangActionVisualStyle visualStyle = BangActionVisualStyle.TsinelasMarker;

        [SerializeField]
        private Sprite bangMarkerSprite = null;

        [SerializeField]
        private Sprite rangeIndicatorSprite = null;

        [SerializeField]
        private Sprite impactSprite = null;

        [Header("Timing")]
        [SerializeField]
        [Min(0.1f)]
        private float cooldownSeconds = 1.25f;

        [SerializeField]
        [Min(0.05f)]
        private float effectDurationSeconds = 0.55f;

        [Header("Range")]
        [SerializeField]
        [Min(0.25f)]
        private float range = 2.25f;

        [SerializeField]
        [Min(0.05f)]
        private float markerDistance = 0.95f;

        [SerializeField]
        private bool showRangeIndicator = true;

        [Header("Tsinelas Animation")]
        [SerializeField]
        [Min(0f)]
        private float arcHeight = 0.22f;

        [SerializeField]
        [Min(0f)]
        private float spinDegrees = 540f;

        [SerializeField]
        [Range(0.1f, 0.95f)]
        private float impactStartsAt = 0.72f;

        private SpriteRenderer bangMarkerRenderer;
        private SpriteRenderer rangeIndicatorRenderer;
        private SpriteRenderer impactRenderer;
        private float lastBangTime = -999f;
        private float effectStartedAt = -999f;
        private float effectVisibleUntil = -999f;
        private PlayerFacingDirection fallbackFacingDirection = PlayerFacingDirection.Down;
        private PlayerFacingDirection lastBangDirection = PlayerFacingDirection.Down;
        private Vector3 effectStartPosition;
        private Vector3 effectEndPosition;
        private Vector2 effectDirectionVector = Vector2.down;

        public BangActionVisualStyle VisualStyle => visualStyle;

        public float CooldownSeconds => cooldownSeconds;

        public float Range => range;

        public float MarkerDistance => markerDistance;

        public PlayerFacingDirection LastBangDirection => lastBangDirection;

        public bool IsEffectVisible => bangMarkerRenderer != null && bangMarkerRenderer.enabled;

        public PlayerFacingDirection CurrentFacingDirection =>
            animationController != null ? animationController.FacingDirection : fallbackFacingDirection;

        private void Awake()
        {
            ResolveReferences();
            CreateVisualsIfNeeded();
            UpdateRangeIndicator();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TryBangNow();
            }

            UpdateRangeIndicator();
            UpdateEffectVisibility(Time.time);
        }

        public bool TryBangNow()
        {
            return TryBang(Time.time);
        }

        public bool TryBang(float now)
        {
            if (!CanBang(now))
            {
                return false;
            }

            ResolveReferences();
            CreateVisualsIfNeeded();

            lastBangTime = now;
            lastBangDirection = CurrentFacingDirection;
            ShowBangEffect(now, lastBangDirection);
            return true;
        }

        public bool CanBang(float now)
        {
            return CooldownRemaining(now) <= 0f;
        }

        public float CooldownRemaining(float now)
        {
            return Mathf.Max(0f, cooldownSeconds - (now - lastBangTime));
        }

        public float CooldownProgress(float now)
        {
            if (cooldownSeconds <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(1f - CooldownRemaining(now) / cooldownSeconds);
        }

        public Vector2 GetDirectionVector(PlayerFacingDirection direction)
        {
            switch (direction)
            {
                case PlayerFacingDirection.Down:
                    return Vector2.down;
                case PlayerFacingDirection.DownRight:
                    return new Vector2(1f, -1f).normalized;
                case PlayerFacingDirection.Right:
                    return Vector2.right;
                case PlayerFacingDirection.UpRight:
                    return new Vector2(1f, 1f).normalized;
                case PlayerFacingDirection.Up:
                    return Vector2.up;
                case PlayerFacingDirection.UpLeft:
                    return new Vector2(-1f, 1f).normalized;
                case PlayerFacingDirection.Left:
                    return Vector2.left;
                case PlayerFacingDirection.DownLeft:
                    return new Vector2(-1f, -1f).normalized;
                default:
                    return Vector2.down;
            }
        }

        public Vector3 GetEffectPosition(Vector3 origin, PlayerFacingDirection direction)
        {
            var directionVector = GetDirectionVector(direction);
            return origin + new Vector3(directionVector.x, directionVector.y, 0f) * markerDistance;
        }

        public float GetEffectRotationZ(PlayerFacingDirection direction)
        {
            var directionVector = GetDirectionVector(direction);
            return Mathf.Atan2(directionVector.y, directionVector.x) * Mathf.Rad2Deg;
        }

        public float GetRangeConeRotationZ(PlayerFacingDirection direction)
        {
            return GetEffectRotationZ(direction) - 90f;
        }

        public void SetFallbackFacingDirection(PlayerFacingDirection direction)
        {
            fallbackFacingDirection = direction;
        }

        private void ShowBangEffect(float now, PlayerFacingDirection direction)
        {
            if (bangMarkerRenderer == null)
            {
                return;
            }

            effectStartedAt = now;
            effectVisibleUntil = now + effectDurationSeconds;
            effectDirectionVector = GetDirectionVector(direction);
            effectStartPosition = transform.position + new Vector3(effectDirectionVector.x, effectDirectionVector.y, 0f) * 0.18f;
            effectEndPosition = GetEffectPosition(transform.position, direction);
            bangMarkerRenderer.enabled = true;
            if (impactRenderer != null)
            {
                impactRenderer.enabled = false;
            }

            ApplyBangAnimation(0f);
        }

        private void UpdateEffectVisibility(float now)
        {
            if (bangMarkerRenderer == null || !bangMarkerRenderer.enabled)
            {
                return;
            }

            if (now > effectVisibleUntil)
            {
                HideBangEffect();
                return;
            }

            var progress = Mathf.InverseLerp(effectStartedAt, effectVisibleUntil, now);
            ApplyBangAnimation(progress);
        }

        private void UpdateRangeIndicator()
        {
            if (rangeIndicatorRenderer == null)
            {
                return;
            }

            rangeIndicatorRenderer.enabled = showRangeIndicator;
            rangeIndicatorRenderer.transform.localScale = Vector3.one * (range * 2f);
            rangeIndicatorRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, GetRangeConeRotationZ(CurrentFacingDirection));
        }

        private void ApplyBangAnimation(float progress)
        {
            if (bangMarkerRenderer == null)
            {
                return;
            }

            var safeImpactStart = Mathf.Clamp(impactStartsAt, 0.1f, 0.95f);
            var travelProgress = Mathf.Clamp01(progress / safeImpactStart);
            var smoothedTravel = Mathf.SmoothStep(0f, 1f, travelProgress);
            var perpendicular = new Vector3(-effectDirectionVector.y, effectDirectionVector.x, 0f);
            var arcOffset = perpendicular * (Mathf.Sin(smoothedTravel * Mathf.PI) * arcHeight);
            var markerPosition = Vector3.Lerp(effectStartPosition, effectEndPosition, smoothedTravel) + arcOffset;
            var squash = progress >= safeImpactStart
                ? 1f - 0.12f * Mathf.Sin(Mathf.Clamp01((progress - safeImpactStart) / (1f - safeImpactStart)) * Mathf.PI)
                : 1f + 0.14f * Mathf.Sin(smoothedTravel * Mathf.PI);

            bangMarkerRenderer.transform.position = markerPosition;
            bangMarkerRenderer.transform.rotation = Quaternion.Euler(0f, 0f, GetEffectRotationZ(lastBangDirection) + spinDegrees * smoothedTravel);
            bangMarkerRenderer.transform.localScale = Vector3.one * squash;
            bangMarkerRenderer.color = Color.white;

            UpdateImpact(progress, safeImpactStart);
        }

        private void UpdateImpact(float progress, float safeImpactStart)
        {
            if (impactRenderer == null)
            {
                return;
            }

            if (progress < safeImpactStart)
            {
                impactRenderer.enabled = false;
                return;
            }

            var impactProgress = Mathf.Clamp01((progress - safeImpactStart) / Mathf.Max(0.01f, 1f - safeImpactStart));
            impactRenderer.enabled = true;
            impactRenderer.transform.position = effectEndPosition;
            impactRenderer.transform.localScale = Vector3.one * Mathf.Lerp(0.45f, 1.2f, impactProgress);
            impactRenderer.transform.rotation = Quaternion.Euler(0f, 0f, 35f * impactProgress);
            impactRenderer.color = new Color(1f, 1f, 1f, 1f - impactProgress * 0.65f);
        }

        private void HideBangEffect()
        {
            bangMarkerRenderer.enabled = false;
            bangMarkerRenderer.transform.localScale = Vector3.one;

            if (impactRenderer != null)
            {
                impactRenderer.enabled = false;
                impactRenderer.color = Color.white;
            }
        }

        private void CreateVisualsIfNeeded()
        {
            if (rangeIndicatorRenderer == null)
            {
                rangeIndicatorRenderer = CreateRenderer("Bang Range Indicator", rangeIndicatorSprite, 3);
            }

            if (bangMarkerRenderer == null)
            {
                bangMarkerRenderer = CreateRenderer("Bang Safe Marker", bangMarkerSprite, 30);
                bangMarkerRenderer.enabled = false;
            }

            if (impactRenderer == null)
            {
                impactRenderer = CreateRenderer("Bang Tsinelas Impact", impactSprite, 31);
                impactRenderer.enabled = false;
            }
        }

        private SpriteRenderer CreateRenderer(string name, Sprite sprite, int sortingOrder)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.zero;

            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private void ResolveReferences()
        {
            if (animationController == null)
            {
                animationController = GetComponent<PlayerAnimationController>();
            }
        }
    }
}
