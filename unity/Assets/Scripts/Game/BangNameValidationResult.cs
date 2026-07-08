namespace Palengke.BangSak.Game
{
    public enum BangNameValidationOutcome
    {
        Valid = 0,
        MissingCalledName = 1,
        MissingTargetName = 2,
        WrongName = 3,
    }

    public readonly struct BangNameValidationResult
    {
        public BangNameValidationResult(
            BangNameValidationOutcome outcome,
            string calledName,
            string actualName,
            string message)
        {
            Outcome = outcome;
            CalledName = calledName ?? string.Empty;
            ActualName = actualName ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public BangNameValidationOutcome Outcome { get; }

        public string CalledName { get; }

        public string ActualName { get; }

        public string Message { get; }

        public bool IsValid => Outcome == BangNameValidationOutcome.Valid;

        public static BangNameValidationResult Valid(string calledName, string actualName)
        {
            return new BangNameValidationResult(
                BangNameValidationOutcome.Valid,
                calledName,
                actualName,
                $"Correct Bang: {actualName}");
        }

        public static BangNameValidationResult MissingCalledName()
        {
            return new BangNameValidationResult(
                BangNameValidationOutcome.MissingCalledName,
                string.Empty,
                string.Empty,
                "Choose who you are calling before Bang.");
        }

        public static BangNameValidationResult MissingTargetName(string calledName)
        {
            return new BangNameValidationResult(
                BangNameValidationOutcome.MissingTargetName,
                calledName,
                string.Empty,
                "That target has no player name yet.");
        }

        public static BangNameValidationResult WrongName(string calledName, string actualName)
        {
            return new BangNameValidationResult(
                BangNameValidationOutcome.WrongName,
                calledName,
                actualName,
                $"Wrong name: called {calledName}, hit {actualName}");
        }
    }
}
