using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class Phase10PrefabSceneTests
{
    [TestCase("Assets/Art/Placeholders/Natural/tree_placeholder.png")]
    [TestCase("Assets/Art/Placeholders/Natural/bush_placeholder.png")]
    [TestCase("Assets/Art/Placeholders/Natural/plant_pot_placeholder.png")]
    [TestCase("Assets/Art/Placeholders/Natural/plant_cluster_placeholder.png")]
    public void NaturalObjectSprites_AreImportedAsDetailedPixelSprites(string assetPath)
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);

        Assert.That(texture, Is.Not.Null);
        Assert.That(texture.width, Is.EqualTo(128));
        Assert.That(texture.height, Is.EqualTo(128));
        Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
        Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
        Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(128f));
    }

    [Test]
    public void PrototypeMap_HasConfiguredPhase10NaturalObjectSpawner()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var naturalRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 10 Natural Objects");

        Assert.That(naturalRoot, Is.Not.Null);

        var spawner = naturalRoot.GetComponent<PrototypeNaturalObjectSpawner>();

        Assert.That(spawner, Is.Not.Null);
        Assert.That(spawner.ComponentIdValue, Is.EqualTo(PrototypeNaturalObjectSpawner.ComponentId));
        Assert.That(spawner.ComponentVersionValue, Is.EqualTo(PrototypeNaturalObjectSpawner.ComponentVersion));
        Assert.That(spawner.ComponentVariantValue, Is.EqualTo(PrototypeNaturalObjectSpawner.ComponentVariant));
        Assert.That(spawner.GroundTilemap, Is.Not.Null);
        Assert.That(spawner.HasRequiredSprites, Is.True);
        Assert.That(spawner.CanBuild, Is.True);
        Assert.That(spawner.MaxObjects, Is.GreaterThanOrEqualTo(12));
        Assert.That(spawner.MinSpacing, Is.GreaterThanOrEqualTo(3));
        Assert.That(spawner.EdgePadding, Is.GreaterThanOrEqualTo(2));
    }
}
