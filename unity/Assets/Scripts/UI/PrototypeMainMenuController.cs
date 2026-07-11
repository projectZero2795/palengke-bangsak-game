using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Palengke.BangSak.Api;
using Palengke.BangSak.Network;

namespace Palengke.BangSak.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class PrototypeMainMenuController : MonoBehaviour
    {
        public const string ComponentId = "prototype_main_menu";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "phase22_local_menu";
        private const string MenuRootName = "Phase 22 Main Menu UI";

        [Header("Component Contract")]
        [SerializeField]
        private string componentId = ComponentId;

        [SerializeField]
        private int componentVersion = ComponentVersion;

        [SerializeField]
        private string componentVariant = ComponentVariant;

        [Header("Scenes")]
        [SerializeField]
        private string prototypeSceneName = "PrototypeMap";

        [SerializeField]
        private int sortingOrder = 40;

        private GameObject menuRoot;
        private GameObject ownedEventSystem;
        private GameObject howToPanel;
        private GameObject settingsPanel;
        private GameObject networkPanel;
        private GameObject leaderboardPanel;
        private Text networkStatusLabel;
        private Text playerSummaryLabel;
        private Text apiStatusLabel;
        private Transform leaderboardRows;
        private PrototypeNetworkRoomController roomController;
        private PalengkeApiClient apiClient;
        private static Sprite roundedPanelSprite;

        public string ComponentIdValue => componentId;

        public int ComponentVersionValue => componentVersion;

        public string ComponentVariantValue => componentVariant;

        public string PrototypeSceneName => prototypeSceneName;

        public bool HasRuntimeMenu => menuRoot != null;

        private void OnEnable()
        {
            CreateMenu();
        }

        private void Start()
        {
            CreateMenu();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                PlayLocal();
                return;
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                ShowHowTo();
                return;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ShowNetworkRoom();
                return;
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                ShowLeaderboard();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HidePanels();
            }

            if (networkPanel != null && networkPanel.activeSelf)
            {
                RefreshNetworkPanel();
            }
        }

        private void OnDestroy()
        {
            DestroyMenu();
        }

        private void OnDisable()
        {
            DestroyMenu();
        }

        public void PlayLocal()
        {
            if (!string.IsNullOrWhiteSpace(prototypeSceneName))
            {
                SceneManager.LoadScene(prototypeSceneName);
            }
        }

        public void ShowHowTo()
        {
            CreateMenu();
            HidePanels();
            howToPanel.SetActive(true);
        }

        public void ShowSettings()
        {
            CreateMenu();
            HidePanels();
            settingsPanel.SetActive(true);
        }

        public void ShowNetworkRoom()
        {
            CreateMenu();
            HidePanels();
            RefreshNetworkPanel();
            networkPanel.SetActive(true);
        }

        public void HidePanels()
        {
            if (howToPanel != null)
            {
                howToPanel.SetActive(false);
            }

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            if (networkPanel != null)
            {
                networkPanel.SetActive(false);
            }

            if (leaderboardPanel != null)
            {
                leaderboardPanel.SetActive(false);
            }
        }

        private void CreateMenu()
        {
            if (menuRoot != null)
            {
                return;
            }

            var existingPreview = transform.Find(MenuRootName);
            if (existingPreview != null)
            {
                DestroyObject(existingPreview.gameObject);
            }

            var canvasObject = new GameObject(MenuRootName);
            canvasObject.transform.SetParent(transform, false);
            if (!Application.isPlaying)
            {
                canvasObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            menuRoot = canvasObject;
            ResolveRoomController();

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);

            canvasObject.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();

            CreateBackdrop(canvasObject.transform);
            CreateMainCard(canvasObject.transform);
            CreateHowToPanel(canvasObject.transform);
            CreateSettingsPanel(canvasObject.transform);
            CreateNetworkPanel(canvasObject.transform);
            CreateLeaderboardPanel(canvasObject.transform);
            HidePanels();
        }

        private void DestroyMenu()
        {
            if (menuRoot != null)
            {
                DestroyObject(menuRoot);
                menuRoot = null;
                DestroyObject(ownedEventSystem);
                ownedEventSystem = null;
                howToPanel = null;
                settingsPanel = null;
                networkPanel = null;
                leaderboardPanel = null;
                networkStatusLabel = null;
                playerSummaryLabel = null;
                apiStatusLabel = null;
                leaderboardRows = null;
            }
        }

        private void ResolveRoomController()
        {
            if (roomController != null)
            {
                return;
            }

            roomController = GetComponent<PrototypeNetworkRoomController>();
            if (roomController == null)
            {
                roomController = gameObject.AddComponent<PrototypeNetworkRoomController>();
            }
        }

        private void ResolveApiClient()
        {
            if (apiClient != null)
            {
                return;
            }

            apiClient = GetComponent<PalengkeApiClient>();
            if (apiClient == null)
            {
                apiClient = gameObject.AddComponent<PalengkeApiClient>();
            }
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            ownedEventSystem = new GameObject("Phase 22 Menu EventSystem");
            ownedEventSystem.transform.SetParent(transform, false);
            if (!Application.isPlaying)
            {
                ownedEventSystem.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            }

            ownedEventSystem.AddComponent<EventSystem>();
            ownedEventSystem.AddComponent<StandaloneInputModule>();
        }

        private static void DestroyObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private void CreateBackdrop(Transform parent)
        {
            var backdrop = new GameObject("Night Barangay Backdrop");
            backdrop.transform.SetParent(parent, false);

            var rect = backdrop.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = backdrop.AddComponent<Image>();
            image.color = new Color(0.012f, 0.022f, 0.042f, 1f);

            CreateGlow(parent, "Moon Glow", new Vector2(250f, 160f), new Vector2(380f, 280f), new Color(0.18f, 0.32f, 0.56f, 0.22f));
            CreateGlow(parent, "Palengke Lamp Glow", new Vector2(-260f, -170f), new Vector2(420f, 260f), new Color(1f, 0.68f, 0.22f, 0.16f));
        }

        private void CreateMainCard(Transform parent)
        {
            var card = CreatePanel(parent, "Main Menu Dashboard", Vector2.zero, new Vector2(660f, 430f), new Color(0.022f, 0.038f, 0.07f, 0.94f));

            CreateStatusBadge(card, "LOCAL MVP", new Vector2(-246f, 160f), new Vector2(112f, 30f), new Color(1f, 0.78f, 0.22f, 0.16f), new Color(1f, 0.78f, 0.22f, 1f));
            CreateStatusBadge(card, "SAFE PLAY", new Vector2(246f, 160f), new Vector2(112f, 30f), new Color(0.22f, 1f, 0.48f, 0.14f), new Color(0.55f, 1f, 0.68f, 1f));

            CreateText(card, "BANG-SAK", new Vector2(0f, 136f), new Vector2(500f, 56f), 46, FontStyle.Bold, new Color(1f, 0.82f, 0.23f, 1f));
            CreateText(card, "Bang. Run. Hide. Sak!", new Vector2(0f, 94f), new Vector2(420f, 28f), 20, FontStyle.Bold, new Color(0.86f, 0.93f, 1f, 1f));
            CreateText(card, "Choose your local prototype mode", new Vector2(0f, 61f), new Vector2(420f, 22f), 13, FontStyle.Bold, new Color(0.55f, 0.67f, 0.86f, 1f));

            CreateDashboardTile(
                card,
                "PLAY",
                "Local",
                "Start round",
                new Vector2(-248f, -38f),
                new Vector2(112f, 122f),
                new Color(0.1f, 0.2f, 0.34f, 1f),
                new Color(0.36f, 0.58f, 1f, 1f),
                PlayLocal);

            CreateDashboardTile(
                card,
                "ROOM",
                "Photon",
                "Create / join",
                new Vector2(-124f, -38f),
                new Vector2(112f, 122f),
                new Color(0.08f, 0.18f, 0.28f, 1f),
                new Color(0.35f, 0.85f, 1f, 1f),
                ShowNetworkRoom);

            CreateDashboardTile(
                card,
                "HOW",
                "Rules",
                "Learn",
                new Vector2(0f, -38f),
                new Vector2(112f, 122f),
                new Color(0.32f, 0.16f, 0.08f, 1f),
                new Color(1f, 0.58f, 0.28f, 1f),
                ShowHowTo);

            CreateDashboardTile(
                card,
                "SCORES",
                "Mock API",
                "Leaderboard",
                new Vector2(124f, -38f),
                new Vector2(112f, 122f),
                new Color(0.18f, 0.12f, 0.3f, 1f),
                new Color(0.72f, 0.52f, 1f, 1f),
                ShowLeaderboard);

            CreateDashboardTile(
                card,
                "SET",
                "Options",
                "Settings",
                new Vector2(248f, -38f),
                new Vector2(112f, 122f),
                new Color(0.08f, 0.2f, 0.12f, 1f),
                new Color(0.35f, 0.92f, 0.52f, 1f),
                ShowSettings);

            var footer = CreatePanel(card, "Dashboard Footer", new Vector2(0f, -163f), new Vector2(560f, 48f), new Color(0.015f, 0.026f, 0.048f, 0.96f));
            ResolveApiClient();
            var user = apiClient.GetCurrentUser();
            playerSummaryLabel = CreateText(footer, $"{user.displayName}  ·  {user.coins} coins", new Vector2(-178f, 0f), new Vector2(190f, 24f), 14, FontStyle.Bold, new Color(0.9f, 0.96f, 1f, 1f));
            CreateText(footer, "P Play · R Room · H Help · L Scores · Esc", new Vector2(98f, 0f), new Vector2(330f, 24f), 11, FontStyle.Bold, new Color(0.55f, 0.66f, 0.82f, 1f));
        }

        private void CreateStatusBadge(Transform parent, string text, Vector2 position, Vector2 size, Color fillColor, Color textColor)
        {
            var badge = CreatePanel(parent, $"{text} Badge", position, size, fillColor);
            CreateText(badge, text, Vector2.zero, size, 11, FontStyle.Bold, textColor);
        }

        private Button CreateDashboardTile(
            Transform parent,
            string title,
            string subtitle,
            string description,
            Vector2 position,
            Vector2 size,
            Color fillColor,
            Color accentColor,
            UnityEngine.Events.UnityAction onClick)
        {
            var tileObject = new GameObject($"{title} Dashboard Tile");
            tileObject.transform.SetParent(parent, false);

            var rect = tileObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = tileObject.AddComponent<Image>();
            ApplyRoundedSprite(image);
            image.color = fillColor;
            AddOutline(tileObject, accentColor, new Vector2(1.4f, -1.4f));
            AddShadow(tileObject, new Color(0f, 0f, 0f, 0.38f), new Vector2(0f, -4f));

            var button = tileObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            var colors = button.colors;
            colors.highlightedColor = Color.Lerp(fillColor, accentColor, 0.28f);
            colors.pressedColor = Color.Lerp(fillColor, Color.black, 0.22f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            CreatePanel(tileObject.transform, $"{title} Icon Plate", new Vector2(0f, 30f), new Vector2(72f, 34f), new Color(accentColor.r, accentColor.g, accentColor.b, 0.16f));
            CreateText(tileObject.transform, title, new Vector2(0f, 31f), new Vector2(90f, 30f), 22, FontStyle.Bold, Color.white);
            CreateText(tileObject.transform, subtitle, new Vector2(0f, -7f), new Vector2(128f, 24f), 15, FontStyle.Bold, accentColor);
            CreateText(tileObject.transform, description, new Vector2(0f, -37f), new Vector2(132f, 28f), 11, FontStyle.Bold, new Color(0.72f, 0.8f, 0.94f, 1f));
            return button;
        }

        private void CreateHowToPanel(Transform parent)
        {
            howToPanel = CreatePanel(parent, "How To Play Panel", Vector2.zero, new Vector2(560f, 430f), new Color(0.025f, 0.04f, 0.075f, 0.97f)).gameObject;
            var panel = howToPanel.transform;

            CreateText(panel, "HOW TO PLAY", new Vector2(0f, 165f), new Vector2(500f, 44f), 30, FontStyle.Bold, new Color(1f, 0.82f, 0.23f, 1f));
            CreateText(
                panel,
                "1. Taya searches for all hiders.\n2. Taya uses Bang + the hider name to catch someone.\n3. Hiders avoid Taya and can use close-range SAK as a safe counter.\n4. Taya wins by catching all hiders.\n5. Hiders win by SAK counter or by surviving until time runs out.",
                new Vector2(0f, 34f),
                new Vector2(480f, 190f),
                18,
                FontStyle.Bold,
                new Color(0.86f, 0.93f, 1f, 1f));

            CreateText(panel, "No realistic weapons. No blood. Cartoon tag energy only.", new Vector2(0f, -84f), new Vector2(470f, 34f), 15, FontStyle.Bold, new Color(0.72f, 1f, 0.72f, 1f));
            CreateButton(panel, "BACK", new Vector2(-90f, -160f), new Vector2(150f, 42f), new Color(0.16f, 0.22f, 0.32f, 1f), HidePanels);
            CreateButton(panel, "PLAY", new Vector2(90f, -160f), new Vector2(150f, 42f), new Color(0.94f, 0.3f, 0.12f, 1f), PlayLocal);
        }

        private void CreateSettingsPanel(Transform parent)
        {
            settingsPanel = CreatePanel(parent, "Settings Placeholder Panel", Vector2.zero, new Vector2(500f, 330f), new Color(0.025f, 0.04f, 0.075f, 0.97f)).gameObject;
            var panel = settingsPanel.transform;

            CreateText(panel, "SETTINGS", new Vector2(0f, 112f), new Vector2(430f, 42f), 30, FontStyle.Bold, new Color(1f, 0.82f, 0.23f, 1f));
            CreateText(panel, "Placeholder for the local prototype.\nLater phases can add volume, language, controls, and accessibility here.", new Vector2(0f, 25f), new Vector2(420f, 86f), 17, FontStyle.Bold, new Color(0.82f, 0.9f, 1f, 1f));
            CreateButton(panel, "BACK", new Vector2(0f, -102f), new Vector2(160f, 42f), new Color(0.16f, 0.22f, 0.32f, 1f), HidePanels);
        }

        private void CreateNetworkPanel(Transform parent)
        {
            networkPanel = CreatePanel(parent, "Network Room Panel", Vector2.zero, new Vector2(560f, 430f), new Color(0.025f, 0.04f, 0.075f, 0.97f)).gameObject;
            var panel = networkPanel.transform;

            CreateText(panel, "ROOM", new Vector2(0f, 165f), new Vector2(500f, 44f), 30, FontStyle.Bold, new Color(1f, 0.82f, 0.23f, 1f));
            CreateText(panel, "Photon-ready scaffold. The real Fusion adapter is wired after the SDK is imported.", new Vector2(0f, 126f), new Vector2(470f, 28f), 13, FontStyle.Bold, new Color(0.72f, 0.82f, 1f, 1f));

            var statusCard = CreatePanel(panel, "Room Status Card", new Vector2(0f, 42f), new Vector2(470f, 138f), new Color(0.012f, 0.024f, 0.045f, 0.95f));
            networkStatusLabel = CreateText(statusCard, string.Empty, Vector2.zero, new Vector2(424f, 104f), 15, FontStyle.Bold, new Color(0.86f, 0.93f, 1f, 1f));

            CreateButton(panel, "CREATE", new Vector2(-170f, -72f), new Vector2(140f, 42f), new Color(0.94f, 0.3f, 0.12f, 1f), OnCreateRoomClicked);
            CreateButton(panel, "JOIN 1234", new Vector2(0f, -72f), new Vector2(150f, 42f), new Color(0.22f, 0.42f, 0.92f, 1f), OnJoinDefaultRoomClicked);
            CreateButton(panel, "LEAVE", new Vector2(170f, -72f), new Vector2(140f, 42f), new Color(0.16f, 0.22f, 0.32f, 1f), OnLeaveRoomClicked);

            CreateButton(panel, "BACK", new Vector2(0f, -156f), new Vector2(160f, 42f), new Color(0.16f, 0.22f, 0.32f, 1f), HidePanels);
            RefreshNetworkPanel();
        }

        public void ShowLeaderboard()
        {
            CreateMenu();
            HidePanels();
            RefreshLeaderboardPanel();
            leaderboardPanel.SetActive(true);
        }

        private void CreateLeaderboardPanel(Transform parent)
        {
            leaderboardPanel = CreatePanel(parent, "Palengke Leaderboard Panel", Vector2.zero, new Vector2(520f, 450f), new Color(0.025f, 0.04f, 0.075f, 0.98f)).gameObject;
            var panel = leaderboardPanel.transform;

            CreateText(panel, "PALENGKE LEADERBOARD", new Vector2(0f, 174f), new Vector2(460f, 42f), 28, FontStyle.Bold, new Color(1f, 0.82f, 0.23f, 1f));
            CreateText(panel, "LIVE PALENGKE SCORES", new Vector2(0f, 139f), new Vector2(300f, 24f), 12, FontStyle.Bold, new Color(0.6f, 0.78f, 1f, 1f));

            leaderboardRows = CreatePanel(panel, "Leaderboard Rows", new Vector2(0f, 15f), new Vector2(430f, 220f), new Color(0.012f, 0.024f, 0.045f, 0.95f));
            ResolveApiClient();
            PopulateLeaderboardRows();

            apiStatusLabel = CreateText(panel, apiClient.StatusMessage, new Vector2(0f, -122f), new Vector2(430f, 34f), 13, FontStyle.Bold, new Color(0.62f, 0.76f, 0.94f, 1f));
            CreateButton(panel, "BACK", new Vector2(0f, -177f), new Vector2(160f, 42f), new Color(0.16f, 0.22f, 0.32f, 1f), HidePanels);
        }

        private void RefreshLeaderboardPanel()
        {
            ResolveApiClient();
            var user = apiClient.GetCurrentUser();
            if (playerSummaryLabel != null)
            {
                playerSummaryLabel.text = $"{user.displayName}  ·  {user.coins} coins";
            }
            if (apiStatusLabel != null)
            {
                apiStatusLabel.text = apiClient.StatusMessage;
            }

            if (!Application.isPlaying)
            {
                PopulateLeaderboardRows();
                return;
            }

            apiClient.BeginSessionRefresh(_ => RefreshApiLabels());
            apiClient.BeginLeaderboardRefresh(_ =>
            {
                PopulateLeaderboardRows();
                RefreshApiLabels();
            });
        }

        private void RefreshApiLabels()
        {
            if (apiClient == null)
            {
                return;
            }
            var user = apiClient.GetCurrentUser();
            if (playerSummaryLabel != null)
            {
                playerSummaryLabel.text = $"{user.displayName}  ·  {user.coins} coins";
            }
            if (apiStatusLabel != null)
            {
                apiStatusLabel.text = apiClient.StatusMessage;
            }
        }

        private void PopulateLeaderboardRows()
        {
            if (leaderboardRows == null || apiClient == null)
            {
                return;
            }
            for (var index = leaderboardRows.childCount - 1; index >= 0; index -= 1)
            {
                DestroyObject(leaderboardRows.GetChild(index).gameObject);
            }

            var entries = apiClient.GetLeaderboard();
            if (entries.Length == 0)
            {
                CreateText(leaderboardRows, "No scores yet", Vector2.zero, new Vector2(360f, 40f), 17, FontStyle.Bold, new Color(0.7f, 0.8f, 0.94f, 1f));
                return;
            }

            var visibleCount = Mathf.Min(entries.Length, 5);
            var currentUser = apiClient.GetCurrentUser();
            for (var index = 0; index < visibleCount; index += 1)
            {
                var entry = entries[index];
                var color = entry.userId == currentUser.userId
                    ? new Color(1f, 0.82f, 0.23f, 1f)
                    : new Color(0.86f, 0.93f, 1f, 1f);
                CreateText(leaderboardRows, $"#{entry.rank}   {entry.displayName}", new Vector2(-95f, 78f - index * 39f), new Vector2(190f, 30f), 17, FontStyle.Bold, color);
                CreateText(leaderboardRows, entry.score.ToString(), new Vector2(130f, 78f - index * 39f), new Vector2(110f, 30f), 17, FontStyle.Bold, color);
            }
        }

        private void OnCreateRoomClicked()
        {
            ResolveRoomController();
            roomController.CreateRoom();
            RefreshNetworkPanel();
        }

        private void OnJoinDefaultRoomClicked()
        {
            ResolveRoomController();
            roomController.JoinDefaultRoom();
            RefreshNetworkPanel();
        }

        private void OnLeaveRoomClicked()
        {
            ResolveRoomController();
            roomController.LeaveRoom();
            RefreshNetworkPanel();
        }

        private void RefreshNetworkPanel()
        {
            if (networkStatusLabel == null)
            {
                return;
            }

            ResolveRoomController();
            var sdkStatus = roomController.IsFusionSdkAvailable ? "Fusion SDK: detected" : "Fusion SDK: not imported";
            var room = roomController.HasActiveRoom ? roomController.ActiveRoomCode : "none";
            networkStatusLabel.text =
                $"{sdkStatus}\n" +
                $"Provider: {roomController.ProviderName}\n" +
                $"State: {roomController.State}\n" +
                $"Room: {room}\n" +
                roomController.StatusMessage;
        }

        private void CreateGlow(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var glow = new GameObject(name);
            glow.transform.SetParent(parent, false);

            var rect = glow.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = glow.AddComponent<Image>();
            ApplyRoundedSprite(image);
            image.color = color;
            image.raycastTarget = false;
        }

        private Transform CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = panel.AddComponent<Image>();
            ApplyRoundedSprite(image);
            image.color = color;

            AddOutline(panel, new Color(0.17f, 0.24f, 0.36f, 0.9f), new Vector2(1.25f, -1.25f));
            AddShadow(panel, new Color(0f, 0f, 0f, 0.45f), new Vector2(0f, -4f));
            return panel.transform;
        }

        private Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObject = new GameObject($"{label} Button");
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = buttonObject.AddComponent<Image>();
            ApplyRoundedSprite(image);
            image.color = color;

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            var colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.16f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            CreateText(buttonObject.transform, label, Vector2.zero, size, Mathf.RoundToInt(size.y * 0.42f), FontStyle.Bold, Color.white);
            return button;
        }

        private Text CreateText(Transform parent, string text, Vector2 position, Vector2 size, int fontSize, FontStyle fontStyle, Color color)
        {
            var textObject = new GameObject($"{text} Text");
            textObject.transform.SetParent(parent, false);

            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var label = textObject.AddComponent<Text>();
            label.text = text;
            label.alignment = TextAnchor.MiddleCenter;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.color = color;
            label.raycastTarget = false;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 10;
            label.resizeTextMaxSize = fontSize;
            return label;
        }

        private static void AddOutline(GameObject target, Color color, Vector2 distance)
        {
            var outline = target.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = distance;
            outline.useGraphicAlpha = true;
        }

        private static void AddShadow(GameObject target, Color color, Vector2 distance)
        {
            var shadow = target.AddComponent<Shadow>();
            shadow.effectColor = color;
            shadow.effectDistance = distance;
            shadow.useGraphicAlpha = true;
        }

        private static void ApplyRoundedSprite(Image image)
        {
            image.sprite = GetRoundedPanelSprite();
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
        }

        private static Sprite GetRoundedPanelSprite()
        {
            if (roundedPanelSprite != null)
            {
                return roundedPanelSprite;
            }

            const int size = 64;
            const int radius = 22;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Runtime Main Menu Rounded Sprite",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y += 1)
            {
                for (var x = 0; x < size; x += 1)
                {
                    var left = Mathf.Min(x + 0.5f, size - x - 0.5f);
                    var bottom = Mathf.Min(y + 0.5f, size - y - 0.5f);
                    var alpha = 1f;

                    if (left < radius && bottom < radius)
                    {
                        var dx = radius - left;
                        var dy = radius - bottom;
                        var distance = Mathf.Sqrt(dx * dx + dy * dy);
                        alpha = Mathf.Clamp01(radius + 0.75f - distance);
                    }

                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);

            roundedPanelSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius));
            roundedPanelSprite.name = "Runtime Main Menu Rounded Sprite";
            return roundedPanelSprite;
        }
    }
}
