using UnityEngine;
using UnityEngine.UI;

namespace Palengke.BangSak.UI
{
    [DisallowMultipleComponent]
    public sealed class AccessibilityCanvasAdapter : MonoBehaviour
    {
        private bool isRefreshing;

        private void OnEnable()
        {
            AccessibilitySettings.SettingsChanged += RefreshNow;
            RefreshNow();
        }

        private void OnDisable()
        {
            AccessibilitySettings.SettingsChanged -= RefreshNow;
        }

        private void Start()
        {
            RefreshNow();
        }

        private void OnTransformChildrenChanged()
        {
            RefreshNow();
        }

        public void RefreshNow()
        {
            if (isRefreshing)
            {
                return;
            }

            isRefreshing = true;
            var labels = GetComponentsInChildren<Text>(true);
            foreach (var label in labels)
            {
                var style = label.GetComponent<AccessibilityTextStyle>();
                if (style == null)
                {
                    style = label.gameObject.AddComponent<AccessibilityTextStyle>();
                }

                style.Apply(label);
            }
            isRefreshing = false;
        }
    }

    [DisallowMultipleComponent]
    public sealed class AccessibilityTextStyle : MonoBehaviour
    {
        private bool captured;
        private int baseFontSize;
        private int baseMinimumSize;
        private int baseMaximumSize;
        private FontStyle baseFontStyle;
        private Color baseColor;
        private Outline managedOutline;

        public void Apply(Text label)
        {
            if (label == null)
            {
                return;
            }

            if (!captured)
            {
                baseFontSize = label.fontSize;
                baseMinimumSize = label.resizeTextMinSize;
                baseMaximumSize = label.resizeTextMaxSize;
                baseFontStyle = label.fontStyle;
                baseColor = label.color;
                captured = true;
            }

            label.fontSize = AccessibilitySettings.ResolveFontSize(baseFontSize);
            label.resizeTextMinSize = AccessibilitySettings.ReadableTextEnabled
                ? Mathf.Max(12, baseMinimumSize)
                : baseMinimumSize;
            label.resizeTextMaxSize = AccessibilitySettings.ResolveFontSize(baseMaximumSize);
            label.fontStyle = AccessibilitySettings.ReadableTextEnabled ? FontStyle.Bold : baseFontStyle;
            label.color = AccessibilitySettings.HighContrastEnabled
                ? ResolveHighContrastColor(baseColor)
                : baseColor;

            if (managedOutline == null)
            {
                managedOutline = gameObject.AddComponent<Outline>();
                managedOutline.effectColor = new Color(0f, 0f, 0f, 0.96f);
                managedOutline.effectDistance = new Vector2(1.4f, -1.4f);
                managedOutline.useGraphicAlpha = true;
            }
            managedOutline.enabled = AccessibilitySettings.HighContrastEnabled;
        }

        public static Color ResolveHighContrastColor(Color source)
        {
            if (source.r > 0.88f && source.g > 0.62f && source.b < 0.48f)
            {
                return new Color(1f, 0.9f, 0.2f, source.a);
            }

            return new Color(1f, 1f, 1f, source.a);
        }
    }
}
