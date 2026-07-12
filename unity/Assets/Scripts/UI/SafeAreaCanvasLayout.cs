using UnityEngine;
using UnityEngine.UI;

namespace Palengke.BangSak.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaCanvasLayout : MonoBehaviour
    {
        public const string SafeAreaRootName = "Phase 34C Safe Area";
        public static readonly Vector2 ReferenceResolution = new Vector2(800f, 600f);
        public const float BalancedScreenMatch = 0.5f;

        private RectTransform safeAreaTransform;
        private Rect lastSafeArea = new Rect(-1f, -1f, -1f, -1f);
        private int lastScreenWidth = -1;
        private int lastScreenHeight = -1;

        public Rect LastNormalizedSafeArea { get; private set; } = new Rect(0f, 0f, 1f, 1f);

        private void OnEnable()
        {
            safeAreaTransform = GetComponent<RectTransform>();
            ApplyCurrentSafeArea();
        }

        private void Update()
        {
            if (Screen.safeArea != lastSafeArea
                || Screen.width != lastScreenWidth
                || Screen.height != lastScreenHeight)
            {
                ApplyCurrentSafeArea();
            }
        }

        public void ApplyCurrentSafeArea()
        {
            ApplySafeArea(Screen.safeArea, Screen.width, Screen.height);
        }

        public void ApplySafeArea(Rect safeArea, int screenWidth, int screenHeight)
        {
            if (safeAreaTransform == null)
            {
                safeAreaTransform = GetComponent<RectTransform>();
            }

            var normalized = CalculateNormalizedSafeArea(safeArea, screenWidth, screenHeight);
            LastNormalizedSafeArea = normalized;
            safeAreaTransform.anchorMin = normalized.min;
            safeAreaTransform.anchorMax = normalized.max;
            safeAreaTransform.offsetMin = Vector2.zero;
            safeAreaTransform.offsetMax = Vector2.zero;

            lastSafeArea = safeArea;
            lastScreenWidth = screenWidth;
            lastScreenHeight = screenHeight;
        }

        public static Rect CalculateNormalizedSafeArea(Rect safeArea, int screenWidth, int screenHeight)
        {
            if (screenWidth <= 0 || screenHeight <= 0 || safeArea.width <= 0f || safeArea.height <= 0f)
            {
                return new Rect(0f, 0f, 1f, 1f);
            }

            var xMin = Mathf.Clamp01(safeArea.xMin / screenWidth);
            var yMin = Mathf.Clamp01(safeArea.yMin / screenHeight);
            var xMax = Mathf.Clamp01(safeArea.xMax / screenWidth);
            var yMax = Mathf.Clamp01(safeArea.yMax / screenHeight);
            return Rect.MinMaxRect(
                Mathf.Min(xMin, xMax),
                Mathf.Min(yMin, yMax),
                Mathf.Max(xMin, xMax),
                Mathf.Max(yMin, yMax));
        }

        public static void ConfigureScaler(CanvasScaler scaler)
        {
            if (scaler == null)
            {
                return;
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = BalancedScreenMatch;
        }

        public static RectTransform GetOrCreateSafeAreaRoot(Transform canvasTransform)
        {
            if (canvasTransform == null)
            {
                return null;
            }

            var existing = canvasTransform.Find(SafeAreaRootName) as RectTransform;
            if (existing != null)
            {
                if (existing.GetComponent<SafeAreaCanvasLayout>() == null)
                {
                    existing.gameObject.AddComponent<SafeAreaCanvasLayout>();
                }

                return existing;
            }

            var safeAreaObject = new GameObject(SafeAreaRootName, typeof(RectTransform));
            safeAreaObject.transform.SetParent(canvasTransform, false);
            var rect = safeAreaObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            safeAreaObject.AddComponent<SafeAreaCanvasLayout>();
            return rect;
        }

        public static void MoveIntoSafeArea(RectTransform element)
        {
            if (element == null)
            {
                return;
            }

            var canvas = element.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            ConfigureScaler(canvas.GetComponent<CanvasScaler>());
            var safeAreaRoot = GetOrCreateSafeAreaRoot(canvas.transform);
            if (safeAreaRoot == null || element.parent == safeAreaRoot)
            {
                return;
            }

            var anchorMin = element.anchorMin;
            var anchorMax = element.anchorMax;
            var pivot = element.pivot;
            var anchoredPosition = element.anchoredPosition;
            var sizeDelta = element.sizeDelta;
            element.SetParent(safeAreaRoot, false);
            element.anchorMin = anchorMin;
            element.anchorMax = anchorMax;
            element.pivot = pivot;
            element.anchoredPosition = anchoredPosition;
            element.sizeDelta = sizeDelta;
        }
    }
}
