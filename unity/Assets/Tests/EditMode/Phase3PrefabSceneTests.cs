using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Player;
using Palengke.BangSak.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class Phase3PrefabSceneTests
{
    [Test]
    public void DefaultPlayerPrefab_HasPhase3MovementComponents()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPlaceholder.prefab");

        Assert.That(prefab, Is.Not.Null);
        Assert.That(prefab.GetComponent<SpriteRenderer>(), Is.Not.Null);
        Assert.That(prefab.GetComponent<Rigidbody2D>(), Is.Not.Null);
        Assert.That(prefab.GetComponent<CircleCollider2D>(), Is.Not.Null);
        Assert.That(prefab.GetComponent<PlayerMovementController>(), Is.Not.Null);
    }

    [Test]
    public void PrototypeMap_HasMovementTestWalls()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var movementRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 3 Movement Test");

        Assert.That(movementRoot, Is.Not.Null);
        Assert.That(movementRoot.GetComponentsInChildren<BoxCollider2D>().Length, Is.GreaterThanOrEqualTo(5));
        Assert.That(movementRoot.GetComponentsInChildren<PlayerMovementController>().Length, Is.EqualTo(1));
    }

    [Test]
    public void PrototypeMap_HasMobileJoystickPlaceholder()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var controlsRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 3 Mobile Controls");

        Assert.That(controlsRoot, Is.Not.Null);
        Assert.That(controlsRoot.GetComponentInChildren<MobileJoystickPlaceholder>(), Is.Not.Null);
    }
}
