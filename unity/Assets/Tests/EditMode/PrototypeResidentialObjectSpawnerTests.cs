using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Game;
using UnityEngine;

public sealed class PrototypeResidentialObjectSpawnerTests
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
        owner = new GameObject("Residential Object Spawner Test");
        var spawner = owner.AddComponent<PrototypeResidentialObjectSpawner>();

        Assert.That(PrototypeResidentialObjectSpawner.ComponentId, Is.EqualTo("prototype_residential_object_spawner"));
        Assert.That(PrototypeResidentialObjectSpawner.ComponentVersion, Is.EqualTo(1));
        Assert.That(PrototypeResidentialObjectSpawner.ComponentVariant, Is.EqualTo("night_barangay_residential_placeholders"));
        Assert.That(spawner.ComponentIdValue, Is.EqualTo(PrototypeResidentialObjectSpawner.ComponentId));
        Assert.That(spawner.ComponentVersionValue, Is.EqualTo(PrototypeResidentialObjectSpawner.ComponentVersion));
        Assert.That(spawner.ComponentVariantValue, Is.EqualTo(PrototypeResidentialObjectSpawner.ComponentVariant));
    }

    [Test]
    public void GetObjectSpecs_CreatesBarangayResidentialSetAwayFromRoadCells()
    {
        owner = new GameObject("Residential Object Spawner Test");
        var ground = owner.AddComponent<PrototypeGroundTilemap>();
        ground.SetMapSize(new Vector2Int(36, 26));
        ground.SetMapSeed(2795);

        var spawner = owner.AddComponent<PrototypeResidentialObjectSpawner>();
        spawner.SetGroundTilemap(ground);

        var specs = spawner.GetObjectSpecs();

        Assert.That(specs.Length, Is.EqualTo(14));
        Assert.That(specs.Count(spec => spec.Kind == ResidentialObjectKind.SmallHouse), Is.EqualTo(2));
        Assert.That(specs.Count(spec => spec.Kind == ResidentialObjectKind.MediumHouse), Is.EqualTo(2));
        Assert.That(specs.Count(spec => spec.Kind == ResidentialObjectKind.FenceHorizontal), Is.EqualTo(4));
        Assert.That(specs.Count(spec => spec.Kind == ResidentialObjectKind.FenceVertical), Is.EqualTo(4));
        Assert.That(specs.Count(spec => spec.Kind == ResidentialObjectKind.Gate), Is.EqualTo(2));

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
    public void BuildObjects_CreatesSpriteRenderersAndWallColliders()
    {
        owner = new GameObject("Residential Object Spawner Test");
        var ground = owner.AddComponent<PrototypeGroundTilemap>();
        ground.SetMapSize(new Vector2Int(36, 26));
        ground.SetMapSeed(2795);

        var spawner = owner.AddComponent<PrototypeResidentialObjectSpawner>();
        spawner.SetGroundTilemap(ground);
        spawner.SetSprites(
            CreateSolidSprite(Color.yellow),
            CreateSolidSprite(Color.gray),
            CreateSolidSprite(Color.red),
            CreateSolidSprite(Color.blue),
            CreateSolidSprite(Color.white));

        var created = spawner.BuildObjects();

        Assert.That(created, Is.EqualTo(14));
        Assert.That(spawner.GeneratedRoot, Is.Not.Null);
        Assert.That(spawner.CountGeneratedObjects(), Is.EqualTo(14));

        var colliders = spawner.GeneratedRoot.GetComponentsInChildren<BoxCollider2D>();
        var renderers = spawner.GeneratedRoot.GetComponentsInChildren<SpriteRenderer>();

        Assert.That(renderers.Length, Is.EqualTo(14));
        Assert.That(renderers.All(renderer => renderer.sprite != null), Is.True);
        Assert.That(colliders.Length, Is.EqualTo(14));
        Assert.That(colliders.All(collider => !collider.isTrigger), Is.True);
        Assert.That(colliders.Any(collider => collider.size.x > 0.7f && collider.size.y < 0.3f), Is.True);
    }

    [Test]
    public void HouseSpecs_UseTightLowerFootprintColliders()
    {
        owner = new GameObject("Residential Object Spawner Test");
        var ground = owner.AddComponent<PrototypeGroundTilemap>();
        ground.SetMapSize(new Vector2Int(36, 26));
        ground.SetMapSeed(2795);

        var spawner = owner.AddComponent<PrototypeResidentialObjectSpawner>();
        spawner.SetGroundTilemap(ground);

        var houseSpecs = spawner.GetObjectSpecs()
            .Where(spec => spec.Kind == ResidentialObjectKind.SmallHouse || spec.Kind == ResidentialObjectKind.MediumHouse)
            .ToArray();

        Assert.That(houseSpecs.Length, Is.EqualTo(4));

        foreach (var spec in houseSpecs)
        {
            var worldColliderSize = Vector2.Scale(spec.ColliderSize, spec.Scale);
            var worldColliderOffset = Vector2.Scale(spec.ColliderOffset, spec.Scale);

            Assert.That(worldColliderSize.x, Is.LessThanOrEqualTo(2.2f));
            Assert.That(worldColliderSize.y, Is.LessThanOrEqualTo(0.6f));
            Assert.That(worldColliderOffset.y, Is.LessThan(0f));
        }
    }

    [Test]
    public void FenceAndGateSpecs_UseTightVisibleFootprintColliders()
    {
        owner = new GameObject("Residential Object Spawner Test");
        var ground = owner.AddComponent<PrototypeGroundTilemap>();
        ground.SetMapSize(new Vector2Int(36, 26));
        ground.SetMapSeed(2795);

        var spawner = owner.AddComponent<PrototypeResidentialObjectSpawner>();
        spawner.SetGroundTilemap(ground);

        var specs = spawner.GetObjectSpecs();
        var horizontalFences = specs.Where(spec => spec.Kind == ResidentialObjectKind.FenceHorizontal).ToArray();
        var verticalFences = specs.Where(spec => spec.Kind == ResidentialObjectKind.FenceVertical).ToArray();
        var gates = specs.Where(spec => spec.Kind == ResidentialObjectKind.Gate).ToArray();

        Assert.That(horizontalFences.Length, Is.EqualTo(4));
        Assert.That(verticalFences.Length, Is.EqualTo(4));
        Assert.That(gates.Length, Is.EqualTo(2));

        foreach (var spec in horizontalFences)
        {
            var worldColliderSize = Vector2.Scale(spec.ColliderSize, spec.Scale);

            Assert.That(worldColliderSize.x, Is.LessThanOrEqualTo(1.7f));
            Assert.That(worldColliderSize.y, Is.LessThanOrEqualTo(0.25f));
        }

        foreach (var spec in verticalFences)
        {
            var worldColliderSize = Vector2.Scale(spec.ColliderSize, spec.Scale);

            Assert.That(worldColliderSize.x, Is.LessThanOrEqualTo(0.2f));
            Assert.That(worldColliderSize.y, Is.LessThanOrEqualTo(1.25f));
        }

        foreach (var spec in gates)
        {
            var worldColliderSize = Vector2.Scale(spec.ColliderSize, spec.Scale);

            Assert.That(worldColliderSize.x, Is.LessThanOrEqualTo(0.9f));
            Assert.That(worldColliderSize.y, Is.LessThanOrEqualTo(0.25f));
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
