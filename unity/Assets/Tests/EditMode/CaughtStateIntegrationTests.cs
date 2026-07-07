using System.Collections.Generic;
using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class CaughtStateIntegrationTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

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
    public void BangHitTarget_MarksCaughtState()
    {
        var source = CreateActionSource("Bang Source").AddComponent<BangActionController>();
        var target = CreateTarget("Bang Target");
        var hitTarget = target.GetComponent<BangHitTarget>();
        var caughtState = target.GetComponent<CaughtStateController>();
        var collider = target.GetComponent<Collider2D>();

        hitTarget.RegisterBangHit(
            source,
            401,
            BangHitResult.HitTarget(hitTarget, collider, Vector2.zero, Vector2.right, Vector2.right, 1f, 401));

        Assert.That(caughtState.IsCaught, Is.True);
        Assert.That(caughtState.Cause, Is.EqualTo(CaughtCause.Bang));
        Assert.That(caughtState.LastCaughtSource, Is.EqualTo(source));
    }

    [Test]
    public void CaughtPlayer_CannotTriggerBangAgain()
    {
        var player = CreateActionSource("Caught Player");
        var bang = player.AddComponent<BangActionController>();
        var caughtState = player.AddComponent<CaughtStateController>();

        caughtState.MarkCaught(bang, CaughtCause.Bang, 403);

        Assert.That(bang.TryBang(0f), Is.False);
    }

    private GameObject CreateActionSource(string name)
    {
        var obj = new GameObject(name);
        createdObjects.Add(obj);
        obj.AddComponent<SpriteRenderer>();
        obj.AddComponent<Rigidbody2D>();
        obj.AddComponent<PlayerMovementController>();
        obj.AddComponent<PlayerAnimationController>();
        return obj;
    }

    private GameObject CreateTarget(string name)
    {
        var obj = new GameObject(name);
        createdObjects.Add(obj);
        obj.AddComponent<SpriteRenderer>();
        obj.AddComponent<CircleCollider2D>();
        obj.AddComponent<CaughtStateController>();
        obj.AddComponent<BangHitTarget>();
        return obj;
    }
}
