using Palengke.BangSak.Player;
using UnityEngine;

namespace Palengke.BangSak.Game
{
    [DisallowMultipleComponent]
    public sealed class PrototypeMapLayoutController : MonoBehaviour
    {
        public const string ComponentId = "prototype_map_layout";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "night_barangay_palengke_v1";
        public const int DefaultMinimumHiderSpawnCount = 4;

        [Header("Component Contract")]
        [SerializeField]
        private string componentId = ComponentId;

        [SerializeField]
        private int componentVersion = ComponentVersion;

        [SerializeField]
        private string componentVariant = ComponentVariant;

        [Header("Dependencies")]
        [SerializeField]
        private PrototypeGroundTilemap groundTilemap;

        [Header("Bounds")]
        [SerializeField]
        private Vector2 mapWorldSize = new Vector2(52f, 36f);

        [SerializeField]
        private Vector2 cameraBoundsCenter = Vector2.zero;

        [SerializeField]
        private Vector2 cameraBoundsSize = new Vector2(50f, 34f);

        [Header("Taya Spawn")]
        [SerializeField]
        private Vector2 tayaSpawnPosition = new Vector2(0f, -6f);

        [SerializeField]
        private PlayerFacingDirection tayaFacingDirection = PlayerFacingDirection.Up;

        [Header("Hider Spawns")]
        [SerializeField]
        private Vector2[] hiderSpawnPositions =
        {
            new Vector2(-20f, -12f),
            new Vector2(20f, -11f),
            new Vector2(-21f, 11f),
            new Vector2(21f, 12f),
            new Vector2(-8f, 14f),
            new Vector2(10f, -14f)
        };

        [SerializeField]
        private PlayerFacingDirection[] hiderFacingDirections =
        {
            PlayerFacingDirection.UpRight,
            PlayerFacingDirection.UpLeft,
            PlayerFacingDirection.DownRight,
            PlayerFacingDirection.DownLeft,
            PlayerFacingDirection.Down,
            PlayerFacingDirection.Up
        };

        [Header("Validation")]
        [SerializeField]
        [Min(1)]
        private int minimumHiderSpawnCount = DefaultMinimumHiderSpawnCount;

        [SerializeField]
        [Min(0f)]
        private float minimumSpawnSeparation = 6f;

        [SerializeField]
        private bool useNightPalengkeDefaultLayout = true;

        [Header("Review Spawn Markers")]
        [SerializeField]
        private bool showSpawnMarkers = true;

        [SerializeField]
        private Sprite spawnMarkerSprite;

        [SerializeField]
        private int spawnMarkerSortingOrder = 85;

        [SerializeField]
        [Min(0.1f)]
        private float tayaSpawnMarkerScale = 0.95f;

        [SerializeField]
        [Min(0.1f)]
        private float hiderSpawnMarkerScale = 0.78f;

        [SerializeField]
        private Color tayaSpawnMarkerColor = new Color(1f, 0.32f, 0.18f, 0.82f);

        [SerializeField]
        private Color hiderSpawnMarkerColor = new Color(0.28f, 0.72f, 1f, 0.72f);

        [SerializeField]
        private Color spawnMarkerLabelColor = new Color(1f, 1f, 1f, 0.92f);

        private Transform generatedSpawnMarkerRoot;

        public string ComponentIdValue => componentId;

        public int ComponentVersionValue => componentVersion;

        public string ComponentVariantValue => componentVariant;

        public PrototypeGroundTilemap GroundTilemap => groundTilemap;

        public Vector2 MapWorldSize => mapWorldSize;

        public Bounds MapBounds => new Bounds(Vector3.zero, new Vector3(Mathf.Max(0f, mapWorldSize.x), Mathf.Max(0f, mapWorldSize.y), 0f));

        public Vector2 CameraBoundsCenter => cameraBoundsCenter;

        public Vector2 CameraBoundsSize => cameraBoundsSize;

        public Bounds CameraBounds => new Bounds(new Vector3(cameraBoundsCenter.x, cameraBoundsCenter.y, 0f), new Vector3(Mathf.Max(0f, cameraBoundsSize.x), Mathf.Max(0f, cameraBoundsSize.y), 0f));

        public float MinimumSpawnSeparation => minimumSpawnSeparation;

        public bool UseNightPalengkeDefaultLayout => useNightPalengkeDefaultLayout;

        public bool ShowSpawnMarkers => showSpawnMarkers;

        public bool HasSpawnMarkerReviewVisuals => showSpawnMarkers && spawnMarkerSprite != null;

        public bool HasPlayableSpawnLayout => string.IsNullOrEmpty(GetFirstValidationIssue());

        private void Awake()
        {
            if (showSpawnMarkers)
            {
                BuildSpawnMarkers();
            }
        }

        public void SetGroundTilemap(PrototypeGroundTilemap ground)
        {
            groundTilemap = ground;
        }

        public PrototypeMapSpawnPoint GetTayaSpawnPoint() =>
            new PrototypeMapSpawnPoint(MapSpawnRole.Taya, 0, tayaSpawnPosition, tayaFacingDirection);

        public PrototypeMapSpawnPoint[] GetHiderSpawnPoints()
        {
            if (hiderSpawnPositions == null)
            {
                return new PrototypeMapSpawnPoint[0];
            }

            var spawnPoints = new PrototypeMapSpawnPoint[hiderSpawnPositions.Length];
            for (var i = 0; i < hiderSpawnPositions.Length; i += 1)
            {
                spawnPoints[i] = new PrototypeMapSpawnPoint(MapSpawnRole.Hider, i, hiderSpawnPositions[i], ResolveHiderFacingDirection(i));
            }

            return spawnPoints;
        }

        public PrototypeMapSpawnPoint[] GetAllSpawnPoints()
        {
            var hiders = GetHiderSpawnPoints();
            var spawnPoints = new PrototypeMapSpawnPoint[hiders.Length + 1];
            spawnPoints[0] = GetTayaSpawnPoint();

            for (var i = 0; i < hiders.Length; i += 1)
            {
                spawnPoints[i + 1] = hiders[i];
            }

            return spawnPoints;
        }

        public int GetSpawnPointCount(MapSpawnRole role) =>
            role == MapSpawnRole.Taya ? 1 : GetHiderSpawnPoints().Length;

        public void SetSpawnMarkerSprite(Sprite markerSprite)
        {
            spawnMarkerSprite = markerSprite;
        }

        public int BuildSpawnMarkers()
        {
            EnsureGeneratedSpawnMarkerRoot();
            ClearGeneratedSpawnMarkerRoot();

            if (!HasSpawnMarkerReviewVisuals)
            {
                return 0;
            }

            var count = 0;
            CreateSpawnMarker(GetTayaSpawnPoint());
            count += 1;

            var hiders = GetHiderSpawnPoints();
            for (var i = 0; i < hiders.Length; i += 1)
            {
                CreateSpawnMarker(hiders[i]);
                count += 1;
            }

            return count;
        }

        public int CountGeneratedSpawnMarkers() =>
            generatedSpawnMarkerRoot == null ? 0 : generatedSpawnMarkerRoot.childCount;

        public bool IsInsideMapBounds(Vector2 position) =>
            IsInsideBounds2D(position, MapBounds);

        public bool IsInsideCameraBounds(Vector2 position) =>
            IsInsideBounds2D(position, CameraBounds);

        public string GetFirstValidationIssue()
        {
            if (componentId != ComponentId)
            {
                return $"Expected component id '{ComponentId}' but found '{componentId}'.";
            }

            if (componentVersion < ComponentVersion)
            {
                return $"Component version {componentVersion} is older than required version {ComponentVersion}.";
            }

            if (mapWorldSize.x <= 0f || mapWorldSize.y <= 0f)
            {
                return "Map world size must be positive.";
            }

            if (cameraBoundsSize.x <= 0f || cameraBoundsSize.y <= 0f)
            {
                return "Camera bounds size must be positive.";
            }

            if (cameraBoundsSize.x > mapWorldSize.x || cameraBoundsSize.y > mapWorldSize.y)
            {
                return "Camera bounds must fit inside the map world size.";
            }

            if (!IsInsideMapBounds(tayaSpawnPosition) || !IsInsideCameraBounds(tayaSpawnPosition))
            {
                return "Taya spawn must be inside both map and camera bounds.";
            }

            if (hiderSpawnPositions == null || hiderSpawnPositions.Length < minimumHiderSpawnCount)
            {
                return $"Map needs at least {minimumHiderSpawnCount} hider spawns.";
            }

            for (var i = 0; i < hiderSpawnPositions.Length; i += 1)
            {
                var hiderPosition = hiderSpawnPositions[i];
                if (!IsInsideMapBounds(hiderPosition) || !IsInsideCameraBounds(hiderPosition))
                {
                    return $"Hider spawn {i} must be inside both map and camera bounds.";
                }

                if (Vector2.Distance(tayaSpawnPosition, hiderPosition) < minimumSpawnSeparation)
                {
                    return $"Hider spawn {i} is too close to Taya spawn.";
                }

                for (var other = i + 1; other < hiderSpawnPositions.Length; other += 1)
                {
                    if (Vector2.Distance(hiderPosition, hiderSpawnPositions[other]) < minimumSpawnSeparation * 0.75f)
                    {
                        return $"Hider spawn {i} is too close to hider spawn {other}.";
                    }
                }
            }

            return string.Empty;
        }

        public void ConfigureForTests(
            Vector2 testMapWorldSize,
            Vector2 testCameraBoundsCenter,
            Vector2 testCameraBoundsSize,
            Vector2 testTayaSpawnPosition,
            Vector2[] testHiderSpawnPositions,
            float testMinimumSpawnSeparation)
        {
            mapWorldSize = testMapWorldSize;
            cameraBoundsCenter = testCameraBoundsCenter;
            cameraBoundsSize = testCameraBoundsSize;
            tayaSpawnPosition = testTayaSpawnPosition;
            hiderSpawnPositions = testHiderSpawnPositions ?? new Vector2[0];
            minimumSpawnSeparation = Mathf.Max(0f, testMinimumSpawnSeparation);
        }

        private PlayerFacingDirection ResolveHiderFacingDirection(int index)
        {
            if (hiderFacingDirections != null && index >= 0 && index < hiderFacingDirections.Length)
            {
                return hiderFacingDirections[index];
            }

            return PlayerFacingDirection.Down;
        }

        private void EnsureGeneratedSpawnMarkerRoot()
        {
            if (generatedSpawnMarkerRoot != null)
            {
                return;
            }

            var existing = transform.Find("Generated Spawn Markers");
            if (existing != null)
            {
                generatedSpawnMarkerRoot = existing;
                return;
            }

            var root = new GameObject("Generated Spawn Markers");
            root.transform.SetParent(transform, false);
            generatedSpawnMarkerRoot = root.transform;
        }

        private void ClearGeneratedSpawnMarkerRoot()
        {
            if (generatedSpawnMarkerRoot == null)
            {
                return;
            }

            for (var i = generatedSpawnMarkerRoot.childCount - 1; i >= 0; i -= 1)
            {
                var child = generatedSpawnMarkerRoot.GetChild(i);
                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private void CreateSpawnMarker(PrototypeMapSpawnPoint spawnPoint)
        {
            var marker = new GameObject(GetSpawnMarkerName(spawnPoint));
            marker.transform.SetParent(generatedSpawnMarkerRoot, false);
            marker.transform.position = new Vector3(spawnPoint.Position.x, spawnPoint.Position.y, 0f);

            var markerScale = spawnPoint.Role == MapSpawnRole.Taya ? tayaSpawnMarkerScale : hiderSpawnMarkerScale;
            marker.transform.localScale = new Vector3(markerScale, markerScale, 1f);

            var renderer = marker.AddComponent<SpriteRenderer>();
            renderer.sprite = spawnMarkerSprite;
            renderer.color = spawnPoint.Role == MapSpawnRole.Taya ? tayaSpawnMarkerColor : hiderSpawnMarkerColor;
            renderer.sortingOrder = spawnMarkerSortingOrder;

            CreateSpawnMarkerLabel(marker.transform, spawnPoint);
        }

        private void CreateSpawnMarkerLabel(Transform marker, PrototypeMapSpawnPoint spawnPoint)
        {
            var label = new GameObject("Review Label");
            label.transform.SetParent(marker, false);
            label.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            label.transform.localScale = Vector3.one;

            var text = label.AddComponent<TextMesh>();
            text.text = spawnPoint.Role == MapSpawnRole.Taya ? "TAYA" : $"H{spawnPoint.SlotIndex + 1}";
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.characterSize = 0.14f;
            text.fontSize = 24;
            text.color = spawnMarkerLabelColor;

            var labelRenderer = label.GetComponent<MeshRenderer>();
            labelRenderer.sortingOrder = spawnMarkerSortingOrder + 1;
        }

        private static string GetSpawnMarkerName(PrototypeMapSpawnPoint spawnPoint) =>
            spawnPoint.Role == MapSpawnRole.Taya
                ? "Taya Spawn Marker"
                : $"Hider Spawn Marker {spawnPoint.SlotIndex + 1:00}";

        private static bool IsInsideBounds2D(Vector2 position, Bounds bounds) =>
            position.x >= bounds.min.x
            && position.x <= bounds.max.x
            && position.y >= bounds.min.y
            && position.y <= bounds.max.y;
    }
}
