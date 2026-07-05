using System.Collections.Generic;
using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class TagHitPhysicsTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();
    private GameObject tagger;
    private TagActionController controller;

    [SetUp]
    public void SetUp()
    {
        tagger = CreatePlayerLikeObject("Tagger", Vector3.zero, withTarget: true);
        tagger.AddComponent<Rigidbody2D>();
        tagger.AddComponent<PlayerMovementController>();
        tagger.AddComponent<PlayerAnimationController>();
        controller = tagger.AddComponent<TagActionController>();
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
    public void ResolveTagHit_HitsTargetAtCloseRange()
    {
        var target = CreatePlayerLikeObject("Close Target", new Vector3(0.58f, 0f, 0f), withTarget: true);
        var tagTarget = target.GetComponent<TagHitTarget>();
        Physics2D.SyncTransforms();

        var result = controller.ResolveTagHit(Vector3.zero, PlayerFacingDirection.Right, 201);

        Assert.That(result.Outcome, Is.EqualTo(TagHitOutcome.HitTarget));
        Assert.That(result.Target, Is.EqualTo(tagTarget));
        Assert.That(result.Distance, Is.LessThanOrEqualTo(controller.TagRange));
        Assert.That(tagTarget.TagHitCount, Is.EqualTo(1));
        Assert.That(tagTarget.LastTagSource, Is.EqualTo(controller));
    }

    [Test]
    public void ResolveTagHit_MissesTargetOutsideCloseRange()
    {
        var target = CreatePlayerLikeObject("Far Target", new Vector3(1.6f, 0f, 0f), withTarget: true);
        var tagTarget = target.GetComponent<TagHitTarget>();
        Physics2D.SyncTransforms();

        var result = controller.ResolveTagHit(Vector3.zero, PlayerFacingDirection.Right, 202);

        Assert.That(result.Outcome, Is.EqualTo(TagHitOutcome.Miss));
        Assert.That(result.Target, Is.Null);
        Assert.That(tagTarget.TagHitCount, Is.EqualTo(0));
    }

    [Test]
    public void ResolveTagHit_IsBlockedByWallBeforeTarget()
    {
        CreateWall("Close Wall", new Vector3(0.35f, 0f, 0f));
        var target = CreatePlayerLikeObject("Target Behind Wall", new Vector3(0.68f, 0f, 0f), withTarget: true);
        var tagTarget = target.GetComponent<TagHitTarget>();
        Physics2D.SyncTransforms();

        var result = controller.ResolveTagHit(Vector3.zero, PlayerFacingDirection.Right, 203);

        Assert.That(result.Outcome, Is.EqualTo(TagHitOutcome.Blocked));
        Assert.That(result.WasBlocked, Is.True);
        Assert.That(result.Target, Is.Null);
        Assert.That(tagTarget.TagHitCount, Is.EqualTo(0));
    }

    [Test]
    public void ResolveTagHit_RegistersOnlyOneHitWhenTargetHasMultipleColliders()
    {
        var target = CreatePlayerLikeObject("Target With Multiple Colliders", new Vector3(0.58f, 0f, 0f), withTarget: true);
        var secondCollider = target.AddComponent<CircleCollider2D>();
        secondCollider.radius = 0.16f;
        secondCollider.offset = new Vector2(0.04f, 0f);
        var tagTarget = target.GetComponent<TagHitTarget>();
        Physics2D.SyncTransforms();

        var result = controller.ResolveTagHit(Vector3.zero, PlayerFacingDirection.Right, 204);

        Assert.That(result.Outcome, Is.EqualTo(TagHitOutcome.HitTarget));
        Assert.That(tagTarget.TagHitCount, Is.EqualTo(1));
    }

    [Test]
    public void TryTag_StoresLastTagResult()
    {
        CreatePlayerLikeObject("Close Target", new Vector3(0.58f, 0f, 0f), withTarget: true);
        tagger.GetComponent<PlayerAnimationController>().ApplyAnimation(Vector2.right, 0.2f);
        Physics2D.SyncTransforms();

        Assert.That(controller.TryTag(0f), Is.True);

        Assert.That(controller.LastTagResult.Outcome, Is.EqualTo(TagHitOutcome.HitTarget));
        Assert.That(controller.LastTagResult.SequenceId, Is.GreaterThan(0));
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
            obj.AddComponent<TagHitTarget>();
        }

        return obj;
    }

    private void CreateWall(string name, Vector3 position)
    {
        var wall = new GameObject(name);
        createdObjects.Add(wall);
        wall.transform.position = position;
        var collider = wall.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.18f, 1.1f);
    }
}
