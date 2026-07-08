using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Network
{
    [DisallowMultipleComponent]
    public sealed class PrototypeNetworkPlayerIdentity : MonoBehaviour
    {
        public const string ComponentId = "prototype_network_player_identity";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "phase24_spawn_owner_marker";

        [Header("Component Contract")]
        [SerializeField]
        private string componentId = ComponentId;

        [SerializeField]
        private int componentVersion = ComponentVersion;

        [SerializeField]
        private string componentVariant = ComponentVariant;

        [Header("Network Preview")]
        [SerializeField]
        private string networkPlayerId = "local-0";

        [SerializeField]
        private string displayName = "Player";

        [SerializeField]
        private PlayerRole role = PlayerRole.Hider;

        [SerializeField]
        private int spawnSlotIndex;

        [SerializeField]
        private bool isLocalPlayer;

        [SerializeField]
        private PlayerFacingDirection facingDirection = PlayerFacingDirection.Down;

        public string ComponentIdValue => componentId;

        public int ComponentVersionValue => componentVersion;

        public string ComponentVariantValue => componentVariant;

        public string NetworkPlayerId => networkPlayerId;

        public string DisplayName => displayName;

        public PlayerRole Role => role;

        public int SpawnSlotIndex => spawnSlotIndex;

        public bool IsLocalPlayer => isLocalPlayer;

        public PlayerFacingDirection FacingDirection => facingDirection;

        public void Configure(PrototypeNetworkPlayerDescriptor descriptor)
        {
            networkPlayerId = string.IsNullOrWhiteSpace(descriptor.NetworkPlayerId)
                ? $"player-{Mathf.Max(0, descriptor.SpawnSlotIndex):00}"
                : descriptor.NetworkPlayerId.Trim();
            displayName = string.IsNullOrWhiteSpace(descriptor.DisplayName)
                ? networkPlayerId
                : descriptor.DisplayName.Trim();
            role = descriptor.Role;
            spawnSlotIndex = Mathf.Max(0, descriptor.SpawnSlotIndex);
            isLocalPlayer = descriptor.IsLocalPlayer;
            facingDirection = descriptor.FacingDirection;
        }
    }
}
