using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
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
        public const string DirectEuNameServer = "ns-eu.photonengine.io";
        public const string GameplaySceneName = "PrototypeMap";
        public const int MaximumConnectionAttempts = 2;
        private const float ConnectionRetryDelaySeconds = 0.75f;
        private const float MovementSendIntervalSeconds = 0.1f;
        private const float RoundSendIntervalSeconds = 0.25f;
        private const float CredentialRefreshIntervalSeconds = 5f;
        private const float MovementSpawnGraceSeconds = 0.75f;
        private const int ReliableMagic = 0x4253414B;
        private const int MaximumIntegrityDiagnostics = 12;

        private static FusionNetworkSession instance;

        private NetworkRunner runner;
        private GameObject runnerObject;
        private bool intentionalShutdown;
        private int outgoingSequence;
        private int localRequestSequence;
        private float nextMovementSendAt;
        private float nextRoundSendAt;
        private float nextCredentialRefreshAt;
        private float movementValidationStartsAt;
        private PrototypeNetworkMovementSyncController localMovement;
        private PrototypeNetworkActionSyncController localAction;
        private PrototypeRoundRulesController roundRules;
        private readonly FusionIntegrityGuard integrityGuard = new FusionIntegrityGuard();
        private readonly Dictionary<int, string> authorityCredentials = new Dictionary<int, string>();
        private string localAuthorityToken = string.Empty;
        private string credentialsRosterFingerprint = string.Empty;
        private string authorityRoundId = string.Empty;
        private int authorityEpoch;
        private int lastAuthoritySequence;
        private int integrityDiagnostics;
        private int reliableSendDiagnostics;
        private int reliableReceiveDiagnostics;
        private bool restartRoundAfterRosterChange;

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

        public string RosterSummary => FormatRosterSummary(ActivePlayerCount);

        public string LocalRosterName => IsConnected ? RosterNameForSlot(LocalPlayerIndex) : "none";

        public bool CanSubmitAuthoritativeScore => IsConnected && IsMasterClient;

        public string AuthorityRoundId => authorityRoundId;

        public int RejectedIntegrityMessageCount => integrityGuard.RejectedCount;

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

            if (IsMasterClient && Time.unscaledTime >= nextCredentialRefreshAt)
            {
                SendCurrentAuthorityGrants();
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
                if (!integrityGuard.ValidateRestart(
                        LocalPlayerIndex,
                        Time.unscaledTime,
                        roundRules.IsFinished,
                        out var rejection))
                {
                    LogIntegrityRejection(FusionNetworkMessageKind.RestartRequest, LocalPlayerIndex, rejection);
                    return false;
                }

                roundRules.RestartRound();
                ResetMovementBaselineForSpawn();
                authorityRoundId = BuildAuthorityRoundId(roundRules.RoundNumber);
                SendRoundState(roundRules.CaptureNetworkSnapshot());
                return true;
            }

            return SendRequestToAuthority(
                FusionNetworkMessageKind.RestartRequest,
                new FusionCommandPayload { command = "restart" });
        }

        private async Task ConnectAsync(string roomCode, bool allowCreate)
        {
            ResetIntegrityState();
            intentionalShutdown = false;
            ActiveRoomCode = roomCode;
            State = PrototypeNetworkRoomState.Connecting;
            StatusMessage = allowCreate
                ? $"Creating Photon room {roomCode} in {FixedRegion.ToUpperInvariant()}..."
                : $"Joining Photon room {roomCode} in {FixedRegion.ToUpperInvariant()}...";

            CleanupRunnerObject();
            string lastFailure = "Unknown connection error";
            for (var attempt = 1; attempt <= MaximumConnectionAttempts; attempt += 1)
            {
                State = PrototypeNetworkRoomState.Connecting;
                var useDirectEuNameServer = ShouldUseDirectEuNameServer(attempt);
                ConfigurePhotonNameServer(useDirectEuNameServer);
                var sceneManager = CreateRunner();
                var result = default(StartGameResult);
                var receivedResult = false;

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
                    receivedResult = true;
                }
                catch (Exception exception)
                {
                    lastFailure = exception.Message;
                    Debug.LogWarning(
                        $"Bang-Sak Photon attempt {attempt}/{MaximumConnectionAttempts} threw: {exception.Message}");
                }

                if (result.Ok)
                {
                    State = PrototypeNetworkRoomState.Connected;
                    StatusMessage = $"Connected to {roomCode} · {ActivePlayerCount}/{MaximumPlayers} players · {FixedRegion.ToUpperInvariant()}.";
                    return;
                }

                if (receivedResult && !string.IsNullOrWhiteSpace(result.ShutdownReason.ToString()))
                {
                    lastFailure = result.ShutdownReason.ToString();
                }

                Debug.LogWarning(
                    $"Bang-Sak Photon attempt {attempt}/{MaximumConnectionAttempts} failed ({lastFailure}); "
                    + $"direct EU name server: {useDirectEuNameServer}.");
                CleanupRunnerObject();

                if (attempt < MaximumConnectionAttempts)
                {
                    StatusMessage = $"Photon connection interrupted ({lastFailure}). Retrying secure EU route "
                        + $"{attempt + 1}/{MaximumConnectionAttempts}...";
                    await WaitForConnectionRetryAsync();
                }
            }

            FailConnection(
                $"Photon could not connect after {MaximumConnectionAttempts} attempts ({lastFailure}). "
                + "Check the connection and select CREATE or JOIN again.");
        }

        private Task WaitForConnectionRetryAsync()
        {
            var completion = new TaskCompletionSource<bool>();
            StartCoroutine(CompleteConnectionRetryAfterDelay(completion));
            return completion.Task;
        }

        private static IEnumerator CompleteConnectionRetryAfterDelay(TaskCompletionSource<bool> completion)
        {
            yield return new WaitForSecondsRealtime(ConnectionRetryDelaySeconds);
            completion.TrySetResult(true);
        }

        private NetworkSceneManagerDefault CreateRunner()
        {
            runnerObject = new GameObject("Bang-Sak Fusion Runner");
            DontDestroyOnLoad(runnerObject);
            runner = runnerObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = false;
            runner.AddCallbacks(this);
            return runnerObject.AddComponent<NetworkSceneManagerDefault>();
        }

        public static string ResolvePhotonNameServer(bool useDirectEuNameServer)
        {
            return useDirectEuNameServer ? DirectEuNameServer : string.Empty;
        }

        private static void ConfigurePhotonNameServer(bool useDirectEuNameServer)
        {
            var server = ResolvePhotonNameServer(useDirectEuNameServer);
            if (string.IsNullOrEmpty(server))
            {
                return;
            }

            var settingsWrapper = PhotonAppSettings.Global;
            var appSettingsField = settingsWrapper.GetType().GetField("AppSettings");
            var appSettings = appSettingsField?.GetValue(settingsWrapper);
            var appSettingsType = appSettings?.GetType();
            var serverField = appSettingsType?.GetField("Server");
            var useNameServerField = appSettingsType?.GetField("UseNameServer");
            var fixedRegionField = appSettingsType?.GetField("FixedRegion");

            if (serverField == null || useNameServerField == null || fixedRegionField == null)
            {
                Debug.LogWarning("Bang-Sak could not configure the direct Photon EU name server; using the SDK default route.");
                return;
            }

            serverField.SetValue(appSettings, server);
            useNameServerField.SetValue(appSettings, true);
            fixedRegionField.SetValue(appSettings, FixedRegion);
            Debug.Log("Bang-Sak Photon route: direct EU secure name server.");
        }

        public static bool ShouldUseDirectEuNameServer(int attempt)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return true;
#else
            return attempt > 1;
#endif
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
            ResetIntegrityState();
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


            EnsureAuthorityCredentials();

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
                if (IsMasterClient && string.IsNullOrWhiteSpace(authorityRoundId))
                {
                    authorityRoundId = BuildAuthorityRoundId(roundRules.RoundNumber);
                }
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
                if (restartRoundAfterRosterChange)
                {
                    controller.RestartRound();
                    authorityRoundId = BuildAuthorityRoundId(controller.RoundNumber);
                }

                ResetMovementBaselineForSpawn();
                SendRoundState(controller.CaptureNetworkSnapshot());
            }

            restartRoundAfterRosterChange = false;
        }

        private void ResetGameplayBindings()
        {
            localMovement = null;
            localAction = null;
            roundRules = null;
        }

        private void ResetIntegrityState()
        {
            authorityCredentials.Clear();
            integrityGuard.Reset();
            localAuthorityToken = string.Empty;
            credentialsRosterFingerprint = string.Empty;
            authorityRoundId = string.Empty;
            authorityEpoch = 0;
            lastAuthoritySequence = 0;
            localRequestSequence = 0;
            nextCredentialRefreshAt = 0f;
            integrityDiagnostics = 0;
        }

        private string[] BuildRosterNames()
        {
            return new[] { "JuanP", "Maria", "Pedro", "Ana" };
        }

        public static string RosterNameForSlot(int playerSlot)
        {
            var names = new[] { "JuanP", "Maria", "Pedro", "Ana" };
            return playerSlot >= 0 && playerSlot < names.Length ? names[playerSlot] : "none";
        }

        public static string FormatRosterSummary(int activePlayerCount)
        {
            var safeCount = Mathf.Clamp(activePlayerCount, 0, MaximumPlayers);
            if (safeCount == 0)
            {
                return "none";
            }

            var names = new string[safeCount];
            for (var index = 0; index < safeCount; index += 1)
            {
                names[index] = RosterNameForSlot(index);
            }

            return string.Join(" · ", names);
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
                sequence = NextLocalRequestSequence(),
                sentAt = snapshot.SentAt
            };
            if (IsMasterClient)
            {
                ProcessMovementRequest(LocalPlayerIndex, payload);
            }
            else
            {
                SendRequestToAuthority(FusionNetworkMessageKind.MovementRequest, payload);
            }
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
                sequence = NextLocalRequestSequence(),
                sentAt = actionEvent.SentAt
            };
            if (IsMasterClient)
            {
                ProcessActionRequest(LocalPlayerIndex, payload, true);
            }
            else
            {
                SendRequestToAuthority(FusionNetworkMessageKind.ActionRequest, payload);
            }
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
                roundNumber = snapshot.RoundNumber,
                caughtPlayerMask = BuildCaughtPlayerMask(),
                tayaCountered = IsTayaCountered(),
                authorityRoundId = authorityRoundId
            };
            BroadcastAuthoritative(FusionNetworkMessageKind.RoundState, payload);
        }

        private bool BroadcastAuthoritative<T>(FusionNetworkMessageKind kind, T payload)
        {
            if (!IsConnected || !IsMasterClient)
            {
                return false;
            }

            EnsureAuthorityCredentials();

            var sent = false;
            foreach (var player in runner.ActivePlayers)
            {
                if (player == runner.LocalPlayer)
                {
                    continue;
                }

                var playerSlot = PlayerSlotFor(player);
                if (authorityCredentials.TryGetValue(playerSlot, out var token))
                {
                    sent |= SendToPlayer(player, kind, payload, token);
                }
                else
                {
                    LogIntegrityRejection(kind, playerSlot, FusionIntegrityRejection.InvalidCredential);
                }
            }

            return sent;
        }

        private bool SendRequestToAuthority<T>(FusionNetworkMessageKind kind, T payload)
        {
            if (!IsConnected
                || IsMasterClient
                || string.IsNullOrWhiteSpace(localAuthorityToken))
            {
                return false;
            }

            return SendToPlayer(runner.GetMasterClient(), kind, payload, localAuthorityToken);
        }

        private bool SendToPlayer<T>(
            PlayerRef player,
            FusionNetworkMessageKind kind,
            T payload,
            string authorityToken = "")
        {
            if (!IsConnected || !player.IsRealPlayer || player == runner.LocalPlayer)
            {
                return false;
            }

            outgoingSequence += 1;
            var data = FusionNetworkProtocol.Encode(
                kind,
                LocalPlayerIndex,
                outgoingSequence,
                payload,
                authorityToken);
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
            var senderSlot = ResolveEnvelopeSenderSlot(-1, envelope.senderIndex, GetRosterSize());
            if (senderSlot < 0)
            {
                return;
            }

            var kind = (FusionNetworkMessageKind)envelope.kind;
            if (kind == FusionNetworkMessageKind.AuthorityGrant)
            {
                ApplyAuthorityGrant(senderSlot, envelope);
                return;
            }

            if (IsMasterClient)
            {
                if (!integrityGuard.ValidateEnvelope(
                        envelope,
                        GetRosterSize(),
                        out var requestRejection))
                {
                    LogIntegrityRejection(kind, senderSlot, requestRejection);
                    return;
                }

                switch (kind)
                {
                    case FusionNetworkMessageKind.MovementRequest:
                        if (FusionNetworkProtocol.TryDecodePayload(envelope, out FusionMovementPayload movementRequest))
                        {
                            ProcessMovementRequest(senderSlot, movementRequest);
                        }
                        return;
                    case FusionNetworkMessageKind.ActionRequest:
                        if (FusionNetworkProtocol.TryDecodePayload(envelope, out FusionActionPayload actionRequest))
                        {
                            ProcessActionRequest(senderSlot, actionRequest, false);
                        }
                        return;
                    case FusionNetworkMessageKind.RestartRequest:
                        ProcessRestartRequest(senderSlot, envelope);
                        return;
                    default:
                        LogIntegrityRejection(kind, senderSlot, FusionIntegrityRejection.InvalidRole);
                        return;
                }
            }

            var masterSlot = PlayerSlotFor(runner.GetMasterClient());
            if (senderSlot != masterSlot
                || string.IsNullOrWhiteSpace(localAuthorityToken)
                || !ConstantTimeEquals(localAuthorityToken, envelope.authorityToken)
                || envelope.sequence <= lastAuthoritySequence)
            {
                LogIntegrityRejection(kind, senderSlot, FusionIntegrityRejection.InvalidCredential);
                return;
            }

            lastAuthoritySequence = envelope.sequence;

            switch (kind)
            {
                case FusionNetworkMessageKind.MovementState:
                    if (FusionNetworkProtocol.TryDecodePayload(envelope, out FusionMovementPayload movement))
                    {
                        ApplyMovement(movement);
                    }
                    break;
                case FusionNetworkMessageKind.ActionState:
                    if (FusionNetworkProtocol.TryDecodePayload(envelope, out FusionActionPayload action))
                    {
                        ApplyAction(action);
                    }
                    break;
                case FusionNetworkMessageKind.RoundState:
                    if (FusionNetworkProtocol.TryDecodePayload(envelope, out FusionRoundPayload round))
                    {
                        ApplyRoundState(round);
                    }
                    break;
                default:
                    LogIntegrityRejection(kind, senderSlot, FusionIntegrityRejection.InvalidRole);
                    break;
            }
        }

        private void ApplyAuthorityGrant(int senderSlot, FusionNetworkEnvelope envelope)
        {
            if (IsMasterClient
                || senderSlot != PlayerSlotFor(runner.GetMasterClient())
                || !FusionNetworkProtocol.TryDecodePayload(
                    envelope,
                    out FusionAuthorityGrantPayload grant)
                || grant.playerIndex != LocalPlayerIndex
                || grant.authorityEpoch <= 0
                || string.IsNullOrWhiteSpace(grant.authorityToken)
                || grant.authorityToken.Length != 32)
            {
                LogIntegrityRejection(
                    FusionNetworkMessageKind.AuthorityGrant,
                    senderSlot,
                    FusionIntegrityRejection.InvalidCredential);
                return;
            }

            authorityEpoch = grant.authorityEpoch;
            localAuthorityToken = grant.authorityToken;
            lastAuthoritySequence = envelope.sequence;
            StatusMessage = $"Room {ActiveRoomCode} ready · authority credential active.";
        }

        private void ProcessMovementRequest(int senderSlot, FusionMovementPayload payload)
        {
            if (Time.unscaledTime < movementValidationStartsAt)
            {
                integrityGuard.ResetMovementState();
            }

            var mapLayout = roundRules != null ? roundRules.MapLayout : FindObjectOfType<PrototypeMapLayoutController>();
            var mapBounds = mapLayout != null
                ? mapLayout.MapBounds
                : new Bounds(Vector3.zero, new Vector3(52f, 36f, 0f));
            if (!integrityGuard.ValidateMovement(
                    senderSlot,
                    payload,
                    mapBounds,
                    Time.unscaledTime,
                    roundRules != null && roundRules.IsRunning,
                    out var rejection))
            {
                LogIntegrityRejection(FusionNetworkMessageKind.MovementRequest, senderSlot, rejection);
                return;
            }

            ApplyMovement(payload);
            BroadcastAuthoritative(FusionNetworkMessageKind.MovementState, payload);
        }

        private void ProcessActionRequest(
            int senderSlot,
            FusionActionPayload payload,
            bool authorityLocalAction)
        {
            if (!integrityGuard.ValidateAction(
                    senderSlot,
                    payload,
                    Time.unscaledTime,
                    roundRules != null && roundRules.IsRunning,
                    out var rejection))
            {
                LogIntegrityRejection(FusionNetworkMessageKind.ActionRequest, senderSlot, rejection);
                return;
            }

            if ((PrototypeNetworkActionKind)payload.kind == PrototypeNetworkActionKind.BangNameCall
                && !IsEligibleHiderName(payload.calledName))
            {
                LogIntegrityRejection(
                    FusionNetworkMessageKind.ActionRequest,
                    senderSlot,
                    FusionIntegrityRejection.InvalidPayload);
                return;
            }

            PrototypeNetworkActionEvent authoritativeEvent;
            if (authorityLocalAction)
            {
                authoritativeEvent = ToActionEvent(payload);
            }
            else
            {
                var actionSync = FindActionSync(FusionIntegrityGuard.NetworkPlayerIdFor(senderSlot));
                if (actionSync == null
                    || !actionSync.TryResolveAuthoritativeAction(
                        payload,
                        Time.time,
                        out authoritativeEvent))
                {
                    LogIntegrityRejection(
                        FusionNetworkMessageKind.ActionRequest,
                        senderSlot,
                        FusionIntegrityRejection.InvalidPayload);
                    return;
                }
            }

            var authoritativePayload = ToActionPayload(authoritativeEvent);
            BroadcastAuthoritative(FusionNetworkMessageKind.ActionState, authoritativePayload);
            if (roundRules != null)
            {
                roundRules.RefreshNetworkActors();
                roundRules.Tick(Time.time);
                SendRoundState(roundRules.CaptureNetworkSnapshot());
            }
        }

        private void ProcessRestartRequest(int senderSlot, FusionNetworkEnvelope envelope)
        {
            if (roundRules == null
                || !FusionNetworkProtocol.TryDecodePayload(envelope, out FusionCommandPayload command)
                || command.command != "restart")
            {
                LogIntegrityRejection(
                    FusionNetworkMessageKind.RestartRequest,
                    senderSlot,
                    FusionIntegrityRejection.InvalidPayload);
                return;
            }

            if (!integrityGuard.ValidateRestart(
                    senderSlot,
                    Time.unscaledTime,
                    roundRules.IsFinished,
                    out var rejection))
            {
                LogIntegrityRejection(FusionNetworkMessageKind.RestartRequest, senderSlot, rejection);
                return;
            }

            roundRules.RestartRound();
            ResetMovementBaselineForSpawn();
            authorityRoundId = BuildAuthorityRoundId(roundRules.RoundNumber);
            SendRoundState(roundRules.CaptureNetworkSnapshot());
        }

        private void EnsureAuthorityCredentials()
        {
            if (!IsConnected || !IsMasterClient)
            {
                return;
            }

            var fingerprint = BuildRosterFingerprint();
            if (fingerprint == credentialsRosterFingerprint && HasCredentialsForActiveRoster())
            {
                return;
            }

            credentialsRosterFingerprint = fingerprint;
            authorityEpoch += 1;
            authorityCredentials.Clear();
            integrityGuard.Reset();
            foreach (var player in runner.ActivePlayers)
            {
                var playerSlot = PlayerSlotFor(player);
                if (playerSlot < 0)
                {
                    continue;
                }

                var token = Guid.NewGuid().ToString("N");
                authorityCredentials[playerSlot] = token;
                integrityGuard.SetCredential(playerSlot, token);
                if (player == runner.LocalPlayer)
                {
                    localAuthorityToken = token;
                    continue;
                }

                SendToPlayer(
                    player,
                    FusionNetworkMessageKind.AuthorityGrant,
                    new FusionAuthorityGrantPayload
                    {
                        playerIndex = playerSlot,
                        authorityEpoch = authorityEpoch,
                        authorityToken = token
                    });
            }


            nextCredentialRefreshAt = Time.unscaledTime + CredentialRefreshIntervalSeconds;
        }

        private void SendCurrentAuthorityGrants()
        {
            if (!IsConnected || !IsMasterClient)
            {
                return;
            }

            EnsureAuthorityCredentials();
            foreach (var player in runner.ActivePlayers)
            {
                if (player == runner.LocalPlayer)
                {
                    continue;
                }

                var playerSlot = PlayerSlotFor(player);
                if (!authorityCredentials.TryGetValue(playerSlot, out var token))
                {
                    continue;
                }

                SendToPlayer(
                    player,
                    FusionNetworkMessageKind.AuthorityGrant,
                    new FusionAuthorityGrantPayload
                    {
                        playerIndex = playerSlot,
                        authorityEpoch = authorityEpoch,
                        authorityToken = token
                    });
            }

            nextCredentialRefreshAt = Time.unscaledTime + CredentialRefreshIntervalSeconds;
        }

        private bool HasCredentialsForActiveRoster()
        {
            var playerCount = 0;
            foreach (var player in runner.ActivePlayers)
            {
                playerCount += 1;
                var playerSlot = PlayerSlotFor(player);
                if (playerSlot < 0 || !authorityCredentials.ContainsKey(playerSlot))
                {
                    return false;
                }
            }

            return playerCount > 0 && authorityCredentials.Count == playerCount;
        }

        private string BuildRosterFingerprint()
        {
            var indices = new List<int>();
            foreach (var player in runner.ActivePlayers)
            {
                indices.Add(player.AsIndex);
            }

            indices.Sort();
            return string.Join(",", indices);
        }

        private static FusionActionPayload ToActionPayload(PrototypeNetworkActionEvent actionEvent)
        {
            return new FusionActionPayload
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
        }

        private static PrototypeNetworkActionEvent ToActionEvent(FusionActionPayload payload)
        {
            return new PrototypeNetworkActionEvent(
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
                payload.sentAt);
        }

        private int BuildCaughtPlayerMask()
        {
            var mask = 0;
            var identities = FindObjectsOfType<PrototypeNetworkPlayerIdentity>();
            for (var index = 0; index < identities.Length; index += 1)
            {
                var identity = identities[index];
                var caught = identity != null ? identity.GetComponent<CaughtStateController>() : null;
                if (caught != null
                    && caught.IsCaught
                    && TryParseNetworkPlayerId(identity.NetworkPlayerId, out var playerSlot)
                    && playerSlot >= 0
                    && playerSlot < MaximumPlayers)
                {
                    mask |= 1 << playerSlot;
                }
            }

            return mask;
        }

        private static bool IsEligibleHiderName(string calledName)
        {
            var normalizedName = PlayerNameIdentity.NormalizeName(calledName);
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return false;
            }

            var identities = FindObjectsOfType<PrototypeNetworkPlayerIdentity>();
            for (var index = 0; index < identities.Length; index += 1)
            {
                var identity = identities[index];
                if (identity == null || identity.Role != PlayerRole.Hider)
                {
                    continue;
                }

                var caught = identity.GetComponent<CaughtStateController>();
                if ((caught == null || !caught.IsCaught)
                    && PlayerNameIdentity.NormalizeName(identity.DisplayName) == normalizedName)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsTayaCountered()
        {
            var taya = FindActionSync(FusionIntegrityGuard.NetworkPlayerIdFor(0));
            var countered = taya != null ? taya.GetComponent<TayaCounteredStateController>() : null;
            return countered != null && countered.IsCountered;
        }

        private void ApplyAuthoritativeActorState(int caughtMask, bool tayaCountered, int roundNumber)
        {
            var identities = FindObjectsOfType<PrototypeNetworkPlayerIdentity>();
            for (var index = 0; index < identities.Length; index += 1)
            {
                var identity = identities[index];
                if (identity == null || !TryParseNetworkPlayerId(identity.NetworkPlayerId, out var playerSlot))
                {
                    continue;
                }

                var caught = identity.GetComponent<CaughtStateController>();
                var shouldBeCaught = (caughtMask & (1 << playerSlot)) != 0;
                if (caught != null && shouldBeCaught && !caught.IsCaught)
                {
                    caught.MarkCaught(this, CaughtCause.Bang, roundNumber * 100 + playerSlot);
                }
                else if (caught != null && !shouldBeCaught && caught.IsCaught)
                {
                    caught.ResetCaughtState();
                }

                if (playerSlot != 0)
                {
                    continue;
                }

                var countered = identity.GetComponent<TayaCounteredStateController>();
                if (countered != null && tayaCountered && !countered.IsCountered)
                {
                    countered.MarkCountered(this, roundNumber * 100);
                }
                else if (countered != null && !tayaCountered && countered.IsCountered)
                {
                    countered.ResetCounteredState();
                }
            }
        }

        private string BuildAuthorityRoundId(int roundNumber)
        {
            return $"bangsak_{ActiveRoomCode}_{Mathf.Max(1, roundNumber)}_{Guid.NewGuid():N}";
        }

        private void ResetMovementBaselineForSpawn()
        {
            integrityGuard.ResetMovementState();
            movementValidationStartsAt = Time.unscaledTime + MovementSpawnGraceSeconds;
        }

        private void LogIntegrityRejection(
            FusionNetworkMessageKind kind,
            int senderSlot,
            FusionIntegrityRejection rejection)
        {
            if (integrityDiagnostics >= MaximumIntegrityDiagnostics)
            {
                return;
            }

            integrityDiagnostics += 1;
            Debug.LogWarning(
                $"Bang-Sak integrity reject: kind={kind}, slot={senderSlot}, reason={rejection}, count={integrityGuard.RejectedCount}.");
        }

        private static bool TryParseNetworkPlayerId(string networkPlayerId, out int playerSlot)
        {
            playerSlot = -1;
            return !string.IsNullOrWhiteSpace(networkPlayerId)
                && networkPlayerId.StartsWith("preview-", StringComparison.Ordinal)
                && int.TryParse(networkPlayerId.Substring("preview-".Length), out playerSlot)
                && playerSlot >= 0
                && playerSlot < MaximumPlayers;
        }

        private static bool ConstantTimeEquals(string left, string right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var difference = 0;
            for (var index = 0; index < left.Length; index += 1)
            {
                difference |= left[index] ^ right[index];
            }

            return difference == 0;
        }

        private int NextLocalRequestSequence()
        {
            if (localRequestSequence == int.MaxValue)
            {
                localRequestSequence = 0;
            }

            localRequestSequence += 1;
            return localRequestSequence;
        }

        private void ApplyMovement(FusionMovementPayload payload)
        {
            if (payload == null || !TryParseNetworkPlayerId(payload.networkPlayerId, out _))
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

        private void ApplyAction(FusionActionPayload payload)
        {
            if (payload == null
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
            authorityRoundId = payload.authorityRoundId ?? string.Empty;
            ApplyAuthoritativeActorState(payload.caughtPlayerMask, payload.tayaCountered, payload.roundNumber);
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

            var masterClient = runner.GetMasterClient();
            var masterPlayerIndex = masterClient.IsRealPlayer ? masterClient.AsIndex : int.MinValue;
            return ResolveRosterSlot(activePlayerIndices.ToArray(), player.AsIndex, masterPlayerIndex);
        }

        public static int ResolveRosterSlot(int[] activePlayerIndices, int playerIndex)
        {
            return ResolveRosterSlot(activePlayerIndices, playerIndex, int.MinValue);
        }

        public static int ResolveRosterSlot(
            int[] activePlayerIndices,
            int playerIndex,
            int authorityPlayerIndex)
        {
            var orderedIndices = BuildDeterministicRoster(activePlayerIndices, authorityPlayerIndex);
            return Array.IndexOf(orderedIndices, playerIndex);
        }

        public static int[] BuildDeterministicRoster(int[] activePlayerIndices, int authorityPlayerIndex)
        {
            if (activePlayerIndices == null || activePlayerIndices.Length == 0)
            {
                return new int[0];
            }

            var orderedIndices = new List<int>();
            for (var index = 0; index < activePlayerIndices.Length; index += 1)
            {
                if (!orderedIndices.Contains(activePlayerIndices[index]))
                {
                    orderedIndices.Add(activePlayerIndices[index]);
                }
            }

            orderedIndices.Sort();
            var authorityPosition = orderedIndices.IndexOf(authorityPlayerIndex);
            if (authorityPosition > 0)
            {
                orderedIndices.RemoveAt(authorityPosition);
                orderedIndices.Insert(0, authorityPlayerIndex);
            }

            return orderedIndices.ToArray();
        }

        public static bool ShouldReturnLastPlayerToLobby(int activePlayerCount)
        {
            return activePlayerCount == 1;
        }

        public static bool CanAcceptReplacement(int activePlayerCount)
        {
            return activePlayerCount >= 1 && activePlayerCount < MaximumPlayers;
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
            localAuthorityToken = string.Empty;
            credentialsRosterFingerprint = string.Empty;
            lastAuthoritySequence = 0;
            if (IsMasterClient)
            {
                EnsureAuthorityCredentials();
            }
            if (SceneManager.GetActiveScene().name == GameplaySceneName)
            {
                StartCoroutine(RebindGameplayAfterRosterChange());
            }
        }

        public void OnPlayerLeft(NetworkRunner networkRunner, PlayerRef player)
        {
            var remainingPlayers = GetRosterSize();
            StatusMessage = ShouldReturnLastPlayerToLobby(remainingPlayers)
                ? $"Only one player remains · room {ActiveRoomCode} stays open for a replacement."
                : $"Player left · {remainingPlayers}/{MaximumPlayers} remain. Room stays available for manual rejoin.";
            localAuthorityToken = string.Empty;
            credentialsRosterFingerprint = string.Empty;
            lastAuthoritySequence = 0;
            if (IsMasterClient)
            {
                EnsureAuthorityCredentials();
            }
            if (SceneManager.GetActiveScene().name == GameplaySceneName)
            {
                if (ShouldReturnLastPlayerToLobby(remainingPlayers))
                {
                    StartCoroutine(ReturnLastPlayerToLobbyAfterAuthorityTransfer());
                }
                else
                {
                    StartCoroutine(RebindGameplayAfterRosterChange());
                }
            }
        }

        private IEnumerator RebindGameplayAfterRosterChange()
        {
            for (var frame = 0; frame < 60; frame += 1)
            {
                if (!IsConnected || SceneManager.GetActiveScene().name != GameplaySceneName)
                {
                    yield break;
                }

                var masterClient = runner.GetMasterClient();
                if (masterClient.IsRealPlayer && PlayerSlotFor(masterClient) == 0)
                {
                    restartRoundAfterRosterChange = true;
                    BindGameplayScene();
                    yield break;
                }

                yield return null;
            }

            StatusMessage = "Roster changed, but Photon has not assigned the next round authority yet.";
        }

        private IEnumerator ReturnLastPlayerToLobbyAfterAuthorityTransfer()
        {
            for (var frame = 0; frame < 60; frame += 1)
            {
                if (!IsConnected || !ShouldReturnLastPlayerToLobby(GetRosterSize()))
                {
                    yield break;
                }

                if (IsMasterClient)
                {
                    EnsureAuthorityCredentials();
                }

                if (runner.IsSceneAuthority)
                {
                    runner.LoadScene(
                        "MainMenu",
                        LoadSceneMode.Single,
                        LocalPhysicsMode.None,
                        true);
                    yield break;
                }

                yield return null;
            }

            StatusMessage = "Only one player remains, but Photon has not transferred scene authority yet.";
        }

        public void OnShutdown(NetworkRunner networkRunner, ShutdownReason shutdownReason)
        {
            if (State == PrototypeNetworkRoomState.Connecting)
            {
                Debug.LogWarning($"Bang-Sak Photon shutdown while connecting: {shutdownReason}.");
                return;
            }

            if (!intentionalShutdown && State != PrototypeNetworkRoomState.Failed)
            {
                State = PrototypeNetworkRoomState.Failed;
                StatusMessage = $"Photon disconnected ({shutdownReason}). Rejoin room {ActiveRoomCode} from the menu.";
            }
        }

        public void OnDisconnectedFromServer(NetworkRunner networkRunner, NetDisconnectReason reason)
        {
            if (State == PrototypeNetworkRoomState.Connecting)
            {
                Debug.LogWarning($"Bang-Sak Photon disconnected while connecting: {reason}.");
                return;
            }

            if (!intentionalShutdown)
            {
                State = PrototypeNetworkRoomState.Failed;
                StatusMessage = $"Photon connection lost ({reason}). Rejoin room {ActiveRoomCode} from the menu.";
            }
        }

        public void OnConnectFailed(NetworkRunner networkRunner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            if (State == PrototypeNetworkRoomState.Connecting)
            {
                StatusMessage = $"Photon route failed ({reason}). Preparing retry...";
                Debug.LogWarning($"Bang-Sak Photon connect failed at {remoteAddress}: {reason}.");
                return;
            }

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
