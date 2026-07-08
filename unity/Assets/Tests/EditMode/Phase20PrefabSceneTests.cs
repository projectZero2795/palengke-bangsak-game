using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Game;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class Phase20PrefabSceneTests
{
    [Test]
    public void PrototypeMap_HasConfiguredPhase20MapLayout()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var layoutRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 20 Map Layout");

        Assert.That(layoutRoot, Is.Not.Null);

        var layout = layoutRoot.GetComponent<PrototypeMapLayoutController>();

        Assert.That(layout, Is.Not.Null);
        Assert.That(layout.ComponentIdValue, Is.EqualTo(PrototypeMapLayoutController.ComponentId));
        Assert.That(layout.ComponentVersionValue, Is.EqualTo(PrototypeMapLayoutController.ComponentVersion));
        Assert.That(layout.ComponentVariantValue, Is.EqualTo(PrototypeMapLayoutController.ComponentVariant));
        Assert.That(layout.GroundTilemap, Is.Not.Null);
        Assert.That(layout.GroundTilemap.MapSize, Is.EqualTo(new Vector2Int(52, 36)));
        Assert.That(layout.MapWorldSize, Is.EqualTo(new Vector2(52f, 36f)));
        Assert.That(layout.CameraBoundsSize, Is.EqualTo(new Vector2(50f, 34f)));
        Assert.That(layout.HasSpawnMarkerReviewVisuals, Is.True);
        Assert.That(layout.HasPlayableSpawnLayout, Is.True, layout.GetFirstValidationIssue());
        Assert.That(layout.GetSpawnPointCount(MapSpawnRole.Taya), Is.EqualTo(1));
        Assert.That(layout.GetSpawnPointCount(MapSpawnRole.Hider), Is.GreaterThanOrEqualTo(PrototypeMapLayoutController.DefaultMinimumHiderSpawnCount));
    }

    [Test]
    public void PrototypeMap_CameraFollowsPlayablePlayerInsidePhase20Bounds()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var cameraRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Main Camera");

        Assert.That(cameraRoot, Is.Not.Null);

        var cameraFollow = cameraRoot.GetComponent<PrototypeCameraFollowController>();

        Assert.That(cameraFollow, Is.Not.Null);
        Assert.That(cameraFollow.ComponentIdValue, Is.EqualTo(PrototypeCameraFollowController.ComponentId));
        Assert.That(cameraFollow.ComponentVersionValue, Is.EqualTo(PrototypeCameraFollowController.ComponentVersion));
        Assert.That(cameraFollow.ComponentVariantValue, Is.EqualTo(PrototypeCameraFollowController.ComponentVariant));
        Assert.That(cameraFollow.HasFollowTarget, Is.True);
        Assert.That(cameraFollow.MapLayout, Is.Not.Null);
    }

    [Test]
    public void PrototypeMap_HasNoIncorrectBaseObjects()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");

        Assert.That(scene.GetRootGameObjects().Any(root => root.name.ToLowerInvariant().Contains("base")), Is.False);
    }

    [Test]
    public void PrototypeMap_MovementWallsMatchPhase20PlayableBounds()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var movementRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 3 Movement Test");

        Assert.That(movementRoot, Is.Not.Null);

        AssertWall(movementRoot.transform.Find("Wall Top"), new Vector3(0f, 17f, 0f), new Vector3(50f, 0.35f, 1f));
        AssertWall(movementRoot.transform.Find("Wall Bottom"), new Vector3(0f, -17f, 0f), new Vector3(50f, 0.35f, 1f));
        AssertWall(movementRoot.transform.Find("Wall Left"), new Vector3(-25f, 0f, 0f), new Vector3(0.35f, 34f, 1f));
        AssertWall(movementRoot.transform.Find("Wall Right"), new Vector3(25f, 0f, 0f), new Vector3(0.35f, 34f, 1f));
    }

    private static void AssertWall(Transform wall, Vector3 expectedPosition, Vector3 expectedScale)
    {
        Assert.That(wall, Is.Not.Null);
        Assert.That(wall.localPosition, Is.EqualTo(expectedPosition));
        Assert.That(wall.localScale, Is.EqualTo(expectedScale));
    }
}
