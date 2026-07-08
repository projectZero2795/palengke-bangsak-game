using Palengke.BangSak.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Palengke.BangSak.UI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeRoundRulesHud : MonoBehaviour
    {
        [SerializeField]
        private PrototypeRoundRulesController controller;

        [SerializeField]
        private Vector2 statusPanelSize = new Vector2(560f, 76f);

        [SerializeField]
        private Vector2 statusPanelOffset = new Vector2(0f, -18f);

        [SerializeField]
        private Vector2 resultPanelSize = new Vector2(360f, 210f);

        [SerializeField]
        private int sortingOrder = 24;

        private GameObject hudRoot;
        private GameObject resultPanel;
        private Text timerLabel;
        private Text hidersLabel;
        private Text roundLabel;
        private Text resultTitleLabel;
        private Text resultMessageLabel;
        private Button restartButton;

        public PrototypeRoundRulesController Controller => controller;

        public Vector2 StatusPanelSize => statusPanelSize;

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
                hidersLabel.text = $"HIDERS\n{controller.RemainingHiders} / {controller.TotalHiders}";
            }

            if (roundLabel != null)
            {
                roundLabel.text = controller.IsFinished
                    ? "FINISHED"
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
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);

            canvasObject.AddComponent<GraphicRaycaster>();

            CreateStatusPanel(canvasObject.transform);
            CreateResultPanel(canvasObject.transform);
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
            background.color = new Color(0.02f, 0.035f, 0.06f, 0.88f);
            AddOutline(panel, new Color(0.16f, 0.22f, 0.32f, 0.82f), new Vector2(2f, -2f));
            AddShadow(panel, new Color(0f, 0f, 0f, 0.48f), new Vector2(0f, -4f));

            var timerChip = CreateChip(
                panel.transform,
                "Timer Chip",
                new Vector2(-192f, 0f),
                new Vector2(144f, 52f),
                new Color(0.08f, 0.11f, 0.16f, 0.94f),
                new Color(1f, 0.72f, 0.22f, 0.9f));

            timerLabel = CreateText(timerChip, "02:30", Vector2.zero, new Vector2(138f, 48f), 31, FontStyle.Bold, Color.white);
            timerLabel.lineSpacing = 0.88f;

            var hidersChip = CreateChip(
                panel.transform,
                "Hiders Chip",
                Vector2.zero,
                new Vector2(166f, 52f),
                new Color(0.07f, 0.12f, 0.19f, 0.94f),
                new Color(0.35f, 0.55f, 1f, 0.74f));

            hidersLabel = CreateText(hidersChip, "HIDERS\n0 / 0", Vector2.zero, new Vector2(156f, 48f), 17, FontStyle.Bold, new Color(0.86f, 0.92f, 1f, 1f));
            hidersLabel.lineSpacing = 0.82f;

            var roundChip = CreateChip(
                panel.transform,
                "Round Chip",
                new Vector2(192f, 0f),
                new Vector2(166f, 52f),
                new Color(0.06f, 0.1f, 0.17f, 0.94f),
                new Color(0.42f, 1f, 0.58f, 0.62f));

            roundLabel = CreateText(roundChip, "ROUND 1", Vector2.zero, new Vector2(156f, 48f), 18, FontStyle.Bold, new Color(0.88f, 1f, 0.9f, 1f));
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
            image.color = fillColor;
            AddOutline(chip, outlineColor, new Vector2(2f, -2f));
            AddShadow(chip, new Color(0f, 0f, 0f, 0.32f), new Vector2(0f, -2f));
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
            background.color = new Color(0.025f, 0.04f, 0.075f, 0.94f);

            resultTitleLabel = CreateText(resultPanel.transform, "Round over", new Vector2(0f, 62f), new Vector2(resultPanelSize.x - 32f, 44f), 27, FontStyle.Bold, Color.white);
            resultMessageLabel = CreateText(resultPanel.transform, "Result message", new Vector2(0f, 14f), new Vector2(resultPanelSize.x - 42f, 58f), 16, FontStyle.Normal, new Color(0.86f, 0.9f, 1f, 1f));

            var buttonObject = new GameObject("Restart Round Button");
            buttonObject.transform.SetParent(resultPanel.transform, false);

            var buttonRect = buttonObject.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.sizeDelta = new Vector2(178f, 46f);
            buttonRect.anchoredPosition = new Vector2(0f, 24f);

            var buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.22f, 0.4f, 0.95f, 1f);

            restartButton = buttonObject.AddComponent<Button>();
            restartButton.targetGraphic = buttonImage;
            restartButton.onClick.AddListener(OnRestartClicked);

            var restartLabel = CreateText(buttonObject.transform, "Restart", Vector2.zero, buttonRect.sizeDelta, 18, FontStyle.Bold, Color.white);
            restartLabel.alignment = TextAnchor.MiddleCenter;

            resultPanel.SetActive(false);
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
                controller.RestartRound();
            }
        }
    }
}
