using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.UI;
using UnityEngine;

public sealed class CaughtStateCounterHudTests
{
    private GameObject hudObject;
    private GameObject firstTarget;
    private GameObject secondTarget;

    [SetUp]
    public void SetUp()
    {
        hudObject = new GameObject("Counter HUD Owner");
        hudObject.AddComponent<CaughtStateCounterHud>();
        firstTarget = CreateCatchable("First Target");
        secondTarget = CreateCatchable("Second Target");
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(secondTarget);
        Object.DestroyImmediate(firstTarget);
        Object.DestroyImmediate(hudObject);
    }

    [Test]
    public void RefreshCounter_TracksRemainingCatchableTargets()
    {
        var hud = hudObject.GetComponent<CaughtStateCounterHud>();
        var caught = firstTarget.GetComponent<CaughtStateController>();

        hud.RefreshTargets();
        hud.RefreshCounter();

        Assert.That(hud.LastTotal, Is.EqualTo(2));
        Assert.That(hud.LastRemaining, Is.EqualTo(2));

        caught.MarkCaught(null, CaughtCause.Bang, 501);
        hud.RefreshCounter();

        Assert.That(hud.LastTotal, Is.EqualTo(2));
        Assert.That(hud.LastRemaining, Is.EqualTo(1));
    }

    private GameObject CreateCatchable(string name)
    {
        var obj = new GameObject(name);
        obj.AddComponent<SpriteRenderer>();
        obj.AddComponent<CaughtStateController>();
        return obj;
    }
}
