using Palengke.BangSak.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Palengke.BangSak.UI
{
    public sealed class SakActionHud : MonoBehaviour
    {
        [SerializeField]
        private SakBaseActor actor;

        [SerializeField]
        private string buttonLabel = "SAK";

        [SerializeField]
        private Vector2 buttonSize = new Vector2(72f, 72f);

        [SerializeField]
        private Vector2 buttonOffset = new Vector2(-184f, 24f);

        [SerializeField]
        private KeyCode keyboardShortcut = KeyCode.R;

        private GameObject hudRoot;
        private Button button;
        private Image buttonImage;
        private Text label;

        private void Start()
        {
            ResolveActor();
            CreateHud();
            Refresh();
        }

        private void Update()
        {
            if (actor != null && actor.CanPressSak() && Input.GetKeyDown(keyboardShortcut))
            {
                actor.TryPressSakNow();
            }

            Refresh();
        }

        public void SetActor(SakBaseActor sakActor)
        {
            actor = sakActor;
        }

        private void CreateHud()
        {
            if (hudRoot != null)
            {
                return;
            }

            var canvasObject = new GameObject("Phase 13 Sak HUD");
            canvasObject.transform.SetParent(null, false);
            hudRoot = canvasObject;

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 22;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);

            canvasObject.AddComponent<GraphicRaycaster>();

            var buttonObject = new GameObject("Sak Button");
            buttonObject.transform.SetParent(canvasObject.transform, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = buttonSize;
            rect.anchoredPosition = buttonOffset;

            buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.15f, 0.64f, 0.25f, 0.96f);
            buttonImage.preserveAspect = true;

            button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(OnSakClicked);

            var textObject = new GameObject("Sak Button Label");
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
            label.resizeTextMinSize = 11;
            label.resizeTextMaxSize = 22;
            label.color = Color.white;
            label.raycastTarget = false;
        }

        private void OnDestroy()
        {
            if (hudRoot != null)
            {
                Destroy(hudRoot);
            }
        }

        private void OnSakClicked()
        {
            if (actor != null)
            {
                actor.TryPressSakNow();
            }
        }

        private void Refresh()
        {
            if (hudRoot == null || actor == null || button == null)
            {
                if (hudRoot != null)
                {
                    hudRoot.SetActive(false);
                }

                return;
            }

            var canPress = actor.CanPressSak();
            hudRoot.SetActive(canPress);
            button.interactable = canPress;

            if (buttonImage != null)
            {
                buttonImage.color = canPress
                    ? new Color(0.15f, 0.64f, 0.25f, 0.96f)
                    : new Color(0.5f, 0.56f, 0.52f, 0.8f);
            }

            if (label != null)
            {
                label.text = buttonLabel;
            }
        }

        private void ResolveActor()
        {
            if (actor == null)
            {
                actor = GetComponent<SakBaseActor>();
            }
        }
    }
}
