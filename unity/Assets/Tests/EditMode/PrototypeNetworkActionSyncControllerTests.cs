using System.Collections.Generic;
using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.Network;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class PrototypeNetworkActionSyncControllerTests
{
    private GameObject actor;
    private PrototypeNetworkActionSyncController sync;
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        for (var index = 0; index < createdObjects.Count; index += 1)
        {
            if (createdObjects[index] != null)
            {
                Object.DestroyImmediate(createdObjects[index]);
            }
        }

        createdObjects.Clear();
    }

    [Test]
    public void ComponentContract_MatchesPhase26BangSakEventSync()
    {
        actor = CreatePlayer("JuanP", "preview-00", PlayerRole.Taya, Vector2.zero);
        sync = actor.AddComponent<PrototypeNetworkActionSyncController>();

        Assert.That(sync.ComponentIdValue, Is.EqualTo(PrototypeNetworkActionSyncController.ComponentId));
        Assert.That(sync.ComponentVersionValue, Is.EqualTo(1));
        Assert.That(sync.ComponentVariantValue, Is.EqualTo("phase26_bang_sak_event_sync"));
    }

    [Test]
    public void TryCaptureLocalActionEvent_CapturesBangNameHit()
    {
        actor = CreatePlayer("JuanP", "preview-00", PlayerRole.Taya, Vector2.zero);
        var hider = CreatePlayer("Maria", "preview-01", PlayerRole.Hider, Vector2.down);
        var identity = actor.GetComponent<PrototypeNetworkPlayerIdentity>();
        var nameCall = actor.GetComponent<BangNameCallController>();
        var bang = actor.GetComponent<BangActionController>();
        sync = actor.AddComponent<PrototypeNetworkActionSyncController>();
        sync.Configure(identity);
        nameCall.SetSelectedTargetName("Maria");
        bang.SetFallbackFacingDirection(PlayerFacingDirection.Down);
        Physics2D.SyncTransforms();

        Assert.That(bang.TryBang(0f), Is.True);
        Assert.That(sync.TryCaptureLocalActionEvent(0.1f, out var actionEvent), Is.True);

        Assert.That(actionEvent.Kind, Is.EqualTo(PrototypeNetworkActionKind.BangNameCall));
        Assert.That(actionEvent.Outcome, Is.EqualTo(PrototypeNetworkActionOutcome.BangHitTarget));
        Assert.That(actionEvent.ActorNetworkPlayerId, Is.EqualTo("preview-00"));
        Assert.That(actionEvent.TargetNetworkPlayerId, Is.EqualTo("preview-01"));
        Assert.That(actionEvent.CalledName, Is.EqualTo("Maria"));
        Assert.That(hider.GetComponent<CaughtStateController>().IsCaught, Is.True);
    }

    [Test]
    public void TryCaptureLocalActionEvent_CapturesSakCounter()
    {
        actor = CreatePlayer("Maria", "preview-01", PlayerRole.Hider, Vector2.zero);
        var taya = CreatePlayer("JuanP", "preview-00", PlayerRole.Taya, Vector2.down);
        var identity = actor.GetComponent<PrototypeNetworkPlayerIdentity>();
        var sak = actor.GetComponent<SakCounterController>();
        sync = actor.AddComponent<PrototypeNetworkActionSyncController>();
        sync.Configure(identity);
        sak.SetFallbackFacingDirection(PlayerFacingDirection.Down);
        Physics2D.SyncTransforms();

        Assert.That(sak.TrySak(0f), Is.True);
        Assert.That(sync.TryCaptureLocalActionEvent(0.1f, out var actionEvent), Is.True);

        Assert.That(actionEvent.Kind, Is.EqualTo(PrototypeNetworkActionKind.SakCounter));
        Assert.That(actionEvent.Outcome, Is.EqualTo(PrototypeNetworkActionOutcome.SakCounteredTaya));
        Assert.That(actionEvent.ActorNetworkPlayerId, Is.EqualTo("preview-01"));
        Assert.That(actionEvent.TargetNetworkPlayerId, Is.EqualTo("preview-00"));
        Assert.That(taya.GetComponent<TayaCounteredStateController>().IsCountered, Is.True);
    }

    [Test]
    public void ApplyRemoteActionEvent_BangHitMarksTargetCaught()
    {
        actor = CreatePlayer("JuanP", "preview-00", PlayerRole.Taya, Vector2.zero);
        var hider = CreatePlayer("Maria", "preview-01", PlayerRole.Hider, Vector2.down);
        sync = actor.AddComponent<PrototypeNetworkActionSyncController>();
        sync.Configure(actor.GetComponent<PrototypeNetworkPlayerIdentity>());

        var actionEvent = new PrototypeNetworkActionEvent(
            PrototypeNetworkActionKind.BangNameCall,
            PrototypeNetworkActionOutcome.BangHitTarget,
            "preview-remote-taya",
            "preview-01",
            "Maria",
            "Maria",
            Vector2.zero,
            Vector2.down,
            Vector2.down,
            PlayerFacingDirection.Down,
            7,
            1f);

        Assert.That(sync.ApplyRemoteActionEvent(actionEvent), Is.True);
        Assert.That(hider.GetComponent<CaughtStateController>().IsCaught, Is.True);
        Assert.That(sync.ApplyRemoteActionEvent(actionEvent), Is.False);
    }

    [Test]
    public void ApplyRemoteActionEvent_SakCounterMarksTayaCountered()
    {
        actor = CreatePlayer("Maria", "preview-01", PlayerRole.Hider, Vector2.zero);
        var taya = CreatePlayer("JuanP", "preview-00", PlayerRole.Taya, Vector2.down);
        sync = actor.AddComponent<PrototypeNetworkActionSyncController>();
        sync.Configure(actor.GetComponent<PrototypeNetworkPlayerIdentity>());

        var actionEvent = new PrototypeNetworkActionEvent(
            PrototypeNetworkActionKind.SakCounter,
            PrototypeNetworkActionOutcome.SakCounteredTaya,
            "preview-remote-hider",
            "preview-00",
            string.Empty,
            "JuanP",
            Vector2.zero,
            Vector2.down,
            Vector2.down,
            PlayerFacingDirection.Down,
            11,
            2f);

        Assert.That(sync.ApplyRemoteActionEvent(actionEvent), Is.True);
        Assert.That(taya.GetComponent<TayaCounteredStateController>().IsCountered, Is.True);
    }

    private GameObject CreatePlayer(string displayName, string networkId, PlayerRole role, Vector2 position)
    {
        var player = new GameObject(displayName);
        createdObjects.Add(player);
        player.transform.position = position;
        player.AddComponent<SpriteRenderer>();
        player.AddComponent<Rigidbody2D>().gravityScale = 0f;
        player.AddComponent<CircleCollider2D>();
        player.AddComponent<PlayerMovementController>();
        player.AddComponent<PlayerAnimationController>();
        player.AddComponent<PlayerNameIdentity>().SetDisplayName(displayName);
        player.AddComponent<BangNameCallController>();
        player.AddComponent<BangActionController>();
        player.AddComponent<BangHitTarget>();
        player.AddComponent<CaughtStateController>();
        player.AddComponent<SakCounterController>();
        player.AddComponent<TayaCounteredStateController>();
        player.AddComponent<PlayerRoleController>().SetRole(role);

        var identity = player.AddComponent<PrototypeNetworkPlayerIdentity>();
        identity.Configure(new PrototypeNetworkPlayerDescriptor(
            networkId,
            displayName,
            role,
            0,
            true,
            position,
            PlayerFacingDirection.Down));

        return player;
    }
}
