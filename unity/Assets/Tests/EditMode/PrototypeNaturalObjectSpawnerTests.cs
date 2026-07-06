using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Game;
using UnityEngine;

public sealed class PrototypeNaturalObjectSpawnerTests
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
    public void ComponentContract_ExposesVersionedMetadata()
    {
        owner = new GameObject("Natural Object Spawner Test");
        var spawner = owner.AddComponent<PrototypeNaturalObjectSpawner>();

        Assert.That(PrototypeNaturalObjectSpawner.ComponentId, Is.EqualTo("prototype_natural_object_spawner"));
        Assert.That(PrototypeNaturalObjectSpawner.ComponentVersion, Is.EqualTo(1));
        Assert.That(PrototypeNaturalObjectSpawner.ComponentVariant, Is.EqualTo("night_market_natural_placeholders"));
        Assert.That(spawner.ComponentIdValue, Is.EqualTo(PrototypeNaturalObjectSpawner.ComponentId));
        Assert.That(spawner.ComponentVersionValue, Is.EqualTo(PrototypeNaturalObjectSpawner.ComponentVersion));
        Assert.That(spawner.ComponentVariantValue, Is.EqualTo(PrototypeNaturalObjectSpawner.ComponentVariant));
    }

    [Test]
    public void GetPlacementCells_UsesGroundFutureObjectCells()
    {
        owner = new GameObject("Natural Object Spawner Test");
        var ground = owner.AddComponent<PrototypeGroundTilemap>();
        ground.SetMapSize(new Vector2Int(36, 26));
        ground.SetMapSeed(2795);

        var spawner = owner.AddComponent<PrototypeNaturalObjectSpawner>();
        spawner.SetGroundTilemap(ground);
        spawner.ConfigurePlacement(maxCount: 10, spacing: 4, padding: 3);

        var cells = spawner.GetPlacementCells();

        Assert.That(cells.Length, Is.EqualTo(10));
        Assert.That(cells, Is.EqualTo(ground.GetFutureObjectPlacementCells(10, 4, 3)));

        foreach (var cell in cells)
        {
            Assert.That(ground.IsValidFutureObjectCell(cell, 3), Is.True);
            Assert.That(ground.ResolveTileKind(cell.x, cell.y), Is.Not.EqualTo(GroundTileKind.Road));
        }
    }

    [Test]
    public void BuildObjects_CreatesSpriteRenderersAndColliders()
    {
        owner = new GameObject("Natural Object Spawner Test");
        var ground = owner.AddComponent<PrototypeGroundTilemap>();
        ground.SetMapSize(new Vector2Int(36, 26));
        ground.SetMapSeed(2795);

        var spawner = owner.AddComponent<PrototypeNaturalObjectSpawner>();
        spawner.SetGroundTilemap(ground);
        spawner.SetSprites(
            CreateSolidSprite(Color.green),
            CreateSolidSprite(Color.magenta),
            CreateSolidSprite(Color.red),
            CreateSolidSprite(Color.cyan));
        spawner.ConfigurePlacement(maxCount: 18, spacing: 4, padding: 3);

        var created = spawner.BuildObjects();

        Assert.That(created, Is.EqualTo(18));
        Assert.That(spawner.GeneratedRoot, Is.Not.Null);
        Assert.That(spawner.CountGeneratedObjects(), Is.EqualTo(18));

        var colliders = spawner.GeneratedRoot.GetComponentsInChildren<CircleCollider2D>();
        var renderers = spawner.GeneratedRoot.GetComponentsInChildren<SpriteRenderer>();

        Assert.That(renderers.Length, Is.EqualTo(18));
        Assert.That(renderers.All(renderer => renderer.sprite != null), Is.True);
        Assert.That(colliders.Length, Is.EqualTo(18));
        Assert.That(colliders.Any(collider => collider.isTrigger), Is.True);
        Assert.That(colliders.Any(collider => !collider.isTrigger), Is.True);
    }

    private static Sprite CreateSolidSprite(Color color)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.SetPixels(new[] { color, color, color, color });
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 2f);
    }
}
