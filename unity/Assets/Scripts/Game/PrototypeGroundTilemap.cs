using UnityEngine;
using UnityEngine.Tilemaps;

namespace Palengke.BangSak.Game
{
    [DisallowMultipleComponent]
    public sealed class PrototypeGroundTilemap : MonoBehaviour
    {
        public const int GroundSortingOrder = -20;

        [Header("Tile Sprites")]
        [SerializeField]
        private Sprite soilTileSprite;

        [SerializeField]
        private Sprite roadTileSprite;

        [SerializeField]
        private Sprite grassTileSprite;

        [SerializeField]
        private Sprite concreteTileSprite;

        [Header("Map")]
        [SerializeField]
        private Vector2Int mapSize = new Vector2Int(16, 12);

        [SerializeField]
        private int tilemapSortingOrder = GroundSortingOrder;

        [SerializeField]
        private bool buildOnAwake = true;

        private Grid grid;
        private Tilemap tilemap;
        private TilemapRenderer tilemapRenderer;

        public Vector2Int MapSize => mapSize;

        public int TilemapSortingOrder => tilemapSortingOrder;

        public bool HasRequiredSprites =>
            soilTileSprite != null
            && roadTileSprite != null
            && grassTileSprite != null
            && concreteTileSprite != null;

        public Tilemap Tilemap => tilemap;

        private void Awake()
        {
            if (buildOnAwake)
            {
                BuildGround();
            }
        }

        public void SetTileSprites(Sprite soil, Sprite road, Sprite grass, Sprite concrete)
        {
            soilTileSprite = soil;
            roadTileSprite = road;
            grassTileSprite = grass;
            concreteTileSprite = concrete;
        }

        public void SetMapSize(Vector2Int size)
        {
            mapSize = new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
        }

        public GroundTileKind ResolveTileKind(int x, int y)
        {
            var centerX = mapSize.x / 2;
            var centerY = mapSize.y / 2;

            if (Mathf.Abs(x - centerX) <= 1 || Mathf.Abs(y - centerY) <= 1)
            {
                return GroundTileKind.Road;
            }

            if (x >= centerX - 2 && x <= centerX + 2 && y >= centerY - 2 && y <= centerY + 2)
            {
                return GroundTileKind.Concrete;
            }

            if ((x < 3 && y > mapSize.y - 4) || (x > mapSize.x - 4 && y < 3) || ((x + y) % 7 == 0))
            {
                return GroundTileKind.Grass;
            }

            return GroundTileKind.Soil;
        }

        public int CountTiles(GroundTileKind kind)
        {
            var count = 0;
            for (var y = 0; y < mapSize.y; y += 1)
            {
                for (var x = 0; x < mapSize.x; x += 1)
                {
                    if (ResolveTileKind(x, y) == kind)
                    {
                        count += 1;
                    }
                }
            }

            return count;
        }

        public void BuildGround()
        {
            EnsureTilemap();

            if (tilemap == null || !HasRequiredSprites)
            {
                return;
            }

            tilemap.ClearAllTiles();

            for (var y = 0; y < mapSize.y; y += 1)
            {
                for (var x = 0; x < mapSize.x; x += 1)
                {
                    var kind = ResolveTileKind(x, y);
                    var cell = new Vector3Int(x - mapSize.x / 2, y - mapSize.y / 2, 0);
                    tilemap.SetTile(cell, CreateRuntimeTile(kind));
                }
            }

            tilemap.CompressBounds();
        }

        private void EnsureTilemap()
        {
            if (grid == null)
            {
                grid = GetComponent<Grid>();
                if (grid == null)
                {
                    grid = gameObject.AddComponent<Grid>();
                }
            }

            if (tilemap != null && tilemapRenderer != null)
            {
                tilemapRenderer.sortingOrder = tilemapSortingOrder;
                return;
            }

            var tilemapTransform = transform.Find("Ground Tilemap");
            GameObject tilemapObject;
            if (tilemapTransform == null)
            {
                tilemapObject = new GameObject("Ground Tilemap");
                tilemapObject.transform.SetParent(transform, false);
            }
            else
            {
                tilemapObject = tilemapTransform.gameObject;
            }

            tilemap = tilemapObject.GetComponent<Tilemap>();
            if (tilemap == null)
            {
                tilemap = tilemapObject.AddComponent<Tilemap>();
            }

            tilemapRenderer = tilemapObject.GetComponent<TilemapRenderer>();
            if (tilemapRenderer == null)
            {
                tilemapRenderer = tilemapObject.AddComponent<TilemapRenderer>();
            }

            tilemapRenderer.sortingOrder = tilemapSortingOrder;
        }

        private Tile CreateRuntimeTile(GroundTileKind kind)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = $"{kind} Ground Tile";
            tile.sprite = GetSprite(kind);
            tile.colliderType = Tile.ColliderType.None;
            return tile;
        }

        private Sprite GetSprite(GroundTileKind kind)
        {
            switch (kind)
            {
                case GroundTileKind.Road:
                    return roadTileSprite;
                case GroundTileKind.Grass:
                    return grassTileSprite;
                case GroundTileKind.Concrete:
                    return concreteTileSprite;
                default:
                    return soilTileSprite;
            }
        }
    }
}
