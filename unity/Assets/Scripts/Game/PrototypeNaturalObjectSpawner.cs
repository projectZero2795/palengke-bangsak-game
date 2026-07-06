using UnityEngine;

namespace Palengke.BangSak.Game
{
    [DisallowMultipleComponent]
    public sealed class PrototypeNaturalObjectSpawner : MonoBehaviour
    {
        public const string ComponentId = "prototype_natural_object_spawner";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "night_market_natural_placeholders";
        public const int DefaultSortingOrder = 12;

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

        [Header("Natural Object Sprites")]
        [SerializeField]
        private Sprite treeSprite;

        [SerializeField]
        private Sprite bushSprite;

        [SerializeField]
        private Sprite plantPotSprite;

        [SerializeField]
        private Sprite plantClusterSprite;

        [Header("Placement")]
        [SerializeField]
        [Min(0)]
        private int maxObjects = 18;

        [SerializeField]
        [Min(1)]
        private int minSpacing = 4;

        [SerializeField]
        [Min(0)]
        private int edgePadding = 3;

        [SerializeField]
        private int objectSortingOrder = DefaultSortingOrder;

        [SerializeField]
        private bool buildOnAwake = true;

        private Transform generatedRoot;

        public string ComponentIdValue => componentId;

        public int ComponentVersionValue => componentVersion;

        public string ComponentVariantValue => componentVariant;

        public PrototypeGroundTilemap GroundTilemap => groundTilemap;

        public int MaxObjects => maxObjects;

        public int MinSpacing => minSpacing;

        public int EdgePadding => edgePadding;

        public int ObjectSortingOrder => objectSortingOrder;

        public Transform GeneratedRoot => generatedRoot;

        public bool HasRequiredSprites =>
            treeSprite != null
            && bushSprite != null
            && plantPotSprite != null
            && plantClusterSprite != null;

        public bool CanBuild => groundTilemap != null && HasRequiredSprites;

        private void Awake()
        {
            if (buildOnAwake)
            {
                BuildObjects();
            }
        }

        public void SetGroundTilemap(PrototypeGroundTilemap ground)
        {
            groundTilemap = ground;
        }

        public void SetSprites(Sprite tree, Sprite bush, Sprite plantPot, Sprite plantCluster)
        {
            treeSprite = tree;
            bushSprite = bush;
            plantPotSprite = plantPot;
            plantClusterSprite = plantCluster;
        }

        public void ConfigurePlacement(int maxCount, int spacing, int padding)
        {
            maxObjects = Mathf.Max(0, maxCount);
            minSpacing = Mathf.Max(1, spacing);
            edgePadding = Mathf.Max(0, padding);
        }

        public Vector2Int[] GetPlacementCells()
        {
            if (groundTilemap == null)
            {
                return new Vector2Int[0];
            }

            return groundTilemap.GetFutureObjectPlacementCells(maxObjects, minSpacing, edgePadding);
        }

        public NaturalObjectKind ResolveObjectKind(Vector2Int cell)
        {
            var hash = HashCell(cell.x, cell.y, 17);
            var roll = hash % 100;

            if (roll < 34)
            {
                return NaturalObjectKind.Tree;
            }

            if (roll < 67)
            {
                return NaturalObjectKind.Bush;
            }

            if (roll < 84)
            {
                return NaturalObjectKind.PlantPot;
            }

            return NaturalObjectKind.PlantCluster;
        }

        public Vector3 CellToWorldPosition(Vector2Int cell)
        {
            if (groundTilemap == null)
            {
                return Vector3.zero;
            }

            var mapSize = groundTilemap.MapSize;
            return new Vector3(
                cell.x - (mapSize.x / 2f) + 0.5f,
                cell.y - (mapSize.y / 2f) + 0.5f,
                0f);
        }

        public int BuildObjects()
        {
            EnsureGeneratedRoot();
            ClearGeneratedRoot();

            if (!CanBuild)
            {
                return 0;
            }

            var cells = GetPlacementCells();

            for (var i = 0; i < cells.Length; i += 1)
            {
                CreateNaturalObject(cells[i], i);
            }

            return cells.Length;
        }

        public int CountGeneratedObjects() =>
            generatedRoot == null ? 0 : generatedRoot.childCount;

        private void EnsureGeneratedRoot()
        {
            if (generatedRoot != null)
            {
                return;
            }

            var existing = transform.Find("Generated Natural Objects");
            if (existing != null)
            {
                generatedRoot = existing;
                return;
            }

            var root = new GameObject("Generated Natural Objects");
            root.transform.SetParent(transform, false);
            generatedRoot = root.transform;
        }

        private void ClearGeneratedRoot()
        {
            if (generatedRoot == null)
            {
                return;
            }

            for (var i = generatedRoot.childCount - 1; i >= 0; i -= 1)
            {
                var child = generatedRoot.GetChild(i);
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

        private void CreateNaturalObject(Vector2Int cell, int index)
        {
            var kind = ResolveObjectKind(cell);
            var instance = new GameObject($"{kind} Placeholder {index:00}");
            instance.transform.SetParent(generatedRoot, false);
            instance.transform.position = CellToWorldPosition(cell);
            instance.transform.localScale = GetScale(kind);

            var renderer = instance.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSprite(kind);
            renderer.sortingOrder = ResolveSortingOrder(cell, kind);

            AddCollider(instance, kind);
        }

        private int ResolveSortingOrder(Vector2Int cell, NaturalObjectKind kind)
        {
            var ySortOffset = groundTilemap == null ? 0 : groundTilemap.MapSize.y - cell.y;
            var kindOffset = kind == NaturalObjectKind.Tree || kind == NaturalObjectKind.PlantCluster ? 2 : 0;
            return objectSortingOrder + ySortOffset + kindOffset;
        }

        private Sprite GetSprite(NaturalObjectKind kind)
        {
            switch (kind)
            {
                case NaturalObjectKind.Bush:
                    return bushSprite;
                case NaturalObjectKind.PlantPot:
                    return plantPotSprite;
                case NaturalObjectKind.PlantCluster:
                    return plantClusterSprite;
                default:
                    return treeSprite;
            }
        }

        private Vector3 GetScale(NaturalObjectKind kind)
        {
            switch (kind)
            {
                case NaturalObjectKind.Bush:
                    return new Vector3(0.95f, 0.95f, 1f);
                case NaturalObjectKind.PlantPot:
                    return new Vector3(0.72f, 0.72f, 1f);
                case NaturalObjectKind.PlantCluster:
                    return new Vector3(1.15f, 1.15f, 1f);
                default:
                    return Vector3.one;
            }
        }

        private void AddCollider(GameObject instance, NaturalObjectKind kind)
        {
            var collider = instance.AddComponent<CircleCollider2D>();

            switch (kind)
            {
                case NaturalObjectKind.Bush:
                    collider.radius = 0.38f;
                    collider.offset = new Vector2(0f, -0.16f);
                    collider.isTrigger = true;
                    break;
                case NaturalObjectKind.PlantPot:
                    collider.radius = 0.18f;
                    collider.offset = new Vector2(0f, -0.22f);
                    break;
                case NaturalObjectKind.PlantCluster:
                    collider.radius = 0.42f;
                    collider.offset = new Vector2(0f, -0.18f);
                    collider.isTrigger = true;
                    break;
                default:
                    collider.radius = 0.28f;
                    collider.offset = new Vector2(0f, -0.28f);
                    break;
            }
        }

        private int HashCell(int x, int y, int salt)
        {
            var seed = groundTilemap == null ? 0 : groundTilemap.MapSeed;
            unchecked
            {
                var hash = (x * 73856093) ^ (y * 19349663) ^ (seed * 83492791) ^ (salt * 265443576);
                return hash == int.MinValue ? 0 : Mathf.Abs(hash);
            }
        }
    }
}
