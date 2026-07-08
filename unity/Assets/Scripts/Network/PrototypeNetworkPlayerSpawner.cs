using System.Collections.Generic;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using Palengke.BangSak.UI;
using UnityEngine;

namespace Palengke.BangSak.Network
{
    [DisallowMultipleComponent]
    public sealed class PrototypeNetworkPlayerSpawner : MonoBehaviour
    {
        public const string ComponentId = "prototype_network_player_spawner";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "phase24_local_network_spawn_preview";
        public const int DefaultPreviewPlayerCount = 4;
        private const string GeneratedRootName = "Generated Network Players";

        [Header("Component Contract")]
        [SerializeField]
        private string componentId = ComponentId;

        [SerializeField]
        private int componentVersion = ComponentVersion;

        [SerializeField]
        private string componentVariant = ComponentVariant;

        [Header("Scene References")]
        [SerializeField]
        private PrototypeMapLayoutController mapLayout;

        [SerializeField]
        private PrototypeCameraFollowController cameraFollow;

        [SerializeField]
        private MobileJoystickPlaceholder mobileJoystick;

        [SerializeField]
        private GameObject playerPrefab;

        [Header("Local Preview")]
        [SerializeField]
        private bool spawnOnAwake = true;

        [SerializeField]
        private bool clearExistingGeneratedPlayers = true;

        [SerializeField]
        private bool disableLegacyPlayablePlayer = true;

        [SerializeField]
        private string legacyPlayablePlayerName = "Playable Player (Phase 3)";

        [SerializeField]
        [Min(2)]
        private int previewPlayerCount = DefaultPreviewPlayerCount;

        [SerializeField]
        [Min(0)]
        private int localPlayerIndex;

        [SerializeField]
        private string[] previewPlayerNames =
        {
            "JuanP",
            "Maria",
            "Pedro",
            "Ana"
        };

        private readonly List<GameObject> spawnedPlayers = new List<GameObject>();
        private Transform generatedRoot;

        public string ComponentIdValue => componentId;

        public int ComponentVersionValue => componentVersion;

        public string ComponentVariantValue => componentVariant;

        public PrototypeMapLayoutController MapLayout => mapLayout;

        public PrototypeCameraFollowController CameraFollow => cameraFollow;

        public MobileJoystickPlaceholder MobileJoystick => mobileJoystick;

        public GameObject PlayerPrefab => playerPrefab;

        public bool SpawnOnAwake => spawnOnAwake;

        public int PreviewPlayerCount => previewPlayerCount;

        public int LocalPlayerIndex => localPlayerIndex;

        public IReadOnlyList<GameObject> SpawnedPlayers => spawnedPlayers;

        public GameObject LastSpawnedLocalPlayer { get; private set; }

        private void Awake()
        {
            ResolveReferences();

            if (!spawnOnAwake)
            {
                return;
            }

            if (disableLegacyPlayablePlayer)
            {
                DisableLegacyPlayablePlayer();
            }

            SpawnPreviewPlayers();
        }

        public void SetReferences(
            PrototypeMapLayoutController layout,
            PrototypeCameraFollowController follow,
            MobileJoystickPlaceholder joystick,
            GameObject prefab)
        {
            mapLayout = layout;
            cameraFollow = follow;
            mobileJoystick = joystick;
            playerPrefab = prefab;
        }

        public void ConfigurePreview(string[] playerNames, int playerCount, int localIndex)
        {
            previewPlayerNames = playerNames ?? previewPlayerNames;
            previewPlayerCount = Mathf.Max(2, playerCount);
            localPlayerIndex = Mathf.Clamp(localIndex, 0, previewPlayerCount - 1);
        }

        public PrototypeNetworkPlayerDescriptor[] BuildPreviewRoster()
        {
            ResolveReferences();

            var totalPlayers = Mathf.Max(2, previewPlayerCount);
            var descriptors = new PrototypeNetworkPlayerDescriptor[totalPlayers];
            var tayaSpawn = mapLayout != null
                ? mapLayout.GetTayaSpawnPoint()
                : new PrototypeMapSpawnPoint(MapSpawnRole.Taya, 0, Vector2.zero, PlayerFacingDirection.Down);
            var hiderSpawns = mapLayout != null ? mapLayout.GetHiderSpawnPoints() : new PrototypeMapSpawnPoint[0];

            for (var index = 0; index < totalPlayers; index += 1)
            {
                var isTaya = index == 0;
                var spawnPoint = isTaya
                    ? tayaSpawn
                    : ResolveHiderSpawn(hiderSpawns, index - 1);
                var role = isTaya ? PlayerRole.Taya : PlayerRole.Hider;
                var name = ResolvePlayerName(index, role);

                descriptors[index] = new PrototypeNetworkPlayerDescriptor(
                    $"preview-{index:00}",
                    name,
                    role,
                    spawnPoint.SlotIndex,
                    index == Mathf.Clamp(localPlayerIndex, 0, totalPlayers - 1),
                    spawnPoint.Position,
                    spawnPoint.FacingDirection);
            }

            return descriptors;
        }

        public int SpawnPreviewPlayers()
        {
            ResolveReferences();
            if (playerPrefab == null)
            {
                return 0;
            }

            EnsureGeneratedRoot();
            if (clearExistingGeneratedPlayers)
            {
                ClearGeneratedPlayers();
            }

            var descriptors = BuildPreviewRoster();
            LastSpawnedLocalPlayer = null;

            for (var index = 0; index < descriptors.Length; index += 1)
            {
                var spawned = Instantiate(
                    playerPrefab,
                    new Vector3(descriptors[index].SpawnPosition.x, descriptors[index].SpawnPosition.y, 0f),
                    Quaternion.identity,
                    generatedRoot);
                spawned.name = $"{descriptors[index].DisplayName} Network Preview";
                ConfigureSpawnedPlayer(spawned, descriptors[index]);
                spawnedPlayers.Add(spawned);
            }

            if (LastSpawnedLocalPlayer != null)
            {
                var movement = LastSpawnedLocalPlayer.GetComponent<PlayerMovementController>();
                if (cameraFollow != null)
                {
                    cameraFollow.SetTarget(LastSpawnedLocalPlayer.transform);
                }

                if (mobileJoystick != null)
                {
                    mobileJoystick.SetTarget(movement);
                }
            }

            Physics2D.SyncTransforms();
            return spawnedPlayers.Count;
        }

        public void ClearGeneratedPlayers()
        {
            spawnedPlayers.Clear();
            LastSpawnedLocalPlayer = null;

            if (generatedRoot == null)
            {
                return;
            }

            for (var index = generatedRoot.childCount - 1; index >= 0; index -= 1)
            {
                DestroyObject(generatedRoot.GetChild(index).gameObject);
            }
        }

        private void ConfigureSpawnedPlayer(GameObject spawned, PrototypeNetworkPlayerDescriptor descriptor)
        {
            var identity = spawned.GetComponent<PrototypeNetworkPlayerIdentity>();
            if (identity == null)
            {
                identity = spawned.AddComponent<PrototypeNetworkPlayerIdentity>();
            }

            identity.Configure(descriptor);

            var nameIdentity = spawned.GetComponent<PlayerNameIdentity>();
            if (nameIdentity != null)
            {
                nameIdentity.SetDisplayName(descriptor.DisplayName);
            }

            var role = spawned.GetComponent<PlayerRoleController>();
            if (role != null)
            {
                role.SetRole(descriptor.Role);
            }

            EnsureRuntimeMovement(spawned, descriptor.IsLocalPlayer);
            ApplyFacingDirection(spawned, descriptor.FacingDirection);

            if (descriptor.IsLocalPlayer)
            {
                LastSpawnedLocalPlayer = spawned;
            }
        }

        private static void EnsureRuntimeMovement(GameObject spawned, bool isLocalPlayer)
        {
            if (spawned.GetComponent<Rigidbody2D>() == null)
            {
                spawned.AddComponent<Rigidbody2D>();
            }

            var movement = spawned.GetComponent<PlayerMovementController>();
            if (movement == null)
            {
                movement = spawned.AddComponent<PlayerMovementController>();
            }

            movement.SetKeyboardInputEnabled(isLocalPlayer);
            if (!isLocalPlayer)
            {
                movement.SetExternalInput(Vector2.zero);
            }
        }

        private static void ApplyFacingDirection(GameObject spawned, PlayerFacingDirection facingDirection)
        {
            var animation = spawned.GetComponent<PlayerAnimationController>();
            if (animation != null)
            {
                animation.ApplyAnimation(ToMovementVector(facingDirection), 1f);
            }

            var bang = spawned.GetComponent<BangActionController>();
            if (bang != null)
            {
                bang.SetFallbackFacingDirection(facingDirection);
            }

            var sak = spawned.GetComponent<SakCounterController>();
            if (sak != null)
            {
                sak.SetFallbackFacingDirection(facingDirection);
            }
        }

        private void ResolveReferences()
        {
            if (mapLayout == null)
            {
                mapLayout = FindObjectOfType<PrototypeMapLayoutController>();
            }

            if (cameraFollow == null)
            {
                cameraFollow = FindObjectOfType<PrototypeCameraFollowController>();
            }

            if (mobileJoystick == null)
            {
                mobileJoystick = FindObjectOfType<MobileJoystickPlaceholder>();
            }
        }

        private void EnsureGeneratedRoot()
        {
            if (generatedRoot != null)
            {
                return;
            }

            var existing = transform.Find(GeneratedRootName);
            if (existing != null)
            {
                generatedRoot = existing;
                return;
            }

            var root = new GameObject(GeneratedRootName);
            root.transform.SetParent(transform, false);
            generatedRoot = root.transform;
        }

        private void DisableLegacyPlayablePlayer()
        {
            if (string.IsNullOrWhiteSpace(legacyPlayablePlayerName))
            {
                return;
            }

            var legacy = GameObject.Find(legacyPlayablePlayerName);
            if (legacy == null)
            {
                return;
            }

            if (generatedRoot != null && legacy.transform.IsChildOf(generatedRoot))
            {
                return;
            }

            legacy.SetActive(false);
        }

        private string ResolvePlayerName(int index, PlayerRole role)
        {
            if (previewPlayerNames != null && index < previewPlayerNames.Length && !string.IsNullOrWhiteSpace(previewPlayerNames[index]))
            {
                return previewPlayerNames[index].Trim();
            }

            return role == PlayerRole.Taya ? "Taya" : $"Hider {index}";
        }

        private static PrototypeMapSpawnPoint ResolveHiderSpawn(PrototypeMapSpawnPoint[] hiderSpawns, int hiderIndex)
        {
            if (hiderSpawns == null || hiderSpawns.Length == 0)
            {
                return new PrototypeMapSpawnPoint(
                    MapSpawnRole.Hider,
                    hiderIndex,
                    new Vector2(1.5f + hiderIndex, 0f),
                    PlayerFacingDirection.Down);
            }

            return hiderSpawns[hiderIndex % hiderSpawns.Length];
        }

        private static Vector2 ToMovementVector(PlayerFacingDirection facingDirection)
        {
            switch (facingDirection)
            {
                case PlayerFacingDirection.Up:
                    return Vector2.up;
                case PlayerFacingDirection.UpRight:
                    return new Vector2(1f, 1f).normalized;
                case PlayerFacingDirection.Right:
                    return Vector2.right;
                case PlayerFacingDirection.DownRight:
                    return new Vector2(1f, -1f).normalized;
                case PlayerFacingDirection.Down:
                    return Vector2.down;
                case PlayerFacingDirection.DownLeft:
                    return new Vector2(-1f, -1f).normalized;
                case PlayerFacingDirection.Left:
                    return Vector2.left;
                case PlayerFacingDirection.UpLeft:
                    return new Vector2(-1f, 1f).normalized;
                default:
                    return Vector2.down;
            }
        }

        private static void DestroyObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
