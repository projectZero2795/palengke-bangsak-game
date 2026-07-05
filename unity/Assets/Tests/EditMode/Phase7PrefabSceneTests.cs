using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.UI;
using UnityEditor;
using UnityEngine;

public sealed class Phase7PrefabSceneTests
{
    [Test]
    public void DefaultPlayerPrefab_HasCloseTagActionAndHud()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPlaceholder.prefab");
        var controller = prefab.GetComponent<TagActionController>();
        var hud = prefab.GetComponent<TagActionHud>();

        Assert.That(prefab.GetComponent<TagHitTarget>(), Is.Not.Null);
        Assert.That(controller, Is.Not.Null);
        Assert.That(hud, Is.Not.Null);
        Assert.That(controller.TagRange, Is.LessThan(1f));
        Assert.That(controller.TagRadius, Is.GreaterThan(0f));
        Assert.That(controller.BlockTagWithSolidColliders, Is.True);

        var serializedHud = new SerializedObject(hud);
        Assert.That(serializedHud.FindProperty("buttonLabel").stringValue, Is.EqualTo("TAG"));
        Assert.That(serializedHud.FindProperty("buttonSize").vector2Value.x, Is.LessThanOrEqualTo(64f));
        Assert.That(serializedHud.FindProperty("buttonOffset").vector2Value.x, Is.LessThan(-70f));
    }

    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Blue.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Green.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Red.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Yellow.prefab")]
    public void ColorVariantPrefabs_AreCloseTagTargets(string assetPath)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

        Assert.That(prefab, Is.Not.Null);
        Assert.That(prefab.GetComponent<SpriteRenderer>(), Is.Not.Null);
        Assert.That(prefab.GetComponent<CircleCollider2D>(), Is.Not.Null);
        Assert.That(prefab.GetComponent<TagHitTarget>(), Is.Not.Null);
    }
}
