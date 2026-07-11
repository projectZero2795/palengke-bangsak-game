using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.Api;
using Palengke.BangSak.UI;
using UnityEditor.SceneManagement;

public sealed class Phase27PalengkeApiTests
{
    [Test]
    public void MainMenu_ProvidesRealApiClientAndLeaderboardPanel()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var root = scene.GetRootGameObjects().First(gameObject => gameObject.name == "Bang-Sak Main Menu Root");
        var menu = root.GetComponent<PrototypeMainMenuController>();
        var client = root.GetComponent<PalengkeApiClient>();

        Assert.That(menu, Is.Not.Null);
        Assert.That(client, Is.Not.Null);
        Assert.That(client.UseMockData, Is.False);
        Assert.That(client.IsProductionApiEnabled, Is.True);
        Assert.That(root.transform.Find("Phase 22 Main Menu UI/Main Menu Dashboard/SCORES Dashboard Tile"), Is.Not.Null);
        Assert.That(root.transform.Find("Phase 22 Main Menu UI/Palengke Leaderboard Panel"), Is.Not.Null);
    }
}
