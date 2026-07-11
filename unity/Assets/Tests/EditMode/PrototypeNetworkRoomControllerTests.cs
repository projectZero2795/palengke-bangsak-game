using NUnit.Framework;
using Palengke.BangSak.Network;
using UnityEngine;

public sealed class PrototypeNetworkRoomControllerTests
{
    private GameObject gameObject;
    private PrototypeNetworkRoomController controller;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject("Prototype Network Room Test");
        controller = gameObject.AddComponent<PrototypeNetworkRoomController>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void ComponentContract_MatchesPhase32FusionRoom()
    {
        Assert.That(controller.ComponentIdValue, Is.EqualTo(PrototypeNetworkRoomController.ComponentId));
        Assert.That(controller.ComponentVersionValue, Is.EqualTo(2));
        Assert.That(controller.ComponentVariantValue, Is.EqualTo("phase32_fusion_shared_room"));
        Assert.That(controller.ProviderName, Is.EqualTo("Photon Fusion 2.1 Shared"));
        Assert.That(controller.IsFusionSdkAvailable, Is.True);
    }

    [Test]
    public void NormalizeRoomCode_TrimsAndUppercases()
    {
        Assert.That(PrototypeNetworkRoomController.NormalizeRoomCode("  ab12  "), Is.EqualTo("AB12"));
    }

    [Test]
    public void IsValidRoomCode_AllowsOnlyCompactLettersAndNumbers()
    {
        Assert.That(PrototypeNetworkRoomController.IsValidRoomCode("1234"), Is.True);
        Assert.That(PrototypeNetworkRoomController.IsValidRoomCode("room9"), Is.True);
        Assert.That(PrototypeNetworkRoomController.IsValidRoomCode("12"), Is.False);
        Assert.That(PrototypeNetworkRoomController.IsValidRoomCode("room-9"), Is.False);
        Assert.That(PrototypeNetworkRoomController.IsValidRoomCode("way-too-long-code"), Is.False);
    }

    [Test]
    public void CreateRoom_PreparesPhotonConnection()
    {
        Assert.That(controller.CreateRoom(), Is.True);

        Assert.That(controller.State, Is.EqualTo(PrototypeNetworkRoomState.Connecting));
        Assert.That(controller.HasActiveRoom, Is.True);
        Assert.That(PrototypeNetworkRoomController.IsValidRoomCode(controller.ActiveRoomCode), Is.True);
        Assert.That(controller.StatusMessage.Length, Is.GreaterThan(0));
    }

    [Test]
    public void JoinRoom_UsesNormalizedRoomCode()
    {
        Assert.That(controller.JoinRoom("  ab12  "), Is.True);

        Assert.That(controller.State, Is.EqualTo(PrototypeNetworkRoomState.Connecting));
        Assert.That(controller.ActiveRoomCode, Is.EqualTo("AB12"));
    }

    [Test]
    public void JoinRoom_RejectsInvalidCodeWithoutChangingRoom()
    {
        controller.JoinRoom("1234");

        Assert.That(controller.JoinRoom("bad-code"), Is.False);
        Assert.That(controller.ActiveRoomCode, Is.EqualTo("1234"));
        Assert.That(controller.StatusMessage, Does.Contain("Room code"));
    }

    [Test]
    public void LeaveRoom_ReturnsToDisconnectedState()
    {
        controller.CreateRoom();

        controller.LeaveRoom();

        Assert.That(controller.State, Is.EqualTo(PrototypeNetworkRoomState.Disconnected));
        Assert.That(controller.HasActiveRoom, Is.False);
    }
}
