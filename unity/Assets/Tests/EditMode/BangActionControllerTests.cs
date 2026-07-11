using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class BangActionControllerTests
{
    private GameObject gameObject;
    private BangActionController controller;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject("Bang Action Test Player");
        gameObject.AddComponent<SpriteRenderer>();
        gameObject.AddComponent<Rigidbody2D>();
        gameObject.AddComponent<PlayerMovementController>();
        gameObject.AddComponent<PlayerAnimationController>();
        controller = gameObject.AddComponent<BangActionController>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void DefaultVisualStyle_IsSafeTsinelasMarker()
    {
        Assert.That(controller.VisualStyle, Is.EqualTo(BangActionVisualStyle.TsinelasMarker));
    }

    [Test]
    public void TryBang_UsesCooldown()
    {
        Assert.That(controller.TryBang(0f), Is.True);
        Assert.That(controller.TryBang(0.5f), Is.False);
        Assert.That(controller.CooldownRemaining(0.5f), Is.GreaterThan(0f));
        Assert.That(controller.TryBang(1.3f), Is.True);
    }

    [Test]
    public void CooldownProgress_ClampsBetweenZeroAndOne()
    {
        controller.TryBang(0f);

        Assert.That(controller.CooldownProgress(0f), Is.EqualTo(0f).Within(0.001f));
        Assert.That(controller.CooldownProgress(100f), Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void NamedBangCooldown_IsIndependentForEachHider()
    {
        gameObject.AddComponent<BangNameCallController>();
        var ana = CreateHider("Ana");
        var pedro = CreateHider("Pedro");

        try
        {
            Assert.That(controller.TryBangTarget("Ana", 0f), Is.True);
            Assert.That(controller.CanBangTarget("Ana", 0.2f), Is.False);
            Assert.That(controller.CanBangTarget("Pedro", 0.2f), Is.True);

            Assert.That(controller.TryBangTarget("Pedro", 0.2f), Is.True);
            Assert.That(controller.CanBangTarget("Ana", 0.3f), Is.False);
            Assert.That(controller.CanBangTarget("Pedro", 0.3f), Is.False);
            Assert.That(controller.CanBangTarget("Ana", 1.3f), Is.True);
            Assert.That(controller.CanBangTarget("Pedro", 1.3f), Is.False);
        }
        finally
        {
            Object.DestroyImmediate(ana);
            Object.DestroyImmediate(pedro);
        }
    }

    private static GameObject CreateHider(string displayName)
    {
        var hider = new GameObject(displayName);
        hider.AddComponent<PlayerNameIdentity>().SetDisplayName(displayName);
        hider.AddComponent<PlayerRoleController>().SetRole(PlayerRole.Hider);
        return hider;
    }

    [Test]
    public void GetDirectionVector_SupportsEightDirections()
    {
        Assert.That(controller.GetDirectionVector(PlayerFacingDirection.Up), Is.EqualTo(Vector2.up));
        Assert.That(controller.GetDirectionVector(PlayerFacingDirection.Down), Is.EqualTo(Vector2.down));
        Assert.That(controller.GetDirectionVector(PlayerFacingDirection.Left), Is.EqualTo(Vector2.left));
        Assert.That(controller.GetDirectionVector(PlayerFacingDirection.Right), Is.EqualTo(Vector2.right));

        var diagonal = controller.GetDirectionVector(PlayerFacingDirection.UpRight);
        Assert.That(diagonal.x, Is.EqualTo(0.707f).Within(0.001f));
        Assert.That(diagonal.y, Is.EqualTo(0.707f).Within(0.001f));
    }

    [Test]
    public void GetEffectPosition_FollowsFacingDirection()
    {
        var origin = new Vector3(2f, 3f, 0f);
        var position = controller.GetEffectPosition(origin, PlayerFacingDirection.Right);

        Assert.That(position.x, Is.EqualTo(origin.x + controller.MarkerDistance).Within(0.001f));
        Assert.That(position.y, Is.EqualTo(origin.y).Within(0.001f));
    }

    [Test]
    public void GetRangeConeRotationZ_AlignsUpPointingConeWithFacingDirection()
    {
        Assert.That(controller.GetRangeConeRotationZ(PlayerFacingDirection.Up), Is.EqualTo(0f).Within(0.001f));
        Assert.That(controller.GetRangeConeRotationZ(PlayerFacingDirection.Right), Is.EqualTo(-90f).Within(0.001f));
        Assert.That(controller.GetRangeConeRotationZ(PlayerFacingDirection.Down), Is.EqualTo(-180f).Within(0.001f));
    }

    [Test]
    public void TryBang_StoresLastBangDirection()
    {
        gameObject.GetComponent<PlayerAnimationController>().ApplyAnimation(new Vector2(-1f, 1f), 0.2f);
        controller.TryBang(0f);

        Assert.That(controller.LastBangDirection, Is.EqualTo(PlayerFacingDirection.UpLeft));
    }
}
