using NUnit.Framework;
using Palengke.BangSak.Game;
using Palengke.BangSak.Network;
using Palengke.BangSak.Player;
using Palengke.BangSak.UI;
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
        mapLayout.ConfigureForTests(
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
        var localMovementSync = spawner.LastSpawnedLocalPlayer.GetComponent<PrototypeNetworkMovementSyncController>();
        var localActionSync = spawner.LastSpawnedLocalPlayer.GetComponent<PrototypeNetworkActionSyncController>();

        Assert.That(localIdentity, Is.Not.Null);
        Assert.That(localIdentity.IsLocalPlayer, Is.True);
        Assert.That(localIdentity.Role, Is.EqualTo(PlayerRole.Taya));
        Assert.That(localMovement.ReadsKeyboardInput, Is.True);
        Assert.That(localMovementSync, Is.Not.Null);
        Assert.That(localMovementSync.Authority, Is.EqualTo(PrototypeNetworkMovementAuthority.LocalAuthority));
        Assert.That(localActionSync, Is.Not.Null);
        Assert.That(localActionSync.IsLocalAuthority, Is.True);

        var remote = spawner.SpawnedPlayers[1];
        var remoteIdentity = remote.GetComponent<PrototypeNetworkPlayerIdentity>();
        var remoteMovement = remote.GetComponent<PlayerMovementController>();
        var remoteMovementSync = remote.GetComponent<PrototypeNetworkMovementSyncController>();
        var remoteActionSync = remote.GetComponent<PrototypeNetworkActionSyncController>();

        Assert.That(remoteIdentity.IsLocalPlayer, Is.False);
        Assert.That(remoteIdentity.Role, Is.EqualTo(PlayerRole.Hider));
        Assert.That(remoteMovement.ReadsKeyboardInput, Is.False);
        Assert.That(remoteMovement.enabled, Is.False);
        Assert.That(remoteMovementSync.Authority, Is.EqualTo(PrototypeNetworkMovementAuthority.RemoteReplica));
        Assert.That(remoteActionSync.IsLocalAuthority, Is.False);
        Assert.That(remote.GetComponent<BangActionHud>().enabled, Is.False);
        Assert.That(remote.GetComponent<BangNameCallHud>().enabled, Is.False);
        Assert.That(remote.GetComponent<SakCounterHud>().enabled, Is.False);
    }

    [Test]
    public void DisableLegacyPreviewActors_DisablesOldPreviewRootsBeforeRoundCounts()
    {
        const string legacyPlayableName = "Test Legacy Playable Player";
        const string legacyPreviewRootName = "Test Legacy Preview Root";
        spawner.ConfigureLegacyPreviewNames(legacyPlayableName, new[] { legacyPreviewRootName });

        var legacyPlayable = new GameObject(legacyPlayableName);
        var legacyPreviewRoot = new GameObject(legacyPreviewRootName);

        var disabledCount = spawner.DisableLegacyPreviewActors();

        Assert.That(disabledCount, Is.EqualTo(2));
        Assert.That(legacyPlayable.activeSelf, Is.False);
        Assert.That(legacyPreviewRoot.activeSelf, Is.False);

        Object.DestroyImmediate(legacyPlayable);
        Object.DestroyImmediate(legacyPreviewRoot);
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
        prefab.AddComponent<BangActionHud>();
        prefab.AddComponent<BangNameCallHud>();
        prefab.AddComponent<SakCounterHud>();
        prefab.AddComponent<PlayerRoleController>();
        return prefab;
    }
}
