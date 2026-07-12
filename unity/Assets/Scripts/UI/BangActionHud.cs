using Palengke.BangSak.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Palengke.BangSak.UI
{
    public sealed class BangActionHud : MonoBehaviour
    {
        [SerializeField]
        private BangActionController controller;

        [SerializeField]
        private string buttonLabel = "";

        [SerializeField]
        private Vector2 buttonSize = new Vector2(72f, 72f);

        [SerializeField]
        private Vector2 buttonOffset = new Vector2(-24f, 24f);

        [SerializeField]
        private Sprite buttonBackgroundSprite = null;

        [SerializeField]
        private Sprite buttonIconSprite = null;

        private GameObject hudRoot;
        private Button button;
        private Image buttonImage;
        private Image iconImage;
        private Text label;
        private bool hudVisible = true;

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

        public void SetController(BangActionController bangController)
        {
            controller = bangController;
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

            var canvasObject = new GameObject("Phase 5 Bang HUD");
            canvasObject.transform.SetParent(null, false);
            hudRoot = canvasObject;

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            SafeAreaCanvasLayout.ConfigureScaler(scaler);

            canvasObject.AddComponent<GraphicRaycaster>();
            var safeAreaRoot = SafeAreaCanvasLayout.GetOrCreateSafeAreaRoot(canvasObject.transform);

            var buttonObject = new GameObject("Bang Button");
            buttonObject.transform.SetParent(safeAreaRoot, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = buttonSize;
            rect.anchoredPosition = buttonOffset;

            buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.sprite = buttonBackgroundSprite;
            buttonImage.color = buttonBackgroundSprite != null ? Color.white : new Color(0.11f, 0.14f, 0.2f, 0.96f);
            buttonImage.preserveAspect = true;

            button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(OnBangClicked);

            var iconObject = new GameObject("Bang Button Tsinelas Icon");
            iconObject.transform.SetParent(buttonObject.transform, false);

            var iconRect = iconObject.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.16f, 0.16f);
            iconRect.anchorMax = new Vector2(0.84f, 0.84f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            iconImage = iconObject.AddComponent<Image>();
            iconImage.sprite = buttonIconSprite;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            var textObject = new GameObject("Bang Button Label");
            textObject.transform.SetParent(buttonObject.transform, false);

            var textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            label = textObject.AddComponent<Text>();
            label.text = buttonLabel;
            label.alignment = TextAnchor.MiddleCenter;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = 18;
            label.fontStyle = FontStyle.Bold;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 12;
            label.resizeTextMaxSize = 20;
            label.color = Color.white;
            label.raycastTarget = false;

            ApplyHudVisibility();
        }

        private void OnDestroy()
        {
            if (hudRoot != null)
            {
                Destroy(hudRoot);
            }
        }

        private void OnBangClicked()
        {
            if (controller != null && controller.isActiveAndEnabled)
            {
                controller.TryBangNow();
            }
        }

        private void Refresh()
        {
            if (!hudVisible || controller == null || button == null)
            {
                return;
            }

            var now = Time.time;
            var canBang = controller.CanBang(now);
            button.interactable = canBang;

            if (iconImage != null)
            {
                iconImage.enabled = canBang && buttonIconSprite != null;
            }

            if (buttonImage != null)
            {
                buttonImage.color = canBang ? Color.white : new Color(0.74f, 0.78f, 0.86f, 0.82f);
            }

            if (label != null)
            {
                label.text = buttonLabel;
            }
        }

        private void ResolveController()
        {
            if (controller == null)
            {
                controller = GetComponent<BangActionController>();
            }
        }

        private void ApplyHudVisibility()
        {
            if (hudRoot != null)
            {
                hudRoot.SetActive(hudVisible);
            }
        }
    }
}
