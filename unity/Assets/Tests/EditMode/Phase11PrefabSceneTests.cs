using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class Phase11PrefabSceneTests
{
    [TestCase("Assets/Art/Placeholders/Residential/small_house_placeholder.png")]
    [TestCase("Assets/Art/Placeholders/Residential/medium_house_placeholder.png")]
    [TestCase("Assets/Art/Placeholders/Residential/fence_horizontal_placeholder.png")]
    [TestCase("Assets/Art/Placeholders/Residential/fence_vertical_placeholder.png")]
    [TestCase("Assets/Art/Placeholders/Residential/gate_placeholder.png")]
    public void ResidentialSprites_AreImportedAsDetailedPixelSprites(string assetPath)
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
    public void PrototypeMap_HasConfiguredPhase11ResidentialObjectSpawner()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var residentialRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 11 Residential Objects");

        Assert.That(residentialRoot, Is.Not.Null);

        var spawner = residentialRoot.GetComponent<PrototypeResidentialObjectSpawner>();

        Assert.That(spawner, Is.Not.Null);
        Assert.That(spawner.ComponentIdValue, Is.EqualTo(PrototypeResidentialObjectSpawner.ComponentId));
        Assert.That(spawner.ComponentVersionValue, Is.EqualTo(PrototypeResidentialObjectSpawner.ComponentVersion));
        Assert.That(spawner.ComponentVariantValue, Is.EqualTo(PrototypeResidentialObjectSpawner.ComponentVariant));
        Assert.That(spawner.GroundTilemap, Is.Not.Null);
        Assert.That(spawner.HasRequiredSprites, Is.True);
        Assert.That(spawner.CanBuild, Is.True);
        Assert.That(spawner.GetObjectSpecs().Length, Is.EqualTo(14));
        Assert.That(spawner.ObjectSortingOrder, Is.GreaterThan(0));
    }
}
