using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Palengke.BangSak.Audio;
using Palengke.BangSak.Game;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public sealed class Phase35DGameplayCueTests
{
    [SetUp]
    public void SetUp()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        BangSakAudioSettings.ResetToDefaults();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var player in Object.FindObjectsOfType<BangSakGameplayCuePlayer>())
        {
            Object.DestroyImmediate(player.gameObject);
        }

        BangSakAudioSettings.ResetToDefaults();
    }

    [Test]
    public void Catalog_HasFourStableVersionedSafeGameplayDefinitions()
    {
        Assert.That(BangSakGameplayCueCatalog.SetId, Is.EqualTo("bangsak.gameplay_action_cues"));
        Assert.That(BangSakGameplayCueCatalog.SetVersion, Is.EqualTo(1));
        Assert.That(BangSakGameplayCueCatalog.MinimumCompatibleVersion, Is.EqualTo(1));
        Assert.That(BangSakGameplayCueCatalog.CueCount, Is.EqualTo(Enum.GetValues(typeof(BangSakGameplayCue)).Length));
        Assert.That(BangSakGameplayCueCatalog.MigrationNote, Does.Contain("unknown future cues"));

        var definitions = Enum.GetValues(typeof(BangSakGameplayCue))
            .Cast<BangSakGameplayCue>()
            .Select(BangSakGameplayCueCatalog.Get)
            .ToArray();

        Assert.That(definitions.Select(definition => definition.StableId).Distinct().Count(), Is.EqualTo(4));
        Assert.That(definitions.All(definition => definition.Version == 1), Is.True);
        Assert.That(definitions.All(definition => definition.DurationSeconds >= 0.1f && definition.DurationSeconds <= 0.2f), Is.True);
        Assert.That(definitions.All(definition => definition.BaseVolume > 0f && definition.BaseVolume <= 0.24f), Is.True);
        Assert.That(definitions.All(definition => definition.StartFrequency >= 250f && definition.StartFrequency <= 800f), Is.True);
        Assert.That(definitions.All(definition => definition.EndFrequency >= 250f && definition.EndFrequency <= 800f), Is.True);
        Assert.That(definitions.Count(definition => definition.StableId.EndsWith("_request")), Is.EqualTo(2));
        Assert.That(definitions.Count(definition => definition.StableId.EndsWith("_confirmed")), Is.EqualTo(2));
    }

    [Test]
    public void Catalog_GeneratesDeterministicBoundedClickFreeSamples()
    {
        var totalBytes = 0;
        var uniqueFingerprints = new HashSet<string>();

        foreach (BangSakGameplayCue cue in Enum.GetValues(typeof(BangSakGameplayCue)))
        {
            var first = BangSakGameplayCueCatalog.CreateSamples(cue);
            var second = BangSakGameplayCueCatalog.CreateSamples(cue);

            CollectionAssert.AreEqual(first, second);
            Assert.That(first[0], Is.Zero);
            Assert.That(first[first.Length - 1], Is.Zero);
            Assert.That(first.Max(sample => Mathf.Abs(sample)), Is.InRange(0.4f, 1f));
            Assert.That(first.All(sample => !float.IsNaN(sample) && !float.IsInfinity(sample)), Is.True);
            uniqueFingerprints.Add(string.Join(",", first.Where((_, index) => index % 257 == 0).Take(12)));
            totalBytes += first.Length * sizeof(float);
        }

        Assert.That(uniqueFingerprints.Count, Is.EqualTo(4));
        Assert.That(totalBytes, Is.LessThan(128 * 1024));
    }

    [Test]
    public void Player_CachesFourClipsOnOneSettingsAwareTwoDimensionalVoice()
    {
        var gameObject = new GameObject("Phase 35D Audio Test");
        var player = gameObject.AddComponent<BangSakGameplayCuePlayer>();
        player.Prepare();

        Assert.That(player.CachedClipCount, Is.EqualTo(4));
        Assert.That(BangSakGameplayCuePlayer.MaximumSimultaneousVoices, Is.EqualTo(1));
        Assert.That(BangSakGameplayCuePlayer.MaximumQueuedCues, Is.EqualTo(1));
        Assert.That(player.HasPendingCue, Is.False);
        Assert.That(gameObject.GetComponents<AudioSource>().Length, Is.EqualTo(1));
        Assert.That(player.Output.playOnAwake, Is.False);
        Assert.That(player.Output.loop, Is.False);
        Assert.That(player.Output.spatialBlend, Is.Zero);

        BangSakAudioSettings.SetMasterVolume(0.5f);
        BangSakAudioSettings.SetSfxVolume(0.4f);
        Assert.That(player.Output.volume, Is.EqualTo(0.2f).Within(0.0001f));
        BangSakAudioSettings.SetMuted(true);
        Assert.That(player.Output.volume, Is.Zero);
    }

    [Test]
    public void AcceptedRequests_PublishOnceAndCooldownRejectionsStaySilent()
    {
        var published = new List<BangSakGameplayCue>();
        BangSakGameplayCueService.CuePublished += published.Add;
        try
        {
            var taya = new GameObject("Bang Request Player");
            var bang = taya.AddComponent<BangActionController>();
            Assert.That(bang.TryBang(0f), Is.True);
            Assert.That(bang.TryBang(0.1f), Is.False);

            var hider = new GameObject("SAK Request Player");
            var sak = hider.AddComponent<SakCounterController>();
            hider.GetComponent<PlayerRoleController>().SetRole(PlayerRole.Hider);
            Assert.That(sak.TrySak(0f), Is.True);
            Assert.That(sak.TrySak(0.1f), Is.False);

            CollectionAssert.AreEqual(
                new[] { BangSakGameplayCue.BangRequest, BangSakGameplayCue.SakRequest },
                published);
        }
        finally
        {
            BangSakGameplayCueService.CuePublished -= published.Add;
        }
    }

    [Test]
    public void ConfirmedStateTransitions_PublishOnceAndKeepVisualState()
    {
        var published = new List<BangSakGameplayCue>();
        BangSakGameplayCueService.CuePublished += published.Add;
        try
        {
            var hider = new GameObject("Caught Hider");
            var caught = hider.AddComponent<CaughtStateController>();
            Assert.That(caught.MarkCaught(null, CaughtCause.Bang, 51), Is.True);
            Assert.That(caught.MarkCaught(null, CaughtCause.Bang, 51), Is.False);

            var taya = new GameObject("Countered Taya");
            var countered = taya.AddComponent<TayaCounteredStateController>();
            Assert.That(countered.MarkCountered(null, 71), Is.True);
            Assert.That(countered.MarkCountered(null, 71), Is.False);

            CollectionAssert.AreEqual(
                new[]
                {
                    BangSakGameplayCue.BangCaughtConfirmed,
                    BangSakGameplayCue.SakCounteredConfirmed
                },
                published);
            Assert.That(caught.IsCaught, Is.True, "The existing caught visual/state equivalent remains authoritative.");
            Assert.That(countered.IsCountered, Is.True, "The existing counter burst/tint equivalent remains authoritative.");
        }
        finally
        {
            BangSakGameplayCueService.CuePublished -= published.Add;
        }
    }

    [Test]
    public void CueFailure_CannotPreventAcceptedGameplayResolution()
    {
        void ThrowingListener(BangSakGameplayCue _) => throw new InvalidOperationException("audio listener test");

        BangSakGameplayCueService.CuePublished += ThrowingListener;
        try
        {
            LogAssert.Expect(
                LogType.Warning,
                new Regex("Bang-Sak gameplay cue BangRequest could not play: audio listener test"));
            var taya = new GameObject("Audio Failure Taya");
            var bang = taya.AddComponent<BangActionController>();

            Assert.That(bang.TryBang(0f), Is.True);
            Assert.That(bang.LastHitResult.SequenceId, Is.EqualTo(1));
            Assert.That(bang.LastHitResult.Outcome, Is.EqualTo(BangHitOutcome.Miss));
        }
        finally
        {
            BangSakGameplayCueService.CuePublished -= ThrowingListener;
        }
    }

    [Test]
    public void Catalog_RejectsUnknownCueAndInvalidSampleRate()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BangSakGameplayCueCatalog.Get((BangSakGameplayCue)99));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BangSakGameplayCueCatalog.CreateSamples(BangSakGameplayCue.BangRequest, 0));
    }
}
