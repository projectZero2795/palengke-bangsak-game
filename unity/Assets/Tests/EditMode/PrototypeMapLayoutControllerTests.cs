using NUnit.Framework;
using Palengke.BangSak.Game;
using UnityEngine;

public sealed class PrototypeMapLayoutControllerTests
{
    [Test]
    public void DefaultLayout_HasSeparatedRoleSpawnPoints()
    {
        var gameObject = new GameObject("Map Layout Test");
        var layout = gameObject.AddComponent<PrototypeMapLayoutController>();

        Assert.That(layout.ComponentIdValue, Is.EqualTo(PrototypeMapLayoutController.ComponentId));
        Assert.That(layout.ComponentVersionValue, Is.EqualTo(PrototypeMapLayoutController.ComponentVersion));
        Assert.That(layout.ComponentVariantValue, Is.EqualTo(PrototypeMapLayoutController.ComponentVariant));
        Assert.That(layout.GetSpawnPointCount(MapSpawnRole.Taya), Is.EqualTo(1));
        Assert.That(layout.GetSpawnPointCount(MapSpawnRole.Hider), Is.GreaterThanOrEqualTo(PrototypeMapLayoutController.DefaultMinimumHiderSpawnCount));
        Assert.That(layout.HasPlayableSpawnLayout, Is.True, layout.GetFirstValidationIssue());

        var taya = layout.GetTayaSpawnPoint();
        foreach (var hider in layout.GetHiderSpawnPoints())
        {
            Assert.That(Vector2.Distance(taya.Position, hider.Position), Is.GreaterThanOrEqualTo(layout.MinimumSpawnSeparation));
        }

        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void DefaultLayout_KeepsAllSpawnPointsInsideCameraBounds()
    {
        var gameObject = new GameObject("Map Layout Bounds Test");
        var layout = gameObject.AddComponent<PrototypeMapLayoutController>();

        Assert.That(layout.MapWorldSize, Is.EqualTo(new Vector2(52f, 36f)));
        Assert.That(layout.CameraBoundsSize, Is.EqualTo(new Vector2(50f, 34f)));

        foreach (var spawnPoint in layout.GetAllSpawnPoints())
        {
            Assert.That(layout.IsInsideMapBounds(spawnPoint.Position), Is.True, $"{spawnPoint.Role} spawn {spawnPoint.SlotIndex} should be inside map bounds.");
            Assert.That(layout.IsInsideCameraBounds(spawnPoint.Position), Is.True, $"{spawnPoint.Role} spawn {spawnPoint.SlotIndex} should be inside camera bounds.");
        }

        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void SpawnMarkers_AreGeneratedWhenReviewSpriteIsConfigured()
    {
        var gameObject = new GameObject("Map Layout Spawn Marker Test");
        var layout = gameObject.AddComponent<PrototypeMapLayoutController>();
        var texture = new Texture2D(4, 4);
        var sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);

        layout.SetSpawnMarkerSprite(sprite);

        var generatedCount = layout.BuildSpawnMarkers();

        Assert.That(layout.HasSpawnMarkerReviewVisuals, Is.True);
        Assert.That(generatedCount, Is.EqualTo(layout.GetAllSpawnPoints().Length));
        Assert.That(layout.CountGeneratedSpawnMarkers(), Is.EqualTo(generatedCount));

        Object.DestroyImmediate(sprite);
        Object.DestroyImmediate(texture);
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void TooCloseHiderSpawn_IsRejected()
    {
        var gameObject = new GameObject("Map Layout Invalid Test");
        var layout = gameObject.AddComponent<PrototypeMapLayoutController>();

        layout.ConfigureForTests(
            new Vector2(20f, 20f),
            Vector2.zero,
            new Vector2(18f, 18f),
            Vector2.zero,
            new[]
            {
                new Vector2(0.5f, 0f),
                new Vector2(7f, 0f),
                new Vector2(-7f, 0f),
                new Vector2(0f, 7f)
            },
            3f);

        Assert.That(layout.HasPlayableSpawnLayout, Is.False);
        Assert.That(layout.GetFirstValidationIssue(), Does.Contain("too close to Taya"));

        Object.DestroyImmediate(gameObject);
    }
}
