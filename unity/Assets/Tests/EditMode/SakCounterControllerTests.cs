using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class SakCounterControllerTests
{
    private GameObject hider;
    private GameObject taya;
    private GameObject wall;
    private SakCounterController sakController;
    private TayaCounteredStateController tayaCounteredState;

    [SetUp]
    public void SetUp()
    {
        hider = CreatePlayer("Hider", Vector3.zero, PlayerRole.Hider);
        sakController = hider.AddComponent<SakCounterController>();
        hider.GetComponent<PlayerRoleController>().SetRole(PlayerRole.Hider);
        sakController.SetFallbackFacingDirection(PlayerFacingDirection.Right);

        taya = CreatePlayer("Taya", new Vector3(0.58f, 0f, 0f), PlayerRole.Taya);
        tayaCounteredState = taya.AddComponent<TayaCounteredStateController>();
        taya.GetComponent<PlayerRoleController>().SetRole(PlayerRole.Taya);

        Physics2D.SyncTransforms();
    }

    [TearDown]
    public void TearDown()
    {
        if (wall != null)
        {
            Object.DestroyImmediate(wall);
        }

        if (taya != null)
        {
            Object.DestroyImmediate(taya);
        }

        if (hider != null)
        {
            Object.DestroyImmediate(hider);
        }
    }

    [Test]
    public void TrySak_CountersTayaAtCloseRange()
    {
        Assert.That(sakController.TrySak(0f), Is.True);

        Assert.That(sakController.LastResult.Outcome, Is.EqualTo(SakCounterOutcome.CounteredTaya));
        Assert.That(sakController.LastResult.Target, Is.EqualTo(tayaCounteredState));
        Assert.That(tayaCounteredState.IsCountered, Is.True);
        Assert.That(tayaCounteredState.CounteredCount, Is.EqualTo(1));
    }

    [Test]
    public void ResolveSakHit_MissesWhenTayaIsTooFar()
    {
        taya.transform.position = new Vector3(1.8f, 0f, 0f);
        Physics2D.SyncTransforms();

        var result = sakController.ResolveSakHit(Vector3.zero, PlayerFacingDirection.Right, 1);

        Assert.That(result.Outcome, Is.EqualTo(SakCounterOutcome.Miss));
        Assert.That(tayaCounteredState.IsCountered, Is.False);
    }

    [Test]
    public void ResolveSakHit_IsBlockedByWallBeforeTaya()
    {
        wall = new GameObject("Wall Before Taya");
        wall.transform.position = new Vector3(0.28f, 0f, 0f);
        var wallCollider = wall.AddComponent<BoxCollider2D>();
        wallCollider.size = new Vector2(0.12f, 0.9f);
        Physics2D.SyncTransforms();

        var result = sakController.ResolveSakHit(Vector3.zero, PlayerFacingDirection.Right, 1);

        Assert.That(result.Outcome, Is.EqualTo(SakCounterOutcome.Blocked));
        Assert.That(tayaCounteredState.IsCountered, Is.False);
    }

    [Test]
    public void TrySak_UsesCooldown()
    {
        Assert.That(sakController.TrySak(0f), Is.True);
        Assert.That(sakController.TrySak(0.5f), Is.False);
        Assert.That(sakController.CooldownRemaining(0.5f), Is.GreaterThan(0f));
        Assert.That(sakController.TrySak(1.3f), Is.True);
    }

    [Test]
    public void TayaRoleCannotUseSak()
    {
        hider.GetComponent<PlayerRoleController>().SetRole(PlayerRole.Taya);

        Assert.That(sakController.CanSak(0f), Is.False);
        Assert.That(sakController.TrySak(0f), Is.False);
    }

    private static GameObject CreatePlayer(string name, Vector3 position, PlayerRole role)
    {
        var player = new GameObject(name);
        player.transform.position = position;
        player.AddComponent<SpriteRenderer>();
        var collider = player.AddComponent<CircleCollider2D>();
        collider.radius = 0.2f;
        player.AddComponent<PlayerRoleController>().SetRole(role);
        return player;
    }
}
