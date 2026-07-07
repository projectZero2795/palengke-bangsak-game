using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class Phase15BaseRemovalTests
{
    [TestCase("Assets/Art/Placeholders/Base/sak_base_placeholder.png")]
    [TestCase("Assets/Scripts/Game/SakAttemptOutcome.cs")]
    [TestCase("Assets/Scripts/Game/SakAttemptResult.cs")]
    [TestCase("Assets/Scripts/Game/SakBaseActor.cs")]
    [TestCase("Assets/Scripts/Game/SakBaseController.cs")]
    [TestCase("Assets/Scripts/UI/SakActionHud.cs")]
    public void RetiredSakBaseAssets_AreRemoved(string assetPath)
    {
        Assert.That(AssetDatabase.LoadMainAssetAtPath(assetPath), Is.Null);
    }

    [Test]
    public void PrototypeMap_DoesNotContainRetiredSakBase()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");

        Assert.That(scene.GetRootGameObjects().Any(root => root.name == "Phase 13 Sak Base"), Is.False);
    }

    [Test]
    public void DefaultPlayerPrefab_HasNoMissingScriptsAfterBaseRemoval()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPlaceholder.prefab");
        Assert.That(prefab, Is.Not.Null);

        var behaviours = prefab.GetComponents<MonoBehaviour>();
        Assert.That(behaviours.Any(behaviour => behaviour == null), Is.False);
    }
}
