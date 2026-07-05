using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Game
{
    [RequireComponent(typeof(PlayerAnimationController))]
    public sealed class TagActionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PlayerAnimationController animationController;

        [SerializeField]
        private Sprite tagFeedbackSprite = null;

        [Header("Timing")]
        [SerializeField]
        [Min(0.1f)]
        private float cooldownSeconds = 0.8f;

        [SerializeField]
        [Min(0.05f)]
        private float effectDurationSeconds = 0.22f;

        [Header("Close Tag Detection")]
        [SerializeField]
        private bool enableTagDetection = true;

        [SerializeField]
        private bool blockTagWithSolidColliders = true;

        [SerializeField]
        private LayerMask tagDetectionLayers = Physics2D.DefaultRaycastLayers;

        [SerializeField]
        [Min(0.1f)]
        private float tagRange = 0.82f;

        [SerializeField]
        [Min(0.01f)]
        private float tagRadius = 0.32f;

        [SerializeField]
        [Min(0f)]
        private float tagStartOffset = 0.16f;

        [Header("Feedback")]
        [SerializeField]
        private Color hitFeedbackColor = new Color(0.42f, 0.92f, 1f, 1f);

        [SerializeField]
        private Color missFeedbackColor = new Color(1f, 0.82f, 0.42f, 1f);

        [SerializeField]
        private Color blockedFeedbackColor = new Color(0.82f, 0.88f, 1f, 1f);

        private SpriteRenderer tagFeedbackRenderer;
        private float lastTagTime = -999f;
        private float effectStartedAt = -999f;
        private float effectVisibleUntil = -999f;
        private PlayerFacingDirection fallbackFacingDirection = PlayerFacingDirection.Down;
        private PlayerFacingDirection lastTagDirection = PlayerFacingDirection.Down;
        private Vector3 effectPosition;
        private Color effectFeedbackColor = Color.white;
        private int tagSequenceId;
        private TagHitResult lastTagResult;

        public float CooldownSeconds => cooldownSeconds;

        public float TagRange => tagRange;

        public float TagRadius => tagRadius;

        public bool BlockTagWithSolidColliders => blockTagWithSolidColliders;

        public PlayerFacingDirection LastTagDirection => lastTagDirection;

        public TagHitResult LastTagResult => lastTagResult;

        public bool IsEffectVisible => tagFeedbackRenderer != null && tagFeedbackRenderer.enabled;

        public PlayerFacingDirection CurrentFacingDirection =>
            animationController != null ? animationController.FacingDirection : fallbackFacingDirection;

        private void Awake()
        {
            ResolveReferences();
            CreateVisualsIfNeeded();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                TryTagNow();
            }

            UpdateEffectVisibility(Time.time);
        }

        public bool TryTagNow()
        {
            return TryTag(Time.time);
        }

        public bool TryTag(float now)
        {
            if (!CanTag(now))
            {
                return false;
            }

            ResolveReferences();
            CreateVisualsIfNeeded();

            lastTagTime = now;
            lastTagDirection = CurrentFacingDirection;
            tagSequenceId += 1;
            lastTagResult = ResolveTagHit(transform.position, lastTagDirection, tagSequenceId);
            ShowTagEffect(now, lastTagResult);
            return true;
        }

        public bool CanTag(float now)
        {
            return CooldownRemaining(now) <= 0f;
        }

        public float CooldownRemaining(float now)
        {
            return Mathf.Max(0f, cooldownSeconds - (now - lastTagTime));
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

        public void SetFallbackFacingDirection(PlayerFacingDirection direction)
        {
            fallbackFacingDirection = direction;
        }

        public TagHitResult ResolveTagHit(Vector3 origin, PlayerFacingDirection direction, int sequenceId)
        {
            var directionVector = GetDirectionVector(direction).normalized;
            if (directionVector.sqrMagnitude <= 0f)
            {
                directionVector = Vector2.down;
            }

            var castOrigin = (Vector2)origin + directionVector * tagStartOffset;
            var castDistance = Mathf.Max(0f, tagRange - tagStartOffset);
            var missPoint = castOrigin + directionVector * castDistance;

            if (!enableTagDetection || castDistance <= 0f)
            {
                return TagHitResult.Miss(castOrigin, missPoint, directionVector, tagRange, sequenceId);
            }

            var hits = Physics2D.CircleCastAll(
                castOrigin,
                tagRadius,
                directionVector,
                castDistance,
                tagDetectionLayers.value);

            if (hits.Length <= 0)
            {
                return TagHitResult.Miss(castOrigin, missPoint, directionVector, tagRange, sequenceId);
            }

            System.Array.Sort(hits, CompareHitsByDistanceAndBlockPriority);

            foreach (var hit in hits)
            {
                if (hit.collider == null || ShouldIgnoreCollider(hit.collider))
                {
                    continue;
                }

                var hitPoint = ResolveHitPoint(hit, castOrigin, directionVector);
                var distance = tagStartOffset + Mathf.Max(0f, hit.distance);
                var target = hit.collider.GetComponentInParent<TagHitTarget>();
                if (target != null && target.isActiveAndEnabled)
                {
                    var result = TagHitResult.HitTarget(
                        target,
                        hit.collider,
                        castOrigin,
                        hitPoint,
                        directionVector,
                        distance,
                        sequenceId);
                    target.RegisterTagHit(this, sequenceId, result);
                    return result;
                }

                if (blockTagWithSolidColliders && !hit.collider.isTrigger)
                {
                    return TagHitResult.Blocked(hit.collider, castOrigin, hitPoint, directionVector, distance, sequenceId);
                }
            }

            return TagHitResult.Miss(castOrigin, missPoint, directionVector, tagRange, sequenceId);
        }

        private void ShowTagEffect(float now, TagHitResult result)
        {
            if (tagFeedbackRenderer == null)
            {
                return;
            }

            effectStartedAt = now;
            effectVisibleUntil = now + effectDurationSeconds;
            effectPosition = new Vector3(result.Point.x, result.Point.y, transform.position.z);
            effectFeedbackColor = GetFeedbackColor(result.Outcome);
            tagFeedbackRenderer.enabled = true;
            ApplyTagAnimation(0f);
        }

        private void UpdateEffectVisibility(float now)
        {
            if (tagFeedbackRenderer == null || !tagFeedbackRenderer.enabled)
            {
                return;
            }

            if (now > effectVisibleUntil)
            {
                HideTagEffect();
                return;
            }

            var progress = Mathf.InverseLerp(effectStartedAt, effectVisibleUntil, now);
            ApplyTagAnimation(progress);
        }

        private void ApplyTagAnimation(float progress)
        {
            if (tagFeedbackRenderer == null)
            {
                return;
            }

            tagFeedbackRenderer.transform.position = effectPosition;
            tagFeedbackRenderer.transform.rotation = Quaternion.Euler(0f, 0f, progress * 28f);
            tagFeedbackRenderer.transform.localScale = Vector3.one * Mathf.Lerp(0.35f, 1.15f, Mathf.SmoothStep(0f, 1f, progress));
            tagFeedbackRenderer.color = new Color(
                effectFeedbackColor.r,
                effectFeedbackColor.g,
                effectFeedbackColor.b,
                effectFeedbackColor.a * (1f - progress * 0.7f));
        }

        private void HideTagEffect()
        {
            tagFeedbackRenderer.enabled = false;
            tagFeedbackRenderer.transform.localScale = Vector3.one;
            tagFeedbackRenderer.color = Color.white;
        }

        private void CreateVisualsIfNeeded()
        {
            if (tagFeedbackRenderer != null)
            {
                return;
            }

            var child = new GameObject("Close Tag Tap Feedback");
            child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.zero;

            tagFeedbackRenderer = child.AddComponent<SpriteRenderer>();
            tagFeedbackRenderer.sprite = tagFeedbackSprite;
            tagFeedbackRenderer.sortingOrder = 32;
            tagFeedbackRenderer.enabled = false;
        }

        private void ResolveReferences()
        {
            if (animationController == null)
            {
                animationController = GetComponent<PlayerAnimationController>();
            }
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

            if (blockTagWithSolidColliders && !hitCollider.isTrigger && hitCollider.GetComponentInParent<TagHitTarget>() == null)
            {
                return 0;
            }

            if (hitCollider.GetComponentInParent<TagHitTarget>() != null)
            {
                return 1;
            }

            return 2;
        }

        private Color GetFeedbackColor(TagHitOutcome outcome)
        {
            switch (outcome)
            {
                case TagHitOutcome.HitTarget:
                    return hitFeedbackColor;
                case TagHitOutcome.Blocked:
                    return blockedFeedbackColor;
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
