using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Game
{
    public enum PrototypeRoundState
    {
        Waiting = 0,
        Running = 1,
        Finished = 2
    }

    public enum PrototypeRoundResult
    {
        None = 0,
        TayaWins = 1,
        HidersWin = 2
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeRoundRulesController : MonoBehaviour
    {
        public const string ComponentId = "prototype_round_rules";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "phase21_local_round_rules";
        public const float DefaultRoundDurationSeconds = 150f;

        [Header("Component Contract")]
        [SerializeField]
        private string componentId = ComponentId;

        [SerializeField]
        private int componentVersion = ComponentVersion;

        [SerializeField]
        private string componentVariant = ComponentVariant;

        [Header("Round")]
        [SerializeField]
        [Min(1f)]
        private float roundDurationSeconds = DefaultRoundDurationSeconds;

        [SerializeField]
        private PrototypeRoundResult timerExpiredResult = PrototypeRoundResult.HidersWin;

        [SerializeField]
        private bool autoStartOnStart = true;

        [SerializeField]
        private bool freezeActorsWhenRoundEnds = true;

        [SerializeField]
        private bool allowKeyboardRestart = true;

        [SerializeField]
        private KeyCode restartKey = KeyCode.R;

        [Header("Map")]
        [SerializeField]
        private PrototypeMapLayoutController mapLayout;

        [SerializeField]
        private bool placeActorsAtMapSpawnsOnRoundStart = true;

        private PlayerRoleController[] roleControllers = new PlayerRoleController[0];
        private CaughtStateController[] caughtStates = new CaughtStateController[0];
        private TayaCounteredStateController[] tayaCounteredStates = new TayaCounteredStateController[0];
        private int[] tayaCounterBaselines = new int[0];
        private float roundStartedAt;
        private float roundEndsAt;

        public string ComponentIdValue => componentId;

        public int ComponentVersionValue => componentVersion;

        public string ComponentVariantValue => componentVariant;

        public float RoundDurationSeconds => roundDurationSeconds;

        public PrototypeRoundResult TimerExpiredResult => timerExpiredResult;

        public PrototypeMapLayoutController MapLayout => mapLayout;

        public PrototypeRoundState State { get; private set; } = PrototypeRoundState.Waiting;

        public PrototypeRoundResult Result { get; private set; } = PrototypeRoundResult.None;

        public string ResultTitle { get; private set; } = "Round ready";

        public string ResultMessage { get; private set; } = "Find all hiders, or use SAK to counter Taya.";

        public int TotalHiders { get; private set; }

        public int RemainingHiders { get; private set; }

        public float RemainingSeconds { get; private set; } = DefaultRoundDurationSeconds;

        public bool IsRunning => State == PrototypeRoundState.Running;

        public bool IsFinished => State == PrototypeRoundState.Finished;

        private void Start()
        {
            RefreshTrackedActors();
            if (autoStartOnStart)
            {
                StartRound(Time.time, true);
            }
        }

        private void Update()
        {
            if (IsRunning)
            {
                Tick(Time.time);
                return;
            }

            if (IsFinished && freezeActorsWhenRoundEnds)
            {
                DisableActorControls();
            }

            if (allowKeyboardRestart && IsFinished && Input.GetKeyDown(restartKey))
            {
                RestartRound();
            }
        }

        public void SetMapLayout(PrototypeMapLayoutController layout)
        {
            mapLayout = layout;
        }

        public void StartRound(float now, bool resetActors)
        {
            RefreshTrackedActors();

            if (resetActors)
            {
                ResetActorStates();
                PlaceActorsAtMapSpawns();
            }

            EnableActorControlsForRound();
            RefreshTrackedActors();
            CaptureTayaCounterBaselines();
            RecalculateHiderCounts();

            roundStartedAt = now;
            roundEndsAt = now + roundDurationSeconds;
            RemainingSeconds = roundDurationSeconds;
            State = PrototypeRoundState.Running;
            Result = PrototypeRoundResult.None;
            ResultTitle = "Round running";
            ResultMessage = "Taya catches all hiders. Hiders can counter with SAK.";
        }

        public void RestartRound()
        {
            StartRound(Time.time, true);
        }

        public void Tick(float now)
        {
            if (!IsRunning)
            {
                return;
            }

            RecalculateHiderCounts();
            RemainingSeconds = Mathf.Max(0f, roundEndsAt - now);

            if (DidAnyHiderCounterTayaThisRound())
            {
                FinishRound(PrototypeRoundResult.HidersWin, "Hiders win!", "A hider countered Taya with SAK.");
                return;
            }

            if (TotalHiders > 0 && RemainingHiders <= 0)
            {
                FinishRound(PrototypeRoundResult.TayaWins, "Taya wins!", "All hiders were caught.");
                return;
            }

            if (now >= roundEndsAt)
            {
                FinishRound(
                    timerExpiredResult,
                    timerExpiredResult == PrototypeRoundResult.TayaWins ? "Taya wins!" : "Hiders win!",
                    "Time is up.");
            }
        }

        public void RefreshTrackedActors()
        {
            roleControllers = FindObjectsOfType<PlayerRoleController>();
            caughtStates = FindObjectsOfType<CaughtStateController>();
            tayaCounteredStates = FindObjectsOfType<TayaCounteredStateController>();
        }

        public string FormatRemainingTime()
        {
            var seconds = Mathf.CeilToInt(Mathf.Max(0f, RemainingSeconds));
            return $"{seconds / 60:00}:{seconds % 60:00}";
        }

        private void FinishRound(PrototypeRoundResult result, string title, string message)
        {
            State = PrototypeRoundState.Finished;
            Result = result;
            ResultTitle = title;
            ResultMessage = message;
            RemainingSeconds = Mathf.Max(0f, RemainingSeconds);

            if (freezeActorsWhenRoundEnds)
            {
                DisableActorControls();
            }
        }

        private void RecalculateHiderCounts()
        {
            var total = 0;
            var remaining = 0;

            for (var index = 0; index < roleControllers.Length; index += 1)
            {
                var role = roleControllers[index];
                if (role == null || !role.IsHider)
                {
                    continue;
                }

                total += 1;
                var caught = role.GetComponent<CaughtStateController>();
                if (caught == null || !caught.IsCaught)
                {
                    remaining += 1;
                }
            }

            TotalHiders = total;
            RemainingHiders = remaining;
        }

        private bool DidAnyHiderCounterTayaThisRound()
        {
            for (var index = 0; index < tayaCounteredStates.Length; index += 1)
            {
                var tayaCountered = tayaCounteredStates[index];
                if (tayaCountered == null)
                {
                    continue;
                }

                var baseline = index < tayaCounterBaselines.Length ? tayaCounterBaselines[index] : 0;
                if (tayaCountered.CounteredCount > baseline)
                {
                    return true;
                }
            }

            return false;
        }

        private void CaptureTayaCounterBaselines()
        {
            tayaCounterBaselines = new int[tayaCounteredStates.Length];
            for (var index = 0; index < tayaCounteredStates.Length; index += 1)
            {
                tayaCounterBaselines[index] = tayaCounteredStates[index] == null ? 0 : tayaCounteredStates[index].CounteredCount;
            }
        }

        private void ResetActorStates()
        {
            for (var index = 0; index < caughtStates.Length; index += 1)
            {
                if (caughtStates[index] != null)
                {
                    caughtStates[index].ResetCaughtState();
                }
            }

            for (var index = 0; index < tayaCounteredStates.Length; index += 1)
            {
                if (tayaCounteredStates[index] != null)
                {
                    tayaCounteredStates[index].ResetCounteredState();
                }
            }
        }

        private void PlaceActorsAtMapSpawns()
        {
            if (!placeActorsAtMapSpawnsOnRoundStart || mapLayout == null)
            {
                return;
            }

            var tayaSpawn = mapLayout.GetTayaSpawnPoint();
            var hiderSpawns = mapLayout.GetHiderSpawnPoints();
            var hiderIndex = 0;
            var tayaIndex = 0;

            for (var index = 0; index < roleControllers.Length; index += 1)
            {
                var role = roleControllers[index];
                if (role == null)
                {
                    continue;
                }

                if (role.IsTaya)
                {
                    role.transform.position = new Vector3(tayaSpawn.Position.x + tayaIndex * 0.45f, tayaSpawn.Position.y, 0f);
                    tayaIndex += 1;
                    continue;
                }

                if (role.IsHider && hiderSpawns.Length > 0)
                {
                    var spawn = hiderSpawns[hiderIndex % hiderSpawns.Length];
                    var repeatOffset = hiderIndex / hiderSpawns.Length * 0.45f;
                    role.transform.position = new Vector3(spawn.Position.x + repeatOffset, spawn.Position.y, 0f);
                    hiderIndex += 1;
                }
            }

            Physics2D.SyncTransforms();
        }

        private void EnableActorControlsForRound()
        {
            for (var index = 0; index < roleControllers.Length; index += 1)
            {
                var role = roleControllers[index];
                if (role == null)
                {
                    continue;
                }

                var movement = role.GetComponent<PlayerMovementController>();
                if (movement != null)
                {
                    movement.enabled = true;
                }

                role.ApplyRoleNow();
            }
        }

        private void DisableActorControls()
        {
            for (var index = 0; index < roleControllers.Length; index += 1)
            {
                var role = roleControllers[index];
                if (role == null)
                {
                    continue;
                }

                var movement = role.GetComponent<PlayerMovementController>();
                if (movement != null)
                {
                    movement.SetExternalInput(Vector2.zero);
                    movement.enabled = false;
                }

                var bang = role.GetComponent<BangActionController>();
                if (bang != null)
                {
                    bang.enabled = false;
                }

                var sak = role.GetComponent<SakCounterController>();
                if (sak != null)
                {
                    sak.enabled = false;
                }
            }
        }
    }
}
