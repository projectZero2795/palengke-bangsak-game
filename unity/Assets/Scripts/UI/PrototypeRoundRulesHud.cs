using Palengke.BangSak.Game;
using Palengke.BangSak.Network;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Palengke.BangSak.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeRoundRulesHud : MonoBehaviour
    {
        [SerializeField]
        private PrototypeRoundRulesController controller;

        [SerializeField]
        private Vector2 statusPanelSize = new Vector2(352f, 48f);

        [SerializeField]
        private Vector2 statusPanelOffset = new Vector2(0f, -8f);

        [SerializeField]
        private Vector2 resultPanelSize = new Vector2(360f, 210f);

        [SerializeField]
        private int sortingOrder = 24;

        [SerializeField]
        private string mainMenuSceneName = "MainMenu";

        [SerializeField]
        private bool showMainMenuButton = true;

        private GameObject hudRoot;
        private GameObject resultPanel;
        private Text timerLabel;
        private Text hidersLabel;
        private Text roundLabel;
        private Text resultTitleLabel;
        private Text resultMessageLabel;
        private Button restartButton;
        private static Sprite roundedHudSprite;

        public PrototypeRoundRulesController Controller => controller;

        public Vector2 StatusPanelSize => statusPanelSize;

        public string MainMenuSceneName => mainMenuSceneName;

        public bool ShowMainMenuButton => showMainMenuButton;

        public bool HasRuntimeHud => hudRoot != null;

        private void Start()
        {
            ResolveController();
            CreateHud();
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void OnDestroy()
        {
            if (hudRoot != null)
            {
                Destroy(hudRoot);
            }
        }

        public void SetController(PrototypeRoundRulesController roundController)
        {
            controller = roundController;
        }

        public void Refresh()
        {
            if (controller == null)
            {
                ResolveController();
            }

            if (controller == null || hudRoot == null)
            {
                return;
            }

            if (timerLabel != null)
            {
                timerLabel.text = controller.FormatRemainingTime();
            }

            if (hidersLabel != null)
            {
                hidersLabel.text = $"HIDERS {controller.RemainingHiders}/{controller.TotalHiders}";
            }

            if (roundLabel != null)
            {
                roundLabel.text = controller.IsFinished
                    ? "DONE"
                    : controller.IsRunning
                        ? $"ROUND {controller.RoundNumber}"
                        : "READY";
            }

            var showResult = controller.IsFinished;
            if (resultPanel != null)
            {
                resultPanel.SetActive(showResult);
            }

            if (showResult)
            {
                if (resultTitleLabel != null)
                {
                    resultTitleLabel.text = controller.ResultTitle;
                    resultTitleLabel.color = controller.Result == PrototypeRoundResult.TayaWins
                        ? new Color(1f, 0.46f, 0.34f, 1f)
                        : new Color(0.45f, 1f, 0.5f, 1f);
                }

                if (resultMessageLabel != null)
                {
                    resultMessageLabel.text = $"{controller.ResultMessage}\nPress R or tap Restart.";
                }
            }
        }

        private void ResolveController()
        {
            if (controller == null)
            {
                controller = FindObjectOfType<PrototypeRoundRulesController>();
            }
        }

        private void CreateHud()
        {
            if (hudRoot != null)
            {
                return;
            }

            var canvasObject = new GameObject("Phase 21 Round Rules HUD");
            canvasObject.transform.SetParent(null, false);
            hudRoot = canvasObject;

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            SafeAreaCanvasLayout.ConfigureScaler(scaler);

            canvasObject.AddComponent<GraphicRaycaster>();
            var safeAreaRoot = SafeAreaCanvasLayout.GetOrCreateSafeAreaRoot(canvasObject.transform);

            CreateStatusPanel(safeAreaRoot);
            CreateResultPanel(safeAreaRoot);
        }

        private void CreateStatusPanel(Transform parent)
        {
            var panel = new GameObject("Round Status Panel");
            panel.transform.SetParent(parent, false);

            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = statusPanelSize;
            rect.anchoredPosition = statusPanelOffset;

            var background = panel.AddComponent<Image>();
            ApplyRoundedSprite(background);
            background.color = new Color(0.02f, 0.035f, 0.06f, 0.88f);
            AddOutline(panel, new Color(0.16f, 0.22f, 0.32f, 0.82f), new Vector2(1f, -1f));
            AddShadow(panel, new Color(0f, 0f, 0f, 0.42f), new Vector2(0f, -2f));

            var timerChip = CreateChip(
                panel.transform,
                "Timer Chip",
                new Vector2(-116f, 0f),
                new Vector2(92f, 34f),
                new Color(0.08f, 0.11f, 0.16f, 0.94f),
                new Color(1f, 0.72f, 0.22f, 0.9f));

            timerLabel = CreateText(timerChip, "02:30", Vector2.zero, new Vector2(86f, 31f), 22, FontStyle.Bold, Color.white);
            timerLabel.lineSpacing = 0.88f;

            var hidersChip = CreateChip(
                panel.transform,
                "Hiders Chip",
                Vector2.zero,
                new Vector2(110f, 34f),
                new Color(0.07f, 0.12f, 0.19f, 0.94f),
                new Color(0.35f, 0.55f, 1f, 0.74f));

            hidersLabel = CreateText(hidersChip, "HIDERS 0/0", Vector2.zero, new Vector2(104f, 31f), 13, FontStyle.Bold, new Color(0.86f, 0.92f, 1f, 1f));
            hidersLabel.lineSpacing = 0.82f;

            var roundChip = CreateChip(
                panel.transform,
                "Round Chip",
                new Vector2(116f, 0f),
                new Vector2(92f, 34f),
                new Color(0.06f, 0.1f, 0.17f, 0.94f),
                new Color(0.42f, 1f, 0.58f, 0.62f));

            roundLabel = CreateText(roundChip, "ROUND 1", Vector2.zero, new Vector2(86f, 31f), 13, FontStyle.Bold, new Color(0.88f, 1f, 0.9f, 1f));
        }

        private Transform CreateChip(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 size,
            Color fillColor,
            Color outlineColor)
        {
            var chip = new GameObject(name);
            chip.transform.SetParent(parent, false);

            var rect = chip.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = chip.AddComponent<Image>();
            ApplyRoundedSprite(image);
            image.color = fillColor;
            AddOutline(chip, outlineColor, new Vector2(1f, -1f));
            AddShadow(chip, new Color(0f, 0f, 0f, 0.28f), new Vector2(0f, -1f));
            return chip.transform;
        }

        private void CreateResultPanel(Transform parent)
        {
            resultPanel = new GameObject("Round Result Panel");
            resultPanel.transform.SetParent(parent, false);

            var rect = resultPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = resultPanelSize;
            rect.anchoredPosition = Vector2.zero;

            var background = resultPanel.AddComponent<Image>();
            ApplyRoundedSprite(background);
            background.color = new Color(0.025f, 0.04f, 0.075f, 0.94f);

            resultTitleLabel = CreateText(resultPanel.transform, "Round over", new Vector2(0f, 62f), new Vector2(resultPanelSize.x - 32f, 44f), 27, FontStyle.Bold, Color.white);
            resultMessageLabel = CreateText(resultPanel.transform, "Result message", new Vector2(0f, 14f), new Vector2(resultPanelSize.x - 42f, 58f), 16, FontStyle.Normal, new Color(0.86f, 0.9f, 1f, 1f));

            if (showMainMenuButton)
            {
                restartButton = CreateResultButton("Restart", new Vector2(-88f, 24f), new Vector2(148f, 42f), new Color(0.22f, 0.4f, 0.95f, 1f), OnRestartClicked);
                CreateResultButton("Menu", new Vector2(88f, 24f), new Vector2(148f, 42f), new Color(0.12f, 0.16f, 0.22f, 1f), OnMainMenuClicked);
            }
            else
            {
                restartButton = CreateResultButton("Restart", new Vector2(0f, 24f), new Vector2(178f, 46f), new Color(0.22f, 0.4f, 0.95f, 1f), OnRestartClicked);
            }

            resultPanel.SetActive(false);
        }

        private Button CreateResultButton(string label, Vector2 position, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObject = new GameObject($"{label} Round Result Button");
            buttonObject.transform.SetParent(resultPanel.transform, false);

            var buttonRect = buttonObject.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.sizeDelta = size;
            buttonRect.anchoredPosition = position;

            var buttonImage = buttonObject.AddComponent<Image>();
            ApplyRoundedSprite(buttonImage);
            buttonImage.color = color;

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(onClick);

            var text = CreateText(buttonObject.transform, label, Vector2.zero, size, 17, FontStyle.Bold, Color.white);
            text.alignment = TextAnchor.MiddleCenter;
            return button;
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
            image.sprite = GetRoundedHudSprite();
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
        }

        private static Sprite GetRoundedHudSprite()
        {
            if (roundedHudSprite != null)
            {
                return roundedHudSprite;
            }

            const int size = 64;
            const int radius = 24;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Runtime Rounded HUD Sprite",
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

            roundedHudSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius));
            roundedHudSprite.name = "Runtime Rounded HUD Sprite";
            return roundedHudSprite;
        }

        private Text CreateText(
            Transform parent,
            string text,
            Vector2 position,
            Vector2 size,
            int fontSize,
            FontStyle fontStyle,
            Color color)
        {
            var textObject = new GameObject($"{text} Text");
            textObject.transform.SetParent(parent, false);

            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var textComponent = textObject.AddComponent<Text>();
            textComponent.text = text;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.fontSize = fontSize;
            textComponent.fontStyle = fontStyle;
            textComponent.color = color;
            textComponent.raycastTarget = false;
            textComponent.resizeTextForBestFit = true;
            textComponent.resizeTextMinSize = 10;
            textComponent.resizeTextMaxSize = fontSize;
            return textComponent;
        }

        private void OnRestartClicked()
        {
            if (controller != null)
            {
                var networkSession = FusionNetworkSession.Active;
                if (networkSession != null
                    && networkSession.IsConnected
                    && networkSession.RequestRoundRestart())
                {
                    return;
                }

                controller.RestartRound();
            }
        }

        private void OnMainMenuClicked()
        {
            if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }
}
