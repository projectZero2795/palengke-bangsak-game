using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Palengke.BangSak.Api
{
    [DisallowMultipleComponent]
    public sealed class PalengkeApiClient : MonoBehaviour
    {
        public const string ComponentId = "palengke_api_client";
        public const int ComponentVersion = 2;
        public const string ComponentVariant = "phase28_real_palengke_api";
        public const string DefaultApiBaseUrl = "https://palengke.es/api/backend";
        public const string SessionPath = "/games/bang-sak/session";
        public const string LeaderboardPath = "/games/bang-sak/leaderboard";
        public const string ScoresPath = "/games/bang-sak/scores";

        [Header("Component Contract")]
        [SerializeField] private string componentId = ComponentId;
        [SerializeField] private int componentVersion = ComponentVersion;
        [SerializeField] private string componentVariant = ComponentVariant;

        [Header("API Configuration")]
        [SerializeField] private string apiBaseUrl = DefaultApiBaseUrl;
        [SerializeField] private bool useMockData;
        [SerializeField, Min(2)] private int timeoutSeconds = 12;

        private readonly PalengkeUser mockUser = new PalengkeUser("mock-juanp", "JuanP", 125);
        private readonly PalengkeLeaderboardEntry[] mockLeaderboard =
        {
            new PalengkeLeaderboardEntry(1, "Maria", 2450),
            new PalengkeLeaderboardEntry(2, "Jose", 2180),
            new PalengkeLeaderboardEntry(3, "JuanP", 1960),
            new PalengkeLeaderboardEntry(4, "Lina", 1720),
            new PalengkeLeaderboardEntry(5, "Tomas", 1490)
        };

        private PalengkeUser currentUser = new PalengkeUser("guest", "Guest Player", 0, true);
        private PalengkeLeaderboardEntry[] leaderboard = new PalengkeLeaderboardEntry[0];
        private string accessToken = string.Empty;

        public string ComponentIdValue => componentId;
        public int ComponentVersionValue => componentVersion;
        public string ComponentVariantValue => componentVariant;
        public string ApiBaseUrl => apiBaseUrl;
        public bool UseMockData => useMockData;
        public bool IsProductionApiEnabled => !useMockData;
        public bool HasAccessToken => !string.IsNullOrWhiteSpace(accessToken);
        public bool HasAuthenticatedSession => !currentUser.guest && HasAccessToken;
        public string StatusMessage { get; private set; } = "Guest mode — sign in to Palengke to save scores and earn coins.";

        private void Start()
        {
            if (useMockData)
            {
                Configure(apiBaseUrl, true);
                return;
            }

            if (!HasAccessToken)
            {
                accessToken = PalengkeWebGlAuthBridge.TryReadAccessToken();
            }
            if (HasAccessToken)
            {
                BeginSessionRefresh();
            }
            BeginLeaderboardRefresh();
        }

        public void Configure(string baseUrl, bool mockData = false)
        {
            apiBaseUrl = NormalizeBaseUrl(baseUrl);
            useMockData = mockData;
            if (useMockData)
            {
                currentUser = CopyUser(mockUser);
                leaderboard = CopyLeaderboard(mockLeaderboard);
                StatusMessage = "Offline mock data — no Palengke API request is made.";
            }
        }

        public void SetAccessToken(string token)
        {
            accessToken = string.IsNullOrWhiteSpace(token) ? string.Empty : token.Trim();
            if (Application.isPlaying && !useMockData && HasAccessToken)
            {
                BeginSessionRefresh();
            }
        }

        public PalengkeUser GetCurrentUser()
        {
            return useMockData ? CopyUser(mockUser) : CopyUser(currentUser);
        }

        public PalengkeLeaderboardEntry[] GetLeaderboard()
        {
            return useMockData ? CopyLeaderboard(mockLeaderboard) : CopyLeaderboard(leaderboard);
        }

        public void BeginSessionRefresh(Action<bool> completed = null)
        {
            if (useMockData)
            {
                Configure(apiBaseUrl, true);
                completed?.Invoke(true);
                return;
            }
            if (!HasAccessToken)
            {
                currentUser = new PalengkeUser("guest", "Guest Player", 0, true);
                StatusMessage = "Guest mode — sign in to Palengke to save scores and earn coins.";
                completed?.Invoke(false);
                return;
            }
            StartCoroutine(RefreshSession(completed));
        }

        public void BeginLeaderboardRefresh(Action<bool> completed = null)
        {
            if (useMockData)
            {
                leaderboard = CopyLeaderboard(mockLeaderboard);
                completed?.Invoke(true);
                return;
            }
            StartCoroutine(RefreshLeaderboard(completed));
        }

        public void SubmitScore(string roundId, int score, Action<PalengkeScoreSubmission> completed = null)
        {
            if (useMockData || !HasAuthenticatedSession)
            {
                StatusMessage = "Guest scores are local only. Sign in to save scores and earn coins.";
                completed?.Invoke(null);
                return;
            }
            StartCoroutine(SubmitScoreRequest(roundId, score, completed));
        }

        public static string NormalizeBaseUrl(string baseUrl)
        {
            return string.IsNullOrWhiteSpace(baseUrl)
                ? DefaultApiBaseUrl
                : baseUrl.Trim().TrimEnd('/');
        }

        public string BuildUrl(string path)
        {
            return NormalizeBaseUrl(apiBaseUrl) + (path.StartsWith("/") ? path : "/" + path);
        }

        private IEnumerator RefreshSession(Action<bool> completed)
        {
            using (var request = UnityWebRequest.Get(BuildUrl(SessionPath)))
            {
                ConfigureRequest(request, true);
                yield return request.SendWebRequest();
                if (!RequestSucceeded(request))
                {
                    currentUser = new PalengkeUser("guest", "Guest Player", 0, true);
                    StatusMessage = request.responseCode == 401
                        ? "Palengke session expired. Continue as guest or sign in again."
                        : "Palengke is unavailable. Continuing safely in guest mode.";
                    completed?.Invoke(false);
                    yield break;
                }

                var response = JsonUtility.FromJson<PalengkeSessionApiResponse>(request.downloadHandler.text);
                if (response == null || string.IsNullOrWhiteSpace(response.user_id))
                {
                    StatusMessage = "Palengke returned an invalid session response.";
                    completed?.Invoke(false);
                    yield break;
                }
                currentUser = new PalengkeUser(response.user_id, response.display_name, response.coins, response.guest);
                StatusMessage = $"Connected to Palengke as {currentUser.displayName}.";
                completed?.Invoke(true);
            }
        }

        private IEnumerator RefreshLeaderboard(Action<bool> completed)
        {
            using (var request = UnityWebRequest.Get(BuildUrl(LeaderboardPath)))
            {
                ConfigureRequest(request, false);
                yield return request.SendWebRequest();
                if (!RequestSucceeded(request))
                {
                    StatusMessage = "Leaderboard unavailable. Local gameplay is still available.";
                    completed?.Invoke(false);
                    yield break;
                }

                var response = JsonUtility.FromJson<PalengkeLeaderboardApiResponse>(request.downloadHandler.text);
                var apiEntries = response != null && response.entries != null
                    ? response.entries
                    : new PalengkeLeaderboardApiEntry[0];
                leaderboard = new PalengkeLeaderboardEntry[apiEntries.Length];
                for (var index = 0; index < apiEntries.Length; index += 1)
                {
                    var entry = apiEntries[index];
                    leaderboard[index] = new PalengkeLeaderboardEntry(
                        entry.rank, entry.display_name, entry.best_score,
                        entry.user_id, entry.games_played, entry.coins);
                }
                completed?.Invoke(true);
            }
        }

        private IEnumerator SubmitScoreRequest(string roundId, int score, Action<PalengkeScoreSubmission> completed)
        {
            var payload = JsonUtility.ToJson(new PalengkeScoreApiRequest { round_id = roundId, score = score });
            using (var request = new UnityWebRequest(BuildUrl(ScoresPath), "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                ConfigureRequest(request, true);
                yield return request.SendWebRequest();
                if (!RequestSucceeded(request))
                {
                    StatusMessage = request.responseCode == 429
                        ? "Score submission is cooling down. Please wait before retrying."
                        : "Score could not be saved; local gameplay is unaffected.";
                    completed?.Invoke(null);
                    yield break;
                }

                var response = JsonUtility.FromJson<PalengkeScoreApiResponse>(request.downloadHandler.text);
                if (response == null || string.IsNullOrWhiteSpace(response.submission_id))
                {
                    StatusMessage = "Palengke returned an invalid score response.";
                    completed?.Invoke(null);
                    yield break;
                }
                currentUser.coins = response.coins_balance;
                StatusMessage = response.duplicate
                    ? "Round score was already saved."
                    : $"Score saved — earned {response.coins_awarded} coins.";
                completed?.Invoke(new PalengkeScoreSubmission
                {
                    submissionId = response.submission_id,
                    roundId = response.round_id,
                    score = response.score,
                    coinsAwarded = response.coins_awarded,
                    coinsBalance = response.coins_balance,
                    duplicate = response.duplicate
                });
            }
        }

        private void ConfigureRequest(UnityWebRequest request, bool requiresAuth)
        {
            request.timeout = timeoutSeconds;
            if (requiresAuth && HasAccessToken)
            {
                request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            }
        }

        private static bool RequestSucceeded(UnityWebRequest request)
        {
            return request.result == UnityWebRequest.Result.Success;
        }

        private static PalengkeUser CopyUser(PalengkeUser source)
        {
            return new PalengkeUser(source.userId, source.displayName, source.coins, source.guest);
        }

        private static PalengkeLeaderboardEntry[] CopyLeaderboard(PalengkeLeaderboardEntry[] source)
        {
            var copy = new PalengkeLeaderboardEntry[source.Length];
            for (var index = 0; index < source.Length; index += 1)
            {
                var entry = source[index];
                copy[index] = new PalengkeLeaderboardEntry(
                    entry.rank, entry.displayName, entry.score,
                    entry.userId, entry.gamesPlayed, entry.coins);
            }
            return copy;
        }
    }
}
