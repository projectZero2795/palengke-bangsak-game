using System.Collections.Generic;
using Palengke.BangSak.Player;
using Palengke.BangSak.UI;
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

        [Header("Hit Detection")]
        [SerializeField]
        private bool enableHitDetection = true;

        [SerializeField]
        private bool blockBangWithSolidColliders = true;

        [SerializeField]
        private LayerMask hitDetectionLayers = Physics2D.DefaultRaycastLayers;

        [SerializeField]
        [Min(0.01f)]
        private float hitRadius = 0.18f;

        [SerializeField]
        [Min(0f)]
        private float hitStartOffset = 0.18f;

        [Header("Hit Feedback")]
        [SerializeField]
        private Color hitFeedbackColor = new Color(0.65f, 1f, 0.55f, 1f);

        [SerializeField]
        private Color missFeedbackColor = new Color(1f, 0.72f, 0.35f, 1f);

        [SerializeField]
        private Color blockedFeedbackColor = new Color(0.85f, 0.9f, 1f, 1f);

        [SerializeField]
        private Color nameMismatchFeedbackColor = new Color(1f, 0.72f, 0.35f, 1f);

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
        private readonly Dictionary<string, float> lastBangTimeByTarget = new Dictionary<string, float>();
        private float effectStartedAt = -999f;
        private float effectVisibleUntil = -999f;
        private PlayerFacingDirection fallbackFacingDirection = PlayerFacingDirection.Down;
        private PlayerFacingDirection lastBangDirection = PlayerFacingDirection.Down;
        private Vector3 effectStartPosition;
        private Vector3 effectEndPosition;
        private Vector2 effectDirectionVector = Vector2.down;
        private Color effectFeedbackColor = Color.white;
        private int bangSequenceId;
        private BangHitResult lastHitResult;
        private BangNameCallController nameCallController;

        public BangActionVisualStyle VisualStyle => visualStyle;

        public float CooldownSeconds => cooldownSeconds;

        public float Range => range;

        public float MarkerDistance => markerDistance;

        public PlayerFacingDirection LastBangDirection => lastBangDirection;

        public bool IsEffectVisible => bangMarkerRenderer != null && bangMarkerRenderer.enabled;

        public BangHitResult LastHitResult => lastHitResult;

        public float HitRadius => hitRadius;

        public bool BlockBangWithSolidColliders => blockBangWithSolidColliders;

        public PlayerFacingDirection CurrentFacingDirection =>
            animationController != null ? animationController.FacingDirection : fallbackFacingDirection;

        private void Awake()
        {
            ResolveReferences();
            CreateVisualsIfNeeded();
            UpdateRangeIndicator();
        }

        private void OnDisable()
        {
            HideBangEffect();

            if (rangeIndicatorRenderer != null)
            {
                rangeIndicatorRenderer.enabled = false;
            }
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
            ResolveReferences();
            if (!CanBang(now))
            {
                return false;
            }

            CreateVisualsIfNeeded();

            RememberBangTime(CurrentCooldownTargetName, now);
            lastBangDirection = CurrentFacingDirection;
            bangSequenceId += 1;
            lastHitResult = ResolveBangHit(transform.position, lastBangDirection, bangSequenceId);
            ShowBangEffect(now, lastBangDirection, lastHitResult);
            AccessibilityCueService.PublishBang(lastHitResult.Outcome);
            return true;
        }

        public bool CanBang(float now)
        {
            if (!isActiveAndEnabled)
            {
                return false;
            }

            return CooldownRemaining(now) <= 0f;
        }

        public float CooldownRemaining(float now)
        {
            return CooldownRemainingForTarget(CurrentCooldownTargetName, now);
        }

        public float CooldownProgress(float now)
        {
            if (cooldownSeconds <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(1f - CooldownRemaining(now) / cooldownSeconds);
        }

        public bool TryBangTargetNow(string targetName)
        {
            return TryBangTarget(targetName, Time.time);
        }

        public bool TryBangTarget(string targetName, float now)
        {
            ResolveReferences();
            if (nameCallController != null)
            {
                nameCallController.SetSelectedTargetName(targetName);
            }

            return TryBang(now);
        }

        public bool CanBangTarget(string targetName, float now)
        {
            return isActiveAndEnabled && CooldownRemainingForTarget(targetName, now) <= 0f;
        }

        public float CooldownRemainingForTarget(string targetName, float now)
        {
            var normalizedTargetName = PlayerNameIdentity.NormalizeName(targetName);
            var lastUsedAt = lastBangTime;
            if (!string.IsNullOrEmpty(normalizedTargetName)
                && lastBangTimeByTarget.TryGetValue(normalizedTargetName, out var targetLastUsedAt))
            {
                lastUsedAt = targetLastUsedAt;
            }
            else if (!string.IsNullOrEmpty(normalizedTargetName))
            {
                lastUsedAt = -999f;
            }

            return Mathf.Max(0f, cooldownSeconds - (now - lastUsedAt));
        }

        public float CooldownProgressForTarget(string targetName, float now)
        {
            return cooldownSeconds <= 0f
                ? 1f
                : Mathf.Clamp01(1f - CooldownRemainingForTarget(targetName, now) / cooldownSeconds);
        }

        private string CurrentCooldownTargetName =>
            nameCallController != null ? nameCallController.SelectedTargetName : string.Empty;

        private void RememberBangTime(string targetName, float now)
        {
            var normalizedTargetName = PlayerNameIdentity.NormalizeName(targetName);
            if (string.IsNullOrEmpty(normalizedTargetName))
            {
                lastBangTime = now;
                return;
            }

            lastBangTimeByTarget[normalizedTargetName] = now;
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

        public BangHitResult ResolveBangHit(Vector3 origin, PlayerFacingDirection direction, int sequenceId)
        {
            ResolveReferences();

            var directionVector = GetDirectionVector(direction).normalized;
            if (directionVector.sqrMagnitude <= 0f)
            {
                directionVector = Vector2.down;
            }

            var castOrigin = (Vector2)origin + directionVector * hitStartOffset;
            var castDistance = Mathf.Max(0f, range - hitStartOffset);
            var missPoint = castOrigin + directionVector * castDistance;

            if (!enableHitDetection || castDistance <= 0f)
            {
                return BangHitResult.Miss(castOrigin, missPoint, directionVector, range, sequenceId);
            }

            var hits = Physics2D.CircleCastAll(
                castOrigin,
                hitRadius,
                directionVector,
                castDistance,
                hitDetectionLayers.value);

            if (hits.Length <= 0)
            {
                return BangHitResult.Miss(castOrigin, missPoint, directionVector, range, sequenceId);
            }

            System.Array.Sort(hits, CompareHitsByDistanceAndBlockPriority);

            foreach (var hit in hits)
            {
                if (hit.collider == null || ShouldIgnoreCollider(hit.collider))
                {
                    continue;
                }

                var hitPoint = ResolveHitPoint(hit, castOrigin, directionVector);
                var distance = hitStartOffset + Mathf.Max(0f, hit.distance);
                var target = hit.collider.GetComponentInParent<BangHitTarget>();
                if (target != null && target.isActiveAndEnabled)
                {
                    var result = BangHitResult.HitTarget(
                        target,
                        hit.collider,
                        castOrigin,
                        hitPoint,
                        directionVector,
                        distance,
                        sequenceId);
                    return RegisterValidatedBangHit(target, sequenceId, result);
                }

                if (blockBangWithSolidColliders && !hit.collider.isTrigger)
                {
                    return BangHitResult.Blocked(hit.collider, castOrigin, hitPoint, directionVector, distance, sequenceId);
                }
            }

            return BangHitResult.Miss(castOrigin, missPoint, directionVector, range, sequenceId);
        }

        private void ShowBangEffect(float now, PlayerFacingDirection direction, BangHitResult hitResult)
        {
            if (bangMarkerRenderer == null)
            {
                return;
            }

            effectStartedAt = now;
            effectVisibleUntil = now + effectDurationSeconds;
            effectDirectionVector = GetDirectionVector(direction);
            effectStartPosition = transform.position + new Vector3(effectDirectionVector.x, effectDirectionVector.y, 0f) * 0.18f;
            effectEndPosition = new Vector3(hitResult.Point.x, hitResult.Point.y, transform.position.z);
            effectFeedbackColor = GetFeedbackColor(hitResult.Outcome);
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

            if (AccessibilitySettings.ReducedMotionEnabled)
            {
                bangMarkerRenderer.transform.position = effectEndPosition;
                bangMarkerRenderer.transform.rotation = Quaternion.Euler(0f, 0f, GetEffectRotationZ(lastBangDirection));
                bangMarkerRenderer.transform.localScale = Vector3.one;
                bangMarkerRenderer.color = Color.white;
                UpdateImpact(1f, Mathf.Clamp(impactStartsAt, 0.1f, 0.95f));
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
            impactRenderer.color = new Color(
                effectFeedbackColor.r,
                effectFeedbackColor.g,
                effectFeedbackColor.b,
                effectFeedbackColor.a * (1f - impactProgress * 0.65f));
        }

        private void HideBangEffect()
        {
            if (bangMarkerRenderer != null)
            {
                bangMarkerRenderer.enabled = false;
                bangMarkerRenderer.transform.localScale = Vector3.one;
            }

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

            if (nameCallController == null)
            {
                nameCallController = GetComponent<BangNameCallController>();
            }
        }

        private BangHitResult RegisterValidatedBangHit(BangHitTarget target, int sequenceId, BangHitResult result)
        {
            if (nameCallController != null && nameCallController.isActiveAndEnabled)
            {
                var validation = nameCallController.ValidateBangTarget(target);
                if (!validation.IsValid)
                {
                    var mismatchResult = BangHitResult.NameMismatch(
                        target,
                        result.Collider,
                        result.Origin,
                        result.Point,
                        result.Direction,
                        result.Distance,
                        sequenceId,
                        validation.Message);
                    target.RegisterBangNameMismatch(this, sequenceId, mismatchResult);
                    return mismatchResult;
                }
            }

            target.RegisterBangHit(this, sequenceId, result);
            return result;
        }

        private bool ShouldIgnoreCollider(Collider2D hitCollider)
        {
            return hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform);
        }

        private int CompareHitsByDistanceAndBlockPriority(RaycastHit2D left, RaycastHit2D right)
        {
            var distanceComparison = left.distance.CompareTo(right.distance);
            if (Mathf.Abs(left.distance - right.distance) > 0.001f)
            {
                return distanceComparison;
            }

            return GetHitPriority(left.collider).CompareTo(GetHitPriority(right.collider));
        }

        private int GetHitPriority(Collider2D hitCollider)
        {
            if (hitCollider == null || ShouldIgnoreCollider(hitCollider))
            {
                return 3;
            }

            if (blockBangWithSolidColliders && !hitCollider.isTrigger && hitCollider.GetComponentInParent<BangHitTarget>() == null)
            {
                return 0;
            }

            if (hitCollider.GetComponentInParent<BangHitTarget>() != null)
            {
                return 1;
            }

            return 2;
        }

        private Color GetFeedbackColor(BangHitOutcome outcome)
        {
            switch (outcome)
            {
                case BangHitOutcome.HitTarget:
                    return hitFeedbackColor;
                case BangHitOutcome.Blocked:
                    return blockedFeedbackColor;
                case BangHitOutcome.NameMismatch:
                    return nameMismatchFeedbackColor;
                default:
                    return missFeedbackColor;
            }
        }

        private static Vector2 ResolveHitPoint(RaycastHit2D hit, Vector2 origin, Vector2 direction)
        {
            if (hit.point != Vector2.zero)
            {
                return hit.point;
            }

            return origin + direction * Mathf.Max(0f, hit.distance);
        }
    }
}
