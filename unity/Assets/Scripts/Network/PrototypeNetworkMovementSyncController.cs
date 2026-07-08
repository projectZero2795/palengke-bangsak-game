using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Network
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PrototypeNetworkMovementSyncController : MonoBehaviour
    {
        public const string ComponentId = "prototype_network_movement_sync";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "phase25_snapshot_smoothing";

        [Header("Component Contract")]
        [SerializeField]
        private string componentId = ComponentId;

        [SerializeField]
        private int componentVersion = ComponentVersion;

        [SerializeField]
        private string componentVariant = ComponentVariant;

        [Header("References")]
        [SerializeField]
        private PrototypeNetworkPlayerIdentity identity;

        [SerializeField]
        private PlayerMovementController movementController;

        [SerializeField]
        private PlayerAnimationController animationController;

        [SerializeField]
        private Rigidbody2D body;

        [Header("Authority")]
        [SerializeField]
        private PrototypeNetworkMovementAuthority authority = PrototypeNetworkMovementAuthority.RemoteReplica;

        [Header("Snapshot Timing")]
        [SerializeField]
        [Min(0.01f)]
        private float snapshotSendIntervalSeconds = 0.05f;

        [SerializeField]
        [Min(1f)]
        private float remoteInterpolationSpeed = 12f;

        [SerializeField]
        [Min(0.1f)]
        private float snapDistance = 2f;

        private int nextLocalSequence;
        private float lastLocalCaptureAt = -999f;
        private Vector3 remoteTargetPosition;

        public string ComponentIdValue => componentId;

        public int ComponentVersionValue => componentVersion;

        public string ComponentVariantValue => componentVariant;

        public PrototypeNetworkMovementAuthority Authority => authority;

        public PrototypeNetworkMovementSnapshot LastLocalSnapshot { get; private set; }

        public PrototypeNetworkMovementSnapshot LastAppliedRemoteSnapshot { get; private set; }

        public bool HasRemoteTarget { get; private set; }

        public float SnapshotSendIntervalSeconds
        {
            get => snapshotSendIntervalSeconds;
            set => snapshotSendIntervalSeconds = Mathf.Max(0.01f, value);
        }

        public float RemoteInterpolationSpeed
        {
            get => remoteInterpolationSpeed;
            set => remoteInterpolationSpeed = Mathf.Max(1f, value);
        }

        public float SnapDistance
        {
            get => snapDistance;
            set => snapDistance = Mathf.Max(0.1f, value);
        }

        private void Awake()
        {
            ResolveReferences();
            ConfigureBodyForAuthority();
        }

        private void Update()
        {
            if (authority == PrototypeNetworkMovementAuthority.LocalAuthority)
            {
                TryCaptureLocalSnapshot(Time.time);
                return;
            }

            SmoothRemoteReplica(Time.deltaTime);
        }

        public void Configure(PrototypeNetworkPlayerIdentity networkIdentity)
        {
            identity = networkIdentity;
            ResolveReferences();
            SetAuthority(identity != null && identity.IsLocalPlayer
                ? PrototypeNetworkMovementAuthority.LocalAuthority
                : PrototypeNetworkMovementAuthority.RemoteReplica);
        }

        public void SetAuthority(PrototypeNetworkMovementAuthority newAuthority)
        {
            authority = newAuthority;
            ResolveReferences();
            ConfigureMovementForAuthority();
            ConfigureBodyForAuthority();
        }

        public PrototypeNetworkMovementSnapshot CaptureSnapshot(float now)
        {
            ResolveReferences();

            var movementInput = movementController != null ? movementController.CurrentInput : Vector2.zero;
            var facingDirection = animationController != null
                ? animationController.FacingDirection
                : PlayerFacingDirection.Down;
            var networkPlayerId = identity != null && !string.IsNullOrWhiteSpace(identity.NetworkPlayerId)
                ? identity.NetworkPlayerId
                : "unassigned";

            nextLocalSequence += 1;
            LastLocalSnapshot = new PrototypeNetworkMovementSnapshot(
                networkPlayerId,
                transform.position,
                movementInput,
                facingDirection,
                nextLocalSequence,
                now);

            return LastLocalSnapshot;
        }

        public bool ApplyRemoteSnapshot(PrototypeNetworkMovementSnapshot snapshot)
        {
            if (authority == PrototypeNetworkMovementAuthority.LocalAuthority)
            {
                return false;
            }

            if (HasRemoteTarget && !snapshot.IsNewerThan(LastAppliedRemoteSnapshot))
            {
                return false;
            }

            LastAppliedRemoteSnapshot = snapshot;
            remoteTargetPosition = new Vector3(snapshot.Position.x, snapshot.Position.y, transform.position.z);
            HasRemoteTarget = true;

            if (Vector2.Distance(transform.position, snapshot.Position) > snapDistance)
            {
                ApplyRemotePosition(remoteTargetPosition);
            }

            if (animationController != null)
            {
                var animationInput = snapshot.MovementInput.sqrMagnitude > 0.001f
                    ? snapshot.MovementInput
                    : ToMovementVector(snapshot.FacingDirection);
                animationController.ApplyAnimation(animationInput, Time.deltaTime);
            }

            return true;
        }

        public Vector3 ResolveSmoothedPosition(Vector3 currentPosition, Vector3 targetPosition, float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return currentPosition;
            }

            var blend = 1f - Mathf.Exp(-remoteInterpolationSpeed * deltaTime);
            return Vector3.Lerp(currentPosition, targetPosition, blend);
        }

        private void TryCaptureLocalSnapshot(float now)
        {
            if (now - lastLocalCaptureAt < snapshotSendIntervalSeconds)
            {
                return;
            }

            CaptureSnapshot(now);
            lastLocalCaptureAt = now;
        }

        private void SmoothRemoteReplica(float deltaTime)
        {
            if (!HasRemoteTarget)
            {
                return;
            }

            var nextPosition = ResolveSmoothedPosition(transform.position, remoteTargetPosition, deltaTime);
            ApplyRemotePosition(nextPosition);
        }

        private void ApplyRemotePosition(Vector3 position)
        {
            transform.position = position;

            if (body != null)
            {
                body.position = new Vector2(position.x, position.y);
            }
        }

        private void ConfigureMovementForAuthority()
        {
            if (movementController == null)
            {
                return;
            }

            var isLocal = authority == PrototypeNetworkMovementAuthority.LocalAuthority;
            movementController.enabled = isLocal;
            movementController.SetKeyboardInputEnabled(isLocal);

            if (!isLocal)
            {
                movementController.SetExternalInput(Vector2.zero);
            }
        }

        private void ConfigureBodyForAuthority()
        {
            if (body == null)
            {
                return;
            }

            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.bodyType = authority == PrototypeNetworkMovementAuthority.LocalAuthority
                ? RigidbodyType2D.Dynamic
                : RigidbodyType2D.Kinematic;
        }

        private void ResolveReferences()
        {
            if (identity == null)
            {
                identity = GetComponent<PrototypeNetworkPlayerIdentity>();
            }

            if (movementController == null)
            {
                movementController = GetComponent<PlayerMovementController>();
            }

            if (animationController == null)
            {
                animationController = GetComponent<PlayerAnimationController>();
            }

            if (body == null)
            {
                body = GetComponent<Rigidbody2D>();
            }
        }

        private static Vector2 ToMovementVector(PlayerFacingDirection facingDirection)
        {
            switch (facingDirection)
            {
                case PlayerFacingDirection.Up:
                    return Vector2.up;
                case PlayerFacingDirection.UpRight:
                    return new Vector2(1f, 1f).normalized;
                case PlayerFacingDirection.Right:
                    return Vector2.right;
                case PlayerFacingDirection.DownRight:
                    return new Vector2(1f, -1f).normalized;
                case PlayerFacingDirection.Down:
                    return Vector2.down;
                case PlayerFacingDirection.DownLeft:
                    return new Vector2(-1f, -1f).normalized;
                case PlayerFacingDirection.Left:
                    return Vector2.left;
                case PlayerFacingDirection.UpLeft:
                    return new Vector2(-1f, 1f).normalized;
                default:
                    return Vector2.down;
            }
        }
    }
}
