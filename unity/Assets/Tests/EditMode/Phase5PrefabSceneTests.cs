using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class Phase5PrefabSceneTests
{
    [Test]
    public void DefaultPlayerPrefab_HasBangActionControllerAndHud()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPlaceholder.prefab");
        var controller = prefab.GetComponent<BangActionController>();
        var hud = prefab.GetComponent<BangActionHud>();

        Assert.That(controller, Is.Not.Null);
        Assert.That(hud, Is.Not.Null);

        var serialized = new SerializedObject(controller);
        var serializedHud = new SerializedObject(hud);

        Assert.That(serialized.FindProperty("visualStyle").enumValueIndex, Is.EqualTo((int)BangActionVisualStyle.TsinelasMarker));
        Assert.That(serialized.FindProperty("bangMarkerSprite").objectReferenceValue, Is.Not.Null);
        Assert.That(serialized.FindProperty("rangeIndicatorSprite").objectReferenceValue, Is.Not.Null);
        Assert.That(serialized.FindProperty("impactSprite").objectReferenceValue, Is.Not.Null);
        Assert.That(serialized.FindProperty("cooldownSeconds").floatValue, Is.GreaterThan(0f));
        Assert.That(serialized.FindProperty("effectDurationSeconds").floatValue, Is.GreaterThanOrEqualTo(0.5f));
        Assert.That(serialized.FindProperty("range").floatValue, Is.GreaterThan(0f));
        Assert.That(serializedHud.FindProperty("buttonSize").vector2Value.x, Is.LessThanOrEqualTo(80f));
        Assert.That(serializedHud.FindProperty("buttonSize").vector2Value.y, Is.LessThanOrEqualTo(80f));
        Assert.That(serializedHud.FindProperty("buttonOffset").vector2Value.x, Is.LessThan(0f));
        Assert.That(serializedHud.FindProperty("buttonOffset").vector2Value.y, Is.GreaterThan(0f));
        Assert.That(serializedHud.FindProperty("buttonBackgroundSprite").objectReferenceValue, Is.Not.Null);
        Assert.That(serializedHud.FindProperty("buttonIconSprite").objectReferenceValue, Is.Not.Null);
    }

    [Test]
    public void PrototypeMap_PlayablePlayerIncludesBangActionController()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var movementRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 3 Movement Test");

        Assert.That(movementRoot, Is.Not.Null);
        Assert.That(movementRoot.GetComponentsInChildren<BangActionController>().Length, Is.EqualTo(1));
    }

    [Test]
    public void BangRangeCone_IsSmoothUiAsset()
    {
        const string assetPath = "Assets/Art/Placeholders/Bang/bang_range_placeholder.png";

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);

        Assert.That(texture, Is.Not.Null);
        Assert.That(texture.width, Is.EqualTo(256));
        Assert.That(texture.height, Is.EqualTo(256));
        Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
        Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(256f));
    }
}
