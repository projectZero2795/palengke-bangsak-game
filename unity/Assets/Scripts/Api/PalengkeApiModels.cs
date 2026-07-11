using System;

namespace Palengke.BangSak.Api
{
    [Serializable]
    public sealed class PalengkeUser
    {
        public string userId;
        public string displayName;
        public long coins;
        public bool guest;

        public PalengkeUser(string userId, string displayName, long coins, bool guest = false)
        {
            this.userId = userId;
            this.displayName = displayName;
            this.coins = coins;
            this.guest = guest;
        }
    }

    [Serializable]
    public sealed class PalengkeLeaderboardEntry
    {
        public int rank;
        public string userId;
        public string displayName;
        public int score;
        public int gamesPlayed;
        public long coins;

        public PalengkeLeaderboardEntry(
            int rank,
            string displayName,
            int score,
            string userId = "",
            int gamesPlayed = 0,
            long coins = 0)
        {
            this.rank = rank;
            this.userId = userId;
            this.displayName = displayName;
            this.score = score;
            this.gamesPlayed = gamesPlayed;
            this.coins = coins;
        }
    }

    [Serializable]
    public sealed class PalengkeScoreSubmission
    {
        public string submissionId;
        public string roundId;
        public int score;
        public int coinsAwarded;
        public long coinsBalance;
        public bool duplicate;
    }

    [Serializable]
    internal sealed class PalengkeSessionApiResponse
    {
        public string user_id;
        public string display_name;
        public long coins;
        public bool guest;
    }

    [Serializable]
    internal sealed class PalengkeLeaderboardApiEntry
    {
        public int rank;
        public string user_id;
        public string display_name;
        public int best_score;
        public int games_played;
        public long coins;
    }

    [Serializable]
    internal sealed class PalengkeLeaderboardApiResponse
    {
        public PalengkeLeaderboardApiEntry[] entries;
    }

    [Serializable]
    internal sealed class PalengkeScoreApiRequest
    {
        public string round_id;
        public int score;
    }

    [Serializable]
    internal sealed class PalengkeScoreApiResponse
    {
        public string submission_id;
        public string round_id;
        public int score;
        public int coins_awarded;
        public long coins_balance;
        public bool duplicate;
    }
}
