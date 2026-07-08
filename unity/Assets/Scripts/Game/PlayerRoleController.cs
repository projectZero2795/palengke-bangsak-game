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
        private CaughtStateController caughtStateController;

        [Header("Role Marker")]
        [SerializeField]
        private bool showRoleBadge = true;

        [SerializeField]
        private Vector3 roleBadgeOffset = new Vector3(0f, 0.82f, 0f);

        [SerializeField]
        [Min(0.01f)]
        private float roleBadgeCharacterSize = 0.11f;

        [SerializeField]
        private int roleBadgeSortingOrder = 70;

        [SerializeField]
        private string tayaBadgeText = "TAYA";

        [SerializeField]
        private string hiderBadgeText = "HIDER";

        [SerializeField]
        private Color tayaBadgeColor = new Color(1f, 0.27f, 0.23f, 1f);

        [SerializeField]
        private Color hiderBadgeColor = new Color(0.24f, 0.85f, 0.36f, 1f);

        private TextMesh roleBadge;
        private MeshRenderer roleBadgeRenderer;

        public PlayerRole Role => role;

        public bool IsTaya => role == PlayerRole.Taya;

        public bool IsHider => role == PlayerRole.Hider;

        public bool CanUseBang => IsTaya;

        public string RoleLabel => IsTaya ? tayaBadgeText : hiderBadgeText;

        public bool RoleBadgeVisible => showRoleBadge;

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
            ApplyCaughtCounting();
            EnsureRoleBadge();
            RefreshRoleBadge();
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

        private void ApplyCaughtCounting()
        {
            if (caughtStateController != null)
            {
                caughtStateController.SetCountAsHider(IsHider);
            }
        }

        private void EnsureRoleBadge()
        {
            if (roleBadge != null || !showRoleBadge)
            {
                return;
            }

            var badgeObject = new GameObject("Role Badge");
            badgeObject.transform.SetParent(transform, false);
            badgeObject.transform.localPosition = roleBadgeOffset;

            roleBadge = badgeObject.AddComponent<TextMesh>();
            roleBadge.anchor = TextAnchor.MiddleCenter;
            roleBadge.alignment = TextAlignment.Center;
            roleBadge.characterSize = roleBadgeCharacterSize;
            roleBadge.fontSize = 32;
            roleBadge.fontStyle = FontStyle.Bold;

            roleBadgeRenderer = badgeObject.GetComponent<MeshRenderer>();
            if (roleBadgeRenderer != null)
            {
                roleBadgeRenderer.sortingOrder = roleBadgeSortingOrder;
            }
        }

        private void RefreshRoleBadge()
        {
            if (roleBadge == null)
            {
                return;
            }

            roleBadge.gameObject.SetActive(showRoleBadge);
            roleBadge.transform.localPosition = roleBadgeOffset;
            roleBadge.characterSize = roleBadgeCharacterSize;
            roleBadge.text = RoleLabel;
            roleBadge.color = IsTaya ? tayaBadgeColor : hiderBadgeColor;

            if (roleBadgeRenderer != null)
            {
                roleBadgeRenderer.sortingOrder = roleBadgeSortingOrder;
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

            if (caughtStateController == null)
            {
                caughtStateController = GetComponent<CaughtStateController>();
            }
        }
    }
}
