namespace Palengke.BangSak.Game
{
    public readonly struct SakAttemptResult
    {
        private SakAttemptResult(SakAttemptOutcome outcome, SakBaseController baseController, SakBaseActor actor, float attemptedAt)
        {
            Outcome = outcome;
            BaseController = baseController;
            Actor = actor;
            AttemptedAt = attemptedAt;
        }

        public SakAttemptOutcome Outcome { get; }

        public SakBaseController BaseController { get; }

        public SakBaseActor Actor { get; }

        public float AttemptedAt { get; }

        public bool Succeeded => Outcome == SakAttemptOutcome.Success;

        public static SakAttemptResult Success(SakBaseController baseController, SakBaseActor actor, float attemptedAt) =>
            new SakAttemptResult(SakAttemptOutcome.Success, baseController, actor, attemptedAt);

        public static SakAttemptResult NoBase(SakBaseActor actor, float attemptedAt) =>
            new SakAttemptResult(SakAttemptOutcome.NoBase, null, actor, attemptedAt);

        public static SakAttemptResult BaseInactive(SakBaseController baseController, SakBaseActor actor, float attemptedAt) =>
            new SakAttemptResult(SakAttemptOutcome.BaseInactive, baseController, actor, attemptedAt);

        public static SakAttemptResult ActorNotEligible(SakBaseController baseController, SakBaseActor actor, float attemptedAt) =>
            new SakAttemptResult(SakAttemptOutcome.ActorNotEligible, baseController, actor, attemptedAt);
    }
}
