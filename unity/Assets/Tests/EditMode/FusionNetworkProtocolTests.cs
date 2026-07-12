using NUnit.Framework;
using Palengke.BangSak.Network;

public sealed class FusionNetworkProtocolTests
{
    [Test]
    public void EncodeDecode_RoundTripsMovementEnvelope()
    {
        var payload = new FusionMovementPayload
        {
            networkPlayerId = "preview-01",
            x = 4.5f,
            y = -2f,
            sequence = 7
        };

        var data = FusionNetworkProtocol.Encode(
            FusionNetworkMessageKind.MovementRequest,
            1,
            12,
            payload,
            "authority-token");

        Assert.That(FusionNetworkProtocol.TryDecode(data, out var envelope), Is.True);
        Assert.That(envelope.kind, Is.EqualTo((int)FusionNetworkMessageKind.MovementRequest));
        Assert.That(envelope.senderIndex, Is.EqualTo(1));
        Assert.That(envelope.sequence, Is.EqualTo(12));
        Assert.That(envelope.authorityToken, Is.EqualTo("authority-token"));
        Assert.That(FusionNetworkProtocol.TryDecodePayload(envelope, out FusionMovementPayload decoded), Is.True);
        Assert.That(decoded.networkPlayerId, Is.EqualTo("preview-01"));
        Assert.That(decoded.x, Is.EqualTo(4.5f));
        Assert.That(decoded.y, Is.EqualTo(-2f));
        Assert.That(decoded.sequence, Is.EqualTo(7));
    }

    [Test]
    public void TryDecode_RejectsMalformedOrOversizedData()
    {
        Assert.That(FusionNetworkProtocol.TryDecode(new byte[] { 1, 2, 3 }, out _), Is.False);
        Assert.That(
            FusionNetworkProtocol.TryDecode(
                new byte[FusionNetworkProtocol.MaximumPayloadBytes + 1],
                out _),
            Is.False);
    }

    [Test]
    public void TryDecode_RejectsUnknownProtocolVersion()
    {
        var data = System.Text.Encoding.UTF8.GetBytes(
            "{\"protocolVersion\":99,\"kind\":3,\"senderIndex\":0,\"sequence\":1,\"authorityToken\":\"\",\"payload\":\"{}\"}");

        Assert.That(FusionNetworkProtocol.TryDecode(data, out _), Is.False);
    }

    [Test]
    public void ResolveRosterSlot_MapsOneBasedAndGappedPlayerRefsToDenseSlots()
    {
        Assert.That(FusionNetworkSession.ResolveRosterSlot(new[] { 1 }, 1), Is.EqualTo(0));
        Assert.That(FusionNetworkSession.ResolveRosterSlot(new[] { 4, 2 }, 2), Is.EqualTo(0));
        Assert.That(FusionNetworkSession.ResolveRosterSlot(new[] { 4, 2 }, 4), Is.EqualTo(1));
        Assert.That(FusionNetworkSession.ResolveRosterSlot(new[] { 4, 2 }, 3), Is.EqualTo(-1));
    }

    [Test]
    public void BuildDeterministicRoster_PutsSharedAuthorityFirstAndCompactsSurvivors()
    {
        Assert.That(
            FusionNetworkSession.BuildDeterministicRoster(new[] { 9, 3, 7 }, 7),
            Is.EqualTo(new[] { 7, 3, 9 }));
        Assert.That(
            FusionNetworkSession.ResolveRosterSlot(new[] { 9, 3, 7 }, 7, 7),
            Is.EqualTo(0));
        Assert.That(
            FusionNetworkSession.ResolveRosterSlot(new[] { 9, 3, 7 }, 3, 7),
            Is.EqualTo(1));
        Assert.That(
            FusionNetworkSession.BuildDeterministicRoster(new[] { 9, 3 }, 9),
            Is.EqualTo(new[] { 9, 3 }),
            "A promoted Shared authority must become Taya after the old authority leaves.");
        Assert.That(
            FusionNetworkSession.BuildDeterministicRoster(new[] { 11, 3, 9 }, 9),
            Is.EqualTo(new[] { 9, 3, 11 }),
            "A replacement must reuse compact capacity without changing the current Taya.");
        Assert.That(
            FusionNetworkSession.BuildDeterministicRoster(new[] { 9 }, 9),
            Is.EqualTo(new[] { 9 }),
            "The sole survivor must not retain a ghost roster entry.");
    }

    [Test]
    public void LeaveRules_ReturnLastPlayerToLobbyAndKeepFreedCapacityReusable()
    {
        Assert.That(FusionNetworkSession.ShouldReturnLastPlayerToLobby(1), Is.True);
        Assert.That(FusionNetworkSession.ShouldReturnLastPlayerToLobby(2), Is.False);
        Assert.That(FusionNetworkSession.CanAcceptReplacement(1), Is.True);
        Assert.That(FusionNetworkSession.CanAcceptReplacement(3), Is.True);
        Assert.That(FusionNetworkSession.CanAcceptReplacement(4), Is.False);
    }

    [Test]
    public void ResolveEnvelopeSenderSlot_AcceptsProxySourceButRejectsKnownMismatch()
    {
        Assert.That(FusionNetworkSession.ResolveEnvelopeSenderSlot(-1, 1, 2), Is.EqualTo(1));
        Assert.That(FusionNetworkSession.ResolveEnvelopeSenderSlot(1, 1, 2), Is.EqualTo(1));
        Assert.That(FusionNetworkSession.ResolveEnvelopeSenderSlot(0, 1, 2), Is.EqualTo(-1));
        Assert.That(FusionNetworkSession.ResolveEnvelopeSenderSlot(-1, 2, 2), Is.EqualTo(-1));
    }

    [Test]
    public void ResolvePhotonNameServer_ReturnsDirectEuRouteOnlyWhenRequested()
    {
        Assert.That(
            FusionNetworkSession.ResolvePhotonNameServer(true),
            Is.EqualTo(FusionNetworkSession.DirectEuNameServer));
        Assert.That(FusionNetworkSession.ResolvePhotonNameServer(false), Is.Empty);
    }

    [Test]
    public void ShouldUseDirectEuNameServer_RetriesDirectOutsideWebGl()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Assert.That(FusionNetworkSession.ShouldUseDirectEuNameServer(1), Is.True);
#else
        Assert.That(FusionNetworkSession.ShouldUseDirectEuNameServer(1), Is.False);
        Assert.That(FusionNetworkSession.ShouldUseDirectEuNameServer(2), Is.True);
#endif
    }
}
