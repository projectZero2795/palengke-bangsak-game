using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class Phase9PrefabSceneTests
{
    [TestCase("Assets/Art/Placeholders/Ground/soil_tile_placeholder.png")]
    [TestCase("Assets/Art/Placeholders/Ground/road_tile_placeholder.png")]
    [TestCase("Assets/Art/Placeholders/Ground/grass_tile_placeholder.png")]
    [TestCase("Assets/Art/Placeholders/Ground/concrete_tile_placeholder.png")]
    public void GroundTileSprites_AreImportedAsPixelTiles(string assetPath)
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);

        Assert.That(texture, Is.Not.Null);
        Assert.That(texture.width, Is.EqualTo(64));
        Assert.That(texture.height, Is.EqualTo(64));
        Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
        Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
        Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(64f));
    }

    [Test]
    public void PrototypeMap_HasConfiguredPhase9GroundTilemap()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var groundRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 9 Ground Tilemap");

        Assert.That(groundRoot, Is.Not.Null);

        var builder = groundRoot.GetComponent<PrototypeGroundTilemap>();

        Assert.That(builder, Is.Not.Null);
        Assert.That(builder.HasRequiredSprites, Is.True);
        Assert.That(builder.MapSize.x, Is.GreaterThanOrEqualTo(16));
        Assert.That(builder.MapSize.y, Is.GreaterThanOrEqualTo(12));
        Assert.That(builder.TilemapSortingOrder, Is.LessThan(0));
        Assert.That(builder.CountTiles(GroundTileKind.Soil), Is.GreaterThan(0));
        Assert.That(builder.CountTiles(GroundTileKind.Road), Is.GreaterThan(0));
        Assert.That(builder.CountTiles(GroundTileKind.Grass), Is.GreaterThan(0));
        Assert.That(builder.CountTiles(GroundTileKind.Concrete), Is.GreaterThan(0));
    }
}
