using UnityEngine;

namespace Palengke.BangSak.Game
{
    [DisallowMultipleComponent]
    public sealed class PrototypeResidentialObjectSpawner : MonoBehaviour
    {
        public const string ComponentId = "prototype_residential_object_spawner";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "night_barangay_residential_placeholders";
        public const int DefaultSortingOrder = 20;

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

        [Header("Residential Object Sprites")]
        [SerializeField]
        private Sprite smallHouseSprite;

        [SerializeField]
        private Sprite mediumHouseSprite;

        [SerializeField]
        private Sprite fenceHorizontalSprite;

        [SerializeField]
        private Sprite fenceVerticalSprite;

        [SerializeField]
        private Sprite gateSprite;

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
            smallHouseSprite != null
            && mediumHouseSprite != null
            && fenceHorizontalSprite != null
            && fenceVerticalSprite != null
            && gateSprite != null;

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

        public void SetSprites(Sprite smallHouse, Sprite mediumHouse, Sprite fenceHorizontal, Sprite fenceVertical, Sprite gate)
        {
            smallHouseSprite = smallHouse;
            mediumHouseSprite = mediumHouse;
            fenceHorizontalSprite = fenceHorizontal;
            fenceVerticalSprite = fenceVertical;
            gateSprite = gate;
        }

        public ResidentialObjectSpec[] GetObjectSpecs()
        {
            if (groundTilemap == null)
            {
                return new ResidentialObjectSpec[0];
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
                CreateResidentialObject(specs[i], i);
            }

            return specs.Length;
        }

        public int CountGeneratedObjects() =>
            generatedRoot == null ? 0 : generatedRoot.childCount;

        private ResidentialObjectSpec[] BuildSpecs(Vector2Int mapSize)
        {
            var leftX = Mathf.Clamp(mapSize.x / 6, 4, mapSize.x - 5);
            var rightX = Mathf.Clamp(mapSize.x - mapSize.x / 6, 5, mapSize.x - 4);
            var topY = Mathf.Clamp(mapSize.y - 6, 5, mapSize.y - 4);
            var bottomY = Mathf.Clamp(5, 3, mapSize.y - 5);
            var topLeftFenceX = Mathf.Clamp(leftX + 7, 4, mapSize.x - 5);
            var topRightFenceX = Mathf.Clamp(rightX - 5, 4, mapSize.x - 5);
            var bottomLeftFenceX = Mathf.Clamp(leftX + 5, 4, mapSize.x - 5);
            var bottomRightFenceX = Mathf.Clamp(rightX - 6, 4, mapSize.x - 5);
            var leftFenceX = Mathf.Clamp(4, 2, mapSize.x - 3);
            var rightFenceX = Mathf.Clamp(mapSize.x - 4, 2, mapSize.x - 3);
            var upperFenceY = Mathf.Clamp(topY - 4, 3, mapSize.y - 4);
            var lowerFenceY = Mathf.Clamp(bottomY + 4, 3, mapSize.y - 4);

            return new[]
            {
                House(ResidentialObjectKind.SmallHouse, new Vector2Int(leftX, topY)),
                House(ResidentialObjectKind.SmallHouse, new Vector2Int(rightX, topY - 1)),
                House(ResidentialObjectKind.MediumHouse, new Vector2Int(leftX + 1, bottomY)),
                House(ResidentialObjectKind.MediumHouse, new Vector2Int(rightX - 1, bottomY)),
                Fence(ResidentialObjectKind.FenceHorizontal, new Vector2Int(topLeftFenceX, topY)),
                Fence(ResidentialObjectKind.FenceHorizontal, new Vector2Int(topRightFenceX, topY)),
                Fence(ResidentialObjectKind.FenceHorizontal, new Vector2Int(bottomLeftFenceX, bottomY)),
                Fence(ResidentialObjectKind.FenceHorizontal, new Vector2Int(bottomRightFenceX, bottomY)),
                Fence(ResidentialObjectKind.FenceVertical, new Vector2Int(leftFenceX, upperFenceY)),
                Fence(ResidentialObjectKind.FenceVertical, new Vector2Int(leftFenceX, lowerFenceY)),
                Fence(ResidentialObjectKind.FenceVertical, new Vector2Int(rightFenceX, upperFenceY)),
                Fence(ResidentialObjectKind.FenceVertical, new Vector2Int(rightFenceX, lowerFenceY)),
                Gate(new Vector2Int(leftX + 9, topY)),
                Gate(new Vector2Int(rightX - 9, bottomY))
            };
        }

        private ResidentialObjectSpec House(ResidentialObjectKind kind, Vector2Int cell)
        {
            if (kind == ResidentialObjectKind.MediumHouse)
            {
                return new ResidentialObjectSpec(
                    kind,
                    cell,
                    new Vector2(3.05f, 2.3f),
                    new Vector2(2.15f, 0.92f),
                    new Vector2(0f, -0.45f),
                    false);
            }

            return new ResidentialObjectSpec(
                kind,
                cell,
                new Vector2(2.45f, 2.0f),
                new Vector2(1.55f, 0.82f),
                new Vector2(0f, -0.42f),
                false);
        }

        private ResidentialObjectSpec Fence(ResidentialObjectKind kind, Vector2Int cell)
        {
            if (kind == ResidentialObjectKind.FenceVertical)
            {
                return new ResidentialObjectSpec(
                    kind,
                    cell,
                    new Vector2(0.72f, 1.8f),
                    new Vector2(0.24f, 1.42f),
                    Vector2.zero,
                    false);
            }

            return new ResidentialObjectSpec(
                kind,
                cell,
                new Vector2(2.0f, 0.72f),
                new Vector2(1.82f, 0.24f),
                Vector2.zero,
                false);
        }

        private ResidentialObjectSpec Gate(Vector2Int cell) =>
            new ResidentialObjectSpec(
                ResidentialObjectKind.Gate,
                cell,
                new Vector2(1.4f, 0.78f),
                new Vector2(1.1f, 0.22f),
                Vector2.zero,
                false);

        private void EnsureGeneratedRoot()
        {
            if (generatedRoot != null)
            {
                return;
            }

            var existing = transform.Find("Generated Residential Objects");
            if (existing != null)
            {
                generatedRoot = existing;
                return;
            }

            var root = new GameObject("Generated Residential Objects");
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

        private void CreateResidentialObject(ResidentialObjectSpec spec, int index)
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

        private int ResolveSortingOrder(ResidentialObjectSpec spec)
        {
            var ySortOffset = groundTilemap == null ? 0 : groundTilemap.MapSize.y - spec.Cell.y;
            var kindOffset = spec.Kind == ResidentialObjectKind.SmallHouse || spec.Kind == ResidentialObjectKind.MediumHouse ? 4 : 1;
            return objectSortingOrder + ySortOffset + kindOffset;
        }

        private Sprite GetSprite(ResidentialObjectKind kind)
        {
            switch (kind)
            {
                case ResidentialObjectKind.MediumHouse:
                    return mediumHouseSprite;
                case ResidentialObjectKind.FenceHorizontal:
                    return fenceHorizontalSprite;
                case ResidentialObjectKind.FenceVertical:
                    return fenceVerticalSprite;
                case ResidentialObjectKind.Gate:
                    return gateSprite;
                default:
                    return smallHouseSprite;
            }
        }
    }

    public struct ResidentialObjectSpec
    {
        public ResidentialObjectSpec(
            ResidentialObjectKind kind,
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

        public ResidentialObjectKind Kind { get; }

        public Vector2Int Cell { get; }

        public Vector2 Scale { get; }

        public Vector2 ColliderSize { get; }

        public Vector2 ColliderOffset { get; }

        public bool IsTrigger { get; }
    }
}
