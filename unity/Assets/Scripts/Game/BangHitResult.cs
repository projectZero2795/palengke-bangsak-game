using UnityEngine;

namespace Palengke.BangSak.Game
{
    public readonly struct BangHitResult
    {
        public BangHitResult(
            BangHitOutcome outcome,
            BangHitTarget target,
            Collider2D collider,
            Vector2 origin,
            Vector2 point,
            Vector2 direction,
            float distance,
            int sequenceId)
        {
            Outcome = outcome;
            Target = target;
            Collider = collider;
            Origin = origin;
            Point = point;
            Direction = direction;
            Distance = distance;
            SequenceId = sequenceId;
        }

        public BangHitOutcome Outcome { get; }

        public BangHitTarget Target { get; }

        public Collider2D Collider { get; }

        public Vector2 Origin { get; }

        public Vector2 Point { get; }

        public Vector2 Direction { get; }

        public float Distance { get; }

        public int SequenceId { get; }

        public bool DidHitTarget => Outcome == BangHitOutcome.HitTarget && Target != null;

        public bool WasBlocked => Outcome == BangHitOutcome.Blocked;

        public bool DidMiss => Outcome == BangHitOutcome.Miss;

        public static BangHitResult Miss(Vector2 origin, Vector2 point, Vector2 direction, float distance, int sequenceId)
        {
            return new BangHitResult(BangHitOutcome.Miss, null, null, origin, point, direction, distance, sequenceId);
        }

        public static BangHitResult HitTarget(
            BangHitTarget target,
            Collider2D collider,
            Vector2 origin,
            Vector2 point,
            Vector2 direction,
            float distance,
            int sequenceId)
        {
            return new BangHitResult(BangHitOutcome.HitTarget, target, collider, origin, point, direction, distance, sequenceId);
        }

        public static BangHitResult Blocked(
            Collider2D collider,
            Vector2 origin,
            Vector2 point,
            Vector2 direction,
            float distance,
            int sequenceId)
        {
            return new BangHitResult(BangHitOutcome.Blocked, null, collider, origin, point, direction, distance, sequenceId);
        }
    }
}
