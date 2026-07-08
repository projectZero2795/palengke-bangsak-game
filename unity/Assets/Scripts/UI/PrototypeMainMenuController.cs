using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Palengke.BangSak.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeMainMenuController : MonoBehaviour
    {
        public const string ComponentId = "prototype_main_menu";
        public const int ComponentVersion = 1;
        public const string ComponentVariant = "phase22_local_menu";

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
        private GameObject howToPanel;
        private GameObject settingsPanel;
        private static Sprite roundedPanelSprite;

        public string ComponentIdValue => componentId;

        public int ComponentVersionValue => componentVersion;

        public string ComponentVariantValue => componentVariant;

        public string PrototypeSceneName => prototypeSceneName;

        public bool HasRuntimeMenu => menuRoot != null;

        private void Start()
        {
            CreateMenu();
        }

        private void Update()
        {
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

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HidePanels();
            }
        }

        private void OnDestroy()
        {
            if (menuRoot != null)
            {
                Destroy(menuRoot);
            }
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
        }

        private void CreateMenu()
        {
            if (menuRoot != null)
            {
                return;
            }

            var canvasObject = new GameObject("Phase 22 Main Menu UI");
            canvasObject.transform.SetParent(null, false);
            menuRoot = canvasObject;

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);

            canvasObject.AddComponent<GraphicRaycaster>();

            CreateBackdrop(canvasObject.transform);
            CreateMainCard(canvasObject.transform);
            CreateHowToPanel(canvasObject.transform);
            CreateSettingsPanel(canvasObject.transform);
            HidePanels();
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
            var card = CreatePanel(parent, "Main Menu Card", Vector2.zero, new Vector2(520f, 420f), new Color(0.025f, 0.04f, 0.075f, 0.92f));

            CreateText(card, "BANG-SAK", new Vector2(0f, 145f), new Vector2(470f, 70f), 54, FontStyle.Bold, new Color(1f, 0.82f, 0.23f, 1f));
            CreateText(card, "Bang. Run. Hide. Sak!", new Vector2(0f, 94f), new Vector2(430f, 34f), 22, FontStyle.Bold, new Color(0.86f, 0.93f, 1f, 1f));
            CreateText(card, "A safe Filipino night palengke prototype.\nTaya catches by saying Bang + name. Hiders can counter with SAK.", new Vector2(0f, 42f), new Vector2(440f, 58f), 16, FontStyle.Normal, new Color(0.76f, 0.84f, 0.95f, 1f));

            CreateButton(card, "PLAY LOCAL", new Vector2(0f, -38f), new Vector2(260f, 48f), new Color(0.94f, 0.3f, 0.12f, 1f), PlayLocal);
            CreateButton(card, "HOW TO PLAY", new Vector2(0f, -98f), new Vector2(260f, 44f), new Color(0.12f, 0.42f, 0.9f, 1f), ShowHowTo);
            CreateButton(card, "SETTINGS", new Vector2(0f, -154f), new Vector2(260f, 40f), new Color(0.12f, 0.16f, 0.22f, 1f), ShowSettings);

            CreateText(card, "Keyboard: P play · H help · Esc close", new Vector2(0f, -198f), new Vector2(430f, 24f), 12, FontStyle.Bold, new Color(0.55f, 0.66f, 0.82f, 1f));
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
