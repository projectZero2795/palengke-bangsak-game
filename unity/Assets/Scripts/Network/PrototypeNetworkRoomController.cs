using System;
using System.Globalization;
using UnityEngine;

namespace Palengke.BangSak.Network
{
    [DisallowMultipleComponent]
    public sealed class PrototypeNetworkRoomController : MonoBehaviour
    {
        public const string ComponentId = "prototype_network_room";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "phase23_photon_room_lifecycle_scaffold";
        public const string DefaultProviderName = "Photon Fusion 2";

        [Header("Component Contract")]
        [SerializeField]
        private string componentId = ComponentId;

        [SerializeField]
        private int componentVersion = ComponentVersion;

        [SerializeField]
        private string componentVariant = ComponentVariant;

        [Header("Provider")]
        [SerializeField]
        private string providerName = DefaultProviderName;

        [Header("Offline Preview")]
        [SerializeField]
        private string defaultJoinRoomCode = "1234";

        [SerializeField]
        private int nextRoomNumber = 1234;

        public string ComponentIdValue => componentId;

        public int ComponentVersionValue => componentVersion;

        public string ComponentVariantValue => componentVariant;

        public string ProviderName => providerName;

        public string DefaultJoinRoomCode => defaultJoinRoomCode;

        public PrototypeNetworkRoomState State { get; private set; } = PrototypeNetworkRoomState.Disconnected;

        public string ActiveRoomCode { get; private set; } = string.Empty;

        public string StatusMessage { get; private set; } = "Ready for offline room preview. Import Photon Fusion to enable real rooms.";

        public bool HasActiveRoom => !string.IsNullOrWhiteSpace(ActiveRoomCode);

        public bool IsFusionSdkAvailable => IsTypeAvailable("Fusion.NetworkRunner, Fusion.Runtime")
            || IsTypeAvailable("Fusion.NetworkRunner, Fusion");

        public bool CreateRoom()
        {
            ActiveRoomCode = GenerateRoomCode();
            State = PrototypeNetworkRoomState.OfflinePreviewCreated;
            StatusMessage = IsFusionSdkAvailable
                ? "Fusion SDK detected. Real Photon adapter is intentionally not wired in this scaffold."
                : "Offline preview room created. Import Photon Fusion before testing two browser clients.";
            return true;
        }

        public bool JoinDefaultRoom()
        {
            return JoinRoom(defaultJoinRoomCode);
        }

        public bool JoinRoom(string roomCode)
        {
            var normalizedRoomCode = NormalizeRoomCode(roomCode);
            if (!IsValidRoomCode(normalizedRoomCode))
            {
                StatusMessage = "Room code must be 3-12 letters or numbers.";
                return false;
            }

            ActiveRoomCode = normalizedRoomCode;
            State = PrototypeNetworkRoomState.OfflinePreviewJoined;
            StatusMessage = IsFusionSdkAvailable
                ? "Fusion SDK detected. Real Photon join will be wired in the adapter phase."
                : "Offline preview room joined. No network connection is made yet.";
            return true;
        }

        public void LeaveRoom()
        {
            ActiveRoomCode = string.Empty;
            State = PrototypeNetworkRoomState.Disconnected;
            StatusMessage = "Left room preview.";
        }

        public static string NormalizeRoomCode(string roomCode)
        {
            return string.IsNullOrWhiteSpace(roomCode)
                ? string.Empty
                : roomCode.Trim().ToUpperInvariant();
        }

        public static bool IsValidRoomCode(string roomCode)
        {
            var normalizedRoomCode = NormalizeRoomCode(roomCode);
            if (normalizedRoomCode.Length < 3 || normalizedRoomCode.Length > 12)
            {
                return false;
            }

            for (var index = 0; index < normalizedRoomCode.Length; index += 1)
            {
                if (!char.IsLetterOrDigit(normalizedRoomCode[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private string GenerateRoomCode()
        {
            if (nextRoomNumber < 1000 || nextRoomNumber > 9998)
            {
                nextRoomNumber = 1234;
            }

            var roomCode = nextRoomNumber.ToString("0000", CultureInfo.InvariantCulture);
            nextRoomNumber += 1;
            return roomCode;
        }

        private static bool IsTypeAvailable(string typeName)
        {
            return Type.GetType(typeName, throwOnError: false) != null;
        }
    }
}
