using UnityEngine;

namespace Palengke.BangSak.Game
{
    public readonly struct SakCounterResult
    {
        public SakCounterResult(
            SakCounterOutcome outcome,
            TayaCounteredStateController target,
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

        public SakCounterOutcome Outcome { get; }

        public TayaCounteredStateController Target { get; }

        public Collider2D Collider { get; }

        public Vector2 Origin { get; }

        public Vector2 Point { get; }

        public Vector2 Direction { get; }

        public float Distance { get; }

        public int SequenceId { get; }

        public bool DidCounterTaya => Outcome == SakCounterOutcome.CounteredTaya && Target != null;

        public bool WasBlocked => Outcome == SakCounterOutcome.Blocked;

        public bool DidMiss => Outcome == SakCounterOutcome.Miss;

        public static SakCounterResult Miss(
            Vector2 origin,
            Vector2 point,
            Vector2 direction,
            float distance,
            int sequenceId)
        {
            return new SakCounterResult(SakCounterOutcome.Miss, null, null, origin, point, direction, distance, sequenceId);
        }

        public static SakCounterResult Blocked(
            Collider2D collider,
            Vector2 origin,
            Vector2 point,
            Vector2 direction,
            float distance,
            int sequenceId)
        {
            return new SakCounterResult(SakCounterOutcome.Blocked, null, collider, origin, point, direction, distance, sequenceId);
        }

        public static SakCounterResult WrongRole(
            TayaCounteredStateController target,
            Collider2D collider,
            Vector2 origin,
            Vector2 point,
            Vector2 direction,
            float distance,
            int sequenceId)
        {
            return new SakCounterResult(SakCounterOutcome.WrongRole, target, collider, origin, point, direction, distance, sequenceId);
        }

        public static SakCounterResult CounteredTaya(
            TayaCounteredStateController target,
            Collider2D collider,
            Vector2 origin,
            Vector2 point,
            Vector2 direction,
            float distance,
            int sequenceId)
        {
            return new SakCounterResult(
                SakCounterOutcome.CounteredTaya,
                target,
                collider,
                origin,
                point,
                direction,
                distance,
                sequenceId);
        }
    }
}
