using NUnit.Framework;
using Palengke.BangSak.Game;
using UnityEngine;

public sealed class SakBaseControllerTests
{
    private GameObject baseOwner;
    private GameObject actorOwner;

    [TearDown]
    public void TearDown()
    {
        if (baseOwner != null)
        {
            Object.DestroyImmediate(baseOwner);
        }

        if (actorOwner != null)
        {
            Object.DestroyImmediate(actorOwner);
        }
    }

    [Test]
    public void ComponentContract_ExposesVersionedMetadata()
    {
        var sakBase = CreateBase();

        Assert.That(SakBaseController.ComponentId, Is.EqualTo("sak_base_controller"));
        Assert.That(SakBaseController.ComponentVersion, Is.EqualTo(1));
        Assert.That(SakBaseController.ComponentVariant, Is.EqualTo("green_flag_base_placeholder"));
        Assert.That(sakBase.ComponentIdValue, Is.EqualTo(SakBaseController.ComponentId));
        Assert.That(sakBase.ComponentVersionValue, Is.EqualTo(SakBaseController.ComponentVersion));
        Assert.That(sakBase.ComponentVariantValue, Is.EqualTo(SakBaseController.ComponentVariant));
    }

    [Test]
    public void ActorCanPressSakOnlyWhenNearActiveBaseAndEligible()
    {
        var sakBase = CreateBase();
        var actor = CreateActor();

        Assert.That(actor.CanPressSak(), Is.False);
        Assert.That(actor.TryPressSak(1f).Outcome, Is.EqualTo(SakAttemptOutcome.NoBase));

        sakBase.RegisterActor(actor);

        Assert.That(actor.IsNearBase, Is.True);
        Assert.That(actor.CanPressSak(), Is.True);

        var result = actor.TryPressSak(2f);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.BaseController, Is.EqualTo(sakBase));
        Assert.That(result.Actor, Is.EqualTo(actor));
        Assert.That(sakBase.SuccessfulSakCount, Is.EqualTo(1));
        Assert.That(actor.SuccessfulSakCount, Is.EqualTo(1));
        Assert.That(sakBase.LastSuccessfulActor, Is.EqualTo(actor));

        sakBase.UnregisterActor(actor);

        Assert.That(actor.IsNearBase, Is.False);
        Assert.That(actor.CanPressSak(), Is.False);
    }

    [Test]
    public void IneligibleActorCannotUseBase()
    {
        var sakBase = CreateBase();
        var actor = CreateActor();

        actor.SetCanUseSak(false);
        sakBase.RegisterActor(actor);

        var result = actor.TryPressSak(3f);

        Assert.That(actor.CanPressSak(), Is.False);
        Assert.That(result.Outcome, Is.EqualTo(SakAttemptOutcome.ActorNotEligible));
        Assert.That(sakBase.SuccessfulSakCount, Is.EqualTo(0));
        Assert.That(actor.SuccessfulSakCount, Is.EqualTo(0));
    }

    [Test]
    public void InactiveBaseRejectsSak()
    {
        var sakBase = CreateBase();
        var actor = CreateActor();

        sakBase.RegisterActor(actor);
        sakBase.SetBaseActive(false);

        var result = actor.TryPressSak(4f);

        Assert.That(actor.IsNearBase, Is.True);
        Assert.That(actor.CanPressSak(), Is.False);
        Assert.That(result.Outcome, Is.EqualTo(SakAttemptOutcome.BaseInactive));
        Assert.That(sakBase.SuccessfulSakCount, Is.EqualTo(0));
    }

    private SakBaseController CreateBase()
    {
        baseOwner = new GameObject("Sak Base Test");
        var collider = baseOwner.AddComponent<CircleCollider2D>();
        var sakBase = baseOwner.AddComponent<SakBaseController>();
        sakBase.SetTriggerRadius(1.15f);

        Assert.That(collider.isTrigger, Is.True);
        Assert.That(collider.radius, Is.EqualTo(1.15f).Within(0.001f));

        return sakBase;
    }

    private SakBaseActor CreateActor()
    {
        actorOwner = new GameObject("Sak Actor Test");
        return actorOwner.AddComponent<SakBaseActor>();
    }
}
