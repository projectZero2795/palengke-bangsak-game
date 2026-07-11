using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Api;
using UnityEngine;

public sealed class PalengkeApiClientTests
{
    private GameObject root;
    private PalengkeApiClient client;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("Palengke API Test");
        client = root.AddComponent<PalengkeApiClient>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(root);
    }

    [Test]
    public void ComponentContract_IdentifiesPhase27MockAdapter()
    {
        Assert.That(client.ComponentIdValue, Is.EqualTo(PalengkeApiClient.ComponentId));
        Assert.That(client.ComponentVersionValue, Is.EqualTo(PalengkeApiClient.ComponentVersion));
        Assert.That(client.ComponentVariantValue, Is.EqualTo(PalengkeApiClient.ComponentVariant));
        Assert.That(client.UseMockData, Is.True);
        Assert.That(client.IsProductionApiEnabled, Is.False);
    }

    [Test]
    public void MockUser_ContainsDisplayNameAndCoins()
    {
        var user = client.GetCurrentUser();

        Assert.That(user.displayName, Is.EqualTo("JuanP"));
        Assert.That(user.coins, Is.EqualTo(125));
    }

    [Test]
    public void MockLeaderboard_IsRankedAndContainsCurrentUser()
    {
        var entries = client.GetLeaderboard();

        Assert.That(entries, Has.Length.EqualTo(5));
        Assert.That(entries[0].rank, Is.EqualTo(1));
        Assert.That(entries[0].score, Is.GreaterThan(entries[1].score));
        Assert.That(entries.Any(entry => entry.displayName == "JuanP"), Is.True);
    }

    [Test]
    public void BaseUrl_IsConfigurableAndNormalized()
    {
        client.Configure(" https://preview.palengke.test/v1/ ");

        Assert.That(client.ApiBaseUrl, Is.EqualTo("https://preview.palengke.test/v1"));
    }
}
