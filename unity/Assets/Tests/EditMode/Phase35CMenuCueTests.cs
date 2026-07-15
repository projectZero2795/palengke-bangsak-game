using System;
using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Audio;
using Palengke.BangSak.UI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class Phase35CMenuCueTests
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
        foreach (var player in Object.FindObjectsOfType<BangSakMenuCuePlayer>())
        {
            Object.DestroyImmediate(player.gameObject);
        }

        BangSakAudioSettings.ResetToDefaults();
    }

    [Test]
    public void Catalog_HasStableVersionedNonStartlingCueDefinitions()
    {
        Assert.That(BangSakMenuCueCatalog.SetId, Is.EqualTo("bangsak.menu_interface_cues"));
        Assert.That(BangSakMenuCueCatalog.SetVersion, Is.EqualTo(2));
        Assert.That(BangSakMenuCueCatalog.MinimumCompatibleVersion, Is.EqualTo(1));
        Assert.That(BangSakMenuCueCatalog.CueCount, Is.EqualTo(Enum.GetValues(typeof(BangSakMenuCue)).Length));
        Assert.That(BangSakMenuCueCatalog.MigrationNote, Does.Contain("unknown future cues"));

        var definitions = Enum.GetValues(typeof(BangSakMenuCue))
            .Cast<BangSakMenuCue>()
            .Select(BangSakMenuCueCatalog.Get)
            .ToArray();

        Assert.That(definitions.Select(definition => definition.StableId).Distinct().Count(), Is.EqualTo(3));
        Assert.That(definitions.Select(definition => definition.Cue).Distinct().Count(), Is.EqualTo(3));
        Assert.That(definitions.All(definition => definition.Version == 2), Is.True);
        Assert.That(definitions.Select(definition => definition.Pattern).Distinct().Count(), Is.EqualTo(3));
        Assert.That(definitions.All(definition => definition.DurationSeconds >= 0.05f && definition.DurationSeconds <= 0.16f), Is.True);
        Assert.That(definitions.All(definition => definition.BaseVolume > 0f && definition.BaseVolume <= 0.2f), Is.True);
        Assert.That(definitions.All(definition => definition.StartFrequency >= 250f && definition.StartFrequency <= 1200f), Is.True);
        Assert.That(definitions.All(definition => definition.EndFrequency >= 250f && definition.EndFrequency <= 1200f), Is.True);
        Assert.That(BangSakMenuCueCatalog.Get(BangSakMenuCue.Navigate).Pattern, Is.EqualTo(BangSakMenuCuePattern.NavigateTick));
        Assert.That(BangSakMenuCueCatalog.Get(BangSakMenuCue.Confirm).Pattern, Is.EqualTo(BangSakMenuCuePattern.ConfirmDoubleChime));
        Assert.That(BangSakMenuCueCatalog.Get(BangSakMenuCue.Back).Pattern, Is.EqualTo(BangSakMenuCuePattern.BackDescendingBubble));
    }

    [Test]
    public void Catalog_GeneratesDeterministicBoundedClickFreeSamples()
    {
        var totalBytes = 0;
        float[] previous = null;

        foreach (BangSakMenuCue cue in Enum.GetValues(typeof(BangSakMenuCue)))
        {
            var first = BangSakMenuCueCatalog.CreateSamples(cue);
            var second = BangSakMenuCueCatalog.CreateSamples(cue);

            CollectionAssert.AreEqual(first, second);
            Assert.That(first[0], Is.Zero);
            Assert.That(first[first.Length - 1], Is.Zero);
            Assert.That(first.Max(sample => Mathf.Abs(sample)), Is.InRange(0.4f, 1f));
            Assert.That(first.All(sample => !float.IsNaN(sample) && !float.IsInfinity(sample)), Is.True);
            if (previous != null)
            {
                Assert.That(first.SequenceEqual(previous), Is.False);
            }

            totalBytes += first.Length * sizeof(float);
            previous = first;
        }

        Assert.That(totalBytes, Is.LessThan(64 * 1024));

        var navigate = BangSakMenuCueCatalog.CreateSamples(BangSakMenuCue.Navigate);
        var confirm = BangSakMenuCueCatalog.CreateSamples(BangSakMenuCue.Confirm);
        var back = BangSakMenuCueCatalog.CreateSamples(BangSakMenuCue.Back);
        Assert.That(
            CountZeroCrossings(navigate, 0.05f, 0.45f),
            Is.LessThan(CountZeroCrossings(navigate, 0.55f, 0.95f)),
            "Navigate must remain a rising tactile tick.");
        Assert.That(
            MaximumAbsoluteSample(confirm, 0.43f, 0.51f),
            Is.LessThan(0.001f),
            "Confirm must retain an audible gap between its two notes.");
        Assert.That(
            CountZeroCrossings(back, 0.05f, 0.45f),
            Is.GreaterThan(CountZeroCrossings(back, 0.55f, 0.95f)),
            "Back must remain a descending cue.");
    }

    [Test]
    public void Player_CachesOnlyThreeClipsAndUsesOneTwoDimensionalSource()
    {
        var gameObject = new GameObject("Phase 35C Audio Test");
        var player = gameObject.AddComponent<BangSakMenuCuePlayer>();

        player.Prepare();
        var originalClips = Enum.GetValues(typeof(BangSakMenuCue))
            .Cast<BangSakMenuCue>()
            .Select(player.GetClip)
            .ToArray();
        player.Prepare();

        Assert.That(player.CachedClipCount, Is.EqualTo(3));
        Assert.That(BangSakMenuCuePlayer.MaximumSimultaneousVoices, Is.EqualTo(1));
        Assert.That(gameObject.GetComponents<AudioSource>().Length, Is.EqualTo(1));
        Assert.That(player.Output.playOnAwake, Is.False);
        Assert.That(player.Output.loop, Is.False);
        Assert.That(player.Output.spatialBlend, Is.Zero);
        CollectionAssert.AreEqual(
            originalClips,
            Enum.GetValues(typeof(BangSakMenuCue)).Cast<BangSakMenuCue>().Select(player.GetClip).ToArray());
    }

    [Test]
    public void Player_TracksMasterSfxAndMuteSettingsImmediately()
    {
        var gameObject = new GameObject("Phase 35C Volume Test");
        var player = gameObject.AddComponent<BangSakMenuCuePlayer>();
        player.Prepare();

        BangSakAudioSettings.SetMasterVolume(0.5f);
        BangSakAudioSettings.SetSfxVolume(0.4f);
        Assert.That(player.Output.volume, Is.EqualTo(0.2f).Within(0.0001f));

        BangSakAudioSettings.SetMuted(true);
        Assert.That(player.Output.volume, Is.Zero);

        BangSakAudioSettings.SetMuted(false);
        Assert.That(player.Output.volume, Is.EqualTo(0.2f).Within(0.0001f));
    }

    [Test]
    public void MainMenu_BindsEveryButtonToAnInspectableCueAndKeepsVisualActions()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var menu = Object.FindObjectOfType<PrototypeMainMenuController>();
        var root = menu.transform.Find(SafeAreaPath);
        var buttons = root.GetComponentsInChildren<Button>(true);

        Assert.That(buttons.Length, Is.GreaterThan(0));
        Assert.That(buttons.All(button => button.GetComponent<BangSakMenuCueBinding>() != null), Is.True);
        AssertCue(root, "Main Menu Dashboard/PLAY Dashboard Tile", BangSakMenuCue.Confirm);
        AssertCue(root, "Main Menu Dashboard/ROOM Dashboard Tile", BangSakMenuCue.Navigate);
        AssertCue(root, "Accessibility Settings Panel/AUDIO Button", BangSakMenuCue.Navigate);
        AssertCue(root, "Audio Settings Panel/MUTE ALL Audio Row/MUTE Button", BangSakMenuCue.Confirm);
        AssertCue(root, "Network Room Panel/CREATE Button", BangSakMenuCue.Confirm);
        AssertCue(root, "Network Room Panel/LEAVE Button", BangSakMenuCue.Back);
        Assert.That(
            buttons.Where(button => button.name == "BACK Button")
                .All(button => button.GetComponent<BangSakMenuCueBinding>().Cue == BangSakMenuCue.Back),
            Is.True);

        var audioPanel = root.Find("Audio Settings Panel").gameObject;
        root.Find("Accessibility Settings Panel/AUDIO Button").GetComponent<Button>().onClick.Invoke();
        Assert.That(audioPanel.activeSelf, Is.True);
        root.Find("Audio Settings Panel/BACK Button").GetComponent<Button>().onClick.Invoke();
        Assert.That(audioPanel.activeSelf, Is.False);
    }

    [Test]
    public void Catalog_RejectsUnknownCueAndInvalidSampleRate()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BangSakMenuCueCatalog.Get((BangSakMenuCue)99));
        Assert.Throws<ArgumentOutOfRangeException>(() => BangSakMenuCueCatalog.CreateSamples(BangSakMenuCue.Navigate, 0));
    }

    private static void AssertCue(Transform root, string path, BangSakMenuCue expected)
    {
        var target = root.Find(path);
        Assert.That(target, Is.Not.Null, path);
        Assert.That(target.GetComponent<BangSakMenuCueBinding>().Cue, Is.EqualTo(expected), path);
    }

    private static int CountZeroCrossings(float[] samples, float startRatio, float endRatio)
    {
        var start = Mathf.Clamp(Mathf.FloorToInt(samples.Length * startRatio), 0, samples.Length - 1);
        var end = Mathf.Clamp(Mathf.CeilToInt(samples.Length * endRatio), start + 1, samples.Length);
        var crossings = 0;
        var previous = samples[start];
        for (var index = start + 1; index < end; index += 1)
        {
            var current = samples[index];
            if ((previous < 0f && current >= 0f) || (previous > 0f && current <= 0f))
            {
                crossings += 1;
            }

            previous = current;
        }

        return crossings;
    }

    private static float MaximumAbsoluteSample(float[] samples, float startRatio, float endRatio)
    {
        var start = Mathf.Clamp(Mathf.FloorToInt(samples.Length * startRatio), 0, samples.Length - 1);
        var end = Mathf.Clamp(Mathf.CeilToInt(samples.Length * endRatio), start + 1, samples.Length);
        var maximum = 0f;
        for (var index = start; index < end; index += 1)
        {
            maximum = Mathf.Max(maximum, Mathf.Abs(samples[index]));
        }

        return maximum;
    }
}
