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
        private BangActionVisualStyle visualStyle = BangActionVisualStyle.CartoonLightBeam;

        [SerializeField]
        private Sprite bangMarkerSprite = null;

        [SerializeField]
        private Sprite rangeIndicatorSprite = null;

        [Header("Timing")]
        [SerializeField]
        [Min(0.1f)]
        private float cooldownSeconds = 1.25f;

        [SerializeField]
        [Min(0.05f)]
        private float effectDurationSeconds = 0.35f;

        [Header("Range")]
        [SerializeField]
        [Min(0.25f)]
        private float range = 2.25f;

        [SerializeField]
        [Min(0.05f)]
        private float markerDistance = 0.95f;

        [SerializeField]
        private bool showRangeIndicator = true;

        private SpriteRenderer bangMarkerRenderer;
        private SpriteRenderer rangeIndicatorRenderer;
        private float lastBangTime = -999f;
        private float effectVisibleUntil = -999f;
        private PlayerFacingDirection fallbackFacingDirection = PlayerFacingDirection.Down;
        private PlayerFacingDirection lastBangDirection = PlayerFacingDirection.Down;

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

            effectVisibleUntil = now + effectDurationSeconds;
            bangMarkerRenderer.transform.position = GetEffectPosition(transform.position, direction);
            bangMarkerRenderer.transform.rotation = Quaternion.Euler(0f, 0f, GetEffectRotationZ(direction));
            bangMarkerRenderer.enabled = true;
        }

        private void UpdateEffectVisibility(float now)
        {
            if (bangMarkerRenderer != null && bangMarkerRenderer.enabled && now > effectVisibleUntil)
            {
                bangMarkerRenderer.enabled = false;
            }
        }

        private void UpdateRangeIndicator()
        {
            if (rangeIndicatorRenderer == null)
            {
                return;
            }

            rangeIndicatorRenderer.enabled = showRangeIndicator;
            rangeIndicatorRenderer.transform.localScale = Vector3.one * (range * 2f);
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
