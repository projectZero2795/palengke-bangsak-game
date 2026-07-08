using NUnit.Framework;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class PlayerMovementControllerTests
{
    private GameObject gameObject;
    private PlayerMovementController controller;

    [SetUp]
    public void SetUp()
    {
        gameObject = new GameObject("PlayerMovementControllerTests");
        gameObject.AddComponent<Rigidbody2D>();
        controller = gameObject.AddComponent<PlayerMovementController>();
        controller.MovementSpeed = 4f;
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void ResolveInput_UsesKeyboard_WhenJoystickIsIdle()
    {
        var input = controller.ResolveInput(Vector2.right, Vector2.zero);

        Assert.That(input, Is.EqualTo(Vector2.right));
    }

    [Test]
    public void ResolveInput_UsesJoystick_WhenJoystickIsActive()
    {
        var input = controller.ResolveInput(Vector2.right, Vector2.up);

        Assert.That(input, Is.EqualTo(Vector2.up));
    }

    [Test]
    public void ResolveInput_ClampsDiagonalInput()
    {
        var input = controller.ResolveInput(new Vector2(1f, 1f), Vector2.zero);

        Assert.That(input.magnitude, Is.LessThanOrEqualTo(1.0001f));
    }

    [Test]
    public void GetMovementDelta_UsesConfiguredSpeed()
    {
        controller.MovementSpeed = 5f;

        var delta = controller.GetMovementDelta(Vector2.right, 0.2f);

        Assert.That(delta.x, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(delta.y, Is.EqualTo(0f).Within(0.0001f));
    }

    [Test]
    public void SetKeyboardInputEnabled_TracksLocalOwnershipFlag()
    {
        Assert.That(controller.ReadsKeyboardInput, Is.True);

        controller.SetKeyboardInputEnabled(false);

        Assert.That(controller.ReadsKeyboardInput, Is.False);
        Assert.That(controller.CurrentInput, Is.EqualTo(Vector2.zero));
    }
}
