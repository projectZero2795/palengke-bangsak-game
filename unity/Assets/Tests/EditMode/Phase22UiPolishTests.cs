using System.Linq;
using NUnit.Framework;
using Palengke.BangSak.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class Phase22UiPolishTests
{
    [Test]
    public void MainMenu_HasConfiguredPhase22MenuController()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var menuRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Bang-Sak Main Menu Root");

        Assert.That(menuRoot, Is.Not.Null);

        var menu = menuRoot.GetComponent<PrototypeMainMenuController>();

        Assert.That(menu, Is.Not.Null);
        Assert.That(menu.ComponentIdValue, Is.EqualTo(PrototypeMainMenuController.ComponentId));
        Assert.That(menu.ComponentVersionValue, Is.EqualTo(PrototypeMainMenuController.ComponentVersion));
        Assert.That(menu.ComponentVariantValue, Is.EqualTo(PrototypeMainMenuController.ComponentVariant));
        Assert.That(menu.PrototypeSceneName, Is.EqualTo("PrototypeMap"));
        Assert.That(
            typeof(PrototypeMainMenuController).GetCustomAttributes(typeof(UnityEngine.ExecuteAlways), false),
            Is.Not.Empty);
        Assert.That(Object.FindObjectOfType<EventSystem>(), Is.Not.Null);
    }

    [Test]
    public void BuildSettings_KeepMenuBeforePrototypeMap()
    {
        var enabledScenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        Assert.That(enabledScenes, Does.Contain("Assets/Scenes/MainMenu.unity"));
        Assert.That(enabledScenes, Does.Contain("Assets/Scenes/PrototypeMap.unity"));
        Assert.That(
            System.Array.IndexOf(enabledScenes, "Assets/Scenes/MainMenu.unity"),
            Is.LessThan(System.Array.IndexOf(enabledScenes, "Assets/Scenes/PrototypeMap.unity")));
    }

    [Test]
    public void PrototypeMap_RoundResultHudCanReturnToMainMenu()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var roundRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 21 Round Rules");

        Assert.That(roundRoot, Is.Not.Null);

        var hud = roundRoot.GetComponent<PrototypeRoundRulesHud>();

        Assert.That(hud, Is.Not.Null);
        Assert.That(hud.ShowMainMenuButton, Is.True);
        Assert.That(hud.MainMenuSceneName, Is.EqualTo("MainMenu"));
    }
}
