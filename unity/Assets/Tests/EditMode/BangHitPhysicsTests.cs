using System.Collections.Generic;
using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class BangHitPhysicsTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();
    private GameObject shooter;
    private BangActionController controller;

    [SetUp]
    public void SetUp()
    {
        shooter = CreatePlayerLikeObject("Shooter", Vector3.zero, withTarget: true);
        shooter.AddComponent<Rigidbody2D>();
        shooter.AddComponent<PlayerMovementController>();
        shooter.AddComponent<PlayerAnimationController>();
        controller = shooter.AddComponent<BangActionController>();
    }

    [TearDown]
    public void TearDown()
    {
        for (var index = createdObjects.Count - 1; index >= 0; index -= 1)
        {
            Object.DestroyImmediate(createdObjects[index]);
        }

        createdObjects.Clear();
    }

    [Test]
    public void ResolveBangHit_HitsTargetPlayerInRange()
    {
        var target = CreatePlayerLikeObject("Target", new Vector3(1.2f, 0f, 0f), withTarget: true);
        var hitTarget = target.GetComponent<BangHitTarget>();
        Physics2D.SyncTransforms();

        var result = controller.ResolveBangHit(Vector3.zero, PlayerFacingDirection.Right, 101);

        Assert.That(result.Outcome, Is.EqualTo(BangHitOutcome.HitTarget));
        Assert.That(result.Target, Is.EqualTo(hitTarget));
        Assert.That(result.Distance, Is.LessThanOrEqualTo(controller.Range));
        Assert.That(hitTarget.HitCount, Is.EqualTo(1));
        Assert.That(hitTarget.LastHitSource, Is.EqualTo(controller));
    }

    [Test]
    public void ResolveBangHit_MissesTargetOutsideRange()
    {
        var target = CreatePlayerLikeObject("Far Target", new Vector3(4f, 0f, 0f), withTarget: true);
        var hitTarget = target.GetComponent<BangHitTarget>();
        Physics2D.SyncTransforms();

        var result = controller.ResolveBangHit(Vector3.zero, PlayerFacingDirection.Right, 102);

        Assert.That(result.Outcome, Is.EqualTo(BangHitOutcome.Miss));
        Assert.That(result.Target, Is.Null);
        Assert.That(hitTarget.HitCount, Is.EqualTo(0));
    }

    [Test]
    public void ResolveBangHit_IsBlockedByWallBeforeTarget()
    {
        CreateWall("Wall", new Vector3(0.8f, 0f, 0f));
        var target = CreatePlayerLikeObject("Target Behind Wall", new Vector3(1.4f, 0f, 0f), withTarget: true);
        var hitTarget = target.GetComponent<BangHitTarget>();
        Physics2D.SyncTransforms();

        var result = controller.ResolveBangHit(Vector3.zero, PlayerFacingDirection.Right, 103);

        Assert.That(result.Outcome, Is.EqualTo(BangHitOutcome.Blocked));
        Assert.That(result.WasBlocked, Is.True);
        Assert.That(result.Target, Is.Null);
        Assert.That(hitTarget.HitCount, Is.EqualTo(0));
    }

    [Test]
    public void ResolveBangHit_RegistersOnlyOneHitWhenTargetHasMultipleColliders()
    {
        var target = CreatePlayerLikeObject("Target With Multiple Colliders", new Vector3(1.2f, 0f, 0f), withTarget: true);
        var secondCollider = target.AddComponent<CircleCollider2D>();
        secondCollider.radius = 0.18f;
        secondCollider.offset = new Vector2(0.06f, 0f);
        var hitTarget = target.GetComponent<BangHitTarget>();
        Physics2D.SyncTransforms();

        var result = controller.ResolveBangHit(Vector3.zero, PlayerFacingDirection.Right, 104);

        Assert.That(result.Outcome, Is.EqualTo(BangHitOutcome.HitTarget));
        Assert.That(hitTarget.HitCount, Is.EqualTo(1));
    }

    [Test]
    public void TryBang_StoresLastHitResult()
    {
        CreatePlayerLikeObject("Target", new Vector3(1.2f, 0f, 0f), withTarget: true);
        shooter.GetComponent<PlayerAnimationController>().ApplyAnimation(Vector2.right, 0.2f);
        Physics2D.SyncTransforms();

        Assert.That(controller.TryBang(0f), Is.True);

        Assert.That(controller.LastHitResult.Outcome, Is.EqualTo(BangHitOutcome.HitTarget));
        Assert.That(controller.LastHitResult.SequenceId, Is.GreaterThan(0));
    }

    private GameObject CreatePlayerLikeObject(string name, Vector3 position, bool withTarget)
    {
        var obj = new GameObject(name);
        createdObjects.Add(obj);
        obj.transform.position = position;
        obj.AddComponent<SpriteRenderer>();
        var collider = obj.AddComponent<CircleCollider2D>();
        collider.radius = 0.28f;
        collider.offset = new Vector2(0f, -0.08f);

        if (withTarget)
        {
            obj.AddComponent<BangHitTarget>();
        }

        return obj;
    }

    private void CreateWall(string name, Vector3 position)
    {
        var wall = new GameObject(name);
        createdObjects.Add(wall);
        wall.transform.position = position;
        var collider = wall.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.25f, 1.25f);
    }
}
