using System;

namespace Palengke.BangSak.Api
{
    [Serializable]
    public sealed class PalengkeUser
    {
        public string userId;
        public string displayName;
        public int coins;

        public PalengkeUser(string userId, string displayName, int coins)
        {
            this.userId = userId;
            this.displayName = displayName;
            this.coins = coins;
        }
    }

    [Serializable]
    public sealed class PalengkeLeaderboardEntry
    {
        public int rank;
        public string displayName;
        public int score;

        public PalengkeLeaderboardEntry(int rank, string displayName, int score)
        {
            this.rank = rank;
            this.displayName = displayName;
            this.score = score;
        }
    }
}
