using UnityEngine;

namespace Palengke.BangSak.Game
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class SakBaseController : MonoBehaviour
    {
        public const string ComponentId = "sak_base_controller";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "green_flag_base_placeholder";

        [Header("Component Contract")]
        [SerializeField]
        private string componentId = ComponentId;

        [SerializeField]
        private int componentVersion = ComponentVersion;

        [SerializeField]
        private string componentVariant = ComponentVariant;

        [Header("Base Rules")]
        [SerializeField]
        private bool isBaseActive = true;

        [SerializeField]
        [Min(0.2f)]
        private float triggerRadius = 1.15f;

        [Header("Visual Feedback")]
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private Color idleColor = Color.white;

        [SerializeField]
        private Color successPulseColor = new Color(0.65f, 1f, 0.48f, 1f);

        [SerializeField]
        [Min(0.05f)]
        private float successPulseSeconds = 0.35f;

        private CircleCollider2D triggerCollider;
        private int successfulSakCount;
        private SakBaseActor lastSuccessfulActor;
        private float lastSuccessfulSakTime = -999f;

        public string ComponentIdValue => componentId;

        public int ComponentVersionValue => componentVersion;

        public string ComponentVariantValue => componentVariant;

        public bool IsBaseActive => isBaseActive;

        public float TriggerRadius => triggerRadius;

        public int SuccessfulSakCount => successfulSakCount;

        public SakBaseActor LastSuccessfulActor => lastSuccessfulActor;

        public float LastSuccessfulSakTime => lastSuccessfulSakTime;

        private void Awake()
        {
            ResolveReferences();
            ConfigureTrigger();
        }

        private void Update()
        {
            UpdateVisualFeedback(Time.time);
        }

        public void SetBaseActive(bool value)
        {
            isBaseActive = value;
        }

        public void SetTriggerRadius(float radius)
        {
            triggerRadius = Mathf.Max(0.2f, radius);
            ConfigureTrigger();
        }

        public SakAttemptResult TryPressSak(SakBaseActor actor, float now)
        {
            if (!isBaseActive)
            {
                return SakAttemptResult.BaseInactive(this, actor, now);
            }

            if (actor == null || !actor.CanUseSak || actor.CurrentBase != this)
            {
                return SakAttemptResult.ActorNotEligible(this, actor, now);
            }

            successfulSakCount += 1;
            lastSuccessfulActor = actor;
            lastSuccessfulSakTime = now;
            UpdateVisualFeedback(now);
            return SakAttemptResult.Success(this, actor, now);
        }

        public void RegisterActor(SakBaseActor actor)
        {
            if (actor != null)
            {
                actor.RegisterBase(this);
            }
        }

        public void UnregisterActor(SakBaseActor actor)
        {
            if (actor != null)
            {
                actor.UnregisterBase(this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            RegisterActor(other.GetComponentInParent<SakBaseActor>());
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            UnregisterActor(other.GetComponentInParent<SakBaseActor>());
        }

        private void ResolveReferences()
        {
            if (triggerCollider == null)
            {
                triggerCollider = GetComponent<CircleCollider2D>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void ConfigureTrigger()
        {
            ResolveReferences();
            if (triggerCollider == null)
            {
                return;
            }

            triggerCollider.isTrigger = true;
            triggerCollider.radius = triggerRadius;
            triggerCollider.offset = Vector2.zero;
        }

        private void UpdateVisualFeedback(float now)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            var pulseRemaining = successPulseSeconds - (now - lastSuccessfulSakTime);
            if (pulseRemaining > 0f)
            {
                var t = Mathf.Clamp01(pulseRemaining / successPulseSeconds);
                spriteRenderer.color = Color.Lerp(idleColor, successPulseColor, t);
            }
            else
            {
                spriteRenderer.color = idleColor;
            }
        }
    }
}
