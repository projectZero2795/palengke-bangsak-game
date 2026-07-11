namespace Palengke.BangSak.Game
{
    public readonly struct PrototypeRoundNetworkSnapshot
    {
        public PrototypeRoundNetworkSnapshot(
            PrototypeRoundState state,
            PrototypeRoundResult result,
            string resultTitle,
            string resultMessage,
            int totalHiders,
            int remainingHiders,
            float remainingSeconds,
            int roundNumber)
        {
            State = state;
            Result = result;
            ResultTitle = resultTitle ?? string.Empty;
            ResultMessage = resultMessage ?? string.Empty;
            TotalHiders = totalHiders;
            RemainingHiders = remainingHiders;
            RemainingSeconds = remainingSeconds;
            RoundNumber = roundNumber;
        }

        public PrototypeRoundState State { get; }

        public PrototypeRoundResult Result { get; }

        public string ResultTitle { get; }

        public string ResultMessage { get; }

        public int TotalHiders { get; }

        public int RemainingHiders { get; }

        public float RemainingSeconds { get; }

        public int RoundNumber { get; }
    }
}
