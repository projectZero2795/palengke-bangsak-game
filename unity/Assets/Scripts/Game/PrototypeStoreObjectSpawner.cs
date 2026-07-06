using UnityEngine;

namespace Palengke.BangSak.Game
{
    [DisallowMultipleComponent]
    public sealed class PrototypeStoreObjectSpawner : MonoBehaviour
    {
        public const string ComponentId = "prototype_store_object_spawner";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "night_palengke_store_placeholders";
        public const int DefaultSortingOrder = 24;

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

        [Header("Store Object Sprites")]
        [SerializeField]
        private Sprite sariSariStoreSprite;

        [SerializeField]
        private Sprite palengkeStallSprite;

        [SerializeField]
        private Sprite foodStallSprite;

        [SerializeField]
        private Sprite signboardSariSprite;

        [SerializeField]
        private Sprite cratesBasketsSprite;

        [Header("Rendering")]
        [SerializeField]
        private int objectSortingOrder = DefaultSortingOrder;

        [SerializeField]
        private bool buildOnAwake = true;

        private Transform generatedRoot;

        public string ComponentIdValue => componentId;

        public int ComponentVersionValue => componentVersion;

        public string ComponentVariantValue => componentVariant;

        public PrototypeGroundTilemap GroundTilemap => groundTilemap;

        public int ObjectSortingOrder => objectSortingOrder;

        public Transform GeneratedRoot => generatedRoot;

        public bool HasRequiredSprites =>
            sariSariStoreSprite != null
            && palengkeStallSprite != null
            && foodStallSprite != null
            && signboardSariSprite != null
            && cratesBasketsSprite != null;

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

        public void SetSprites(Sprite sariSariStore, Sprite palengkeStall, Sprite foodStall, Sprite signboardSari, Sprite cratesBaskets)
        {
            sariSariStoreSprite = sariSariStore;
            palengkeStallSprite = palengkeStall;
            foodStallSprite = foodStall;
            signboardSariSprite = signboardSari;
            cratesBasketsSprite = cratesBaskets;
        }

        public StoreObjectSpec[] GetObjectSpecs()
        {
            if (groundTilemap == null)
            {
                return new StoreObjectSpec[0];
            }

            return BuildSpecs(groundTilemap.MapSize);
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

            var specs = GetObjectSpecs();

            for (var i = 0; i < specs.Length; i += 1)
            {
                CreateStoreObject(specs[i], i);
            }

            return specs.Length;
        }

        public int CountGeneratedObjects() =>
            generatedRoot == null ? 0 : generatedRoot.childCount;

        private StoreObjectSpec[] BuildSpecs(Vector2Int mapSize)
        {
            var centerX = mapSize.x / 2;
            var centerY = mapSize.y / 2;

            return new[]
            {
                Store(StoreObjectKind.SariSariStore, ClampCell(centerX - 10, centerY - 3, mapSize)),
                Store(StoreObjectKind.SariSariStore, ClampCell(centerX + 10, centerY + 3, mapSize)),
                Store(StoreObjectKind.PalengkeStall, ClampCell(centerX - 5, centerY + 3, mapSize)),
                Store(StoreObjectKind.PalengkeStall, ClampCell(centerX + 5, centerY - 3, mapSize)),
                Store(StoreObjectKind.FoodStall, ClampCell(centerX - 7, centerY - 4, mapSize)),
                Store(StoreObjectKind.FoodStall, ClampCell(centerX + 7, centerY + 4, mapSize)),
                Store(StoreObjectKind.SignboardSari, ClampCell(centerX - 3, centerY - 3, mapSize)),
                Store(StoreObjectKind.SignboardSari, ClampCell(centerX + 3, centerY + 3, mapSize)),
                Store(StoreObjectKind.CratesBaskets, ClampCell(centerX - 4, centerY - 4, mapSize)),
                Store(StoreObjectKind.CratesBaskets, ClampCell(centerX + 7, centerY - 5, mapSize)),
                Store(StoreObjectKind.CratesBaskets, ClampCell(centerX - 8, centerY + 5, mapSize)),
                Store(StoreObjectKind.CratesBaskets, ClampCell(centerX + 8, centerY - 2, mapSize))
            };
        }

        private Vector2Int ClampCell(int x, int y, Vector2Int mapSize) =>
            new Vector2Int(
                Mathf.Clamp(x, 3, mapSize.x - 4),
                Mathf.Clamp(y, 3, mapSize.y - 4));

        private StoreObjectSpec Store(StoreObjectKind kind, Vector2Int cell)
        {
            switch (kind)
            {
                case StoreObjectKind.PalengkeStall:
                    return new StoreObjectSpec(kind, cell, new Vector2(2.2f, 1.5f), new Vector2(0.72f, 0.24f), new Vector2(0f, -0.24f), false);
                case StoreObjectKind.FoodStall:
                    return new StoreObjectSpec(kind, cell, new Vector2(1.9f, 1.45f), new Vector2(0.62f, 0.22f), new Vector2(0f, -0.22f), false);
                case StoreObjectKind.SignboardSari:
                    return new StoreObjectSpec(kind, cell, new Vector2(0.85f, 0.75f), new Vector2(0.46f, 0.28f), new Vector2(0f, -0.18f), false);
                case StoreObjectKind.CratesBaskets:
                    return new StoreObjectSpec(kind, cell, new Vector2(0.9f, 0.75f), new Vector2(0.52f, 0.28f), new Vector2(0f, -0.14f), false);
                default:
                    return new StoreObjectSpec(kind, cell, new Vector2(2.4f, 1.9f), new Vector2(0.62f, 0.26f), new Vector2(0f, -0.24f), false);
            }
        }

        private void EnsureGeneratedRoot()
        {
            if (generatedRoot != null)
            {
                return;
            }

            var existing = transform.Find("Generated Store Objects");
            if (existing != null)
            {
                generatedRoot = existing;
                return;
            }

            var root = new GameObject("Generated Store Objects");
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

        private void CreateStoreObject(StoreObjectSpec spec, int index)
        {
            var instance = new GameObject($"{spec.Kind} Placeholder {index:00}");
            instance.transform.SetParent(generatedRoot, false);
            instance.transform.position = CellToWorldPosition(spec.Cell);
            instance.transform.localScale = new Vector3(spec.Scale.x, spec.Scale.y, 1f);

            var renderer = instance.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSprite(spec.Kind);
            renderer.sortingOrder = ResolveSortingOrder(spec);

            var collider = instance.AddComponent<BoxCollider2D>();
            collider.size = spec.ColliderSize;
            collider.offset = spec.ColliderOffset;
            collider.isTrigger = spec.IsTrigger;
        }

        private int ResolveSortingOrder(StoreObjectSpec spec)
        {
            var ySortOffset = groundTilemap == null ? 0 : groundTilemap.MapSize.y - spec.Cell.y;
            var kindOffset = spec.Kind == StoreObjectKind.SariSariStore || spec.Kind == StoreObjectKind.PalengkeStall ? 3 : 1;
            return objectSortingOrder + ySortOffset + kindOffset;
        }

        private Sprite GetSprite(StoreObjectKind kind)
        {
            switch (kind)
            {
                case StoreObjectKind.PalengkeStall:
                    return palengkeStallSprite;
                case StoreObjectKind.FoodStall:
                    return foodStallSprite;
                case StoreObjectKind.SignboardSari:
                    return signboardSariSprite;
                case StoreObjectKind.CratesBaskets:
                    return cratesBasketsSprite;
                default:
                    return sariSariStoreSprite;
            }
        }
    }

    public struct StoreObjectSpec
    {
        public StoreObjectSpec(
            StoreObjectKind kind,
            Vector2Int cell,
            Vector2 scale,
            Vector2 colliderSize,
            Vector2 colliderOffset,
            bool isTrigger)
        {
            Kind = kind;
            Cell = cell;
            Scale = scale;
            ColliderSize = colliderSize;
            ColliderOffset = colliderOffset;
            IsTrigger = isTrigger;
        }

        public StoreObjectKind Kind { get; }

        public Vector2Int Cell { get; }

        public Vector2 Scale { get; }

        public Vector2 ColliderSize { get; }

        public Vector2 ColliderOffset { get; }

        public bool IsTrigger { get; }
    }
}
