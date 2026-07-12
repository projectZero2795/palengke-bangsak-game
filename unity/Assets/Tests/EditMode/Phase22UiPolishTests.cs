using System.Linq;
using System.Reflection;
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
        var menuEventSystem = menu.transform.Find("Phase 22 Menu EventSystem");
        Assert.That(menuEventSystem, Is.Not.Null);
        Assert.That(menuEventSystem.GetComponent<EventSystem>(), Is.Not.Null);
        Assert.That(menu.HasRuntimeMenu, Is.True);
        var safeAreaPath = $"Phase 22 Main Menu UI/{SafeAreaCanvasLayout.SafeAreaRootName}";
        Assert.That(menu.transform.Find($"{safeAreaPath}/Main Menu Dashboard/PLAY Dashboard Tile"), Is.Not.Null);
        Assert.That(menu.transform.Find($"{safeAreaPath}/Main Menu Dashboard/HOW Dashboard Tile"), Is.Not.Null);
        Assert.That(menu.transform.Find($"{safeAreaPath}/Main Menu Dashboard/SET Dashboard Tile"), Is.Not.Null);
        Assert.That(menu.transform.Find($"{safeAreaPath}/Network Room Panel/Room Status Card/ROOM STATUS Text"), Is.Not.Null);
        Assert.That(menu.transform.Find($"{safeAreaPath}/Network Room Panel/JOIN 1234 Button"), Is.Not.Null);
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

    [Test]
    public void PrototypeMap_RoundHudContainsLeaveConfirmationUi()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PrototypeMap.unity");
        var roundRoot = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Phase 21 Round Rules");
        var hud = roundRoot.GetComponent<PrototypeRoundRulesHud>();
        var createHud = typeof(PrototypeRoundRulesHud).GetMethod("CreateHud", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(createHud, Is.Not.Null);
        createHud.Invoke(hud, null);

        Assert.That(hud.HasLeaveConfirmationUi, Is.True);
        Assert.That(hud.IsLeaveConfirmationVisible, Is.False);
        var runtimeRoot = GameObject.Find("Phase 21 Round Rules HUD");
        Assert.That(runtimeRoot, Is.Not.Null);
        var leaveButton = runtimeRoot.transform.Find($"{SafeAreaCanvasLayout.SafeAreaRootName}/Leave Game Button");
        var blocker = runtimeRoot.transform.Find($"{SafeAreaCanvasLayout.SafeAreaRootName}/Leave Confirmation Blocker");
        Assert.That(leaveButton, Is.Not.Null);
        Assert.That(leaveButton.gameObject.activeSelf, Is.False, "Local play must not show the multiplayer leave control.");
        Assert.That(blocker, Is.Not.Null);
        Assert.That(blocker.Find("Leave Game Confirmation Panel"), Is.Not.Null);

        blocker.gameObject.SetActive(true);
        hud.CancelLeaveConfirmation();
        Assert.That(hud.IsLeaveConfirmationVisible, Is.False);
        Object.DestroyImmediate(runtimeRoot);
    }
}
