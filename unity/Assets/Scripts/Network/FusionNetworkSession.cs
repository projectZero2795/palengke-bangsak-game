using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Palengke.BangSak.Network
{
    [DisallowMultipleComponent]
    public sealed class FusionNetworkSession : MonoBehaviour, INetworkRunnerCallbacks
    {
        public const int MaximumPlayers = 4;
        public const string FixedRegion = "eu";
        public const string GameplaySceneName = "PrototypeMap";
        private const float MovementSendIntervalSeconds = 0.1f;
        private const float RoundSendIntervalSeconds = 0.25f;
        private const int ReliableMagic = 0x4253414B;

        private static FusionNetworkSession instance;

        private NetworkRunner runner;
        private GameObject runnerObject;
        private bool intentionalShutdown;
        private int outgoingSequence;
        private float nextMovementSendAt;
        private float nextRoundSendAt;
        private PrototypeNetworkMovementSyncController localMovement;
        private PrototypeNetworkActionSyncController localAction;
        private PrototypeRoundRulesController roundRules;
        private int reliableSendDiagnostics;
        private int reliableReceiveDiagnostics;

        public static FusionNetworkSession Active => instance;

        public static FusionNetworkSession EnsureInstance()
        {
            if (instance != null)
            {
                return instance;
            }

            var existing = FindObjectOfType<FusionNetworkSession>();
            if (existing != null)
            {
                instance = existing;
                return instance;
            }

            var sessionObject = new GameObject("Phase 32 Fusion Network Session");
            instance = sessionObject.AddComponent<FusionNetworkSession>();
            return instance;
        }

        public PrototypeNetworkRoomState State { get; private set; } = PrototypeNetworkRoomState.Disconnected;

        public string ActiveRoomCode { get; private set; } = string.Empty;

        public string StatusMessage { get; private set; } = "Ready for a Photon room.";

        public bool IsConnected => State == PrototypeNetworkRoomState.Connected
            && runner != null
            && runner.IsRunning;

        public bool IsMasterClient => IsConnected && runner.IsSharedModeMasterClient;

        public int LocalPlayerIndex => IsConnected ? Mathf.Max(0, PlayerSlotFor(runner.LocalPlayer)) : 0;

        public int ActivePlayerCount => IsConnected ? GetRosterSize() : 0;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnUnitySceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnUnitySceneLoaded;
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Update()
        {
            if (!IsConnected || SceneManager.GetActiveScene().name != GameplaySceneName)
            {
                return;
            }

            if (localMovement == null || localAction == null || roundRules == null)
            {
                BindGameplayScene();
            }

            if (localMovement != null && Time.unscaledTime >= nextMovementSendAt)
            {
                SendMovement(localMovement.CaptureSnapshot(Time.time));
                nextMovementSendAt = Time.unscaledTime + MovementSendIntervalSeconds;
            }

            if (localAction != null && localAction.TryCaptureLocalActionEvent(Time.time, out var actionEvent))
            {
                SendAction(actionEvent);
            }

            if (IsMasterClient && roundRules != null && Time.unscaledTime >= nextRoundSendAt)
            {
                SendRoundState(roundRules.CaptureNetworkSnapshot());
                nextRoundSendAt = Time.unscaledTime + RoundSendIntervalSeconds;
            }
        }

        public bool BeginConnect(string roomCode, bool allowCreate)
        {
            if (State == PrototypeNetworkRoomState.Connecting
                || State == PrototypeNetworkRoomState.Leaving)
            {
                return false;
            }

            if (IsConnected)
            {
                StatusMessage = $"Already connected to room {ActiveRoomCode}.";
                return false;
            }

            _ = ConnectAsync(roomCode, allowCreate);
            return true;
        }

        public void BeginLeave()
        {
            if (State == PrototypeNetworkRoomState.Leaving)
            {
                return;
            }

            _ = LeaveAsync();
        }

        public bool RequestNetworkRound(string sceneName)
        {
            if (!IsConnected || string.IsNullOrWhiteSpace(sceneName))
            {
                return false;
            }

            if (runner.IsSceneAuthority)
            {
                runner.LoadScene(
                    sceneName.Trim(),
                    LoadSceneMode.Single,
                    LocalPhysicsMode.None,
                    true);
                return true;
            }

            StatusMessage = "Waiting for the room creator to start the round.";
            return true;
        }

        public bool RequestRoundRestart()
        {
            if (!IsConnected || roundRules == null)
            {
                return false;
            }

            if (IsMasterClient)
            {
                roundRules.RestartRound();
                SendRoundState(roundRules.CaptureNetworkSnapshot());
                return true;
            }

            return SendToPlayer(
                runner.GetMasterClient(),
                FusionNetworkMessageKind.RestartRequest,
                new FusionCommandPayload { command = "restart" });
        }

        private async Task ConnectAsync(string roomCode, bool allowCreate)
        {
            ActiveRoomCode = roomCode;
            State = PrototypeNetworkRoomState.Connecting;
            StatusMessage = allowCreate
                ? $"Creating Photon room {roomCode} in {FixedRegion.ToUpperInvariant()}..."
                : $"Joining Photon room {roomCode} in {FixedRegion.ToUpperInvariant()}...";

            CleanupRunnerObject();
            runnerObject = new GameObject("Bang-Sak Fusion Runner");
            DontDestroyOnLoad(runnerObject);
            runner = runnerObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = false;
            runner.AddCallbacks(this);
            var sceneManager = runnerObject.AddComponent<NetworkSceneManagerDefault>();

            StartGameResult result;
            try
            {
                result = await runner.StartGame(new StartGameArgs
                {
                    GameMode = GameMode.Shared,
                    SessionName = roomCode,
                    PlayerCount = MaximumPlayers,
                    SceneManager = sceneManager,
                    IsOpen = true,
                    IsVisible = false,
                    EnableClientSessionCreation = allowCreate,
                    UseCachedRegions = true
                });
            }
            catch (Exception exception)
            {
                FailConnection($"Photon connection failed: {exception.Message}");
                return;
            }

            if (!result.Ok)
            {
                FailConnection($"Photon connection failed: {result.ShutdownReason}.");
                return;
            }

            State = PrototypeNetworkRoomState.Connected;
            StatusMessage = $"Connected to {roomCode} · {ActivePlayerCount}/{MaximumPlayers} players · {FixedRegion.ToUpperInvariant()}.";
        }

        private async Task LeaveAsync()
        {
            intentionalShutdown = true;
            State = PrototypeNetworkRoomState.Leaving;
            StatusMessage = "Leaving Photon room...";

            var currentRunner = runner;
            if (currentRunner != null && currentRunner.IsRunning)
            {
                try
                {
                    await currentRunner.Shutdown();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Bang-Sak Photon leave warning: {exception.Message}");
                }
            }

            ResetSession("Left Photon room.");
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

        private void FailConnection(string message)
        {
            State = PrototypeNetworkRoomState.Failed;
            StatusMessage = message;
            CleanupRunnerObject();
        }

        private void ResetSession(string message)
        {
            CleanupRunnerObject();
            ResetGameplayBindings();
            ActiveRoomCode = string.Empty;
            State = PrototypeNetworkRoomState.Disconnected;
            StatusMessage = message;
            intentionalShutdown = false;
        }

        private void CleanupRunnerObject()
        {
            runner = null;
            if (runnerObject != null)
            {
                Destroy(runnerObject);
                runnerObject = null;
            }
        }

        private void OnUnitySceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResetGameplayBindings();
            if (IsConnected && scene.name == GameplaySceneName)
            {
                StartCoroutine(BindGameplayAfterSceneLoad());
            }
        }

        private IEnumerator BindGameplayAfterSceneLoad()
        {
            yield return null;
            BindGameplayScene();
        }

        private void BindGameplayScene()
        {
            if (!IsConnected || SceneManager.GetActiveScene().name != GameplaySceneName)
            {
                return;
            }

            var spawner = FindObjectOfType<PrototypeNetworkPlayerSpawner>();
            if (spawner != null)
            {
                spawner.ConfigurePreview(BuildRosterNames(), GetRosterSize(), LocalPlayerIndex);
                spawner.SpawnPreviewPlayers();
                var localPlayer = spawner.LastSpawnedLocalPlayer;
                localMovement = localPlayer != null
                    ? localPlayer.GetComponent<PrototypeNetworkMovementSyncController>()
                    : null;
                localAction = localPlayer != null
                    ? localPlayer.GetComponent<PrototypeNetworkActionSyncController>()
                    : null;
            }

            roundRules = FindObjectOfType<PrototypeRoundRulesController>();
            if (roundRules != null)
            {
                roundRules.SetNetworkStateMode(true, IsMasterClient);
                StartCoroutine(RefreshRoundActorsAfterSpawn(roundRules));
            }

            nextMovementSendAt = Time.unscaledTime;
            nextRoundSendAt = Time.unscaledTime;
            StatusMessage = $"Playing room {ActiveRoomCode} · {(IsMasterClient ? "round authority" : "remote round state")}.";
        }

        private IEnumerator RefreshRoundActorsAfterSpawn(PrototypeRoundRulesController controller)
        {
            yield return null;
            if (controller == null || controller != roundRules)
            {
                yield break;
            }

            controller.RefreshNetworkActors();
            if (IsMasterClient)
            {
                SendRoundState(controller.CaptureNetworkSnapshot());
            }
        }

        private void ResetGameplayBindings()
        {
            localMovement = null;
            localAction = null;
            roundRules = null;
        }

        private string[] BuildRosterNames()
        {
            return new[] { "JuanP", "Maria", "Pedro", "Ana" };
        }

        private int GetRosterSize()
        {
            var playerCount = 0;
            if (runner == null)
            {
                return playerCount;
            }

            foreach (var player in runner.ActivePlayers)
            {
                playerCount += 1;
            }

            return Mathf.Clamp(playerCount, 0, MaximumPlayers);
        }

        private void SendMovement(PrototypeNetworkMovementSnapshot snapshot)
        {
            var payload = new FusionMovementPayload
            {
                networkPlayerId = snapshot.NetworkPlayerId,
                x = snapshot.Position.x,
                y = snapshot.Position.y,
                inputX = snapshot.MovementInput.x,
                inputY = snapshot.MovementInput.y,
                facingDirection = (int)snapshot.FacingDirection,
                sequence = snapshot.Sequence,
                sentAt = snapshot.SentAt
            };
            Broadcast(FusionNetworkMessageKind.Movement, payload);
        }

        private void SendAction(PrototypeNetworkActionEvent actionEvent)
        {
            var payload = new FusionActionPayload
            {
                kind = (int)actionEvent.Kind,
                outcome = (int)actionEvent.Outcome,
                actorNetworkPlayerId = actionEvent.ActorNetworkPlayerId,
                targetNetworkPlayerId = actionEvent.TargetNetworkPlayerId,
                calledName = actionEvent.CalledName,
                targetDisplayName = actionEvent.TargetDisplayName,
                originX = actionEvent.Origin.x,
                originY = actionEvent.Origin.y,
                pointX = actionEvent.Point.x,
                pointY = actionEvent.Point.y,
                directionX = actionEvent.Direction.x,
                directionY = actionEvent.Direction.y,
                facingDirection = (int)actionEvent.FacingDirection,
                sequence = actionEvent.Sequence,
                sentAt = actionEvent.SentAt
            };
            Broadcast(FusionNetworkMessageKind.Action, payload);
        }

        private void SendRoundState(PrototypeRoundNetworkSnapshot snapshot)
        {
            var payload = new FusionRoundPayload
            {
                state = (int)snapshot.State,
                result = (int)snapshot.Result,
                resultTitle = snapshot.ResultTitle,
                resultMessage = snapshot.ResultMessage,
                totalHiders = snapshot.TotalHiders,
                remainingHiders = snapshot.RemainingHiders,
                remainingSeconds = snapshot.RemainingSeconds,
                roundNumber = snapshot.RoundNumber
            };
            Broadcast(FusionNetworkMessageKind.RoundState, payload);
        }

        private bool Broadcast<T>(FusionNetworkMessageKind kind, T payload)
        {
            if (!IsConnected)
            {
                return false;
            }

            var sent = false;
            foreach (var player in runner.ActivePlayers)
            {
                if (player == runner.LocalPlayer)
                {
                    continue;
                }

                sent |= SendToPlayer(player, kind, payload);
            }

            return sent;
        }

        private bool SendToPlayer<T>(PlayerRef player, FusionNetworkMessageKind kind, T payload)
        {
            if (!IsConnected || !player.IsRealPlayer || player == runner.LocalPlayer)
            {
                return false;
            }

            outgoingSequence += 1;
            var data = FusionNetworkProtocol.Encode(kind, LocalPlayerIndex, outgoingSequence, payload);
            var key = ReliableKey.FromInts(ReliableMagic, (int)kind, LocalPlayerIndex, outgoingSequence);
            runner.SendReliableDataToPlayer(player, key, data);
            if (reliableSendDiagnostics < 8)
            {
                reliableSendDiagnostics += 1;
                Debug.Log(
                    $"Bang-Sak reliable send {kind}: slot {LocalPlayerIndex} -> PlayerRef {player.AsIndex}, {data.Length} bytes.");
            }
            return true;
        }

        private void HandleEnvelope(PlayerRef source, FusionNetworkEnvelope envelope)
        {
            // Shared Mode routes peer-to-peer reliable data through the Photon server.
            // Its callback PlayerRef identifies the local target, so the envelope owns
            // sender identification until Phase 33 adds stronger integrity validation.
            var callbackSourceSlot = runner.GameMode == GameMode.Shared
                ? -1
                : PlayerSlotFor(source);
            var senderSlot = ResolveEnvelopeSenderSlot(
                callbackSourceSlot,
                envelope.senderIndex,
                GetRosterSize());
            if (senderSlot < 0)
            {
                return;
            }

            var kind = (FusionNetworkMessageKind)envelope.kind;
            switch (kind)
            {
                case FusionNetworkMessageKind.Movement:
                    if (FusionNetworkProtocol.TryDecodePayload(envelope, out FusionMovementPayload movement))
                    {
                        ApplyMovement(senderSlot, movement);
                    }
                    break;
                case FusionNetworkMessageKind.Action:
                    if (FusionNetworkProtocol.TryDecodePayload(envelope, out FusionActionPayload action))
                    {
                        ApplyAction(senderSlot, action);
                    }
                    break;
                case FusionNetworkMessageKind.RoundState:
                    if (!IsMasterClient
                        && senderSlot == PlayerSlotFor(runner.GetMasterClient())
                        && FusionNetworkProtocol.TryDecodePayload(envelope, out FusionRoundPayload round))
                    {
                        ApplyRoundState(round);
                    }
                    break;
                case FusionNetworkMessageKind.RestartRequest:
                    if (IsMasterClient && roundRules != null)
                    {
                        roundRules.RestartRound();
                        SendRoundState(roundRules.CaptureNetworkSnapshot());
                    }
                    break;
            }
        }

        private void ApplyMovement(int senderSlot, FusionMovementPayload payload)
        {
            if (payload == null || payload.networkPlayerId != NetworkPlayerIdFor(senderSlot))
            {
                return;
            }

            if (!Enum.IsDefined(typeof(PlayerFacingDirection), payload.facingDirection))
            {
                return;
            }

            var target = FindMovementSync(payload.networkPlayerId);
            target?.ApplyRemoteSnapshot(new PrototypeNetworkMovementSnapshot(
                payload.networkPlayerId,
                new Vector2(payload.x, payload.y),
                new Vector2(payload.inputX, payload.inputY),
                (PlayerFacingDirection)payload.facingDirection,
                payload.sequence,
                payload.sentAt));
        }

        private void ApplyAction(int senderSlot, FusionActionPayload payload)
        {
            if (payload == null
                || payload.actorNetworkPlayerId != NetworkPlayerIdFor(senderSlot)
                || !Enum.IsDefined(typeof(PrototypeNetworkActionKind), payload.kind)
                || !Enum.IsDefined(typeof(PrototypeNetworkActionOutcome), payload.outcome)
                || !Enum.IsDefined(typeof(PlayerFacingDirection), payload.facingDirection))
            {
                return;
            }

            var target = FindActionSync(payload.actorNetworkPlayerId);
            target?.ApplyRemoteActionEvent(new PrototypeNetworkActionEvent(
                (PrototypeNetworkActionKind)payload.kind,
                (PrototypeNetworkActionOutcome)payload.outcome,
                payload.actorNetworkPlayerId,
                payload.targetNetworkPlayerId,
                payload.calledName,
                payload.targetDisplayName,
                new Vector2(payload.originX, payload.originY),
                new Vector2(payload.pointX, payload.pointY),
                new Vector2(payload.directionX, payload.directionY),
                (PlayerFacingDirection)payload.facingDirection,
                payload.sequence,
                payload.sentAt));
        }

        private void ApplyRoundState(FusionRoundPayload payload)
        {
            if (payload == null
                || roundRules == null
                || !Enum.IsDefined(typeof(PrototypeRoundState), payload.state)
                || !Enum.IsDefined(typeof(PrototypeRoundResult), payload.result))
            {
                return;
            }

            roundRules.ApplyNetworkSnapshot(new PrototypeRoundNetworkSnapshot(
                (PrototypeRoundState)payload.state,
                (PrototypeRoundResult)payload.result,
                payload.resultTitle,
                payload.resultMessage,
                payload.totalHiders,
                payload.remainingHiders,
                payload.remainingSeconds,
                payload.roundNumber));
        }

        private static PrototypeNetworkMovementSyncController FindMovementSync(string networkPlayerId)
        {
            var identities = FindObjectsOfType<PrototypeNetworkPlayerIdentity>();
            for (var index = 0; index < identities.Length; index += 1)
            {
                if (identities[index].NetworkPlayerId == networkPlayerId)
                {
                    return identities[index].GetComponent<PrototypeNetworkMovementSyncController>();
                }
            }

            return null;
        }

        private static PrototypeNetworkActionSyncController FindActionSync(string networkPlayerId)
        {
            var identities = FindObjectsOfType<PrototypeNetworkPlayerIdentity>();
            for (var index = 0; index < identities.Length; index += 1)
            {
                if (identities[index].NetworkPlayerId == networkPlayerId)
                {
                    return identities[index].GetComponent<PrototypeNetworkActionSyncController>();
                }
            }

            return null;
        }

        private static string NetworkPlayerIdFor(int playerIndex)
        {
            return $"preview-{Mathf.Max(0, playerIndex):00}";
        }

        private int PlayerSlotFor(PlayerRef player)
        {
            if (runner == null || !player.IsRealPlayer)
            {
                return -1;
            }

            var activePlayerIndices = new List<int>();
            foreach (var activePlayer in runner.ActivePlayers)
            {
                activePlayerIndices.Add(activePlayer.AsIndex);
            }

            return ResolveRosterSlot(activePlayerIndices.ToArray(), player.AsIndex);
        }

        public static int ResolveRosterSlot(int[] activePlayerIndices, int playerIndex)
        {
            if (activePlayerIndices == null || activePlayerIndices.Length == 0)
            {
                return -1;
            }

            var sortedIndices = (int[])activePlayerIndices.Clone();
            Array.Sort(sortedIndices);
            return Array.IndexOf(sortedIndices, playerIndex);
        }

        public static int ResolveEnvelopeSenderSlot(
            int callbackSourceSlot,
            int envelopeSenderSlot,
            int rosterSize)
        {
            if (envelopeSenderSlot < 0 || envelopeSenderSlot >= rosterSize)
            {
                return -1;
            }

            if (callbackSourceSlot >= 0 && callbackSourceSlot != envelopeSenderSlot)
            {
                return -1;
            }

            return envelopeSenderSlot;
        }

        public void OnPlayerJoined(NetworkRunner networkRunner, PlayerRef player)
        {
            StatusMessage = $"Connected to {ActiveRoomCode} · {GetRosterSize()}/{MaximumPlayers} players · {FixedRegion.ToUpperInvariant()}.";
            if (SceneManager.GetActiveScene().name == GameplaySceneName)
            {
                BindGameplayScene();
            }
        }

        public void OnPlayerLeft(NetworkRunner networkRunner, PlayerRef player)
        {
            StatusMessage = $"Player left · {GetRosterSize()}/{MaximumPlayers} remain. Room stays available for manual rejoin.";
            if (SceneManager.GetActiveScene().name == GameplaySceneName)
            {
                BindGameplayScene();
            }
        }

        public void OnShutdown(NetworkRunner networkRunner, ShutdownReason shutdownReason)
        {
            if (!intentionalShutdown && State != PrototypeNetworkRoomState.Failed)
            {
                State = PrototypeNetworkRoomState.Failed;
                StatusMessage = $"Photon disconnected ({shutdownReason}). Rejoin room {ActiveRoomCode} from the menu.";
            }
        }

        public void OnDisconnectedFromServer(NetworkRunner networkRunner, NetDisconnectReason reason)
        {
            if (!intentionalShutdown)
            {
                State = PrototypeNetworkRoomState.Failed;
                StatusMessage = $"Photon connection lost ({reason}). Rejoin room {ActiveRoomCode} from the menu.";
            }
        }

        public void OnConnectFailed(NetworkRunner networkRunner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            FailConnection($"Photon connection failed: {reason}.");
        }

        public void OnReliableDataReceived(
            NetworkRunner networkRunner,
            PlayerRef player,
            ReliableKey key,
            ReadOnlySpan<byte> data)
        {
            if (reliableReceiveDiagnostics < 8)
            {
                reliableReceiveDiagnostics += 1;
                Debug.Log(
                    $"Bang-Sak reliable receive: PlayerRef {player.AsIndex}, slot {PlayerSlotFor(player)}, {data.Length} bytes.");
            }

            if (FusionNetworkProtocol.TryDecode(data.ToArray(), out var envelope))
            {
                HandleEnvelope(player, envelope);
            }
        }

        public void OnObjectExitAOI(NetworkRunner networkRunner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectEnterAOI(NetworkRunner networkRunner, NetworkObject obj, PlayerRef player) { }

        public void OnConnectRequest(NetworkRunner networkRunner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

        public void OnUserSimulationMessage(NetworkRunner networkRunner, SimulationMessagePtr message) { }

        public void OnReliableDataProgress(NetworkRunner networkRunner, PlayerRef player, ReliableKey key, float progress) { }

        public void OnInput(NetworkRunner networkRunner, NetworkInput input) { }

        public void OnInputMissing(NetworkRunner networkRunner, PlayerRef player, NetworkInput input) { }

        public void OnConnectedToServer(NetworkRunner networkRunner) { }

        public void OnSessionListUpdated(NetworkRunner networkRunner, List<SessionInfo> sessionList) { }

        public void OnCustomAuthenticationResponse(NetworkRunner networkRunner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner networkRunner, HostMigrationToken hostMigrationToken) { }

        public void OnSceneLoadDone(NetworkRunner networkRunner) { }

        public void OnSceneLoadStart(NetworkRunner networkRunner) { }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            instance = null;
        }
    }
}
