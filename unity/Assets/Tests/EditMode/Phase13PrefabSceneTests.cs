using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class Phase13PrefabSceneTests
{
    [Test]
    public void SakBaseSprite_IsImportedAsDetailedPixelSprite()
    {
        const string assetPath = "Assets/Art/Placeholders/Base/sak_base_placeholder.png";

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
    public void PlayerPrefab_HasSakActorAndHud()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPlaceholder.prefab");

        Assert.That(prefab, Is.Not.Null);

        var actor = prefab.GetComponent<SakBaseActor>();
        var hud = prefab.GetComponent<SakActionHud>();

        Assert.That(actor, Is.Not.Null);
        Assert.That(actor.CanUseSak, Is.True);
        Assert.That(hud, Is.Not.Null);
    }

    [Test]
    public void PrototypeMap_HasConfiguredPhase13SakBase()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var baseRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 13 Sak Base");

        Assert.That(baseRoot, Is.Not.Null);

        var sakBase = baseRoot.GetComponent<SakBaseController>();
        var collider = baseRoot.GetComponent<CircleCollider2D>();
        var renderer = baseRoot.GetComponent<SpriteRenderer>();

        Assert.That(sakBase, Is.Not.Null);
        Assert.That(sakBase.ComponentIdValue, Is.EqualTo(SakBaseController.ComponentId));
        Assert.That(sakBase.ComponentVersionValue, Is.EqualTo(SakBaseController.ComponentVersion));
        Assert.That(sakBase.ComponentVariantValue, Is.EqualTo(SakBaseController.ComponentVariant));
        Assert.That(sakBase.IsBaseActive, Is.True);
        Assert.That(sakBase.TriggerRadius, Is.GreaterThanOrEqualTo(1f));
        Assert.That(collider, Is.Not.Null);
        Assert.That(collider.isTrigger, Is.True);
        Assert.That(renderer, Is.Not.Null);
        Assert.That(renderer.sprite, Is.Not.Null);
    }
}
