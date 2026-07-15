using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Palengke.BangSak.Audio;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public sealed class Phase35ERoundCueTests
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
        foreach (var player in Object.FindObjectsOfType<BangSakRoundCuePlayer>())
        {
            Object.DestroyImmediate(player.gameObject);
        }

        BangSakAudioSettings.ResetToDefaults();
    }

    [Test]
    public void Catalog_HasFiveStableSemanticDefinitions()
    {
        Assert.That(BangSakRoundCueCatalog.SetId, Is.EqualTo("bangsak.round_reveal_result_cues"));
        Assert.That(BangSakRoundCueCatalog.SetVersion, Is.EqualTo(1));
        Assert.That(BangSakRoundCueCatalog.MinimumCompatibleVersion, Is.EqualTo(1));
        Assert.That(BangSakRoundCueCatalog.CueCount, Is.EqualTo(Enum.GetValues(typeof(BangSakRoundCue)).Length));
        Assert.That(BangSakRoundCueCatalog.MigrationNote, Does.Contain("unknown future cues"));

        var definitions = Enum.GetValues(typeof(BangSakRoundCue))
            .Cast<BangSakRoundCue>()
            .Select(BangSakRoundCueCatalog.Get)
            .ToArray();

        Assert.That(definitions.Select(definition => definition.StableId).Distinct().Count(), Is.EqualTo(5));
        Assert.That(definitions.Select(definition => definition.Pattern).Distinct().Count(), Is.EqualTo(5));
        Assert.That(definitions.All(definition => definition.Version == 1), Is.True);
        Assert.That(definitions.All(definition => definition.DurationSeconds >= 0.18f && definition.DurationSeconds <= 0.32f), Is.True);
        Assert.That(definitions.All(definition => definition.BaseVolume > 0f && definition.BaseVolume <= 0.22f), Is.True);
        Assert.That(definitions.All(definition => definition.StartFrequency >= 300f && definition.StartFrequency <= 1100f), Is.True);
        Assert.That(definitions.All(definition => definition.EndFrequency >= 300f && definition.EndFrequency <= 1100f), Is.True);
    }

    [Test]
    public void Catalog_GeneratesDeterministicBoundedDistinctSamples()
    {
        var totalBytes = 0;
        var fingerprints = new HashSet<string>();

        foreach (BangSakRoundCue cue in Enum.GetValues(typeof(BangSakRoundCue)))
        {
            var first = BangSakRoundCueCatalog.CreateSamples(cue);
            var second = BangSakRoundCueCatalog.CreateSamples(cue);

            CollectionAssert.AreEqual(first, second);
            Assert.That(first[0], Is.Zero);
            Assert.That(first[first.Length - 1], Is.Zero);
            Assert.That(first.Max(sample => Mathf.Abs(sample)), Is.InRange(0.35f, 1f));
            Assert.That(first.All(sample => !float.IsNaN(sample) && !float.IsInfinity(sample)), Is.True);
            fingerprints.Add(string.Join(",", first.Where((_, index) => index % 313 == 0).Take(16)));
            totalBytes += first.Length * sizeof(float);
        }

        Assert.That(fingerprints.Count, Is.EqualTo(5));
        Assert.That(totalBytes, Is.LessThan(256 * 1024));

        var roundStarted = BangSakRoundCueCatalog.CreateSamples(BangSakRoundCue.RoundStarted);
        var reveal = BangSakRoundCueCatalog.CreateSamples(BangSakRoundCue.RevealConfirmed);
        var pickup = BangSakRoundCueCatalog.CreateSamples(BangSakRoundCue.PickupReadyConfirmed);
        Assert.That(MaximumAbsoluteSample(roundStarted, 0.23f, 0.28f), Is.LessThan(0.001f));
        Assert.That(MaximumAbsoluteSample(roundStarted, 0.52f, 0.58f), Is.LessThan(0.001f));
        Assert.That(MaximumAbsoluteSample(reveal, 0.4f, 0.53f), Is.LessThan(0.001f));
        Assert.That(
            CountZeroCrossings(pickup, 0.02f, 0.31f),
            Is.LessThan(CountZeroCrossings(pickup, 0.68f, 0.98f)),
            "Pickup-ready must remain a rising sparkle.");
    }

    [Test]
    public void Player_CachesFiveClipsOnOneSettingsAwareVoice()
    {
        var gameObject = new GameObject("Phase 35E Audio Test");
        var player = gameObject.AddComponent<BangSakRoundCuePlayer>();
        player.Prepare();

        Assert.That(player.CachedClipCount, Is.EqualTo(5));
        Assert.That(BangSakRoundCuePlayer.MaximumSimultaneousVoices, Is.EqualTo(1));
        Assert.That(BangSakRoundCuePlayer.MaximumQueuedCues, Is.EqualTo(1));
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
    public void LocalRound_PublishesStartAndWinnerOnceFromConfirmedState()
    {
        var published = new List<BangSakRoundCue>();
        BangSakRoundCueService.CuePublished += published.Add;
        try
        {
            var round = CreateRound(out _, out var hider);
            round.StartRound(0f, true);
            hider.GetComponent<CaughtStateController>().MarkCaught(round, CaughtCause.Bang, 1);
            round.Tick(1f);
            round.Tick(2f);

            CollectionAssert.AreEqual(
                new[] { BangSakRoundCue.RoundStarted, BangSakRoundCue.TayaWins },
                published);
            Assert.That(round.State, Is.EqualTo(PrototypeRoundState.Finished));
            Assert.That(round.ResultTitle, Is.EqualTo("Taya wins!"));
        }
        finally
        {
            BangSakRoundCueService.CuePublished -= published.Add;
        }
    }

    [Test]
    public void RemoteSnapshots_PublishOnlyNewAuthoritativeTransitions()
    {
        var published = new List<BangSakRoundCue>();
        BangSakRoundCueService.CuePublished += published.Add;
        try
        {
            var roundObject = new GameObject("Remote Round");
            var round = roundObject.AddComponent<PrototypeRoundRulesController>();
            round.SetNetworkStateMode(true, false);
            var running = new PrototypeRoundNetworkSnapshot(
                PrototypeRoundState.Running,
                PrototypeRoundResult.None,
                "Round running",
                "Network round",
                2,
                2,
                140f,
                1);
            var finished = new PrototypeRoundNetworkSnapshot(
                PrototypeRoundState.Finished,
                PrototypeRoundResult.HidersWin,
                "Hiders win!",
                "Network result",
                2,
                1,
                12f,
                1);

            Assert.That(round.ApplyNetworkSnapshot(running), Is.True);
            Assert.That(round.ApplyNetworkSnapshot(running), Is.True);
            Assert.That(round.ApplyNetworkSnapshot(finished), Is.True);
            Assert.That(round.ApplyNetworkSnapshot(finished), Is.True);

            CollectionAssert.AreEqual(
                new[] { BangSakRoundCue.RoundStarted, BangSakRoundCue.HidersWin },
                published);
        }
        finally
        {
            BangSakRoundCueService.CuePublished -= published.Add;
        }
    }

    [Test]
    public void FutureRevealAndPickupHooks_StaySilentUntilExplicitConfirmation()
    {
        var published = new List<BangSakRoundCue>();
        BangSakRoundCueService.CuePublished += published.Add;
        try
        {
            var round = CreateRound(out _, out _);
            round.StartRound(0f, true);
            CollectionAssert.AreEqual(new[] { BangSakRoundCue.RoundStarted }, published);

            BangSakRoundCueService.PublishRevealConfirmed();
            BangSakRoundCueService.PublishPickupReadyConfirmed();

            CollectionAssert.AreEqual(
                new[]
                {
                    BangSakRoundCue.RoundStarted,
                    BangSakRoundCue.RevealConfirmed,
                    BangSakRoundCue.PickupReadyConfirmed
                },
                published);
        }
        finally
        {
            BangSakRoundCueService.CuePublished -= published.Add;
        }
    }

    [Test]
    public void CueFailure_CannotPreventRoundStart()
    {
        void ThrowingListener(BangSakRoundCue _) => throw new InvalidOperationException("round audio listener test");

        BangSakRoundCueService.CuePublished += ThrowingListener;
        try
        {
            LogAssert.Expect(
                LogType.Warning,
                new Regex("Bang-Sak round cue RoundStarted could not play: round audio listener test"));
            var round = CreateRound(out _, out _);

            round.StartRound(0f, true);

            Assert.That(round.State, Is.EqualTo(PrototypeRoundState.Running));
            Assert.That(round.RoundNumber, Is.EqualTo(1));
        }
        finally
        {
            BangSakRoundCueService.CuePublished -= ThrowingListener;
        }
    }

    [Test]
    public void Catalog_RejectsUnknownCueAndInvalidSampleRate()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BangSakRoundCueCatalog.Get((BangSakRoundCue)99));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BangSakRoundCueCatalog.CreateSamples(BangSakRoundCue.RoundStarted, 0));
    }

    private static PrototypeRoundRulesController CreateRound(out GameObject taya, out GameObject hider)
    {
        var roundObject = new GameObject("Phase 35E Round");
        var round = roundObject.AddComponent<PrototypeRoundRulesController>();
        taya = CreatePlayer("Taya", PlayerRole.Taya);
        taya.AddComponent<TayaCounteredStateController>();
        hider = CreatePlayer("Maria", PlayerRole.Hider);
        return round;
    }

    private static GameObject CreatePlayer(string name, PlayerRole role)
    {
        var player = new GameObject(name);
        player.AddComponent<SpriteRenderer>();
        player.AddComponent<Rigidbody2D>();
        player.AddComponent<CircleCollider2D>();
        player.AddComponent<PlayerMovementController>();
        player.AddComponent<PlayerRoleController>().SetRole(role);
        player.AddComponent<BangActionController>();
        player.AddComponent<SakCounterController>();
        player.AddComponent<CaughtStateController>();
        return player;
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
