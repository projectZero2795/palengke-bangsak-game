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
        Assert.That(serialized.FindProperty("directionalIdleSprites").arraySize, Is.EqualTo(8));
        Assert.That(serialized.FindProperty("directionalWalkSprites").arraySize, Is.EqualTo(32));
        Assert.That(serialized.FindProperty("walkFramesPerDirection").intValue, Is.EqualTo(4));
        Assert.That(serialized.FindProperty("flipHorizontally").boolValue, Is.False);

        AssertAllSpritesAreAssigned(serialized.FindProperty("directionalIdleSprites"));
        AssertAllSpritesAreAssigned(serialized.FindProperty("directionalWalkSprites"));
    }

    [Test]
    public void PrototypeMap_PlayablePlayerIncludesAnimationController()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var movementRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 3 Movement Test");

        Assert.That(movementRoot, Is.Not.Null);
        Assert.That(movementRoot.GetComponentsInChildren<PlayerAnimationController>().Length, Is.EqualTo(1));
    }

    private static void AssertAllSpritesAreAssigned(SerializedProperty sprites)
    {
        for (var index = 0; index < sprites.arraySize; index++)
        {
            Assert.That(
                sprites.GetArrayElementAtIndex(index).objectReferenceValue,
                Is.Not.Null,
                $"{sprites.name}[{index}] should be assigned.");
        }
    }
}
