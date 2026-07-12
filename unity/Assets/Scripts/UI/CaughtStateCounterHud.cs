using Palengke.BangSak.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Palengke.BangSak.UI
{
    public sealed class CaughtStateCounterHud : MonoBehaviour
    {
        [SerializeField]
        private string labelPrefix = "Hiders Left";

        [SerializeField]
        private Vector2 labelSize = new Vector2(138f, 58f);

        [SerializeField]
        private Vector2 labelOffset = new Vector2(-18f, -18f);

        [SerializeField]
        [Min(0.05f)]
        private float refreshIntervalSeconds = 0.2f;

        private GameObject hudRoot;
        private Text label;
        private CaughtStateController[] trackedTargets = new CaughtStateController[0];
        private float nextRefreshAt;

        public int LastRemaining { get; private set; }

        public int LastTotal { get; private set; }

        private void Start()
        {
            CreateHud();
            RefreshTargets();
            RefreshCounter();
        }

        private void Update()
        {
            if (Time.time < nextRefreshAt)
            {
                return;
            }

            nextRefreshAt = Time.time + refreshIntervalSeconds;
            RefreshTargets();
            RefreshCounter();
        }

        private void OnDestroy()
        {
            if (hudRoot != null)
            {
                Destroy(hudRoot);
            }
        }

        public void RefreshTargets()
        {
            trackedTargets = FindObjectsOfType<CaughtStateController>();
        }

        public void RefreshCounter()
        {
            var total = 0;
            var remaining = 0;

            for (var index = 0; index < trackedTargets.Length; index += 1)
            {
                var target = trackedTargets[index];
                if (target == null || !target.CountAsHider)
                {
                    continue;
                }

                total += 1;
                if (!target.IsCaught)
                {
                    remaining += 1;
                }
            }

            LastTotal = total;
            LastRemaining = remaining;

            if (label != null)
            {
                label.text = $"{labelPrefix}\n{remaining} / {total}";
            }
        }

        private void CreateHud()
        {
            if (hudRoot != null)
            {
                return;
            }

            var canvasObject = new GameObject("Phase 8 Caught Counter HUD");
            canvasObject.transform.SetParent(null, false);
            hudRoot = canvasObject;

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 19;

            var scaler = canvasObject.AddComponent<CanvasScaler>();
            SafeAreaCanvasLayout.ConfigureScaler(scaler);
            var safeAreaRoot = SafeAreaCanvasLayout.GetOrCreateSafeAreaRoot(canvasObject.transform);

            var labelObject = new GameObject("Hiders Left Label");
            labelObject.transform.SetParent(safeAreaRoot, false);

            var rect = labelObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = labelSize;
            rect.anchoredPosition = labelOffset;

            var background = labelObject.AddComponent<Image>();
            background.color = new Color(0.03f, 0.05f, 0.08f, 0.82f);

            label = labelObject.AddComponent<Text>();
            label.alignment = TextAnchor.MiddleCenter;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontStyle = FontStyle.Bold;
            label.fontSize = 16;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 12;
            label.resizeTextMaxSize = 20;
            label.color = Color.white;
            label.raycastTarget = false;
        }
    }
}
