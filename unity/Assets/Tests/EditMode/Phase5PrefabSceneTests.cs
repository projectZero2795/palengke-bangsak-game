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
        var serialized = new SerializedObject(controller);

        Assert.That(controller, Is.Not.Null);
        Assert.That(hud, Is.Not.Null);
        Assert.That(serialized.FindProperty("visualStyle").enumValueIndex, Is.EqualTo((int)BangActionVisualStyle.CartoonLightBeam));
        Assert.That(serialized.FindProperty("bangMarkerSprite").objectReferenceValue, Is.Not.Null);
        Assert.That(serialized.FindProperty("rangeIndicatorSprite").objectReferenceValue, Is.Not.Null);
        Assert.That(serialized.FindProperty("cooldownSeconds").floatValue, Is.GreaterThan(0f));
        Assert.That(serialized.FindProperty("range").floatValue, Is.GreaterThan(0f));
    }

    [Test]
    public void PrototypeMap_PlayablePlayerIncludesBangActionController()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var movementRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 3 Movement Test");

        Assert.That(movementRoot, Is.Not.Null);
        Assert.That(movementRoot.GetComponentsInChildren<BangActionController>().Length, Is.EqualTo(1));
    }
}
