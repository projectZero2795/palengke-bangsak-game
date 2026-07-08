using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.UI;
using UnityEditor;
using UnityEngine;

public sealed class Phase17PrefabSceneTests
{
    [Test]
    public void DefaultPlayerPrefab_IsConfiguredAsTaya()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPlaceholder.prefab");
        var roleController = prefab.GetComponent<PlayerRoleController>();
        var caughtState = prefab.GetComponent<CaughtStateController>();

        Assert.That(roleController, Is.Not.Null);
        Assert.That(roleController.Role, Is.EqualTo(PlayerRole.Taya));
        Assert.That(roleController.CanUseBang, Is.True);
        Assert.That(prefab.GetComponent<BangActionController>(), Is.Not.Null);
        Assert.That(prefab.GetComponent<BangActionHud>(), Is.Not.Null);
        Assert.That(caughtState, Is.Not.Null);
        Assert.That(caughtState.CountAsHider, Is.False);
    }

    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Blue.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Green.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Red.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Yellow.prefab")]
    public void ColorVariantPrefabs_AreConfiguredAsHiders(string assetPath)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        var roleController = prefab.GetComponent<PlayerRoleController>();
        var caughtState = prefab.GetComponent<CaughtStateController>();

        Assert.That(roleController, Is.Not.Null);
        Assert.That(roleController.Role, Is.EqualTo(PlayerRole.Hider));
        Assert.That(roleController.CanUseBang, Is.False);
        Assert.That(prefab.GetComponent<BangActionController>(), Is.Null);
        Assert.That(prefab.GetComponent<BangActionHud>(), Is.Null);
        Assert.That(caughtState, Is.Not.Null);
        Assert.That(caughtState.CountAsHider, Is.True);
    }
}
