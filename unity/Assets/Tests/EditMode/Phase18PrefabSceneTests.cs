using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.UI;
using UnityEditor;
using UnityEngine;

public sealed class Phase18PrefabSceneTests
{
    [Test]
    public void DefaultPlayerPrefab_HasNameCallComponents()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPlaceholder.prefab");
        var identity = prefab.GetComponent<PlayerNameIdentity>();
        var nameCall = prefab.GetComponent<BangNameCallController>();
        var nameHud = prefab.GetComponent<BangNameCallHud>();
        var genericBangHud = prefab.GetComponent<BangActionHud>();

        Assert.That(identity, Is.Not.Null);
        Assert.That(identity.DisplayName, Is.EqualTo("JuanP"));
        Assert.That(nameCall, Is.Not.Null);
        Assert.That(nameCall.SelectedTargetName, Is.EqualTo("Maria"));
        Assert.That(nameHud, Is.Not.Null);
        Assert.That(genericBangHud, Is.Not.Null);
        Assert.That(genericBangHud.enabled, Is.False);

        var serializedHud = new SerializedObject(nameHud);
        var iconProperty = serializedHud.FindProperty("buttonIconSprite");
        Assert.That(iconProperty.objectReferenceValue, Is.Not.Null);
    }

    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Blue.prefab", "Maria")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Green.prefab", "Pedro")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Red.prefab", "Ana")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Yellow.prefab", "Luis")]
    public void HiderPrefabs_HaveNamesButNoNameCallHud(string assetPath, string expectedName)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        var identity = prefab.GetComponent<PlayerNameIdentity>();

        Assert.That(identity, Is.Not.Null);
        Assert.That(identity.DisplayName, Is.EqualTo(expectedName));
        Assert.That(prefab.GetComponent<BangNameCallController>(), Is.Null);
        Assert.That(prefab.GetComponent<BangNameCallHud>(), Is.Null);
    }
}
