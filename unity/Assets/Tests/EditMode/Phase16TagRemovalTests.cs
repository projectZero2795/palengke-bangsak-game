using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class Phase16TagRemovalTests
{
    [TestCase("Assets/Scripts/Game/TagActionController.cs")]
    [TestCase("Assets/Scripts/Game/TagHitOutcome.cs")]
    [TestCase("Assets/Scripts/Game/TagHitResult.cs")]
    [TestCase("Assets/Scripts/Game/TagHitTarget.cs")]
    [TestCase("Assets/Scripts/UI/TagActionHud.cs")]
    public void RetiredTagAssets_AreRemoved(string assetPath)
    {
        Assert.That(AssetDatabase.LoadMainAssetAtPath(assetPath), Is.Null);
    }

    [Test]
    public void DefaultPlayerPrefab_HasNoMissingScriptsAfterTagRemoval()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPlaceholder.prefab");
        Assert.That(prefab, Is.Not.Null);

        var behaviours = prefab.GetComponents<MonoBehaviour>();
        Assert.That(behaviours.Any(behaviour => behaviour == null), Is.False);
    }

    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Blue.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Green.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Red.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Yellow.prefab")]
    public void ColorVariantPrefabs_HaveNoMissingScriptsAfterTagRemoval(string assetPath)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        Assert.That(prefab, Is.Not.Null);

        var behaviours = prefab.GetComponents<MonoBehaviour>();
        Assert.That(behaviours.Any(behaviour => behaviour == null), Is.False);
    }
}
