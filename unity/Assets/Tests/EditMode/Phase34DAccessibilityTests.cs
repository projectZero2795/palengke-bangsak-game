using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public sealed class Phase34DAccessibilityTests
{
    [SetUp]
    public void SetUp()
    {
        AccessibilitySettings.ResetToDefaults();
    }

    [TearDown]
    public void TearDown()
    {
        AccessibilitySettings.ResetToDefaults();
    }

    [Test]
    public void Defaults_KeepReadableTextAndVisualCuesEnabled()
    {
        Assert.That(AccessibilitySettings.ReadableTextEnabled, Is.True);
        Assert.That(AccessibilitySettings.HighContrastEnabled, Is.False);
        Assert.That(AccessibilitySettings.ReducedMotionEnabled, Is.False);
        Assert.That(AccessibilitySettings.VisualCuesEnabled, Is.True);
    }

    [Test]
    public void Preferences_PersistEachIndependentToggle()
    {
        AccessibilitySettings.SetReadableText(false);
        AccessibilitySettings.SetHighContrast(true);
        AccessibilitySettings.SetReducedMotion(true);
        AccessibilitySettings.SetVisualCues(false);

        Assert.That(PlayerPrefs.GetInt(AccessibilitySettings.ReadableTextKey), Is.Zero);
        Assert.That(PlayerPrefs.GetInt(AccessibilitySettings.HighContrastKey), Is.EqualTo(1));
        Assert.That(PlayerPrefs.GetInt(AccessibilitySettings.ReducedMotionKey), Is.EqualTo(1));
        Assert.That(PlayerPrefs.GetInt(AccessibilitySettings.VisualCuesKey), Is.Zero);
    }

    [Test]
    public void ReadableText_ScalesFontWhileKeepingBestFitBounds()
    {
        var canvasObject = new GameObject("Accessibility Test Canvas");
        var labelObject = new GameObject("Accessibility Test Label");
        labelObject.transform.SetParent(canvasObject.transform, false);
        var label = labelObject.AddComponent<Text>();
        label.fontSize = 20;
        label.resizeTextMinSize = 10;
        label.resizeTextMaxSize = 20;
        label.fontStyle = FontStyle.Normal;

        var adapter = canvasObject.AddComponent<AccessibilityCanvasAdapter>();
        adapter.RefreshNow();

        Assert.That(label.fontSize, Is.EqualTo(24));
        Assert.That(label.resizeTextMinSize, Is.EqualTo(12));
        Assert.That(label.resizeTextMaxSize, Is.EqualTo(24));
        Assert.That(label.fontStyle, Is.EqualTo(FontStyle.Bold));
        Object.DestroyImmediate(canvasObject);
    }

    [Test]
    public void HighContrast_AddsBrightTextAndDarkOutline()
    {
        AccessibilitySettings.SetHighContrast(true);
        var labelObject = new GameObject("Contrast Test Label");
        var label = labelObject.AddComponent<Text>();
        label.color = new Color(0.35f, 0.45f, 0.6f, 1f);
        var style = labelObject.AddComponent<AccessibilityTextStyle>();

        style.Apply(label);

        Assert.That(label.color, Is.EqualTo(Color.white));
        Assert.That(labelObject.GetComponent<Outline>(), Is.Not.Null);
        Assert.That(labelObject.GetComponent<Outline>().enabled, Is.True);
        Object.DestroyImmediate(labelObject);
    }

    [Test]
    public void ReducedMotion_ReplacesAnimatedPulseWithStableMidpoint()
    {
        Assert.That(AccessibilitySettings.ResolvePulse(0.9f), Is.EqualTo(0.9f));

        AccessibilitySettings.SetReducedMotion(true);

        Assert.That(AccessibilitySettings.ResolvePulse(0.1f), Is.EqualTo(0.5f));
        Assert.That(AccessibilitySettings.ResolvePulse(0.9f), Is.EqualTo(0.5f));
    }

    [Test]
    public void VisualCueService_UsesTextOutcomeAndRespectsToggle()
    {
        AccessibilityVisualCue received = default;
        var count = 0;
        System.Action<AccessibilityVisualCue> handler = cue =>
        {
            received = cue;
            count += 1;
        };
        AccessibilityCueService.CueRequested += handler;

        Assert.That(AccessibilityCueService.PublishBang(BangHitOutcome.HitTarget), Is.True);
        Assert.That(received.Message, Is.EqualTo("BANG! CAUGHT"));
        Assert.That(count, Is.EqualTo(1));

        AccessibilitySettings.SetVisualCues(false);
        Assert.That(AccessibilityCueService.PublishSak(SakCounterOutcome.CounteredTaya), Is.False);
        Assert.That(count, Is.EqualTo(1));
        AccessibilityCueService.CueRequested -= handler;
    }

    [Test]
    public void MainMenu_SettingsExposeFourReviewableAccessibilityOptions()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var menu = Object.FindObjectOfType<PrototypeMainMenuController>();
        var safeAreaPath = $"Phase 22 Main Menu UI/{SafeAreaCanvasLayout.SafeAreaRootName}";

        Assert.That(menu, Is.Not.Null);
        Assert.That(menu.transform.Find($"{safeAreaPath}/Accessibility Settings Panel/READABLE TEXT Text"), Is.Not.Null);
        Assert.That(menu.transform.Find($"{safeAreaPath}/Accessibility Settings Panel/HIGH CONTRAST Text"), Is.Not.Null);
        Assert.That(menu.transform.Find($"{safeAreaPath}/Accessibility Settings Panel/REDUCED MOTION Text"), Is.Not.Null);
        Assert.That(menu.transform.Find($"{safeAreaPath}/Accessibility Settings Panel/VISUAL ACTION CUES Text"), Is.Not.Null);
        Assert.That(menu.transform.Find($"{safeAreaPath}/Accessibility Settings Panel/Motion Preview Marker"), Is.Not.Null);
    }
}
