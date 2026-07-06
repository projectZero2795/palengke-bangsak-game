using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class Phase12PrefabSceneTests
{
    [TestCase("Assets/Art/Placeholders/Stores/sari_sari_store_placeholder.png")]
    [TestCase("Assets/Art/Placeholders/Stores/palengke_stall_placeholder.png")]
    [TestCase("Assets/Art/Placeholders/Stores/food_stall_placeholder.png")]
    [TestCase("Assets/Art/Placeholders/Stores/signboard_sari_placeholder.png")]
    [TestCase("Assets/Art/Placeholders/Stores/crates_baskets_placeholder.png")]
    public void StoreSprites_AreImportedAsDetailedPixelSprites(string assetPath)
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
    public void PrototypeMap_HasConfiguredPhase12StoreObjectSpawner()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var storeRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 12 Store Objects");

        Assert.That(storeRoot, Is.Not.Null);

        var spawner = storeRoot.GetComponent<PrototypeStoreObjectSpawner>();

        Assert.That(spawner, Is.Not.Null);
        Assert.That(spawner.ComponentIdValue, Is.EqualTo(PrototypeStoreObjectSpawner.ComponentId));
        Assert.That(spawner.ComponentVersionValue, Is.EqualTo(PrototypeStoreObjectSpawner.ComponentVersion));
        Assert.That(spawner.ComponentVariantValue, Is.EqualTo(PrototypeStoreObjectSpawner.ComponentVariant));
        Assert.That(spawner.GroundTilemap, Is.Not.Null);
        Assert.That(spawner.HasRequiredSprites, Is.True);
        Assert.That(spawner.CanBuild, Is.True);
        Assert.That(spawner.GetObjectSpecs().Length, Is.EqualTo(12));
        Assert.That(spawner.ObjectSortingOrder, Is.GreaterThan(0));
    }
}
