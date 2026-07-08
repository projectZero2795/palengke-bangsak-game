using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Network
{
    public readonly struct PrototypeNetworkMovementSnapshot
    {
        public PrototypeNetworkMovementSnapshot(
            string networkPlayerId,
            Vector2 position,
            Vector2 movementInput,
            PlayerFacingDirection facingDirection,
            int sequence,
            float sentAt)
        {
            NetworkPlayerId = networkPlayerId;
            Position = position;
            MovementInput = movementInput;
            FacingDirection = facingDirection;
            Sequence = sequence;
            SentAt = sentAt;
        }

        public string NetworkPlayerId { get; }

        public Vector2 Position { get; }

        public Vector2 MovementInput { get; }

        public PlayerFacingDirection FacingDirection { get; }

        public int Sequence { get; }

        public float SentAt { get; }

        public bool IsNewerThan(PrototypeNetworkMovementSnapshot other)
        {
            if (Sequence != other.Sequence)
            {
                return Sequence > other.Sequence;
            }

            return SentAt > other.SentAt;
        }
    }
}
