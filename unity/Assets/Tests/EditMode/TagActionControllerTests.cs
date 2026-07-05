using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class TagActionControllerTests
{
    private GameObject gameObject;
    private TagActionController controller;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject("Tag Action Test Player");
        gameObject.AddComponent<SpriteRenderer>();
        gameObject.AddComponent<Rigidbody2D>();
        gameObject.AddComponent<PlayerMovementController>();
        gameObject.AddComponent<PlayerAnimationController>();
        controller = gameObject.AddComponent<TagActionController>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void TagRange_IsShorterThanBangStyleRange()
    {
        Assert.That(controller.TagRange, Is.GreaterThan(0f));
        Assert.That(controller.TagRange, Is.LessThan(1f));
    }

    [Test]
    public void TryTag_UsesCooldown()
    {
        Assert.That(controller.TryTag(0f), Is.True);
        Assert.That(controller.TryTag(0.2f), Is.False);
        Assert.That(controller.CooldownRemaining(0.2f), Is.GreaterThan(0f));
        Assert.That(controller.TryTag(0.9f), Is.True);
    }

    [Test]
    public void CooldownProgress_ClampsBetweenZeroAndOne()
    {
        controller.TryTag(0f);

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

        var diagonal = controller.GetDirectionVector(PlayerFacingDirection.DownLeft);
        Assert.That(diagonal.x, Is.EqualTo(-0.707f).Within(0.001f));
        Assert.That(diagonal.y, Is.EqualTo(-0.707f).Within(0.001f));
    }

    [Test]
    public void TryTag_StoresLastTagDirection()
    {
        gameObject.GetComponent<PlayerAnimationController>().ApplyAnimation(new Vector2(1f, 1f), 0.2f);
        controller.TryTag(0f);

        Assert.That(controller.LastTagDirection, Is.EqualTo(PlayerFacingDirection.UpRight));
    }
}
