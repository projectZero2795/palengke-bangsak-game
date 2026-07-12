using NUnit.Framework;
using Palengke.BangSak.UI;
using UnityEngine;
using UnityEngine.UI;

public sealed class SafeAreaCanvasLayoutTests
{
    [Test]
    public void CalculateNormalizedSafeArea_ConvertsPixelInsets()
    {
        var normalized = SafeAreaCanvasLayout.CalculateNormalizedSafeArea(
            new Rect(96f, 24f, 2208f, 1032f),
            2400,
            1080);

        Assert.That(normalized.xMin, Is.EqualTo(0.04f).Within(0.0001f));
        Assert.That(normalized.yMin, Is.EqualTo(24f / 1080f).Within(0.0001f));
        Assert.That(normalized.xMax, Is.EqualTo(0.96f).Within(0.0001f));
        Assert.That(normalized.yMax, Is.EqualTo(1056f / 1080f).Within(0.0001f));
    }

    [Test]
    public void CalculateNormalizedSafeArea_ClampsAndFallsBackSafely()
    {
        var clamped = SafeAreaCanvasLayout.CalculateNormalizedSafeArea(
            new Rect(-50f, -25f, 2600f, 1200f),
            2400,
            1080);

        Assert.That(clamped, Is.EqualTo(new Rect(0f, 0f, 1f, 1f)));
        Assert.That(
            SafeAreaCanvasLayout.CalculateNormalizedSafeArea(Rect.zero, 0, 0),
            Is.EqualTo(new Rect(0f, 0f, 1f, 1f)));
    }

    [Test]
    public void ApplySafeArea_UpdatesAnchorsWithoutPixelOffsets()
    {
        var root = new GameObject("Safe Area Test", typeof(RectTransform), typeof(SafeAreaCanvasLayout));
        try
        {
            var layout = root.GetComponent<SafeAreaCanvasLayout>();
            var rect = root.GetComponent<RectTransform>();

            layout.ApplySafeArea(new Rect(100f, 0f, 2200f, 1080f), 2400, 1080);

            Assert.That(rect.anchorMin.x, Is.EqualTo(1f / 24f).Within(0.0001f));
            Assert.That(rect.anchorMax.x, Is.EqualTo(23f / 24f).Within(0.0001f));
            Assert.That(rect.anchorMin.y, Is.Zero.Within(0.0001f));
            Assert.That(rect.anchorMax.y, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(rect.offsetMin, Is.EqualTo(Vector2.zero));
            Assert.That(rect.offsetMax, Is.EqualTo(Vector2.zero));
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void ConfigureScaler_UsesBalancedWideScreenScaling()
    {
        var root = new GameObject("Scaler Test", typeof(RectTransform), typeof(CanvasScaler));
        try
        {
            var scaler = root.GetComponent<CanvasScaler>();

            SafeAreaCanvasLayout.ConfigureScaler(scaler);

            Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(800f, 600f)));
            Assert.That(scaler.screenMatchMode, Is.EqualTo(CanvasScaler.ScreenMatchMode.MatchWidthOrHeight));
            Assert.That(scaler.matchWidthOrHeight, Is.EqualTo(0.5f).Within(0.0001f));
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void MoveIntoSafeArea_PreservesControlLayout()
    {
        var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        var controlObject = new GameObject("Control", typeof(RectTransform));
        try
        {
            controlObject.transform.SetParent(canvasObject.transform, false);
            var control = controlObject.GetComponent<RectTransform>();
            control.anchorMin = Vector2.zero;
            control.anchorMax = Vector2.zero;
            control.pivot = new Vector2(0.5f, 0.5f);
            control.anchoredPosition = new Vector2(82f, 82f);
            control.sizeDelta = new Vector2(104f, 104f);

            SafeAreaCanvasLayout.MoveIntoSafeArea(control);

            Assert.That(control.parent.name, Is.EqualTo(SafeAreaCanvasLayout.SafeAreaRootName));
            Assert.That(control.anchoredPosition, Is.EqualTo(new Vector2(82f, 82f)));
            Assert.That(control.sizeDelta, Is.EqualTo(new Vector2(104f, 104f)));
            Assert.That(
                canvasObject.transform.Find(SafeAreaCanvasLayout.SafeAreaRootName)
                    .GetComponent<SafeAreaCanvasLayout>(),
                Is.Not.Null);
        }
        finally
        {
            Object.DestroyImmediate(canvasObject);
        }
    }
}
