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
        private BangActionController bangActionController;

        [SerializeField]
        private Vector2 panelSize = new Vector2(238f, 118f);

        [SerializeField]
        private Vector2 panelOffset = new Vector2(-18f, 18f);

        [SerializeField]
        [Range(1, 8)]
        private int maxVisibleTargets = 4;

        private GameObject hudRoot;
        private Transform buttonRoot;
        private Text feedbackLabel;
        private string renderedTargetSignature = string.Empty;

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
            panelRect.anchorMin = new Vector2(1f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(1f, 0f);
            panelRect.sizeDelta = panelSize;
            panelRect.anchoredPosition = panelOffset;

            var panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0.04f, 0.07f, 0.12f, 0.86f);

            var buttonRootObject = new GameObject("Bang Person Buttons");
            buttonRootObject.transform.SetParent(panelObject.transform, false);
            var buttonRootRect = buttonRootObject.AddComponent<RectTransform>();
            buttonRootRect.anchorMin = Vector2.zero;
            buttonRootRect.anchorMax = Vector2.one;
            buttonRootRect.offsetMin = Vector2.zero;
            buttonRootRect.offsetMax = Vector2.zero;
            buttonRoot = buttonRootObject.transform;

            feedbackLabel = CreateText(
                panelObject.transform,
                "Tap who you see",
                new Vector2(10f, -90f),
                new Vector2(218f, 22f),
                12,
                FontStyle.Normal,
                new Color(0.84f, 0.9f, 1f, 1f));
        }

        private Button CreateTargetButton(Transform parent, string targetName, int index)
        {
            var buttonObject = new GameObject($"Bang {targetName} Button");
            buttonObject.transform.SetParent(parent, false);

            var column = index % 2;
            var row = index / 2;
            var position = new Vector2(10f + column * 112f, 76f - row * 40f);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.sizeDelta = new Vector2(106f, 34f);
            rect.anchoredPosition = position;

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.64f, 0.16f, 0.14f, 0.96f);

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => OnBangTargetClicked(targetName));

            var label = CreateText(
                buttonObject.transform,
                $"Bang\n{targetName}",
                Vector2.zero,
                new Vector2(106f, 34f),
                13,
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

            RebuildButtonsIfNeeded();

            if (feedbackLabel != null)
            {
                feedbackLabel.text = controller.LastFeedbackMessage;
            }
        }

        private void RebuildButtonsIfNeeded()
        {
            if (buttonRoot == null || controller == null)
            {
                return;
            }

            var targets = controller.GetSelectableTargets();
            var targetCount = Mathf.Min(targets.Count, maxVisibleTargets);
            var signature = string.Empty;
            for (var index = 0; index < targetCount; index += 1)
            {
                signature += targets[index].DisplayName + "|";
            }

            if (signature == renderedTargetSignature)
            {
                return;
            }

            renderedTargetSignature = signature;
            for (var index = buttonRoot.childCount - 1; index >= 0; index -= 1)
            {
                Destroy(buttonRoot.GetChild(index).gameObject);
            }

            for (var index = 0; index < targetCount; index += 1)
            {
                CreateTargetButton(buttonRoot, targets[index].DisplayName, index);
            }
        }

        private void OnBangTargetClicked(string targetName)
        {
            if (controller == null)
            {
                return;
            }

            controller.SetSelectedTargetName(targetName);
            if (bangActionController != null && bangActionController.isActiveAndEnabled)
            {
                bangActionController.TryBangNow();
            }
        }

        private void ResolveController()
        {
            if (controller == null)
            {
                controller = GetComponent<BangNameCallController>();
            }

            if (bangActionController == null)
            {
                bangActionController = GetComponent<BangActionController>();
            }
        }
    }
}
