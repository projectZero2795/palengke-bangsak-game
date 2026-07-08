using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.UI;
using UnityEditor;
using UnityEngine;

public sealed class Phase19PrefabSceneTests
{
    [Test]
    public void DefaultPlayerPrefab_HasTayaCounterFeedbackAndDisabledSakAbility()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPlaceholder.prefab");
        var roleController = prefab.GetComponent<PlayerRoleController>();
        var sakController = prefab.GetComponent<SakCounterController>();
        var sakHud = prefab.GetComponent<SakCounterHud>();

        Assert.That(prefab.GetComponent<TayaCounteredStateController>(), Is.Not.Null);
        Assert.That(sakController, Is.Not.Null);
        Assert.That(sakHud, Is.Not.Null);
        Assert.That(roleController, Is.Not.Null);
        Assert.That(roleController.Role, Is.EqualTo(PlayerRole.Taya));
        Assert.That(roleController.CanUseSak, Is.False);
    }

    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Blue.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Green.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Red.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Yellow.prefab")]
    public void HiderPrefabs_HaveSafeSakCounterWithoutOldBaseHud(string assetPath)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        var roleController = prefab.GetComponent<PlayerRoleController>();
        var sakController = prefab.GetComponent<SakCounterController>();

        Assert.That(prefab, Is.Not.Null);
        Assert.That(roleController, Is.Not.Null);
        Assert.That(roleController.Role, Is.EqualTo(PlayerRole.Hider));
        Assert.That(roleController.CanUseSak, Is.True);
        Assert.That(sakController, Is.Not.Null);
        Assert.That(prefab.GetComponent<SakCounterHud>(), Is.Null);
    }

    [Test]
    public void RetiredBaseHud_DoesNotReturn()
    {
        Assert.That(AssetDatabase.LoadMainAssetAtPath("Assets/Scripts/UI/SakActionHud.cs"), Is.Null);
    }
}
