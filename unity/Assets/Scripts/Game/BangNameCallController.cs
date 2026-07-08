using System.Collections.Generic;
using UnityEngine;

namespace Palengke.BangSak.Game
{
    [DisallowMultipleComponent]
    public sealed class BangNameCallController : MonoBehaviour
    {
        [Header("Selection")]
        [SerializeField]
        private string selectedTargetName = string.Empty;

        [SerializeField]
        private bool autoSelectFirstHider = true;

        [SerializeField]
        private bool skipCaughtHiders = true;

        private readonly List<PlayerNameIdentity> selectableTargets = new List<PlayerNameIdentity>();
        private int selectedIndex = -1;

        public string SelectedTargetName => PlayerNameIdentity.NormalizeName(selectedTargetName);

        public string LastFeedbackMessage { get; private set; } = "Choose a hider name.";

        public BangNameValidationOutcome LastValidationOutcome { get; private set; } =
            BangNameValidationOutcome.MissingCalledName;

        public int SelectableTargetCount
        {
            get
            {
                RefreshTargets();
                return selectableTargets.Count;
            }
        }

        private void Awake()
        {
            RefreshTargets();
        }

        private void OnEnable()
        {
            RefreshTargets();
        }

        public void SetSelectedTargetName(string targetName)
        {
            selectedTargetName = PlayerNameIdentity.NormalizeName(targetName);
            RefreshTargets();
        }

        public BangNameValidationResult ValidateBangTarget(BangHitTarget hitTarget)
        {
            RefreshTargets();

            var calledName = SelectedTargetName;
            if (string.IsNullOrEmpty(calledName))
            {
                return RememberValidation(BangNameValidationResult.MissingCalledName());
            }

            var targetIdentity = hitTarget != null ? hitTarget.GetComponentInParent<PlayerNameIdentity>() : null;
            if (targetIdentity == null)
            {
                return RememberValidation(BangNameValidationResult.MissingTargetName(calledName));
            }

            var actualName = targetIdentity.DisplayName;
            return RememberValidation(targetIdentity.MatchesCalledName(calledName)
                ? BangNameValidationResult.Valid(calledName, actualName)
                : BangNameValidationResult.WrongName(calledName, actualName));
        }

        public IReadOnlyList<PlayerNameIdentity> GetSelectableTargets()
        {
            RefreshTargets();
            return selectableTargets;
        }

        private void RefreshTargets()
        {
            selectableTargets.Clear();

            var identities = FindObjectsOfType<PlayerNameIdentity>();
            for (var index = 0; index < identities.Length; index += 1)
            {
                var identity = identities[index];
                if (identity == null || identity.gameObject == gameObject)
                {
                    continue;
                }

                var role = identity.GetComponent<PlayerRoleController>();
                if (role != null && !role.IsHider)
                {
                    continue;
                }

                var caughtState = identity.GetComponent<CaughtStateController>();
                if (skipCaughtHiders && caughtState != null && caughtState.IsCaught)
                {
                    continue;
                }

                selectableTargets.Add(identity);
            }

            ReconcileSelection();
        }

        private void ReconcileSelection()
        {
            selectedIndex = -1;
            var normalizedSelectedName = SelectedTargetName;

            for (var index = 0; index < selectableTargets.Count; index += 1)
            {
                if (selectableTargets[index].MatchesCalledName(normalizedSelectedName))
                {
                    selectedIndex = index;
                    selectedTargetName = selectableTargets[index].DisplayName;
                    return;
                }
            }

            if (autoSelectFirstHider && selectableTargets.Count > 0)
            {
                selectedIndex = 0;
                selectedTargetName = selectableTargets[0].DisplayName;
                return;
            }

            if (selectableTargets.Count == 0)
            {
                selectedTargetName = string.Empty;
            }
        }

        private BangNameValidationResult RememberValidation(BangNameValidationResult result)
        {
            LastValidationOutcome = result.Outcome;
            LastFeedbackMessage = result.Message;
            return result;
        }
    }
}
