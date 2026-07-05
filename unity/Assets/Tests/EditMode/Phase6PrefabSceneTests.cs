using NUnit.Framework;
using Palengke.BangSak.Game;
using UnityEditor;
using UnityEngine;

public sealed class Phase6PrefabSceneTests
{
    [Test]
    public void DefaultPlayerPrefab_HasBangHitTargetAndConfiguredHitDetection()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPlaceholder.prefab");
        var controller = prefab.GetComponent<BangActionController>();

        Assert.That(prefab.GetComponent<BangHitTarget>(), Is.Not.Null);
        Assert.That(prefab.GetComponent<CircleCollider2D>(), Is.Not.Null);
        Assert.That(controller, Is.Not.Null);
        Assert.That(controller.HitRadius, Is.GreaterThan(0f));
        Assert.That(controller.BlockBangWithSolidColliders, Is.True);
    }

    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Blue.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Green.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Red.prefab")]
    [TestCase("Assets/Prefabs/Players/PlayerPlaceholder_Yellow.prefab")]
    public void ColorVariantPrefabs_ArePracticeHitTargets(string assetPath)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

        Assert.That(prefab, Is.Not.Null);
        Assert.That(prefab.GetComponent<SpriteRenderer>(), Is.Not.Null);
        Assert.That(prefab.GetComponent<CircleCollider2D>(), Is.Not.Null);
        Assert.That(prefab.GetComponent<BangHitTarget>(), Is.Not.Null);
    }
}
