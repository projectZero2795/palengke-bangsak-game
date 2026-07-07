using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class CaughtStateControllerTests
{
    private GameObject player;
    private PlayerMovementController movement;
    private BangActionController bang;
    private CaughtStateController caughtState;

    [SetUp]
    public void SetUp()
    {
        player = new GameObject("Catchable Player");
        player.AddComponent<SpriteRenderer>();
        player.AddComponent<Rigidbody2D>();
        movement = player.AddComponent<PlayerMovementController>();
        player.AddComponent<PlayerAnimationController>();
        bang = player.AddComponent<BangActionController>();
        caughtState = player.AddComponent<CaughtStateController>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(player);
    }

    [Test]
    public void MarkCaught_DisablesMovementAndActions()
    {
        var changed = caughtState.MarkCaught(bang, CaughtCause.Bang, 301);

        Assert.That(changed, Is.True);
        Assert.That(caughtState.IsCaught, Is.True);
        Assert.That(caughtState.Cause, Is.EqualTo(CaughtCause.Bang));
        Assert.That(caughtState.CatchSequenceId, Is.EqualTo(301));
        Assert.That(caughtState.LastCaughtSource, Is.EqualTo(bang));
        Assert.That(movement.enabled, Is.False);
        Assert.That(bang.enabled, Is.False);
        Assert.That(player.GetComponentInChildren<TextMesh>(true), Is.Null);

        var starRoot = player.transform.Find("Caught Dizzy Stars");
        Assert.That(starRoot, Is.Not.Null);
        Assert.That(starRoot.gameObject.activeSelf, Is.True);
        Assert.That(starRoot.GetComponentsInChildren<SpriteRenderer>(true).Length, Is.EqualTo(3));
    }

    [Test]
    public void ResetCaughtState_RestoresMovementAndActions()
    {
        caughtState.MarkCaught(bang, CaughtCause.Bang, 302);

        caughtState.ResetCaughtState();

        Assert.That(caughtState.IsCaught, Is.False);
        Assert.That(caughtState.Cause, Is.EqualTo(CaughtCause.None));
        Assert.That(caughtState.CatchSequenceId, Is.EqualTo(-1));
        Assert.That(caughtState.LastCaughtSource, Is.Null);
        Assert.That(movement.enabled, Is.True);
        Assert.That(bang.enabled, Is.True);
        Assert.That(player.transform.Find("Caught Dizzy Stars").gameObject.activeSelf, Is.False);
    }

    [Test]
    public void MarkCaught_DoesNotReapplyWhenAlreadyCaught()
    {
        Assert.That(caughtState.MarkCaught(bang, CaughtCause.Bang, 303), Is.True);

        Assert.That(caughtState.MarkCaught(bang, CaughtCause.Bang, 304), Is.False);

        Assert.That(caughtState.Cause, Is.EqualTo(CaughtCause.Bang));
        Assert.That(caughtState.CatchSequenceId, Is.EqualTo(303));
    }
}
