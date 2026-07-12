using Palengke.BangSak.Player;
using Palengke.BangSak.UI;
using UnityEngine;

namespace Palengke.BangSak.Game
{
    [RequireComponent(typeof(PlayerRoleController))]
    public sealed class SakCounterController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PlayerAnimationController animationController;

        [SerializeField]
        private PlayerRoleController roleController;

        [Header("Safe SAK Design")]
        [SerializeField]
        [Min(0.1f)]
        private float cooldownSeconds = 1.25f;

        [SerializeField]
        [Min(0.1f)]
        private float range = 0.78f;

        [SerializeField]
        [Min(0.01f)]
        private float hitRadius = 0.26f;

        [SerializeField]
        [Min(0f)]
        private float hitStartOffset = 0.12f;

        [SerializeField]
        private bool blockSakWithSolidColliders = true;

        [SerializeField]
        private LayerMask hitDetectionLayers = Physics2D.DefaultRaycastLayers;

        [SerializeField]
        private bool enableKeyboardShortcut = false;

        [Header("Feedback")]
        [SerializeField]
        [Min(0.1f)]
        private float effectSeconds = 0.38f;

        [SerializeField]
        private Color counterColor = new Color(0.46f, 1f, 0.42f, 1f);

        [SerializeField]
        private Color missColor = new Color(1f, 0.8f, 0.36f, 1f);

        [SerializeField]
        private Color blockedColor = new Color(0.75f, 0.85f, 1f, 1f);

        private SpriteRenderer burstRenderer;
        private float lastSakTime = -999f;
        private float effectStartedAt = -999f;
        private float effectVisibleUntil = -999f;
        private int sakSequenceId;
        private PlayerFacingDirection fallbackFacingDirection = PlayerFacingDirection.Down;
        private SakCounterResult lastResult;

        public float CooldownSeconds => cooldownSeconds;

        public float Range => range;

        public float HitRadius => hitRadius;

        public bool BlockSakWithSolidColliders => blockSakWithSolidColliders;

        public SakCounterResult LastResult => lastResult;

        public bool CanUseSak => roleController != null && roleController.IsHider;

        public PlayerFacingDirection CurrentFacingDirection =>
            animationController != null ? animationController.FacingDirection : fallbackFacingDirection;

        private void Awake()
        {
            ResolveReferences();
            CreateVisualsIfNeeded();
        }

        private void OnDisable()
        {
            HideEffect();
        }

        private void Update()
        {
            if (enableKeyboardShortcut && Input.GetKeyDown(KeyCode.E))
            {
                TrySakNow();
            }

            UpdateEffect(Time.time);
        }

        public bool TrySakNow()
        {
            return TrySak(Time.time);
        }

        public bool TrySak(float now)
        {
            if (!CanSak(now))
            {
                return false;
            }

            ResolveReferences();
            CreateVisualsIfNeeded();

            lastSakTime = now;
            sakSequenceId += 1;
            lastResult = ResolveSakHit(transform.position, CurrentFacingDirection, sakSequenceId);
            ShowEffect(now, lastResult);
            AccessibilityCueService.PublishSak(lastResult.Outcome);
            return true;
        }

        public bool CanSak(float now)
        {
            ResolveReferences();
            if (!isActiveAndEnabled || !CanUseSak)
            {
                return false;
            }

            return CooldownRemaining(now) <= 0f;
        }

        public float CooldownRemaining(float now)
        {
            return Mathf.Max(0f, cooldownSeconds - (now - lastSakTime));
        }

        public float CooldownProgress(float now)
        {
            if (cooldownSeconds <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(1f - CooldownRemaining(now) / cooldownSeconds);
        }

        public void SetFallbackFacingDirection(PlayerFacingDirection direction)
        {
            fallbackFacingDirection = direction;
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

        public SakCounterResult ResolveSakHit(Vector3 origin, PlayerFacingDirection direction, int sequenceId)
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

            if (castDistance <= 0f)
            {
                return SakCounterResult.Miss(castOrigin, missPoint, directionVector, range, sequenceId);
            }

            var hits = Physics2D.CircleCastAll(
                castOrigin,
                hitRadius,
                directionVector,
                castDistance,
                hitDetectionLayers.value);

            if (hits.Length <= 0)
            {
                return SakCounterResult.Miss(castOrigin, missPoint, directionVector, range, sequenceId);
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
                var target = hit.collider.GetComponentInParent<TayaCounteredStateController>();
                if (target != null && target.isActiveAndEnabled)
                {
                    if (!target.CanBeCounteredBySak())
                    {
                        return SakCounterResult.WrongRole(
                            target,
                            hit.collider,
                            castOrigin,
                            hitPoint,
                            directionVector,
                            distance,
                            sequenceId);
                    }

                    var result = SakCounterResult.CounteredTaya(
                        target,
                        hit.collider,
                        castOrigin,
                        hitPoint,
                        directionVector,
                        distance,
                        sequenceId);
                    target.MarkCountered(this, sequenceId);
                    return result;
                }

                if (blockSakWithSolidColliders && !hit.collider.isTrigger)
                {
                    return SakCounterResult.Blocked(hit.collider, castOrigin, hitPoint, directionVector, distance, sequenceId);
                }
            }

            return SakCounterResult.Miss(castOrigin, missPoint, directionVector, range, sequenceId);
        }

        private void ShowEffect(float now, SakCounterResult result)
        {
            if (burstRenderer == null)
            {
                return;
            }

            effectStartedAt = now;
            effectVisibleUntil = now + effectSeconds;
            burstRenderer.enabled = true;
            burstRenderer.transform.position = result.Point;
            burstRenderer.transform.localScale = Vector3.one * 0.42f;
            burstRenderer.color = GetFeedbackColor(result.Outcome);
        }

        private void UpdateEffect(float now)
        {
            if (burstRenderer == null || !burstRenderer.enabled)
            {
                return;
            }

            if (now >= effectVisibleUntil)
            {
                HideEffect();
                return;
            }

            if (AccessibilitySettings.ReducedMotionEnabled)
            {
                burstRenderer.transform.localScale = Vector3.one * 0.78f;
                burstRenderer.transform.rotation = Quaternion.identity;
                var staticColor = burstRenderer.color;
                staticColor.a = 1f;
                burstRenderer.color = staticColor;
                return;
            }

            var progress = Mathf.InverseLerp(effectStartedAt, effectVisibleUntil, now);
            burstRenderer.transform.localScale = Vector3.one * Mathf.Lerp(0.42f, 1.05f, progress);
            burstRenderer.transform.rotation = Quaternion.Euler(0f, 0f, 45f * progress);
            var color = burstRenderer.color;
            color.a = 1f - progress * 0.75f;
            burstRenderer.color = color;
        }

        private void HideEffect()
        {
            if (burstRenderer != null)
            {
                burstRenderer.enabled = false;
            }
        }

        private void CreateVisualsIfNeeded()
        {
            if (burstRenderer != null)
            {
                return;
            }

            var burstObject = new GameObject("Safe SAK Burst");
            burstObject.transform.SetParent(transform, false);
            burstObject.transform.localPosition = Vector3.zero;
            burstRenderer = burstObject.AddComponent<SpriteRenderer>();
            burstRenderer.sprite = CreateBurstSprite();
            burstRenderer.sortingOrder = 47;
            burstRenderer.enabled = false;
        }

        private Color GetFeedbackColor(SakCounterOutcome outcome)
        {
            switch (outcome)
            {
                case SakCounterOutcome.CounteredTaya:
                    return counterColor;
                case SakCounterOutcome.Blocked:
                    return blockedColor;
                default:
                    return missColor;
            }
        }

        private void ResolveReferences()
        {
            if (animationController == null)
            {
                animationController = GetComponent<PlayerAnimationController>();
            }

            if (roleController == null)
            {
                roleController = GetComponent<PlayerRoleController>();
            }
        }

        private bool ShouldIgnoreCollider(Collider2D hitCollider)
        {
            return hitCollider.transform == transform || hitCollider.transform.IsChildOf(transform);
        }

        private int CompareHitsByDistanceAndBlockPriority(RaycastHit2D left, RaycastHit2D right)
        {
            if (Mathf.Abs(left.distance - right.distance) > 0.001f)
            {
                return left.distance.CompareTo(right.distance);
            }

            return GetHitPriority(left.collider).CompareTo(GetHitPriority(right.collider));
        }

        private int GetHitPriority(Collider2D hitCollider)
        {
            if (hitCollider == null || ShouldIgnoreCollider(hitCollider))
            {
                return 3;
            }

            if (blockSakWithSolidColliders
                && !hitCollider.isTrigger
                && hitCollider.GetComponentInParent<TayaCounteredStateController>() == null)
            {
                return 0;
            }

            if (hitCollider.GetComponentInParent<TayaCounteredStateController>() != null)
            {
                return 1;
            }

            return 2;
        }

        private static Vector2 ResolveHitPoint(RaycastHit2D hit, Vector2 origin, Vector2 direction)
        {
            if (hit.point != Vector2.zero)
            {
                return hit.point;
            }

            return origin + direction * Mathf.Max(0f, hit.distance);
        }

        private static Sprite CreateBurstSprite()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Safe SAK Burst Runtime Sprite",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[size * size];
            var center = new Vector2(size * 0.5f, size * 0.5f);
            for (var y = 0; y < size; y += 1)
            {
                for (var x = 0; x < size; x += 1)
                {
                    var offset = new Vector2(x + 0.5f, y + 0.5f) - center;
                    var distance = offset.magnitude;
                    var angle = Mathf.Atan2(offset.y, offset.x);
                    var burst = Mathf.Abs(Mathf.Sin(angle * 7f));
                    var outer = Mathf.Lerp(14f, 42f, burst);
                    var ring = Mathf.Clamp01(1f - Mathf.Abs(distance - 19f) / 7f);
                    var rays = distance < outer && distance > 8f ? Mathf.Clamp01(1f - distance / 44f) : 0f;
                    var alpha = Mathf.Max(ring * 0.8f, rays);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size);
        }
    }
}
