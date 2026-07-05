using NUnit.Framework;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class PlayerAnimationControllerTests
{
    private GameObject gameObject;
    private SpriteRenderer spriteRenderer;
    private PlayerAnimationController animationController;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject("Animation Test Player");
        gameObject.AddComponent<Rigidbody2D>();
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        gameObject.AddComponent<PlayerMovementController>();
        animationController = gameObject.AddComponent<PlayerAnimationController>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void ResolveFacingDirection_PrefersDominantHorizontalAxis()
    {
        Assert.That(
            animationController.ResolveFacingDirection(new Vector2(-1f, 0.2f), PlayerFacingDirection.Down),
            Is.EqualTo(PlayerFacingDirection.Left));

        Assert.That(
            animationController.ResolveFacingDirection(new Vector2(1f, 0.2f), PlayerFacingDirection.Down),
            Is.EqualTo(PlayerFacingDirection.Right));
    }

    [Test]
    public void ResolveFacingDirection_UsesVerticalAxisWhenDominant()
    {
        Assert.That(
            animationController.ResolveFacingDirection(new Vector2(0.1f, 1f), PlayerFacingDirection.Down),
            Is.EqualTo(PlayerFacingDirection.Up));

        Assert.That(
            animationController.ResolveFacingDirection(new Vector2(0.1f, -1f), PlayerFacingDirection.Up),
            Is.EqualTo(PlayerFacingDirection.Down));
    }

    [Test]
    public void ResolveFacingDirection_KeepsFallbackWhenStationary()
    {
        Assert.That(
            animationController.ResolveFacingDirection(Vector2.zero, PlayerFacingDirection.Left),
            Is.EqualTo(PlayerFacingDirection.Left));
    }

    [Test]
    public void ApplyAnimation_FlipsSpriteWhenFacingLeft()
    {
        animationController.ApplyAnimation(Vector2.left, 0.2f);

        Assert.That(animationController.FacingDirection, Is.EqualTo(PlayerFacingDirection.Left));
        Assert.That(spriteRenderer.flipX, Is.True);
    }

    [Test]
    public void ApplyAnimation_UnflipsSpriteWhenFacingRight()
    {
        animationController.ApplyAnimation(Vector2.left, 0.2f);
        animationController.ApplyAnimation(Vector2.right, 0.2f);

        Assert.That(animationController.FacingDirection, Is.EqualTo(PlayerFacingDirection.Right));
        Assert.That(spriteRenderer.flipX, Is.False);
    }

    [Test]
    public void ResolveNextWalkFrameIndex_LoopsThroughFrames()
    {
        animationController.FramesPerSecond = 8f;

        Assert.That(animationController.ResolveNextWalkFrameIndex(3, 4, 0.125f), Is.EqualTo(0));
    }

    [Test]
    public void ApplyAnimation_ReportsWalkingOnlyWhenInputPassesThreshold()
    {
        animationController.ApplyAnimation(new Vector2(0.001f, 0f), 0.2f);
        Assert.That(animationController.IsWalking, Is.False);

        animationController.ApplyAnimation(Vector2.up, 0.2f);
        Assert.That(animationController.IsWalking, Is.True);
    }
}
