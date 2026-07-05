using UnityEngine;

namespace Palengke.BangSak.Player
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(PlayerMovementController))]
    public sealed class PlayerAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private PlayerMovementController movementController;

        [Header("Sprites")]
        [SerializeField]
        private Sprite idleSprite = null;

        [SerializeField]
        private Sprite[] walkSprites = new Sprite[0];

        [Header("Timing")]
        [SerializeField]
        [Min(1f)]
        private float framesPerSecond = 8f;

        [SerializeField]
        [Min(0f)]
        private float walkingInputThreshold = 0.01f;

        [SerializeField]
        private bool flipHorizontally = true;

        private float frameTimer;
        private int walkFrameIndex;
        private PlayerFacingDirection facingDirection = PlayerFacingDirection.Down;

        public bool IsWalking { get; private set; }

        public PlayerFacingDirection FacingDirection => facingDirection;

        public int WalkFrameIndex => walkFrameIndex;

        public float FramesPerSecond
        {
            get => framesPerSecond;
            set => framesPerSecond = Mathf.Max(1f, value);
        }

        private void Awake()
        {
            ResolveReferences();
            ApplyIdleFrame();
        }

        private void LateUpdate()
        {
            var input = movementController != null ? movementController.CurrentInput : Vector2.zero;
            ApplyAnimation(input, Time.deltaTime);
        }

        public void ApplyAnimation(Vector2 movementInput, float deltaTime)
        {
            ResolveReferences();
            IsWalking = IsMoving(movementInput, walkingInputThreshold);

            if (IsWalking)
            {
                facingDirection = ResolveFacingDirection(movementInput, facingDirection);
                StepWalkAnimation(deltaTime);
            }
            else
            {
                walkFrameIndex = 0;
                frameTimer = 0f;
                ApplyIdleFrame();
            }

            ApplyFacingDirection(facingDirection);
        }

        public PlayerFacingDirection ResolveFacingDirection(
            Vector2 movementInput,
            PlayerFacingDirection fallbackDirection)
        {
            if (!IsMoving(movementInput, walkingInputThreshold))
            {
                return fallbackDirection;
            }

            if (Mathf.Abs(movementInput.x) >= Mathf.Abs(movementInput.y))
            {
                return movementInput.x < 0f ? PlayerFacingDirection.Left : PlayerFacingDirection.Right;
            }

            return movementInput.y < 0f ? PlayerFacingDirection.Down : PlayerFacingDirection.Up;
        }

        public int ResolveNextWalkFrameIndex(int currentIndex, int frameCount, float elapsedSeconds)
        {
            if (frameCount <= 0)
            {
                return 0;
            }

            if (elapsedSeconds <= 0f)
            {
                return Mathf.Abs(currentIndex) % frameCount;
            }

            var frameStep = Mathf.Max(1, Mathf.FloorToInt(elapsedSeconds * framesPerSecond));
            return (currentIndex + frameStep) % frameCount;
        }

        public bool IsMoving(Vector2 movementInput, float threshold)
        {
            return movementInput.sqrMagnitude > threshold * threshold;
        }

        private void StepWalkAnimation(float deltaTime)
        {
            if (walkSprites == null || walkSprites.Length == 0)
            {
                ApplyIdleFrame();
                return;
            }

            frameTimer += Mathf.Max(0f, deltaTime);
            var secondsPerFrame = 1f / framesPerSecond;

            while (frameTimer >= secondsPerFrame)
            {
                frameTimer -= secondsPerFrame;
                walkFrameIndex = (walkFrameIndex + 1) % walkSprites.Length;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = walkSprites[walkFrameIndex];
            }
        }

        private void ApplyIdleFrame()
        {
            if (spriteRenderer != null && idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
        }

        private void ApplyFacingDirection(PlayerFacingDirection direction)
        {
            if (spriteRenderer == null || !flipHorizontally)
            {
                return;
            }

            spriteRenderer.flipX = direction == PlayerFacingDirection.Left;
        }

        private void ResolveReferences()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (movementController == null)
            {
                movementController = GetComponent<PlayerMovementController>();
            }
        }
    }
}
