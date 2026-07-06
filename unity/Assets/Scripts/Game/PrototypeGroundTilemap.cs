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
        private Vector2Int mapSize = new Vector2Int(36, 26);

        [SerializeField]
        private int mapSeed = 2795;

        [SerializeField]
        private int tilemapSortingOrder = GroundSortingOrder;

        [SerializeField]
        private bool buildOnAwake = true;

        private Grid grid;
        private Tilemap tilemap;
        private TilemapRenderer tilemapRenderer;

        public Vector2Int MapSize => mapSize;

        public int MapSeed => mapSeed;

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

        public void SetMapSeed(int seed)
        {
            mapSeed = seed;
        }

        public GroundTileKind ResolveTileKind(int x, int y)
        {
            var centerX = mapSize.x / 2;
            var centerY = mapSize.y / 2;
            var innerLeft = 2;
            var innerRight = mapSize.x - 3;
            var innerBottom = 2;
            var innerTop = mapSize.y - 3;

            var centralHorizontalLane = Mathf.Abs(y - centerY) <= 1 && x >= innerLeft && x <= innerRight;
            var centralVerticalLane = Mathf.Abs(x - centerX) <= 1 && y >= innerBottom && y <= innerTop;
            var lowerMarketLane = Mathf.Abs(y - (centerY - 7)) <= 0 && x >= 5 && x <= mapSize.x - 6;
            var softDiagonalShortcut = Mathf.Abs((x - centerX) + (y - centerY)) <= 1
                && x >= 7
                && x <= mapSize.x - 8
                && y >= 5
                && y <= mapSize.y - 6;

            if (centralHorizontalLane || centralVerticalLane || lowerMarketLane || softDiagonalShortcut)
            {
                return GroundTileKind.Road;
            }

            var centerMarketPad = x >= centerX - 2 && x <= centerX + 2 && y >= centerY - 2 && y <= centerY + 2;
            var leftMarketPad = x >= 5 && x <= 9 && y >= centerY - 4 && y <= centerY - 1;
            var rightMarketPad = x >= mapSize.x - 10 && x <= mapSize.x - 6 && y >= centerY + 1 && y <= centerY + 4;

            if (centerMarketPad || leftMarketPad || rightMarketPad)
            {
                return GroundTileKind.Concrete;
            }

            var upperLeftGarden = x < 5 && y > mapSize.y - 7;
            var lowerRightGarden = x > mapSize.x - 6 && y < 6;
            var softEdgeGrass = (x < 2 || x > mapSize.x - 3 || y < 2 || y > mapSize.y - 3)
                && ((x + y) % 3 != 0);
            var scatteredGrassPatch = HashCell(x, y, 41) % 37 == 0
                && x > 3
                && x < mapSize.x - 4
                && y > 3
                && y < mapSize.y - 4;

            if (upperLeftGarden || lowerRightGarden || softEdgeGrass || scatteredGrassPatch)
            {
                return GroundTileKind.Grass;
            }

            return GroundTileKind.Soil;
        }

        public bool IsValidFutureObjectCell(Vector2Int cell, int edgePadding = 2)
        {
            if (cell.x < edgePadding || cell.y < edgePadding || cell.x >= mapSize.x - edgePadding || cell.y >= mapSize.y - edgePadding)
            {
                return false;
            }

            var kind = ResolveTileKind(cell.x, cell.y);
            return kind == GroundTileKind.Soil || kind == GroundTileKind.Grass || kind == GroundTileKind.Concrete;
        }

        public Vector2Int[] GetFutureObjectPlacementCells(int maxCount, int minSpacing = 3, int edgePadding = 2)
        {
            if (maxCount <= 0)
            {
                return new Vector2Int[0];
            }

            minSpacing = Mathf.Max(1, minSpacing);
            edgePadding = Mathf.Max(0, edgePadding);

            var candidates = new System.Collections.Generic.List<Vector2Int>();

            for (var y = edgePadding; y < mapSize.y - edgePadding; y += 1)
            {
                for (var x = edgePadding; x < mapSize.x - edgePadding; x += 1)
                {
                    var cell = new Vector2Int(x, y);
                    if (!IsValidFutureObjectCell(cell, edgePadding))
                    {
                        continue;
                    }

                    if (HashCell(x, y, 97) % 5 != 0)
                    {
                        continue;
                    }

                    var tooClose = false;
                    foreach (var chosen in candidates)
                    {
                        if (Mathf.Abs(chosen.x - x) + Mathf.Abs(chosen.y - y) < minSpacing)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        candidates.Add(cell);
                    }
                }
            }

            candidates.Sort((a, b) => HashCell(a.x, a.y, 131).CompareTo(HashCell(b.x, b.y, 131)));

            if (candidates.Count > maxCount)
            {
                candidates.RemoveRange(maxCount, candidates.Count - maxCount);
            }

            return candidates.ToArray();
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

        private int HashCell(int x, int y, int salt)
        {
            unchecked
            {
                var hash = (x * 73856093) ^ (y * 19349663) ^ (mapSeed * 83492791) ^ (salt * 265443576);
                return hash == int.MinValue ? 0 : Mathf.Abs(hash);
            }
        }
    }
}
