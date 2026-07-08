using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Network
{
    public readonly struct PrototypeNetworkActionEvent
    {
        public PrototypeNetworkActionEvent(
            PrototypeNetworkActionKind kind,
            PrototypeNetworkActionOutcome outcome,
            string actorNetworkPlayerId,
            string targetNetworkPlayerId,
            string calledName,
            string targetDisplayName,
            Vector2 origin,
            Vector2 point,
            Vector2 direction,
            PlayerFacingDirection facingDirection,
            int sequence,
            float sentAt)
        {
            Kind = kind;
            Outcome = outcome;
            ActorNetworkPlayerId = actorNetworkPlayerId ?? string.Empty;
            TargetNetworkPlayerId = targetNetworkPlayerId ?? string.Empty;
            CalledName = calledName ?? string.Empty;
            TargetDisplayName = targetDisplayName ?? string.Empty;
            Origin = origin;
            Point = point;
            Direction = direction;
            FacingDirection = facingDirection;
            Sequence = sequence;
            SentAt = sentAt;
        }

        public PrototypeNetworkActionKind Kind { get; }

        public PrototypeNetworkActionOutcome Outcome { get; }

        public string ActorNetworkPlayerId { get; }

        public string TargetNetworkPlayerId { get; }

        public string CalledName { get; }

        public string TargetDisplayName { get; }

        public Vector2 Origin { get; }

        public Vector2 Point { get; }

        public Vector2 Direction { get; }

        public PlayerFacingDirection FacingDirection { get; }

        public int Sequence { get; }

        public float SentAt { get; }

        public bool HasTarget => !string.IsNullOrWhiteSpace(TargetNetworkPlayerId);

        public bool IsNewerThan(PrototypeNetworkActionEvent other)
        {
            if (Sequence != other.Sequence)
            {
                return Sequence > other.Sequence;
            }

            return SentAt > other.SentAt;
        }
    }
}
