using NUnit.Framework;
using Palengke.BangSak.Game;
using UnityEngine;
using UnityEngine.Tilemaps;

public sealed class PrototypeGroundTilemapTests
{
    private GameObject owner;

    [TearDown]
    public void TearDown()
    {
        if (owner != null)
        {
            Object.DestroyImmediate(owner);
        }
    }

    [Test]
    public void ResolveTileKind_ProducesAllRequiredGroundTypes()
    {
        owner = new GameObject("Ground Tilemap Test");
        var builder = owner.AddComponent<PrototypeGroundTilemap>();
        builder.SetMapSize(new Vector2Int(16, 12));

        Assert.That(builder.CountTiles(GroundTileKind.Soil), Is.GreaterThan(0));
        Assert.That(builder.CountTiles(GroundTileKind.Road), Is.GreaterThan(0));
        Assert.That(builder.CountTiles(GroundTileKind.Grass), Is.GreaterThan(0));
        Assert.That(builder.CountTiles(GroundTileKind.Concrete), Is.GreaterThan(0));
    }

    [Test]
    public void BuildGround_CreatesGridTilemapAndRenderer()
    {
        owner = new GameObject("Ground Tilemap Test");
        var builder = owner.AddComponent<PrototypeGroundTilemap>();
        builder.SetMapSize(new Vector2Int(8, 8));
        builder.SetTileSprites(
            CreateSolidSprite(Color.yellow),
            CreateSolidSprite(Color.gray),
            CreateSolidSprite(Color.green),
            CreateSolidSprite(Color.white));

        builder.BuildGround();

        Assert.That(owner.GetComponent<Grid>(), Is.Not.Null);
        Assert.That(builder.Tilemap, Is.Not.Null);
        Assert.That(owner.transform.Find("Ground Tilemap"), Is.Not.Null);

        var renderer = owner.GetComponentInChildren<TilemapRenderer>();
        Assert.That(renderer, Is.Not.Null);
        Assert.That(renderer.sortingOrder, Is.EqualTo(PrototypeGroundTilemap.GroundSortingOrder));
        Assert.That(builder.Tilemap.cellBounds.size.x, Is.GreaterThan(0));
        Assert.That(builder.Tilemap.cellBounds.size.y, Is.GreaterThan(0));
    }

    private static Sprite CreateSolidSprite(Color color)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.SetPixels(new[] { color, color, color, color });
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 2f);
    }
}
