using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Network
{
    public readonly struct PrototypeNetworkPlayerDescriptor
    {
        public PrototypeNetworkPlayerDescriptor(
            string networkPlayerId,
            string displayName,
            PlayerRole role,
            int spawnSlotIndex,
            bool isLocalPlayer,
            Vector2 spawnPosition,
            PlayerFacingDirection facingDirection)
        {
            NetworkPlayerId = networkPlayerId;
            DisplayName = displayName;
            Role = role;
            SpawnSlotIndex = spawnSlotIndex;
            IsLocalPlayer = isLocalPlayer;
            SpawnPosition = spawnPosition;
            FacingDirection = facingDirection;
        }

        public string NetworkPlayerId { get; }

        public string DisplayName { get; }

        public PlayerRole Role { get; }

        public int SpawnSlotIndex { get; }

        public bool IsLocalPlayer { get; }

        public Vector2 SpawnPosition { get; }

        public PlayerFacingDirection FacingDirection { get; }
    }
}
