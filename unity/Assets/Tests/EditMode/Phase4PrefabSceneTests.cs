using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class Phase4PrefabSceneTests
{
    [Test]
    public void DefaultPlayerPrefab_HasAnimationControllerAndSprites()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPlaceholder.prefab");
        var animationController = prefab.GetComponent<PlayerAnimationController>();
        var serialized = new SerializedObject(animationController);

        Assert.That(animationController, Is.Not.Null);
        Assert.That(serialized.FindProperty("idleSprite").objectReferenceValue, Is.Not.Null);
        Assert.That(serialized.FindProperty("walkSprites").arraySize, Is.EqualTo(4));
    }

    [Test]
    public void PrototypeMap_PlayablePlayerIncludesAnimationController()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var movementRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 3 Movement Test");

        Assert.That(movementRoot, Is.Not.Null);
        Assert.That(movementRoot.GetComponentsInChildren<PlayerAnimationController>().Length, Is.EqualTo(1));
    }
}
