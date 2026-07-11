using NUnit.Framework;
using Palengke.BangSak.Network;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class FusionIntegrityGuardTests
{
    private FusionIntegrityGuard guard;

    [SetUp]
    public void SetUp()
    {
        guard = new FusionIntegrityGuard();
        guard.SetCredential(0, "master-token");
        guard.SetCredential(1, "hider-token");
    }

    [Test]
    public void ValidateEnvelope_AcceptsBoundCredentialAndRejectsForgeryAndReplay()
    {
        var accepted = Envelope(1, 1, "hider-token");
        Assert.That(guard.ValidateEnvelope(accepted, 2, out var acceptedReason), Is.True);
        Assert.That(acceptedReason, Is.EqualTo(FusionIntegrityRejection.None));

        Assert.That(guard.ValidateEnvelope(accepted, 2, out var replayReason), Is.False);
        Assert.That(replayReason, Is.EqualTo(FusionIntegrityRejection.Replay));

        var forged = Envelope(1, 2, "master-token");
        Assert.That(guard.ValidateEnvelope(forged, 2, out var forgedReason), Is.False);
        Assert.That(forgedReason, Is.EqualTo(FusionIntegrityRejection.InvalidCredential));
    }

    [Test]
    public void ValidateMovement_RejectsOutOfBoundsTeleportAndBurstRate()
    {
        var bounds = new Bounds(Vector3.zero, new Vector3(20f, 20f, 0f));
        var first = Movement(1, Vector2.zero);
        Assert.That(guard.ValidateMovement(1, first, bounds, 1f, true, out _), Is.True);

        var burst = Movement(2, new Vector2(0.1f, 0f));
        Assert.That(guard.ValidateMovement(1, burst, bounds, 1.001f, true, out var burstReason), Is.False);
        Assert.That(burstReason, Is.EqualTo(FusionIntegrityRejection.RateLimited));

        var teleport = Movement(3, new Vector2(9f, 0f));
        Assert.That(guard.ValidateMovement(1, teleport, bounds, 1.2f, true, out var teleportReason), Is.False);
        Assert.That(teleportReason, Is.EqualTo(FusionIntegrityRejection.ImpossibleMovement));

        var outside = Movement(4, new Vector2(25f, 0f));
        Assert.That(guard.ValidateMovement(1, outside, bounds, 3f, true, out var outsideReason), Is.False);
        Assert.That(outsideReason, Is.EqualTo(FusionIntegrityRejection.ImpossibleMovement));
    }

    [Test]
    public void ValidateMovement_RejectsNonFiniteAndOutOfStatePayloads()
    {
        var bounds = new Bounds(Vector3.zero, new Vector3(20f, 20f, 0f));
        var invalid = Movement(1, Vector2.zero);
        invalid.x = float.NaN;
        Assert.That(guard.ValidateMovement(1, invalid, bounds, 1f, true, out var invalidReason), Is.False);
        Assert.That(invalidReason, Is.EqualTo(FusionIntegrityRejection.InvalidPayload));

        var waiting = Movement(2, Vector2.zero);
        Assert.That(guard.ValidateMovement(1, waiting, bounds, 2f, false, out var stateReason), Is.False);
        Assert.That(stateReason, Is.EqualTo(FusionIntegrityRejection.OutOfState));
    }

    [Test]
    public void ValidateAction_EnforcesRoleCooldownSequenceAndRoundState()
    {
        var hiderSak = Action(1, PrototypeNetworkActionKind.SakCounter, string.Empty);
        Assert.That(guard.ValidateAction(1, hiderSak, 1f, true, out _), Is.True);

        var repeated = Action(2, PrototypeNetworkActionKind.SakCounter, string.Empty);
        Assert.That(guard.ValidateAction(1, repeated, 1.1f, true, out var cooldownReason), Is.False);
        Assert.That(cooldownReason, Is.EqualTo(FusionIntegrityRejection.RateLimited));

        var wrongRole = Action(3, PrototypeNetworkActionKind.BangNameCall, "Maria");
        wrongRole.actorNetworkPlayerId = "preview-01";
        Assert.That(guard.ValidateAction(1, wrongRole, 3f, true, out var roleReason), Is.False);
        Assert.That(roleReason, Is.EqualTo(FusionIntegrityRejection.InvalidRole));

        var stopped = Action(4, PrototypeNetworkActionKind.SakCounter, string.Empty);
        Assert.That(guard.ValidateAction(1, stopped, 4f, false, out var stateReason), Is.False);
        Assert.That(stateReason, Is.EqualTo(FusionIntegrityRejection.OutOfState));
    }

    [Test]
    public void ValidateAction_AllowsTayaPerTargetCooldownButRejectsDuplicateSequence()
    {
        var maria = Action(1, PrototypeNetworkActionKind.BangNameCall, "Maria");
        Assert.That(guard.ValidateAction(0, maria, 1f, true, out _), Is.True);

        var pedro = Action(2, PrototypeNetworkActionKind.BangNameCall, "Pedro");
        Assert.That(guard.ValidateAction(0, pedro, 1.1f, true, out _), Is.True);

        var replay = Action(2, PrototypeNetworkActionKind.BangNameCall, "Ana");
        Assert.That(guard.ValidateAction(0, replay, 3f, true, out var reason), Is.False);
        Assert.That(reason, Is.EqualTo(FusionIntegrityRejection.Replay));
    }

    [Test]
    public void ValidateAction_RejectsMissingOrOversizedCalledName()
    {
        var missing = Action(1, PrototypeNetworkActionKind.BangNameCall, string.Empty);
        Assert.That(guard.ValidateAction(0, missing, 1f, true, out var missingReason), Is.False);
        Assert.That(missingReason, Is.EqualTo(FusionIntegrityRejection.InvalidPayload));

        var oversized = Action(
            2,
            PrototypeNetworkActionKind.BangNameCall,
            new string('A', FusionIntegrityGuard.MaximumCalledNameLength + 1));
        Assert.That(guard.ValidateAction(0, oversized, 2f, true, out var oversizedReason), Is.False);
        Assert.That(oversizedReason, Is.EqualTo(FusionIntegrityRejection.InvalidPayload));
    }

    [Test]
    public void ValidateRestart_RequiresFinishedRoundAndRateLimitsRequests()
    {
        Assert.That(guard.ValidateRestart(1, 1f, false, out var stateReason), Is.False);
        Assert.That(stateReason, Is.EqualTo(FusionIntegrityRejection.OutOfState));
        Assert.That(guard.ValidateRestart(1, 2f, true, out _), Is.True);
        Assert.That(guard.ValidateRestart(1, 2.1f, true, out var rateReason), Is.False);
        Assert.That(rateReason, Is.EqualTo(FusionIntegrityRejection.RateLimited));
    }

    [Test]
    public void Reset_RotatesCredentialsAndClearsReplayState()
    {
        Assert.That(guard.ValidateEnvelope(Envelope(1, 7, "hider-token"), 2, out _), Is.True);
        guard.Reset();
        guard.SetCredential(1, "new-token");

        Assert.That(guard.ValidateEnvelope(Envelope(1, 1, "hider-token"), 2, out var oldReason), Is.False);
        Assert.That(oldReason, Is.EqualTo(FusionIntegrityRejection.InvalidCredential));
        Assert.That(guard.ValidateEnvelope(Envelope(1, 1, "new-token"), 2, out _), Is.True);
    }

    [Test]
    public void ResetMovementState_AllowsAuthoritySpawnPlacementWithoutChangingCredential()
    {
        var bounds = new Bounds(Vector3.zero, new Vector3(50f, 34f, 0f));
        Assert.That(guard.ValidateMovement(1, Movement(1, Vector2.zero), bounds, 1f, true, out _), Is.True);

        guard.ResetMovementState();
        var spawnPlacement = Movement(2, new Vector2(-20f, -12f));

        Assert.That(guard.ValidateMovement(1, spawnPlacement, bounds, 1.01f, true, out _), Is.True);
        Assert.That(guard.CredentialFor(1), Is.EqualTo("hider-token"));
    }

    private static FusionNetworkEnvelope Envelope(int sender, int sequence, string token)
    {
        return new FusionNetworkEnvelope
        {
            protocolVersion = FusionNetworkProtocol.Version,
            kind = (int)FusionNetworkMessageKind.MovementRequest,
            senderIndex = sender,
            sequence = sequence,
            authorityToken = token,
            payload = "{}"
        };
    }

    private static FusionMovementPayload Movement(int sequence, Vector2 position)
    {
        return new FusionMovementPayload
        {
            networkPlayerId = "preview-01",
            x = position.x,
            y = position.y,
            inputX = 0f,
            inputY = 0f,
            facingDirection = (int)PlayerFacingDirection.Down,
            sequence = sequence,
            sentAt = sequence
        };
    }

    private static FusionActionPayload Action(
        int sequence,
        PrototypeNetworkActionKind kind,
        string calledName)
    {
        return new FusionActionPayload
        {
            kind = (int)kind,
            actorNetworkPlayerId = kind == PrototypeNetworkActionKind.BangNameCall
                ? "preview-00"
                : "preview-01",
            calledName = calledName,
            facingDirection = (int)PlayerFacingDirection.Down,
            sequence = sequence,
            sentAt = sequence
        };
    }
}
