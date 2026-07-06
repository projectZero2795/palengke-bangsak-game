using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Game;
using UnityEngine;

public sealed class PrototypeStoreObjectSpawnerTests
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
        owner = new GameObject("Store Object Spawner Test");
        var spawner = owner.AddComponent<PrototypeStoreObjectSpawner>();

        Assert.That(PrototypeStoreObjectSpawner.ComponentId, Is.EqualTo("prototype_store_object_spawner"));
        Assert.That(PrototypeStoreObjectSpawner.ComponentVersion, Is.EqualTo(1));
        Assert.That(PrototypeStoreObjectSpawner.ComponentVariant, Is.EqualTo("night_palengke_store_placeholders"));
        Assert.That(spawner.ComponentIdValue, Is.EqualTo(PrototypeStoreObjectSpawner.ComponentId));
        Assert.That(spawner.ComponentVersionValue, Is.EqualTo(PrototypeStoreObjectSpawner.ComponentVersion));
        Assert.That(spawner.ComponentVariantValue, Is.EqualTo(PrototypeStoreObjectSpawner.ComponentVariant));
    }

    [Test]
    public void GetObjectSpecs_CreatesMarketplaceSetAwayFromRoadCells()
    {
        owner = new GameObject("Store Object Spawner Test");
        var ground = owner.AddComponent<PrototypeGroundTilemap>();
        ground.SetMapSize(new Vector2Int(36, 26));
        ground.SetMapSeed(2795);

        var spawner = owner.AddComponent<PrototypeStoreObjectSpawner>();
        spawner.SetGroundTilemap(ground);

        var specs = spawner.GetObjectSpecs();

        Assert.That(specs.Length, Is.EqualTo(12));
        Assert.That(specs.Count(spec => spec.Kind == StoreObjectKind.SariSariStore), Is.EqualTo(2));
        Assert.That(specs.Count(spec => spec.Kind == StoreObjectKind.PalengkeStall), Is.EqualTo(2));
        Assert.That(specs.Count(spec => spec.Kind == StoreObjectKind.FoodStall), Is.EqualTo(2));
        Assert.That(specs.Count(spec => spec.Kind == StoreObjectKind.SignboardSari), Is.EqualTo(2));
        Assert.That(specs.Count(spec => spec.Kind == StoreObjectKind.CratesBaskets), Is.EqualTo(4));
        Assert.That(specs.Any(spec => ground.ResolveTileKind(spec.Cell.x, spec.Cell.y) == GroundTileKind.Concrete), Is.True);

        foreach (var spec in specs)
        {
            Assert.That(spec.Cell.x, Is.InRange(0, ground.MapSize.x - 1));
            Assert.That(spec.Cell.y, Is.InRange(0, ground.MapSize.y - 1));
            Assert.That(ground.ResolveTileKind(spec.Cell.x, spec.Cell.y), Is.Not.EqualTo(GroundTileKind.Road));
            Assert.That(spec.Scale.x, Is.GreaterThan(0f));
            Assert.That(spec.Scale.y, Is.GreaterThan(0f));
            Assert.That(spec.ColliderSize.x, Is.GreaterThan(0f));
            Assert.That(spec.ColliderSize.y, Is.GreaterThan(0f));
            Assert.That(spec.IsTrigger, Is.False);
        }
    }

    [Test]
    public void BuildObjects_CreatesSpriteRenderersAndColliders()
    {
        owner = new GameObject("Store Object Spawner Test");
        var ground = owner.AddComponent<PrototypeGroundTilemap>();
        ground.SetMapSize(new Vector2Int(36, 26));
        ground.SetMapSeed(2795);

        var spawner = owner.AddComponent<PrototypeStoreObjectSpawner>();
        spawner.SetGroundTilemap(ground);
        spawner.SetSprites(
            CreateSolidSprite(Color.yellow),
            CreateSolidSprite(Color.green),
            CreateSolidSprite(Color.red),
            CreateSolidSprite(Color.blue),
            CreateSolidSprite(Color.magenta));

        var created = spawner.BuildObjects();

        Assert.That(created, Is.EqualTo(12));
        Assert.That(spawner.GeneratedRoot, Is.Not.Null);
        Assert.That(spawner.CountGeneratedObjects(), Is.EqualTo(12));

        var colliders = spawner.GeneratedRoot.GetComponentsInChildren<BoxCollider2D>();
        var renderers = spawner.GeneratedRoot.GetComponentsInChildren<SpriteRenderer>();

        Assert.That(renderers.Length, Is.EqualTo(12));
        Assert.That(renderers.All(renderer => renderer.sprite != null), Is.True);
        Assert.That(colliders.Length, Is.EqualTo(12));
        Assert.That(colliders.All(collider => !collider.isTrigger), Is.True);
    }

    [Test]
    public void StoreSpecs_UseTightVisibleFootprintColliders()
    {
        owner = new GameObject("Store Object Spawner Test");
        var ground = owner.AddComponent<PrototypeGroundTilemap>();
        ground.SetMapSize(new Vector2Int(36, 26));
        ground.SetMapSeed(2795);

        var spawner = owner.AddComponent<PrototypeStoreObjectSpawner>();
        spawner.SetGroundTilemap(ground);

        foreach (var spec in spawner.GetObjectSpecs())
        {
            var worldColliderSize = Vector2.Scale(spec.ColliderSize, spec.Scale);

            Assert.That(worldColliderSize.x, Is.LessThanOrEqualTo(1.7f));
            Assert.That(worldColliderSize.y, Is.LessThanOrEqualTo(0.5f));
            Assert.That(Vector2.Scale(spec.ColliderOffset, spec.Scale).y, Is.LessThanOrEqualTo(0f));
        }
    }

    private static Sprite CreateSolidSprite(Color color)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.SetPixels(new[] { color, color, color, color });
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 2f);
    }
}
