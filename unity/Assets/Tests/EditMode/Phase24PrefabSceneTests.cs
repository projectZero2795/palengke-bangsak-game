using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Network;
using UnityEditor.SceneManagement;

public sealed class Phase24PrefabSceneTests
{
    [Test]
    public void PrototypeMap_HasConfiguredPhase24NetworkPlayerSpawner()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var spawnerRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 24 Network Player Spawner");

        Assert.That(spawnerRoot, Is.Not.Null);

        var spawner = spawnerRoot.GetComponent<PrototypeNetworkPlayerSpawner>();

        Assert.That(spawner, Is.Not.Null);
        Assert.That(spawner.ComponentIdValue, Is.EqualTo(PrototypeNetworkPlayerSpawner.ComponentId));
        Assert.That(spawner.ComponentVersionValue, Is.EqualTo(PrototypeNetworkPlayerSpawner.ComponentVersion));
        Assert.That(spawner.ComponentVariantValue, Is.EqualTo(PrototypeNetworkPlayerSpawner.ComponentVariant));
        Assert.That(spawner.MapLayout, Is.Not.Null);
        Assert.That(spawner.CameraFollow, Is.Not.Null);
        Assert.That(spawner.MobileJoystick, Is.Not.Null);
        Assert.That(spawner.PlayerPrefab, Is.Not.Null);
        Assert.That(spawner.SpawnOnAwake, Is.True);
        Assert.That(spawner.PreviewPlayerCount, Is.EqualTo(PrototypeNetworkPlayerSpawner.DefaultPreviewPlayerCount));
        Assert.That(spawner.LocalPlayerIndex, Is.EqualTo(0));
    }
}
