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
        private string buttonLabel = "BANG!";

        [SerializeField]
        private Vector2 buttonSize = new Vector2(88f, 88f);

        [SerializeField]
        private Vector2 buttonOffset = new Vector2(-24f, 24f);

        private GameObject hudRoot;
        private Button button;
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

        public void SetController(BangActionController bangController)
        {
            controller = bangController;
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
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);

            canvasObject.AddComponent<GraphicRaycaster>();

            var buttonObject = new GameObject("Bang Button");
            buttonObject.transform.SetParent(canvasObject.transform, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = buttonSize;
            rect.anchoredPosition = buttonOffset;

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.86f, 0.22f, 0.18f, 0.92f);

            button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(OnBangClicked);

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
            if (controller != null)
            {
                controller.TryBangNow();
            }
        }

        private void Refresh()
        {
            if (controller == null || button == null)
            {
                return;
            }

            var now = Time.time;
            var canBang = controller.CanBang(now);
            button.interactable = canBang;

            if (label != null)
            {
                label.text = canBang ? buttonLabel : controller.CooldownRemaining(now).ToString("0.0");
            }
        }

        private void ResolveController()
        {
            if (controller == null)
            {
                controller = GetComponent<BangActionController>();
            }
        }
    }
}
