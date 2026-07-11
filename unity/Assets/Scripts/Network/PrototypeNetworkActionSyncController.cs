using System.Collections.Generic;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Network
{
    [DisallowMultipleComponent]
    public sealed class PrototypeNetworkActionSyncController : MonoBehaviour
    {
        public const string ComponentId = "prototype_network_action_sync";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "phase26_bang_sak_event_sync";

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
        private BangActionController bangActionController;

        [SerializeField]
        private BangNameCallController bangNameCallController;

        [SerializeField]
        private SakCounterController sakCounterController;

        private readonly Dictionary<string, int> lastAppliedSequences = new Dictionary<string, int>();
        private int lastCapturedBangSequence;
        private int lastCapturedSakSequence;

        public string ComponentIdValue => componentId;

        public int ComponentVersionValue => componentVersion;

        public string ComponentVariantValue => componentVariant;

        public PrototypeNetworkPlayerIdentity Identity => identity;

        public bool IsLocalAuthority => identity != null && identity.IsLocalPlayer;

        public PrototypeNetworkActionEvent LastLocalActionEvent { get; private set; }

        public PrototypeNetworkActionEvent LastAppliedRemoteActionEvent { get; private set; }

        public void Configure(PrototypeNetworkPlayerIdentity networkIdentity)
        {
            identity = networkIdentity;
            ResolveReferences();
            lastCapturedBangSequence = 0;
            lastCapturedSakSequence = 0;
            lastAppliedSequences.Clear();
        }

        public bool TryCaptureLocalActionEvent(float now, out PrototypeNetworkActionEvent actionEvent)
        {
            ResolveReferences();
            actionEvent = default(PrototypeNetworkActionEvent);

            if (!IsLocalAuthority)
            {
                return false;
            }

            if (TryCaptureBangEvent(now, out actionEvent))
            {
                LastLocalActionEvent = actionEvent;
                return true;
            }

            if (TryCaptureSakEvent(now, out actionEvent))
            {
                LastLocalActionEvent = actionEvent;
                return true;
            }

            return false;
        }

        public bool ApplyRemoteActionEvent(PrototypeNetworkActionEvent actionEvent)
        {
            ResolveReferences();

            if (string.IsNullOrWhiteSpace(actionEvent.ActorNetworkPlayerId) || actionEvent.Sequence <= 0)
            {
                return false;
            }

            if (IsLocalAuthority && actionEvent.ActorNetworkPlayerId == identity.NetworkPlayerId)
            {
                return false;
            }

            var sequenceKey = BuildSequenceKey(actionEvent);
            if (lastAppliedSequences.TryGetValue(sequenceKey, out var lastSequence)
                && actionEvent.Sequence <= lastSequence)
            {
                return false;
            }

            bool applied;
            switch (actionEvent.Outcome)
            {
                case PrototypeNetworkActionOutcome.BangHitTarget:
                    applied = ApplyRemoteBangHit(actionEvent);
                    break;
                case PrototypeNetworkActionOutcome.BangNameMismatch:
                    applied = ApplyRemoteBangNameMismatch(actionEvent);
                    break;
                case PrototypeNetworkActionOutcome.SakCounteredTaya:
                    applied = ApplyRemoteSakCounter(actionEvent);
                    break;
                default:
                    applied = true;
                    break;
            }

            if (!applied)
            {
                return false;
            }

            lastAppliedSequences[sequenceKey] = actionEvent.Sequence;
            LastAppliedRemoteActionEvent = actionEvent;
            return true;
        }

        public bool TryResolveAuthoritativeAction(
            FusionActionPayload request,
            float now,
            out PrototypeNetworkActionEvent actionEvent)
        {
            ResolveReferences();
            actionEvent = default(PrototypeNetworkActionEvent);
            if (request == null
                || identity == null
                || request.actorNetworkPlayerId != identity.NetworkPlayerId
                || !System.Enum.IsDefined(typeof(PrototypeNetworkActionKind), request.kind)
                || !System.Enum.IsDefined(typeof(PlayerFacingDirection), request.facingDirection))
            {
                return false;
            }

            var facing = (PlayerFacingDirection)request.facingDirection;
            var kind = (PrototypeNetworkActionKind)request.kind;
            if (kind == PrototypeNetworkActionKind.BangNameCall && bangActionController != null)
            {
                if (bangNameCallController != null)
                {
                    bangNameCallController.SetSelectedTargetName(request.calledName);
                }

                bangActionController.SetFallbackFacingDirection(facing);
                var result = bangActionController.ResolveBangHit(transform.position, facing, request.sequence);
                var targetIdentity = result.Target != null
                    ? result.Target.GetComponentInParent<PrototypeNetworkPlayerIdentity>()
                    : null;
                var targetName = result.Target != null
                    ? result.Target.GetComponentInParent<PlayerNameIdentity>()
                    : null;
                actionEvent = new PrototypeNetworkActionEvent(
                    kind,
                    MapBangOutcome(result.Outcome),
                    identity.NetworkPlayerId,
                    targetIdentity != null ? targetIdentity.NetworkPlayerId : string.Empty,
                    request.calledName,
                    targetName != null ? targetName.DisplayName : string.Empty,
                    result.Origin,
                    result.Point,
                    result.Direction,
                    facing,
                    request.sequence,
                    now);
                return true;
            }

            if (kind == PrototypeNetworkActionKind.SakCounter && sakCounterController != null)
            {
                sakCounterController.SetFallbackFacingDirection(facing);
                var result = sakCounterController.ResolveSakHit(transform.position, facing, request.sequence);
                var targetIdentity = result.Target != null
                    ? result.Target.GetComponentInParent<PrototypeNetworkPlayerIdentity>()
                    : null;
                var targetName = result.Target != null
                    ? result.Target.GetComponentInParent<PlayerNameIdentity>()
                    : null;
                actionEvent = new PrototypeNetworkActionEvent(
                    kind,
                    MapSakOutcome(result.Outcome),
                    identity.NetworkPlayerId,
                    targetIdentity != null ? targetIdentity.NetworkPlayerId : string.Empty,
                    string.Empty,
                    targetName != null ? targetName.DisplayName : string.Empty,
                    result.Origin,
                    result.Point,
                    result.Direction,
                    facing,
                    request.sequence,
                    now);
                return true;
            }

            return false;
        }

        private bool TryCaptureBangEvent(float now, out PrototypeNetworkActionEvent actionEvent)
        {
            actionEvent = default(PrototypeNetworkActionEvent);
            if (bangActionController == null)
            {
                return false;
            }

            var result = bangActionController.LastHitResult;
            if (result.SequenceId <= 0 || result.SequenceId <= lastCapturedBangSequence)
            {
                return false;
            }

            lastCapturedBangSequence = result.SequenceId;
            actionEvent = BuildBangEvent(result, now);
            return true;
        }

        private bool TryCaptureSakEvent(float now, out PrototypeNetworkActionEvent actionEvent)
        {
            actionEvent = default(PrototypeNetworkActionEvent);
            if (sakCounterController == null)
            {
                return false;
            }

            var result = sakCounterController.LastResult;
            if (result.SequenceId <= 0 || result.SequenceId <= lastCapturedSakSequence)
            {
                return false;
            }

            lastCapturedSakSequence = result.SequenceId;
            actionEvent = BuildSakEvent(result, now);
            return true;
        }

        private PrototypeNetworkActionEvent BuildBangEvent(BangHitResult result, float now)
        {
            var targetIdentity = result.Target != null
                ? result.Target.GetComponentInParent<PrototypeNetworkPlayerIdentity>()
                : null;
            var targetName = result.Target != null
                ? result.Target.GetComponentInParent<PlayerNameIdentity>()
                : null;
            var calledName = bangNameCallController != null
                ? bangNameCallController.SelectedTargetName
                : string.Empty;

            return new PrototypeNetworkActionEvent(
                PrototypeNetworkActionKind.BangNameCall,
                MapBangOutcome(result.Outcome),
                ResolveActorId(),
                targetIdentity != null ? targetIdentity.NetworkPlayerId : string.Empty,
                calledName,
                targetName != null ? targetName.DisplayName : string.Empty,
                result.Origin,
                result.Point,
                result.Direction,
                bangActionController.LastBangDirection,
                result.SequenceId,
                now);
        }

        private PrototypeNetworkActionEvent BuildSakEvent(SakCounterResult result, float now)
        {
            var targetIdentity = result.Target != null
                ? result.Target.GetComponentInParent<PrototypeNetworkPlayerIdentity>()
                : null;
            var targetName = result.Target != null
                ? result.Target.GetComponentInParent<PlayerNameIdentity>()
                : null;

            return new PrototypeNetworkActionEvent(
                PrototypeNetworkActionKind.SakCounter,
                MapSakOutcome(result.Outcome),
                ResolveActorId(),
                targetIdentity != null ? targetIdentity.NetworkPlayerId : string.Empty,
                string.Empty,
                targetName != null ? targetName.DisplayName : string.Empty,
                result.Origin,
                result.Point,
                result.Direction,
                sakCounterController.CurrentFacingDirection,
                result.SequenceId,
                now);
        }

        private bool ApplyRemoteBangHit(PrototypeNetworkActionEvent actionEvent)
        {
            var target = FindPlayerByNetworkId(actionEvent.TargetNetworkPlayerId);
            var bangTarget = target != null ? target.GetComponent<BangHitTarget>() : null;
            if (bangTarget == null)
            {
                return false;
            }

            var targetCollider = target.GetComponentInChildren<Collider2D>();
            var result = BangHitResult.HitTarget(
                bangTarget,
                targetCollider,
                actionEvent.Origin,
                actionEvent.Point,
                actionEvent.Direction,
                Vector2.Distance(actionEvent.Origin, actionEvent.Point),
                actionEvent.Sequence);
            bangTarget.RegisterBangHit(null, actionEvent.Sequence, result);
            return true;
        }

        private bool ApplyRemoteBangNameMismatch(PrototypeNetworkActionEvent actionEvent)
        {
            var target = FindPlayerByNetworkId(actionEvent.TargetNetworkPlayerId);
            var bangTarget = target != null ? target.GetComponent<BangHitTarget>() : null;
            if (bangTarget == null)
            {
                return false;
            }

            var targetCollider = target.GetComponentInChildren<Collider2D>();
            var result = BangHitResult.NameMismatch(
                bangTarget,
                targetCollider,
                actionEvent.Origin,
                actionEvent.Point,
                actionEvent.Direction,
                Vector2.Distance(actionEvent.Origin, actionEvent.Point),
                actionEvent.Sequence,
                $"Wrong name: called {actionEvent.CalledName}, hit {actionEvent.TargetDisplayName}");
            bangTarget.RegisterBangNameMismatch(null, actionEvent.Sequence, result);
            return true;
        }

        private bool ApplyRemoteSakCounter(PrototypeNetworkActionEvent actionEvent)
        {
            var target = FindPlayerByNetworkId(actionEvent.TargetNetworkPlayerId);
            var counterTarget = target != null ? target.GetComponent<TayaCounteredStateController>() : null;
            if (counterTarget == null)
            {
                return false;
            }

            return counterTarget.MarkCountered(this, actionEvent.Sequence);
        }

        private string ResolveActorId()
        {
            return identity != null && !string.IsNullOrWhiteSpace(identity.NetworkPlayerId)
                ? identity.NetworkPlayerId
                : "unassigned";
        }

        private void ResolveReferences()
        {
            if (identity == null)
            {
                identity = GetComponent<PrototypeNetworkPlayerIdentity>();
            }

            if (bangActionController == null)
            {
                bangActionController = GetComponent<BangActionController>();
            }

            if (bangNameCallController == null)
            {
                bangNameCallController = GetComponent<BangNameCallController>();
            }

            if (sakCounterController == null)
            {
                sakCounterController = GetComponent<SakCounterController>();
            }
        }

        private static string BuildSequenceKey(PrototypeNetworkActionEvent actionEvent)
        {
            return $"{actionEvent.ActorNetworkPlayerId}:{actionEvent.Kind}";
        }

        private static PrototypeNetworkActionOutcome MapBangOutcome(BangHitOutcome outcome)
        {
            switch (outcome)
            {
                case BangHitOutcome.HitTarget:
                    return PrototypeNetworkActionOutcome.BangHitTarget;
                case BangHitOutcome.Blocked:
                    return PrototypeNetworkActionOutcome.BangBlocked;
                case BangHitOutcome.NameMismatch:
                    return PrototypeNetworkActionOutcome.BangNameMismatch;
                default:
                    return PrototypeNetworkActionOutcome.Miss;
            }
        }

        private static PrototypeNetworkActionOutcome MapSakOutcome(SakCounterOutcome outcome)
        {
            switch (outcome)
            {
                case SakCounterOutcome.CounteredTaya:
                    return PrototypeNetworkActionOutcome.SakCounteredTaya;
                case SakCounterOutcome.Blocked:
                    return PrototypeNetworkActionOutcome.SakBlocked;
                case SakCounterOutcome.WrongRole:
                    return PrototypeNetworkActionOutcome.SakWrongRole;
                default:
                    return PrototypeNetworkActionOutcome.Miss;
            }
        }

        private static GameObject FindPlayerByNetworkId(string networkPlayerId)
        {
            if (string.IsNullOrWhiteSpace(networkPlayerId))
            {
                return null;
            }

            var identities = FindObjectsOfType<PrototypeNetworkPlayerIdentity>();
            for (var index = 0; index < identities.Length; index += 1)
            {
                var current = identities[index];
                if (current != null && current.NetworkPlayerId == networkPlayerId)
                {
                    return current.gameObject;
                }
            }

            return null;
        }
    }
}
