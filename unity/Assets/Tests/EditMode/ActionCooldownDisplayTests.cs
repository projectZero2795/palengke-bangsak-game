using NUnit.Framework;
using Palengke.BangSak.UI;
using UnityEditor;
using UnityEngine;

public sealed class ActionCooldownDisplayTests
{
    [TestCase(1.25f, 1.25f, 1f)]
    [TestCase(0.625f, 1.25f, 0.5f)]
    [TestCase(0f, 1.25f, 0f)]
    [TestCase(-1f, 1.25f, 0f)]
    public void RemainingFraction_IsClamped(float remaining, float duration, float expected)
    {
        Assert.That(ActionCooldownDisplay.RemainingFraction(remaining, duration), Is.EqualTo(expected).Within(0.001f));
    }

    [Test]
    public void FormatSeconds_ShowsUnitsAndReadyState()
    {
        Assert.That(ActionCooldownDisplay.FormatSeconds(1.24f), Is.EqualTo("1.2s"));
        Assert.That(ActionCooldownDisplay.FormatSeconds(0f), Is.EqualTo("READY"));
    }

    [Test]
    public void NamedBangPanel_HasDedicatedCooldownFooterSpace()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerPlaceholder.prefab");
        var hud = prefab.GetComponent<BangNameCallHud>();
        var serialized = new SerializedObject(hud);

        Assert.That(serialized.FindProperty("panelSize").vector2Value.y, Is.GreaterThanOrEqualTo(132f));
    }
}
