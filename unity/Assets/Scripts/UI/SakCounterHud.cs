using Palengke.BangSak.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Palengke.BangSak.UI
{
    public sealed class SakCounterHud : MonoBehaviour
    {
        [SerializeField]
        private SakCounterController controller;

        [SerializeField]
        private Vector2 buttonSize = new Vector2(66f, 66f);

        [SerializeField]
        private Vector2 buttonOffset = new Vector2(-24f, 104f);

        [SerializeField]
        private bool hudEnabledForThisObject = true;

        private GameObject hudRoot;
        private Button button;
        private Image buttonImage;
        private Image cooldownFill;
        private Text label;
        private bool hudVisible = true;

        public bool HudEnabledForThisObject => hudEnabledForThisObject;

        private void Start()
        {
            ResolveController();
            CreateHud();
            ApplyHudVisibility();
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

        public void SetHudVisible(bool visible)
        {
            hudVisible = visible;
            ApplyHudVisibility();
        }

        private void CreateHud()
        {
            if (hudRoot != null)
            {
                return;
            }

            var canvasObject = new GameObject("Phase 19 SAK Counter HUD");
            canvasObject.transform.SetParent(null, false);
            hudRoot = canvasObject;

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 18;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);

            canvasObject.AddComponent<GraphicRaycaster>();

            var buttonObject = new GameObject("SAK Counter Button");
            buttonObject.transform.SetParent(canvasObject.transform, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = buttonSize;
            rect.anchoredPosition = buttonOffset;

            buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.12f, 0.46f, 0.21f, 0.96f);

            button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(OnSakClicked);

            var cooldownObject = new GameObject("SAK Cooldown Radial Fill");
            cooldownObject.transform.SetParent(buttonObject.transform, false);
            var cooldownRect = cooldownObject.AddComponent<RectTransform>();
            cooldownRect.anchorMin = new Vector2(0.08f, 0.08f);
            cooldownRect.anchorMax = new Vector2(0.92f, 0.92f);
            cooldownRect.offsetMin = Vector2.zero;
            cooldownRect.offsetMax = Vector2.zero;

            cooldownFill = cooldownObject.AddComponent<Image>();
            cooldownFill.color = new Color(0.02f, 0.08f, 0.035f, 0.8f);
            cooldownFill.type = Image.Type.Filled;
            cooldownFill.fillMethod = Image.FillMethod.Radial360;
            cooldownFill.fillOrigin = (int)Image.Origin360.Top;
            cooldownFill.fillClockwise = false;
            cooldownFill.raycastTarget = false;

            label = CreateText(buttonObject.transform, "SAK", Vector2.zero, buttonSize, 18, FontStyle.Bold, Color.white);
            label.alignment = TextAnchor.MiddleCenter;
            ApplyHudVisibility();
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
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = position;
            rect.offsetMax = Vector2.zero;
            rect.sizeDelta = size;

            var textComponent = textObject.AddComponent<Text>();
            textComponent.text = text;
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

        private void OnSakClicked()
        {
            if (controller != null && controller.isActiveAndEnabled)
            {
                controller.TrySakNow();
            }
        }

        private void Refresh()
        {
            if (!hudVisible || !hudEnabledForThisObject || controller == null || button == null)
            {
                return;
            }

            var now = Time.time;
            var canSak = controller.CanSak(now);
            var remaining = controller.CooldownRemaining(now);
            button.interactable = canSak;

            if (buttonImage != null)
            {
                buttonImage.color = canSak
                    ? new Color(0.12f, 0.54f, 0.24f, 0.96f)
                    : new Color(0.42f, 0.52f, 0.44f, 0.78f);
            }

            if (label != null)
            {
                label.text = canSak ? "SAK" : ActionCooldownDisplay.FormatSeconds(remaining);
            }

            if (cooldownFill != null)
            {
                cooldownFill.enabled = !canSak;
                cooldownFill.fillAmount = ActionCooldownDisplay.RemainingFraction(remaining, controller.CooldownSeconds);
            }
        }

        private void ResolveController()
        {
            if (controller == null)
            {
                controller = GetComponent<SakCounterController>();
            }
        }

        private void ApplyHudVisibility()
        {
            if (hudRoot != null)
            {
                hudRoot.SetActive(hudVisible && hudEnabledForThisObject);
            }
        }
    }
}
