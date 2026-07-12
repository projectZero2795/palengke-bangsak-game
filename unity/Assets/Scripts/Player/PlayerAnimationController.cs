using UnityEngine;
using Palengke.BangSak.UI;

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

        [Header("Directional Sprites")]
        [SerializeField]
        private Sprite[] directionalIdleSprites = new Sprite[8];

        [SerializeField]
        private Sprite[] directionalWalkSprites = new Sprite[32];

        [SerializeField]
        [Min(1)]
        private int walkFramesPerDirection = 4;

        [Header("Timing")]
        [SerializeField]
        [Min(1f)]
        private float framesPerSecond = 8f;

        [SerializeField]
        [Min(0f)]
        private float walkingInputThreshold = 0.01f;

        [SerializeField]
        private bool flipHorizontally = false;

        private float frameTimer;
        private int walkFrameIndex;
        private PlayerFacingDirection facingDirection = PlayerFacingDirection.Down;

        public bool IsWalking { get; private set; }

        public PlayerFacingDirection FacingDirection => facingDirection;

        public int WalkFrameIndex => walkFrameIndex;

        public int DirectionCount => 8;

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
                if (AccessibilitySettings.ReducedMotionEnabled)
                {
                    frameTimer = 0f;
                    walkFrameIndex = 0;
                    ApplyReducedMotionWalkFrame();
                }
                else
                {
                    StepWalkAnimation(deltaTime);
                }
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

            var degrees = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg;

            if (degrees >= -22.5f && degrees < 22.5f)
            {
                return PlayerFacingDirection.Right;
            }

            if (degrees >= 22.5f && degrees < 67.5f)
            {
                return PlayerFacingDirection.UpRight;
            }

            if (degrees >= 67.5f && degrees < 112.5f)
            {
                return PlayerFacingDirection.Up;
            }

            if (degrees >= 112.5f && degrees < 157.5f)
            {
                return PlayerFacingDirection.UpLeft;
            }

            if (degrees >= -67.5f && degrees < -22.5f)
            {
                return PlayerFacingDirection.DownRight;
            }

            if (degrees >= -112.5f && degrees < -67.5f)
            {
                return PlayerFacingDirection.Down;
            }

            if (degrees >= -157.5f && degrees < -112.5f)
            {
                return PlayerFacingDirection.DownLeft;
            }

            return PlayerFacingDirection.Left;
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

        public int ResolveDirectionalWalkSpriteIndex(PlayerFacingDirection direction, int frameIndex)
        {
            var safeDirection = Mathf.Clamp((int)direction, 0, DirectionCount - 1);
            var safeFrameCount = Mathf.Max(1, walkFramesPerDirection);
            var safeFrame = Mathf.Abs(frameIndex) % safeFrameCount;

            return safeDirection * safeFrameCount + safeFrame;
        }

        public int GetWalkFrameCount(PlayerFacingDirection direction)
        {
            return HasDirectionalWalkSprites(direction)
                ? Mathf.Max(1, walkFramesPerDirection)
                : Mathf.Max(0, walkSprites?.Length ?? 0);
        }

        public bool IsMoving(Vector2 movementInput, float threshold)
        {
            return movementInput.sqrMagnitude > threshold * threshold;
        }

        private void StepWalkAnimation(float deltaTime)
        {
            var frameCount = GetWalkFrameCount(facingDirection);
            if (frameCount <= 0)
            {
                ApplyIdleFrame();
                return;
            }

            frameTimer += Mathf.Max(0f, deltaTime);
            var secondsPerFrame = 1f / framesPerSecond;

            while (frameTimer >= secondsPerFrame)
            {
                frameTimer -= secondsPerFrame;
                walkFrameIndex = (walkFrameIndex + 1) % frameCount;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = GetWalkSprite(facingDirection, walkFrameIndex);
            }
        }

        private void ApplyReducedMotionWalkFrame()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = GetWalkFrameCount(facingDirection) > 0
                ? GetWalkSprite(facingDirection, 0)
                : GetIdleSprite(facingDirection);
        }

        private void ApplyIdleFrame()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = GetIdleSprite(facingDirection);
            }
        }

        private void ApplyFacingDirection(PlayerFacingDirection direction)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (HasDirectionalIdleSprite(direction) || HasDirectionalWalkSprites(direction))
            {
                spriteRenderer.flipX = false;
                return;
            }

            if (!flipHorizontally)
            {
                return;
            }

            spriteRenderer.flipX = direction == PlayerFacingDirection.Left;
        }

        private Sprite GetIdleSprite(PlayerFacingDirection direction)
        {
            var directionIndex = (int)direction;
            if (
                directionalIdleSprites != null
                && directionIndex >= 0
                && directionIndex < directionalIdleSprites.Length
                && directionalIdleSprites[directionIndex] != null
            )
            {
                return directionalIdleSprites[directionIndex];
            }

            return idleSprite;
        }

        private Sprite GetWalkSprite(PlayerFacingDirection direction, int frameIndex)
        {
            if (HasDirectionalWalkSprites(direction))
            {
                var spriteIndex = ResolveDirectionalWalkSpriteIndex(direction, frameIndex);
                return directionalWalkSprites[spriteIndex];
            }

            if (walkSprites == null || walkSprites.Length == 0)
            {
                return GetIdleSprite(direction);
            }

            return walkSprites[Mathf.Abs(frameIndex) % walkSprites.Length];
        }

        private bool HasDirectionalIdleSprite(PlayerFacingDirection direction)
        {
            var directionIndex = (int)direction;
            return directionalIdleSprites != null
                && directionIndex >= 0
                && directionIndex < directionalIdleSprites.Length
                && directionalIdleSprites[directionIndex] != null;
        }

        private bool HasDirectionalWalkSprites(PlayerFacingDirection direction)
        {
            if (directionalWalkSprites == null || walkFramesPerDirection <= 0)
            {
                return false;
            }

            var start = ResolveDirectionalWalkSpriteIndex(direction, 0);
            if (start < 0 || start + walkFramesPerDirection > directionalWalkSprites.Length)
            {
                return false;
            }

            for (var frame = 0; frame < walkFramesPerDirection; frame++)
            {
                if (directionalWalkSprites[start + frame] == null)
                {
                    return false;
                }
            }

            return true;
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
