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
    public void DefaultVisualStyle_IsSafeCartoonLightBeam()
    {
        Assert.That(controller.VisualStyle, Is.EqualTo(BangActionVisualStyle.CartoonLightBeam));
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
    public void TryBang_StoresLastBangDirection()
    {
        gameObject.GetComponent<PlayerAnimationController>().ApplyAnimation(new Vector2(-1f, 1f), 0.2f);
        controller.TryBang(0f);

        Assert.That(controller.LastBangDirection, Is.EqualTo(PlayerFacingDirection.UpLeft));
    }
}
