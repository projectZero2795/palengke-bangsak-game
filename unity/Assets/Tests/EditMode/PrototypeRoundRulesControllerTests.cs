using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class PrototypeRoundRulesControllerTests
{
    private GameObject roundObject;
    private PrototypeRoundRulesController roundRules;
    private GameObject taya;
    private GameObject firstHider;
    private GameObject secondHider;

    [SetUp]
    public void SetUp()
    {
        roundObject = new GameObject("Round Rules");
        roundRules = roundObject.AddComponent<PrototypeRoundRulesController>();
        taya = CreatePlayer("Taya", PlayerRole.Taya);
        taya.AddComponent<TayaCounteredStateController>();
        firstHider = CreatePlayer("Maria", PlayerRole.Hider);
        secondHider = CreatePlayer("Pedro", PlayerRole.Hider);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(secondHider);
        Object.DestroyImmediate(firstHider);
        Object.DestroyImmediate(taya);
        Object.DestroyImmediate(roundObject);
    }

    [Test]
    public void StartRound_TracksTimerAndHiders()
    {
        roundRules.StartRound(10f, true);

        Assert.That(roundRules.State, Is.EqualTo(PrototypeRoundState.Running));
        Assert.That(roundRules.Result, Is.EqualTo(PrototypeRoundResult.None));
        Assert.That(roundRules.TotalHiders, Is.EqualTo(2));
        Assert.That(roundRules.RemainingHiders, Is.EqualTo(2));
        Assert.That(roundRules.FormatRemainingTime(), Is.EqualTo("02:30"));
    }

    [Test]
    public void TayaWins_WhenAllHidersAreCaught()
    {
        roundRules.StartRound(0f, true);

        firstHider.GetComponent<CaughtStateController>().MarkCaught(roundRules, CaughtCause.Bang, 1);
        secondHider.GetComponent<CaughtStateController>().MarkCaught(roundRules, CaughtCause.Bang, 2);
        roundRules.Tick(12f);

        Assert.That(roundRules.State, Is.EqualTo(PrototypeRoundState.Finished));
        Assert.That(roundRules.Result, Is.EqualTo(PrototypeRoundResult.TayaWins));
        Assert.That(roundRules.RemainingHiders, Is.EqualTo(0));
    }

    [Test]
    public void HidersWin_WhenTayaIsCounteredBySak()
    {
        roundRules.StartRound(0f, true);

        taya.GetComponent<TayaCounteredStateController>().MarkCountered(roundRules, 77);
        roundRules.Tick(1f);

        Assert.That(roundRules.State, Is.EqualTo(PrototypeRoundState.Finished));
        Assert.That(roundRules.Result, Is.EqualTo(PrototypeRoundResult.HidersWin));
        Assert.That(roundRules.ResultMessage, Does.Contain("SAK"));
    }

    [Test]
    public void HidersWin_WhenTimerExpiresByDefault()
    {
        roundRules.StartRound(0f, true);

        roundRules.Tick(PrototypeRoundRulesController.DefaultRoundDurationSeconds + 0.1f);

        Assert.That(roundRules.State, Is.EqualTo(PrototypeRoundState.Finished));
        Assert.That(roundRules.Result, Is.EqualTo(PrototypeRoundResult.HidersWin));
        Assert.That(roundRules.ResultMessage, Does.Contain("Time"));
    }

    [Test]
    public void RestartRound_ResetsCaughtStateAndResult()
    {
        roundRules.StartRound(0f, true);
        firstHider.GetComponent<CaughtStateController>().MarkCaught(roundRules, CaughtCause.Bang, 1);
        secondHider.GetComponent<CaughtStateController>().MarkCaught(roundRules, CaughtCause.Bang, 2);
        roundRules.Tick(1f);

        roundRules.RestartRound();

        Assert.That(roundRules.State, Is.EqualTo(PrototypeRoundState.Running));
        Assert.That(roundRules.Result, Is.EqualTo(PrototypeRoundResult.None));
        Assert.That(firstHider.GetComponent<CaughtStateController>().IsCaught, Is.False);
        Assert.That(secondHider.GetComponent<CaughtStateController>().IsCaught, Is.False);
        Assert.That(roundRules.RemainingHiders, Is.EqualTo(2));
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
}
