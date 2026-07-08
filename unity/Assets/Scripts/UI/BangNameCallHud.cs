using Palengke.BangSak.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Palengke.BangSak.UI
{
    public sealed class BangNameCallHud : MonoBehaviour
    {
        [SerializeField]
        private BangNameCallController controller;

        [SerializeField]
        private Vector2 panelSize = new Vector2(260f, 86f);

        [SerializeField]
        private Vector2 panelOffset = new Vector2(18f, -18f);

        private GameObject hudRoot;
        private Text selectedNameLabel;
        private Text feedbackLabel;

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

        private void CreateHud()
        {
            if (hudRoot != null)
            {
                return;
            }

            var canvasObject = new GameObject("Phase 18 Bang Name HUD");
            canvasObject.transform.SetParent(null, false);
            hudRoot = canvasObject;

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 19;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);

            canvasObject.AddComponent<GraphicRaycaster>();

            var panelObject = new GameObject("Bang Name Panel");
            panelObject.transform.SetParent(canvasObject.transform, false);
            var panelRect = panelObject.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.sizeDelta = panelSize;
            panelRect.anchoredPosition = panelOffset;

            var panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0.04f, 0.07f, 0.12f, 0.86f);

            CreateButton(panelObject.transform, "<", new Vector2(8f, -12f), OnPreviousClicked);
            CreateButton(panelObject.transform, ">", new Vector2(214f, -12f), OnNextClicked);

            selectedNameLabel = CreateText(
                panelObject.transform,
                "Call: —",
                new Vector2(48f, -10f),
                new Vector2(166f, 34f),
                18,
                FontStyle.Bold,
                Color.white);

            feedbackLabel = CreateText(
                panelObject.transform,
                "Choose a hider name",
                new Vector2(12f, -48f),
                new Vector2(236f, 28f),
                12,
                FontStyle.Normal,
                new Color(0.84f, 0.9f, 1f, 1f));
        }

        private Button CreateButton(Transform parent, string text, Vector2 position, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObject = new GameObject($"{text} Target Button");
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(38f, 32f);
            rect.anchoredPosition = position;

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.16f, 0.23f, 0.36f, 0.96f);

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            var label = CreateText(
                buttonObject.transform,
                text,
                Vector2.zero,
                new Vector2(38f, 32f),
                18,
                FontStyle.Bold,
                Color.white);
            label.alignment = TextAnchor.MiddleCenter;
            return button;
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
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var label = textObject.AddComponent<Text>();
            label.text = text;
            label.alignment = TextAnchor.MiddleLeft;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.color = color;
            label.raycastTarget = false;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 8;
            label.resizeTextMaxSize = fontSize;
            return label;
        }

        private void Refresh()
        {
            var hasController = controller != null && controller.isActiveAndEnabled;
            if (hudRoot != null)
            {
                hudRoot.SetActive(hasController);
            }

            if (!hasController)
            {
                return;
            }

            if (selectedNameLabel != null)
            {
                var selectedName = controller.SelectedTargetName;
                selectedNameLabel.text = string.IsNullOrEmpty(selectedName)
                    ? "Call: —"
                    : $"Call: {selectedName}";
            }

            if (feedbackLabel != null)
            {
                feedbackLabel.text = controller.LastFeedbackMessage;
            }
        }

        private void OnPreviousClicked()
        {
            if (controller != null)
            {
                controller.SelectPreviousTarget();
            }
        }

        private void OnNextClicked()
        {
            if (controller != null)
            {
                controller.SelectNextTarget();
            }
        }

        private void ResolveController()
        {
            if (controller == null)
            {
                controller = GetComponent<BangNameCallController>();
            }
        }
    }
}
