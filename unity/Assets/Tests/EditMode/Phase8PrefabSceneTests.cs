using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.UI;
using UnityEditor;
using UnityEngine;

public sealed class Phase8PrefabSceneTests
{
    [Test]
    public void DefaultPlayerPrefab_HasCaughtStateAndCounterHud()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPlaceholder.prefab");
        var caughtState = prefab.GetComponent<CaughtStateController>();

        Assert.That(caughtState, Is.Not.Null);
        Assert.That(prefab.GetComponent<CaughtStateCounterHud>(), Is.Not.Null);
        Assert.That(caughtState.CountAsHider, Is.False);
    }

    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Blue.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Green.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Red.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Yellow.prefab")]
    public void ColorVariantPrefabs_AreCatchableHiders(string assetPath)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        var caughtState = prefab.GetComponent<CaughtStateController>();

        Assert.That(caughtState, Is.Not.Null);
        Assert.That(caughtState.CountAsHider, Is.True);
        Assert.That(prefab.GetComponent<BangHitTarget>(), Is.Not.Null);
    }
}
