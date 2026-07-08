using Palengke.BangSak.UI;
using UnityEngine;

namespace Palengke.BangSak.Game
{
    [DisallowMultipleComponent]
    public sealed class PlayerRoleController : MonoBehaviour
    {
        [Header("Role")]
        [SerializeField]
        private PlayerRole role = PlayerRole.Hider;

        [Header("References")]
        [SerializeField]
        private BangActionController bangActionController;

        [SerializeField]
        private BangActionHud bangActionHud;

        [SerializeField]
        private SakCounterController sakCounterController;

        [SerializeField]
        private SakCounterHud sakCounterHud;

        [SerializeField]
        private CaughtStateController caughtStateController;

        public PlayerRole Role => role;

        public bool IsTaya => role == PlayerRole.Taya;

        public bool IsHider => role == PlayerRole.Hider;

        public bool CanUseBang => IsTaya;

        public bool CanUseSak => IsHider;

        private void Awake()
        {
            ApplyRoleNow();
        }

        private void OnEnable()
        {
            ApplyRoleNow();
        }

        public void SetRole(PlayerRole newRole)
        {
            role = newRole;
            ApplyRoleNow();
        }

        public void ApplyRoleNow()
        {
            ResolveReferences();
            ApplyBangAvailability();
            ApplySakAvailability();
            ApplyCaughtCounting();
        }

        private void ApplyBangAvailability()
        {
            if (bangActionController != null)
            {
                bangActionController.enabled = CanUseBang;
            }

            if (bangActionHud != null)
            {
                bangActionHud.SetHudVisible(CanUseBang);
            }
        }

        private void ApplySakAvailability()
        {
            if (sakCounterController != null)
            {
                sakCounterController.enabled = CanUseSak;
            }

            if (sakCounterHud != null)
            {
                sakCounterHud.SetHudVisible(CanUseSak);
            }
        }

        private void ApplyCaughtCounting()
        {
            if (caughtStateController != null)
            {
                caughtStateController.SetCountAsHider(IsHider);
            }
        }

        private void ResolveReferences()
        {
            if (bangActionController == null)
            {
                bangActionController = GetComponent<BangActionController>();
            }

            if (bangActionHud == null)
            {
                bangActionHud = GetComponent<BangActionHud>();
            }

            if (sakCounterController == null)
            {
                sakCounterController = GetComponent<SakCounterController>();
            }

            if (sakCounterHud == null)
            {
                sakCounterHud = GetComponent<SakCounterHud>();
            }

            if (caughtStateController == null)
            {
                caughtStateController = GetComponent<CaughtStateController>();
            }
        }
    }
}
