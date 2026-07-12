using System.Globalization;
using UnityEngine;

namespace Palengke.BangSak.Network
{
    [DisallowMultipleComponent]
    public sealed class PrototypeNetworkRoomController : MonoBehaviour
    {
        public const string ComponentId = "prototype_network_room";
        public const int ComponentVersion = 2;
        public const string ComponentVariant = "phase32_fusion_shared_room";
        public const string DefaultProviderName = "Photon Fusion 2.1 Shared";

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

        [Header("Room")]
        [SerializeField]
        private string defaultJoinRoomCode = "1234";

        [SerializeField]
        private int nextRoomNumber = 1234;

        private FusionNetworkSession session;
        private PrototypeNetworkRoomState localState = PrototypeNetworkRoomState.Disconnected;
        private string localRoomCode = string.Empty;
        private string localStatusMessage = "Ready for a Photon room.";

        public string ComponentIdValue => componentId;

        public int ComponentVersionValue => componentVersion;

        public string ComponentVariantValue => componentVariant;

        public string ProviderName => providerName;

        public string DefaultJoinRoomCode => defaultJoinRoomCode;

        public PrototypeNetworkRoomState State => session != null ? session.State : localState;

        public string ActiveRoomCode => session != null ? session.ActiveRoomCode : localRoomCode;

        public string StatusMessage => session != null ? session.StatusMessage : localStatusMessage;

        public bool HasActiveRoom => !string.IsNullOrWhiteSpace(ActiveRoomCode);

        public bool IsFusionSdkAvailable => true;

        public bool IsConnected => session != null && session.IsConnected;

        public int ActivePlayerCount => session != null ? session.ActivePlayerCount : 0;

        public string RosterSummary => session != null ? session.RosterSummary : "none";

        public string LocalRosterName => session != null ? session.LocalRosterName : "none";

        private void OnEnable()
        {
            ResolveSession();
        }

        public bool CreateRoom()
        {
            ResolveSession();
            var roomCode = GenerateRoomCode();
            localRoomCode = roomCode;
            localState = PrototypeNetworkRoomState.Connecting;
            localStatusMessage = $"Creating Photon room {roomCode}...";

            return session == null || session.BeginConnect(roomCode, allowCreate: true);
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
                localStatusMessage = "Room code must be 3-12 letters or numbers.";
                return false;
            }

            ResolveSession();
            localRoomCode = normalizedRoomCode;
            localState = PrototypeNetworkRoomState.Connecting;
            localStatusMessage = $"Joining Photon room {normalizedRoomCode}...";
            return session == null || session.BeginConnect(normalizedRoomCode, allowCreate: false);
        }

        public void LeaveRoom()
        {
            ResolveSession();
            if (session != null)
            {
                session.BeginLeave();
                return;
            }

            localRoomCode = string.Empty;
            localState = PrototypeNetworkRoomState.Disconnected;
            localStatusMessage = "Left Photon room.";
        }

        public bool StartNetworkRound(string sceneName)
        {
            ResolveSession();
            return session != null && session.RequestNetworkRound(sceneName);
        }

        public bool RestartNetworkRound()
        {
            ResolveSession();
            return session != null && session.RequestRoundRestart();
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

        private void ResolveSession()
        {
            if (session == null && Application.isPlaying)
            {
                session = FusionNetworkSession.EnsureInstance();
            }
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
    }
}
