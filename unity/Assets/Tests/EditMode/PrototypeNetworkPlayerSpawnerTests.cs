using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.Network;
using Palengke.BangSak.Player;
using UnityEngine;

public sealed class PrototypeNetworkPlayerSpawnerTests
{
    private GameObject spawnerObject;
    private GameObject mapObject;
    private GameObject playerPrefab;
    private PrototypeNetworkPlayerSpawner spawner;
    private PrototypeMapLayoutController mapLayout;

    [SetUp]
    public void SetUp()
    {
        mapObject = new GameObject("Map Layout");
        mapLayout = mapObject.AddComponent<PrototypeMapLayoutController>();
        mapLayout.TestConfigureLayout(
            new Vector2(30f, 20f),
            Vector2.zero,
            new Vector2(30f, 20f),
            new Vector2(0f, -4f),
            new[]
            {
                new Vector2(-6f, 4f),
                new Vector2(6f, 4f),
                new Vector2(0f, 6f)
            },
            2f);

        playerPrefab = CreatePlayerPrefab();
        spawnerObject = new GameObject("Network Player Spawner");
        spawner = spawnerObject.AddComponent<PrototypeNetworkPlayerSpawner>();
        spawner.SetReferences(mapLayout, null, null, playerPrefab);
        spawner.ConfigurePreview(new[] { "JuanP", "Maria", "Pedro", "Ana" }, 4, 0);
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(spawnerObject);
        Object.DestroyImmediate(mapObject);
        Object.DestroyImmediate(playerPrefab);
    }

    [Test]
    public void ComponentContract_MatchesPhase24SpawnPreview()
    {
        Assert.That(spawner.ComponentIdValue, Is.EqualTo(PrototypeNetworkPlayerSpawner.ComponentId));
        Assert.That(spawner.ComponentVersionValue, Is.EqualTo(1));
        Assert.That(spawner.ComponentVariantValue, Is.EqualTo("phase24_local_network_spawn_preview"));
    }

    [Test]
    public void BuildPreviewRoster_AssignsTayaFirstThenHiders()
    {
        var roster = spawner.BuildPreviewRoster();

        Assert.That(roster, Has.Length.EqualTo(4));
        Assert.That(roster[0].DisplayName, Is.EqualTo("JuanP"));
        Assert.That(roster[0].Role, Is.EqualTo(PlayerRole.Taya));
        Assert.That(roster[0].IsLocalPlayer, Is.True);
        Assert.That(roster[0].SpawnPosition, Is.EqualTo(new Vector2(0f, -4f)));

        Assert.That(roster[1].DisplayName, Is.EqualTo("Maria"));
        Assert.That(roster[1].Role, Is.EqualTo(PlayerRole.Hider));
        Assert.That(roster[1].IsLocalPlayer, Is.False);
        Assert.That(roster[1].SpawnPosition, Is.EqualTo(new Vector2(-6f, 4f)));
    }

    [Test]
    public void SpawnPreviewPlayers_AddsNetworkIdentityAndOwnershipControl()
    {
        var count = spawner.SpawnPreviewPlayers();

        Assert.That(count, Is.EqualTo(4));
        Assert.That(spawner.LastSpawnedLocalPlayer, Is.Not.Null);

        var localIdentity = spawner.LastSpawnedLocalPlayer.GetComponent<PrototypeNetworkPlayerIdentity>();
        var localMovement = spawner.LastSpawnedLocalPlayer.GetComponent<PlayerMovementController>();

        Assert.That(localIdentity, Is.Not.Null);
        Assert.That(localIdentity.IsLocalPlayer, Is.True);
        Assert.That(localIdentity.Role, Is.EqualTo(PlayerRole.Taya));
        Assert.That(localMovement.ReadsKeyboardInput, Is.True);

        var remote = spawner.SpawnedPlayers[1];
        var remoteIdentity = remote.GetComponent<PrototypeNetworkPlayerIdentity>();
        var remoteMovement = remote.GetComponent<PlayerMovementController>();

        Assert.That(remoteIdentity.IsLocalPlayer, Is.False);
        Assert.That(remoteIdentity.Role, Is.EqualTo(PlayerRole.Hider));
        Assert.That(remoteMovement.ReadsKeyboardInput, Is.False);
    }

    private static GameObject CreatePlayerPrefab()
    {
        var prefab = new GameObject("Runtime Player Prefab");
        prefab.AddComponent<SpriteRenderer>();
        prefab.AddComponent<Rigidbody2D>();
        prefab.AddComponent<CircleCollider2D>();
        prefab.AddComponent<PlayerMovementController>();
        prefab.AddComponent<PlayerAnimationController>();
        prefab.AddComponent<PlayerNameIdentity>();
        prefab.AddComponent<PlayerRoleController>();
        return prefab;
    }
}
