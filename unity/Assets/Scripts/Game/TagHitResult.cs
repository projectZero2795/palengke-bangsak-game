using UnityEngine;

namespace Palengke.BangSak.Game
{
    public readonly struct TagHitResult
    {
        public TagHitResult(
            TagHitOutcome outcome,
            TagHitTarget target,
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

        public TagHitOutcome Outcome { get; }

        public TagHitTarget Target { get; }

        public Collider2D Collider { get; }

        public Vector2 Origin { get; }

        public Vector2 Point { get; }

        public Vector2 Direction { get; }

        public float Distance { get; }

        public int SequenceId { get; }

        public bool DidHitTarget => Outcome == TagHitOutcome.HitTarget && Target != null;

        public bool WasBlocked => Outcome == TagHitOutcome.Blocked;

        public bool DidMiss => Outcome == TagHitOutcome.Miss;

        public static TagHitResult Miss(Vector2 origin, Vector2 point, Vector2 direction, float distance, int sequenceId)
        {
            return new TagHitResult(TagHitOutcome.Miss, null, null, origin, point, direction, distance, sequenceId);
        }

        public static TagHitResult HitTarget(
            TagHitTarget target,
            Collider2D collider,
            Vector2 origin,
            Vector2 point,
            Vector2 direction,
            float distance,
            int sequenceId)
        {
            return new TagHitResult(TagHitOutcome.HitTarget, target, collider, origin, point, direction, distance, sequenceId);
        }

        public static TagHitResult Blocked(
            Collider2D collider,
            Vector2 origin,
            Vector2 point,
            Vector2 direction,
            float distance,
            int sequenceId)
        {
            return new TagHitResult(TagHitOutcome.Blocked, null, collider, origin, point, direction, distance, sequenceId);
        }
    }
}
