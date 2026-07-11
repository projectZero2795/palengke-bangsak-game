using System.Collections.Generic;
using Palengke.BangSak.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Palengke.BangSak.UI
{
    public sealed class BangNameCallHud : MonoBehaviour
    {
        private sealed class TargetCooldownVisual
        {
            public Button Button;
            public Image ProgressFill;
            public Text SecondsLabel;
        }

        [SerializeField]
        private BangNameCallController controller;

        [SerializeField]
        private BangActionController bangActionController;

        [SerializeField]
        private Vector2 panelSize = new Vector2(238f, 132f);

        [SerializeField]
        private Vector2 panelOffset = new Vector2(-18f, 18f);

        [SerializeField]
        [Range(1, 8)]
        private int maxVisibleTargets = 4;

        [SerializeField]
        private Sprite buttonIconSprite = null;

        private GameObject hudRoot;
        private Transform buttonRoot;
        private Text feedbackLabel;
        private Text cooldownLabel;
        private Image cooldownBarFill;
        private readonly List<TargetCooldownVisual> targetCooldownVisuals = new List<TargetCooldownVisual>();
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

            var cooldownTrackObject = new GameObject("Bang Cooldown Track");
            cooldownTrackObject.transform.SetParent(panelObject.transform, false);
            var cooldownTrackRect = cooldownTrackObject.AddComponent<RectTransform>();
            cooldownTrackRect.anchorMin = new Vector2(0f, 0f);
            cooldownTrackRect.anchorMax = new Vector2(0f, 0f);
            cooldownTrackRect.pivot = new Vector2(0f, 0f);
            cooldownTrackRect.anchoredPosition = new Vector2(10f, 6f);
            cooldownTrackRect.sizeDelta = new Vector2(218f, 6f);
            var cooldownTrack = cooldownTrackObject.AddComponent<Image>();
            cooldownTrack.color = new Color(0.015f, 0.025f, 0.045f, 0.95f);
            cooldownTrack.raycastTarget = false;

            var cooldownFillObject = new GameObject("Bang Cooldown Fill");
            cooldownFillObject.transform.SetParent(cooldownTrackObject.transform, false);
            var cooldownFillRect = cooldownFillObject.AddComponent<RectTransform>();
            cooldownFillRect.anchorMin = Vector2.zero;
            cooldownFillRect.anchorMax = Vector2.one;
            cooldownFillRect.offsetMin = Vector2.zero;
            cooldownFillRect.offsetMax = Vector2.zero;
            cooldownBarFill = cooldownFillObject.AddComponent<Image>();
            cooldownBarFill.color = new Color(1f, 0.58f, 0.2f, 1f);
            cooldownBarFill.type = Image.Type.Filled;
            cooldownBarFill.fillMethod = Image.FillMethod.Horizontal;
            cooldownBarFill.raycastTarget = false;

            cooldownLabel = CreateText(
                panelObject.transform,
                "BANG READY",
                new Vector2(164f, -(panelSize.y - 30f)),
                new Vector2(64f, 16f),
                10,
                FontStyle.Bold,
                new Color(1f, 0.78f, 0.28f, 1f));

            feedbackLabel = CreateText(
                panelObject.transform,
                "Tap the hider you see",
                new Vector2(10f, -(panelSize.y - 37f)),
                new Vector2(150f, 22f),
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
            var position = new Vector2(10f + column * 112f, panelSize.y - 42f - row * 40f);

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

            CreateIcon(buttonObject.transform, buttonIconSprite);

            var label = CreateText(
                buttonObject.transform,
                targetName,
                new Vector2(buttonIconSprite == null ? 10f : 36f, -2f),
                new Vector2(buttonIconSprite == null ? 68f : 42f, 30f),
                15,
                FontStyle.Bold,
                Color.white);
            label.alignment = TextAnchor.MiddleCenter;

            var progressObject = new GameObject($"{targetName} Cooldown Progress");
            progressObject.transform.SetParent(buttonObject.transform, false);
            var progressRect = progressObject.AddComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0f, 0f);
            progressRect.anchorMax = new Vector2(1f, 0f);
            progressRect.pivot = new Vector2(0f, 0f);
            progressRect.offsetMin = new Vector2(4f, 3f);
            progressRect.offsetMax = new Vector2(-4f, 7f);
            var progressFill = progressObject.AddComponent<Image>();
            progressFill.color = new Color(1f, 0.78f, 0.24f, 1f);
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.raycastTarget = false;

            var secondsLabel = CreateText(
                buttonObject.transform,
                string.Empty,
                new Vector2(79f, -3f),
                new Vector2(24f, 15f),
                9,
                FontStyle.Bold,
                new Color(1f, 0.9f, 0.52f, 1f));
            secondsLabel.alignment = TextAnchor.MiddleCenter;

            targetCooldownVisuals.Add(new TargetCooldownVisual
            {
                Button = button,
                ProgressFill = progressFill,
                SecondsLabel = secondsLabel
            });
            return button;
        }

        private void CreateIcon(Transform parent, Sprite iconSprite)
        {
            if (iconSprite == null)
            {
                return;
            }

            var iconObject = new GameObject("Tsinelas Icon");
            iconObject.transform.SetParent(parent, false);

            var rect = iconObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(24f, 24f);
            rect.anchoredPosition = new Vector2(8f, -5f);

            var icon = iconObject.AddComponent<Image>();
            icon.sprite = iconSprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
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

            RefreshCooldown();
        }

        private void RefreshCooldown()
        {
            if (bangActionController == null)
            {
                return;
            }

            var now = Time.time;
            var canBang = bangActionController.CanBang(now);
            var remaining = bangActionController.CooldownRemaining(now);
            for (var index = 0; index < targetCooldownVisuals.Count; index += 1)
            {
                var visual = targetCooldownVisuals[index];
                if (visual.Button != null)
                {
                    visual.Button.interactable = canBang;
                }

                if (visual.ProgressFill != null)
                {
                    visual.ProgressFill.fillAmount = canBang
                        ? 1f
                        : bangActionController.CooldownProgress(now);
                    visual.ProgressFill.color = canBang
                        ? new Color(0.36f, 1f, 0.5f, 1f)
                        : new Color(1f, 0.78f, 0.24f, 1f);
                }

                if (visual.SecondsLabel != null)
                {
                    visual.SecondsLabel.text = canBang
                        ? string.Empty
                        : ActionCooldownDisplay.FormatSeconds(remaining);
                }
            }

            if (cooldownBarFill != null)
            {
                cooldownBarFill.fillAmount = canBang
                    ? 1f
                    : bangActionController.CooldownProgress(now);
                cooldownBarFill.color = canBang
                    ? new Color(0.28f, 0.9f, 0.42f, 1f)
                    : new Color(1f, 0.58f, 0.2f, 1f);
            }

            if (cooldownLabel != null)
            {
                cooldownLabel.text = canBang
                    ? "READY"
                    : ActionCooldownDisplay.FormatSeconds(remaining);
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
            targetCooldownVisuals.Clear();
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
