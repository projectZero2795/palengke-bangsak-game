using System.Collections.Generic;
using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class Phase18NamedBangTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();
    private GameObject shooter;
    private BangActionController bang;
    private BangNameCallController nameCall;

    [SetUp]
    public void SetUp()
    {
        shooter = CreateShooter("JuanP");
        nameCall = shooter.AddComponent<BangNameCallController>();
        bang = shooter.AddComponent<BangActionController>();
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
    public void ResolveBangHit_CorrectCalledNameCatchesHider()
    {
        var maria = CreateNamedHider("Maria", new Vector3(1.2f, 0f, 0f));
        CreateNamedHider("Pedro", new Vector3(0f, 1.2f, 0f));
        var caughtState = maria.GetComponent<CaughtStateController>();

        nameCall.SetSelectedTargetName("Maria");
        Physics2D.SyncTransforms();

        var result = bang.ResolveBangHit(Vector3.zero, PlayerFacingDirection.Right, 1801);

        Assert.That(result.Outcome, Is.EqualTo(BangHitOutcome.HitTarget));
        Assert.That(nameCall.LastValidationOutcome, Is.EqualTo(BangNameValidationOutcome.Valid));
        Assert.That(caughtState.IsCaught, Is.True);
        Assert.That(result.FeedbackMessage, Is.EqualTo(string.Empty));
    }

    [Test]
    public void ResolveBangHit_WrongCalledNameDoesNotCatchHider()
    {
        var maria = CreateNamedHider("Maria", new Vector3(1.2f, 0f, 0f));
        CreateNamedHider("Pedro", new Vector3(0f, 1.2f, 0f));
        var hitTarget = maria.GetComponent<BangHitTarget>();
        var caughtState = maria.GetComponent<CaughtStateController>();

        nameCall.SetSelectedTargetName("Pedro");
        Physics2D.SyncTransforms();

        var result = bang.ResolveBangHit(Vector3.zero, PlayerFacingDirection.Right, 1802);

        Assert.That(result.Outcome, Is.EqualTo(BangHitOutcome.NameMismatch));
        Assert.That(result.WasNameMismatch, Is.True);
        Assert.That(result.DidHitTarget, Is.True);
        Assert.That(result.Target, Is.EqualTo(hitTarget));
        Assert.That(result.FeedbackMessage, Does.Contain("Wrong name"));
        Assert.That(nameCall.LastValidationOutcome, Is.EqualTo(BangNameValidationOutcome.WrongName));
        Assert.That(nameCall.LastFeedbackMessage, Does.Contain("called Pedro"));
        Assert.That(caughtState.IsCaught, Is.False);
        Assert.That(hitTarget.HitCount, Is.EqualTo(0));
        Assert.That(hitTarget.NameMismatchCount, Is.EqualTo(1));
    }

    [Test]
    public void PlayerNameIdentity_MatchesCalledNameIgnoringCaseAndExtraSpaces()
    {
        var hider = CreateNamedHider("Maria Clara", Vector3.right);
        var identity = hider.GetComponent<PlayerNameIdentity>();

        Assert.That(identity.MatchesCalledName(" maria   clara "), Is.True);
        Assert.That(identity.MatchesCalledName("Pedro"), Is.False);
    }

    private GameObject CreateShooter(string displayName)
    {
        var obj = new GameObject($"Shooter {displayName}");
        createdObjects.Add(obj);
        obj.AddComponent<SpriteRenderer>();
        obj.AddComponent<Rigidbody2D>();
        obj.AddComponent<PlayerMovementController>();
        obj.AddComponent<PlayerAnimationController>();

        var identity = obj.AddComponent<PlayerNameIdentity>();
        identity.SetDisplayName(displayName);

        var role = obj.AddComponent<PlayerRoleController>();
        role.SetRole(PlayerRole.Taya);
        return obj;
    }

    private GameObject CreateNamedHider(string displayName, Vector3 position)
    {
        var obj = new GameObject($"Hider {displayName}");
        createdObjects.Add(obj);
        obj.transform.position = position;
        obj.AddComponent<SpriteRenderer>();

        var collider = obj.AddComponent<CircleCollider2D>();
        collider.radius = 0.28f;
        collider.offset = new Vector2(0f, -0.08f);

        var identity = obj.AddComponent<PlayerNameIdentity>();
        identity.SetDisplayName(displayName);

        obj.AddComponent<CaughtStateController>();
        obj.AddComponent<BangHitTarget>();

        var role = obj.AddComponent<PlayerRoleController>();
        role.SetRole(PlayerRole.Hider);
        return obj;
    }
}
