using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Audio;
using Palengke.BangSak.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public sealed class Phase35BAudioSettingsUiTests
{
    private const string SafeAreaPath = "Phase 22 Main Menu UI/" + SafeAreaCanvasLayout.SafeAreaRootName;

    [SetUp]
    public void SetUp()
    {
        BangSakAudioSettings.ResetToDefaults();
    }

    [TearDown]
    public void TearDown()
    {
        BangSakAudioSettings.ResetToDefaults();
    }

    [Test]
    public void MainMenu_ExposesAudioPageAndFourStoredSettings()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var menu = Object.FindObjectOfType<PrototypeMainMenuController>();
        var root = menu.transform;

        Assert.That(root.Find($"{SafeAreaPath}/Accessibility Settings Panel/AUDIO Button"), Is.Not.Null);
        Assert.That(root.Find($"{SafeAreaPath}/Audio Settings Panel/MUTE ALL Audio Row/MUTE ALL Text"), Is.Not.Null);
        Assert.That(root.Find($"{SafeAreaPath}/Audio Settings Panel/MASTER Audio Row/MASTER Text"), Is.Not.Null);
        Assert.That(root.Find($"{SafeAreaPath}/Audio Settings Panel/MUSIC Audio Row/MUSIC Text"), Is.Not.Null);
        Assert.That(root.Find($"{SafeAreaPath}/Audio Settings Panel/SFX Audio Row/SFX Text"), Is.Not.Null);
    }

    [Test]
    public void AudioPage_AllInteractiveTargetsMeetMinimumTouchSize()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var menu = Object.FindObjectOfType<PrototypeMainMenuController>();
        var panel = menu.transform.Find($"{SafeAreaPath}/Audio Settings Panel");
        var buttons = panel.GetComponentsInChildren<Button>(true);

        Assert.That(buttons.Length, Is.EqualTo(9));
        Assert.That(buttons.All(button => button.GetComponent<RectTransform>().sizeDelta.x >= 44f), Is.True);
        Assert.That(buttons.All(button => button.GetComponent<RectTransform>().sizeDelta.y >= 42f), Is.True);
    }

    [Test]
    public void AudioPage_OpensWithoutAccessibilityPageAndReturnsToIt()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var menu = Object.FindObjectOfType<PrototypeMainMenuController>();
        var accessibilityPanel = menu.transform.Find($"{SafeAreaPath}/Accessibility Settings Panel").gameObject;
        var audioPanel = menu.transform.Find($"{SafeAreaPath}/Audio Settings Panel").gameObject;

        menu.ShowAudioSettings();
        Assert.That(audioPanel.activeSelf, Is.True);
        Assert.That(accessibilityPanel.activeSelf, Is.False);

        menu.ShowSettings();
        Assert.That(audioPanel.activeSelf, Is.False);
        Assert.That(accessibilityPanel.activeSelf, Is.True);
    }

    [Test]
    public void AudioControls_UpdatePersistentValuesAndVisiblePercentages()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var menu = Object.FindObjectOfType<PrototypeMainMenuController>();

        menu.ToggleAudioMuted();
        menu.DecreaseMasterVolume();
        menu.DecreaseMasterVolume();
        menu.DecreaseMusicVolume();
        menu.DecreaseSfxVolume();

        Assert.That(BangSakAudioSettings.Muted, Is.True);
        Assert.That(BangSakAudioSettings.MasterVolume, Is.EqualTo(0.8f).Within(0.0001f));
        Assert.That(BangSakAudioSettings.MusicVolume, Is.EqualTo(0.9f).Within(0.0001f));
        Assert.That(BangSakAudioSettings.SfxVolume, Is.EqualTo(0.9f).Within(0.0001f));

        Assert.That(FindValue(menu, "MASTER").text, Is.EqualTo("80%"));
        Assert.That(FindValue(menu, "MUSIC").text, Is.EqualTo("90%"));
        Assert.That(FindValue(menu, "SFX").text, Is.EqualTo("90%"));
    }

    private static Text FindValue(PrototypeMainMenuController menu, string channel)
    {
        return menu.transform
            .Find($"{SafeAreaPath}/Audio Settings Panel/{channel} Audio Row/{channel} VALUE Text")
            .GetComponent<Text>();
    }
}
