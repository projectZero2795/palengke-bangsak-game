using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class Phase21PrefabSceneTests
{
    [Test]
    public void PrototypeMap_HasConfiguredPhase21RoundRules()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var roundRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 21 Round Rules");

        Assert.That(roundRoot, Is.Not.Null);

        var roundRules = roundRoot.GetComponent<PrototypeRoundRulesController>();
        var hud = roundRoot.GetComponent<PrototypeRoundRulesHud>();

        Assert.That(roundRules, Is.Not.Null);
        Assert.That(roundRules.ComponentIdValue, Is.EqualTo(PrototypeRoundRulesController.ComponentId));
        Assert.That(roundRules.ComponentVersionValue, Is.EqualTo(PrototypeRoundRulesController.ComponentVersion));
        Assert.That(roundRules.ComponentVariantValue, Is.EqualTo(PrototypeRoundRulesController.ComponentVariant));
        Assert.That(roundRules.RoundDurationSeconds, Is.EqualTo(PrototypeRoundRulesController.DefaultRoundDurationSeconds));
        Assert.That(roundRules.TimerExpiredResult, Is.EqualTo(PrototypeRoundResult.HidersWin));
        Assert.That(roundRules.MapLayout, Is.Not.Null);
        Assert.That(hud, Is.Not.Null);
        Assert.That(hud.Controller, Is.EqualTo(roundRules));
        Assert.That(hud.StatusPanelSize, Is.EqualTo(new Vector2(352f, 48f)));
    }

    [Test]
    public void DefaultPlayerPrefab_NoLongerOwnsOldCaughtCounterHud()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPlaceholder.prefab");

        Assert.That(prefab, Is.Not.Null);
        Assert.That(prefab.GetComponent<CaughtStateCounterHud>(), Is.Null);
    }
}
