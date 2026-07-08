using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.Network;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class PrototypeNetworkMovementSyncControllerTests
{
    private GameObject playerObject;
    private PrototypeNetworkPlayerIdentity identity;
    private PlayerMovementController movement;
    private PrototypeNetworkMovementSyncController sync;

    [SetUp]
    public void SetUp()
    {
        playerObject = new GameObject("Network Sync Player");
        playerObject.AddComponent<SpriteRenderer>();
        playerObject.AddComponent<Rigidbody2D>();
        movement = playerObject.AddComponent<PlayerMovementController>();
        playerObject.AddComponent<PlayerAnimationController>();
        identity = playerObject.AddComponent<PrototypeNetworkPlayerIdentity>();
        sync = playerObject.AddComponent<PrototypeNetworkMovementSyncController>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(playerObject);
    }

    [Test]
    public void ComponentContract_MatchesPhase25SnapshotSmoothing()
    {
        Assert.That(sync.ComponentIdValue, Is.EqualTo(PrototypeNetworkMovementSyncController.ComponentId));
        Assert.That(sync.ComponentVersionValue, Is.EqualTo(1));
        Assert.That(sync.ComponentVariantValue, Is.EqualTo("phase25_snapshot_smoothing"));
    }

    [Test]
    public void Configure_LocalIdentityKeepsMovementAuthority()
    {
        identity.Configure(CreateDescriptor(isLocalPlayer: true));

        sync.Configure(identity);

        Assert.That(sync.Authority, Is.EqualTo(PrototypeNetworkMovementAuthority.LocalAuthority));
        Assert.That(movement.enabled, Is.True);
        Assert.That(movement.ReadsKeyboardInput, Is.True);
    }

    [Test]
    public void Configure_RemoteIdentityDisablesLocalInput()
    {
        identity.Configure(CreateDescriptor(isLocalPlayer: false));

        sync.Configure(identity);

        Assert.That(sync.Authority, Is.EqualTo(PrototypeNetworkMovementAuthority.RemoteReplica));
        Assert.That(movement.enabled, Is.False);
        Assert.That(movement.ReadsKeyboardInput, Is.False);
    }

    [Test]
    public void CaptureSnapshot_IncrementsSequenceAndIncludesIdentity()
    {
        identity.Configure(CreateDescriptor(isLocalPlayer: true));
        playerObject.transform.position = new Vector3(2f, -3f, 0f);
        sync.Configure(identity);

        var first = sync.CaptureSnapshot(1f);
        var second = sync.CaptureSnapshot(1.1f);

        Assert.That(first.NetworkPlayerId, Is.EqualTo("preview-test"));
        Assert.That(first.Position, Is.EqualTo(new Vector2(2f, -3f)));
        Assert.That(first.Sequence, Is.EqualTo(1));
        Assert.That(second.Sequence, Is.EqualTo(2));
        Assert.That(second.IsNewerThan(first), Is.True);
    }

    [Test]
    public void ApplyRemoteSnapshot_AcceptsNewerSnapshotsOnly()
    {
        identity.Configure(CreateDescriptor(isLocalPlayer: false));
        sync.Configure(identity);

        var first = new PrototypeNetworkMovementSnapshot(
            "preview-test",
            new Vector2(1f, 1f),
            Vector2.right,
            PlayerFacingDirection.Right,
            5,
            10f);
        var older = new PrototypeNetworkMovementSnapshot(
            "preview-test",
            new Vector2(2f, 2f),
            Vector2.up,
            PlayerFacingDirection.Up,
            4,
            11f);

        Assert.That(sync.ApplyRemoteSnapshot(first), Is.True);
        Assert.That(sync.ApplyRemoteSnapshot(older), Is.False);
        Assert.That(sync.LastAppliedRemoteSnapshot.Sequence, Is.EqualTo(5));
    }

    [Test]
    public void ResolveSmoothedPosition_MovesTowardRemoteTarget()
    {
        sync.RemoteInterpolationSpeed = 10f;

        var current = Vector3.zero;
        var target = new Vector3(10f, 0f, 0f);
        var next = sync.ResolveSmoothedPosition(current, target, 0.1f);

        Assert.That(next.x, Is.GreaterThan(0f));
        Assert.That(next.x, Is.LessThan(10f));
    }

    private static PrototypeNetworkPlayerDescriptor CreateDescriptor(bool isLocalPlayer)
    {
        return new PrototypeNetworkPlayerDescriptor(
            "preview-test",
            "JuanP",
            PlayerRole.Taya,
            0,
            isLocalPlayer,
            Vector2.zero,
            PlayerFacingDirection.Down);
    }
}
