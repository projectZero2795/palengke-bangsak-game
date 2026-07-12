using UnityEngine;
using UnityEngine.UI;

namespace Palengke.BangSak.UI
{
    [DisallowMultipleComponent]
    public sealed class AccessibilityCueHud : MonoBehaviour
    {
        private const float VisibleSeconds = 1.15f;
        private GameObject cuePanel;
        private Text cueLabel;
        private float visibleUntil = -1f;

        public bool IsCueVisible => cuePanel != null && cuePanel.activeSelf;
        public string CurrentMessage => cueLabel != null ? cueLabel.text : string.Empty;

        private void OnEnable()
        {
            AccessibilityCueService.CueRequested += ShowCue;
            AccessibilitySettings.SettingsChanged += RefreshSetting;
        }

        private void OnDisable()
        {
            AccessibilityCueService.CueRequested -= ShowCue;
            AccessibilitySettings.SettingsChanged -= RefreshSetting;
        }

        private void Update()
        {
            if (IsCueVisible && Time.unscaledTime >= visibleUntil)
            {
                cuePanel.SetActive(false);
            }
        }

        public void Initialize(Transform safeAreaRoot)
        {
            if (cuePanel != null || safeAreaRoot == null)
            {
                return;
            }

            cuePanel = new GameObject("Accessibility Visual Cue");
            cuePanel.transform.SetParent(safeAreaRoot, false);
            var rect = cuePanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -62f);
            rect.sizeDelta = new Vector2(300f, 42f);

            var background = cuePanel.AddComponent<Image>();
            background.color = new Color(0.01f, 0.018f, 0.03f, 0.94f);
            background.raycastTarget = false;

            var labelObject = new GameObject("Visual Cue Label");
            labelObject.transform.SetParent(cuePanel.transform, false);
            var labelRect = labelObject.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10f, 4f);
            labelRect.offsetMax = new Vector2(-10f, -4f);

            cueLabel = labelObject.AddComponent<Text>();
            cueLabel.alignment = TextAnchor.MiddleCenter;
            cueLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            cueLabel.fontSize = 20;
            cueLabel.fontStyle = FontStyle.Bold;
            cueLabel.resizeTextForBestFit = true;
            cueLabel.resizeTextMinSize = 13;
            cueLabel.resizeTextMaxSize = 20;
            cueLabel.raycastTarget = false;
            cuePanel.SetActive(false);
        }

        public void ShowCue(AccessibilityVisualCue cue)
        {
            if (!AccessibilitySettings.VisualCuesEnabled || cuePanel == null || cueLabel == null)
            {
                return;
            }

            cueLabel.text = cue.Message;
            cueLabel.color = cue.Color;
            cuePanel.SetActive(true);
            visibleUntil = Time.unscaledTime + VisibleSeconds;
        }

        private void RefreshSetting()
        {
            if (!AccessibilitySettings.VisualCuesEnabled && cuePanel != null)
            {
                cuePanel.SetActive(false);
            }
        }
    }
}
