using System;
using UnityEngine;

namespace Palengke.BangSak.Api
{
    [DisallowMultipleComponent]
    public sealed class PalengkeApiClient : MonoBehaviour
    {
        public const string ComponentId = "palengke_api_client";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "phase27_mock_api";
        public const string DefaultApiBaseUrl = "https://api.palengke.es";

        [Header("Component Contract")]
        [SerializeField] private string componentId = ComponentId;
        [SerializeField] private int componentVersion = ComponentVersion;
        [SerializeField] private string componentVariant = ComponentVariant;

        [Header("API Configuration")]
        [SerializeField] private string apiBaseUrl = DefaultApiBaseUrl;
        [SerializeField] private bool useMockData = true;

        private readonly PalengkeUser mockUser = new PalengkeUser("mock-juanp", "JuanP", 125);
        private readonly PalengkeLeaderboardEntry[] mockLeaderboard =
        {
            new PalengkeLeaderboardEntry(1, "Maria", 2450),
            new PalengkeLeaderboardEntry(2, "Jose", 2180),
            new PalengkeLeaderboardEntry(3, "JuanP", 1960),
            new PalengkeLeaderboardEntry(4, "Lina", 1720),
            new PalengkeLeaderboardEntry(5, "Tomas", 1490)
        };

        public string ComponentIdValue => componentId;
        public int ComponentVersionValue => componentVersion;
        public string ComponentVariantValue => componentVariant;
        public string ApiBaseUrl => apiBaseUrl;
        public bool UseMockData => useMockData;
        public bool IsProductionApiEnabled => false;
        public string StatusMessage => "Offline mock data — no Palengke API request is made.";

        public void Configure(string baseUrl, bool mockData = true)
        {
            apiBaseUrl = NormalizeBaseUrl(baseUrl);
            useMockData = mockData;
        }

        public PalengkeUser GetCurrentUser()
        {
            EnsureMockMode();
            return new PalengkeUser(mockUser.userId, mockUser.displayName, mockUser.coins);
        }

        public PalengkeLeaderboardEntry[] GetLeaderboard()
        {
            EnsureMockMode();
            var copy = new PalengkeLeaderboardEntry[mockLeaderboard.Length];
            for (var index = 0; index < mockLeaderboard.Length; index += 1)
            {
                var entry = mockLeaderboard[index];
                copy[index] = new PalengkeLeaderboardEntry(entry.rank, entry.displayName, entry.score);
            }

            return copy;
        }

        public static string NormalizeBaseUrl(string baseUrl)
        {
            return string.IsNullOrWhiteSpace(baseUrl)
                ? DefaultApiBaseUrl
                : baseUrl.Trim().TrimEnd('/');
        }

        private void EnsureMockMode()
        {
            if (!useMockData)
            {
                throw new InvalidOperationException(
                    "The production Palengke API adapter is not available in Phase 27. Enable mock data.");
            }
        }
    }
}
