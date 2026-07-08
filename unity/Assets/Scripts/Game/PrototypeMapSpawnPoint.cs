using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Game
{
    public struct PrototypeMapSpawnPoint
    {
        public PrototypeMapSpawnPoint(MapSpawnRole role, int slotIndex, Vector2 position, PlayerFacingDirection facingDirection)
        {
            Role = role;
            SlotIndex = slotIndex;
            Position = position;
            FacingDirection = facingDirection;
        }

        public MapSpawnRole Role { get; }

        public int SlotIndex { get; }

        public Vector2 Position { get; }

        public PlayerFacingDirection FacingDirection { get; }
    }
}
