using Palengke.BangSak.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Palengke.BangSak.UI
{
    public sealed class TagActionHud : MonoBehaviour
    {
        [SerializeField]
        private TagActionController controller;

        [SerializeField]
        private string buttonLabel = "TAG";

        [SerializeField]
        private Vector2 buttonSize = new Vector2(64f, 64f);

        [SerializeField]
        private Vector2 buttonOffset = new Vector2(-104f, 24f);

        [SerializeField]
        private Sprite buttonBackgroundSprite = null;

        private GameObject hudRoot;
        private Button button;
        private Image buttonImage;
        private Text label;

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

        public void SetController(TagActionController tagController)
        {
            controller = tagController;
        }

        private void CreateHud()
        {
            if (hudRoot != null)
            {
                return;
            }

            var canvasObject = new GameObject("Phase 7 Tag HUD");
            canvasObject.transform.SetParent(null, false);
            hudRoot = canvasObject;

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 21;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);

            canvasObject.AddComponent<GraphicRaycaster>();

            var buttonObject = new GameObject("Tag Button");
            buttonObject.transform.SetParent(canvasObject.transform, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = buttonSize;
            rect.anchoredPosition = buttonOffset;

            buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.sprite = buttonBackgroundSprite;
            buttonImage.color = buttonBackgroundSprite != null ? Color.white : new Color(0.07f, 0.22f, 0.18f, 0.96f);
            buttonImage.preserveAspect = true;

            button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(OnTagClicked);

            var textObject = new GameObject("Tag Button Label");
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
            label.fontSize = 15;
            label.fontStyle = FontStyle.Bold;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 10;
            label.resizeTextMaxSize = 18;
            label.color = new Color(0.8f, 1f, 0.95f, 1f);
            label.raycastTarget = false;
        }

        private void OnDestroy()
        {
            if (hudRoot != null)
            {
                Destroy(hudRoot);
            }
        }

        private void OnTagClicked()
        {
            if (controller != null)
            {
                controller.TryTagNow();
            }
        }

        private void Refresh()
        {
            if (controller == null || button == null)
            {
                return;
            }

            var now = Time.time;
            var canTag = controller.CanTag(now);
            button.interactable = canTag;

            if (buttonImage != null)
            {
                buttonImage.color = canTag ? Color.white : new Color(0.74f, 0.78f, 0.86f, 0.82f);
            }

            if (label != null)
            {
                label.text = canTag ? buttonLabel : controller.CooldownRemaining(now).ToString("0.0");
            }
        }

        private void ResolveController()
        {
            if (controller == null)
            {
                controller = GetComponent<TagActionController>();
            }
        }
    }
}
