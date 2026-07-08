using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using Palengke.BangSak.UI;
using UnityEngine;

public sealed class PlayerRoleControllerTests
{
    private GameObject player;
    private BangActionController bang;
    private SakCounterController sak;
    private CaughtStateController caughtState;
    private PlayerRoleController roleController;

    [SetUp]
    public void SetUp()
    {
        player = new GameObject("Role Test Player");
        player.AddComponent<SpriteRenderer>();
        player.AddComponent<Rigidbody2D>();
        player.AddComponent<CircleCollider2D>();
        player.AddComponent<PlayerMovementController>();
        player.AddComponent<PlayerAnimationController>();
        bang = player.AddComponent<BangActionController>();
        player.AddComponent<BangActionHud>();
        caughtState = player.AddComponent<CaughtStateController>();
        roleController = player.AddComponent<PlayerRoleController>();
        sak = player.AddComponent<SakCounterController>();
        player.AddComponent<SakCounterHud>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(player);
    }

    [Test]
    public void SetRole_TayaEnablesBangAndDoesNotCountAsHider()
    {
        roleController.SetRole(PlayerRole.Taya);

        Assert.That(roleController.Role, Is.EqualTo(PlayerRole.Taya));
        Assert.That(roleController.CanUseBang, Is.True);
        Assert.That(roleController.CanUseSak, Is.False);
        Assert.That(bang.enabled, Is.True);
        Assert.That(sak.enabled, Is.False);
        Assert.That(caughtState.CountAsHider, Is.False);
    }

    [Test]
    public void SetRole_HiderDisablesBangAndCountsAsHider()
    {
        roleController.SetRole(PlayerRole.Hider);

        Assert.That(roleController.Role, Is.EqualTo(PlayerRole.Hider));
        Assert.That(roleController.CanUseBang, Is.False);
        Assert.That(roleController.CanUseSak, Is.True);
        Assert.That(bang.enabled, Is.False);
        Assert.That(sak.enabled, Is.True);
        Assert.That(caughtState.CountAsHider, Is.True);
    }

    [Test]
    public void RolesAreGameplayOnlyWithoutFloatingLabels()
    {
        roleController.SetRole(PlayerRole.Taya);
        Assert.That(roleController.IsTaya, Is.True);
        Assert.That(roleController.IsHider, Is.False);
        Assert.That(player.GetComponentInChildren<TextMesh>(true), Is.Null);

        roleController.SetRole(PlayerRole.Hider);
        Assert.That(roleController.IsTaya, Is.False);
        Assert.That(roleController.IsHider, Is.True);
        Assert.That(player.GetComponentInChildren<TextMesh>(true), Is.Null);
    }
}
