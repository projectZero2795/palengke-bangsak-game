using System;
using Palengke.BangSak.Game;
using UnityEngine;

namespace Palengke.BangSak.Api
{
    [DisallowMultipleComponent]
    public sealed class PalengkeRoundScoreSubmitter : MonoBehaviour
    {
        private PrototypeRoundRulesController roundRules;
        private PalengkeApiClient apiClient;
        private int observedRoundNumber;
        private int submittedRoundNumber;
        private string roundId = string.Empty;

        public static void EnsureAttached(GameObject roundRulesObject)
        {
            if (roundRulesObject != null && roundRulesObject.GetComponent<PalengkeRoundScoreSubmitter>() == null)
            {
                roundRulesObject.AddComponent<PalengkeRoundScoreSubmitter>();
            }
        }

        private void Start()
        {
            roundRules = GetComponent<PrototypeRoundRulesController>();
            apiClient = FindObjectOfType<PalengkeApiClient>();
            if (apiClient == null)
            {
                var clientObject = new GameObject("Phase 28 Palengke API Client");
                apiClient = clientObject.AddComponent<PalengkeApiClient>();
            }
        }

        private void Update()
        {
            if (roundRules == null || apiClient == null)
            {
                return;
            }

            if (roundRules.RoundNumber != observedRoundNumber)
            {
                observedRoundNumber = roundRules.RoundNumber;
                roundId = "bangsak_" + Guid.NewGuid().ToString("N");
            }

            if (!roundRules.IsFinished
                || submittedRoundNumber == roundRules.RoundNumber
                || !apiClient.HasAuthenticatedSession)
            {
                return;
            }

            submittedRoundNumber = roundRules.RoundNumber;
            var score = CalculateScore(
                roundRules.Result,
                roundRules.RemainingSeconds,
                roundRules.TotalHiders,
                roundRules.RemainingHiders);
            apiClient.SubmitScore(roundId, score);
        }

        public static int CalculateScore(
            PrototypeRoundResult result,
            float remainingSeconds,
            int totalHiders,
            int remainingHiders)
        {
            var timeBonus = Mathf.Max(0, Mathf.FloorToInt(remainingSeconds)) * 5;
            var caughtHiders = Mathf.Max(0, totalHiders - remainingHiders);
            var objectiveScore = caughtHiders * 200;
            var winBonus = result == PrototypeRoundResult.None ? 0 : 500;
            return Mathf.Clamp(winBonus + objectiveScore + timeBonus, 0, 100000);
        }
    }
}
