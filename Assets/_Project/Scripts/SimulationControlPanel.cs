using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[RequireComponent(typeof(SimulationController))]
public class SimulationControlPanel : MonoBehaviour
{
    private static readonly Color PanelColor = new(0.055f, 0.065f, 0.08f, 0.94f);
    private static readonly Color SurfaceColor = new(0.105f, 0.12f, 0.145f, 1f);
    private static readonly Color TextColor = new(0.91f, 0.92f, 0.94f, 1f);
    private static readonly Color MutedColor = new(0.53f, 0.57f, 0.63f, 1f);
    private static readonly Color AccentColor = new(0.43f, 0.60f, 0.88f, 1f);
    private static readonly Color RunningColor = new(0.40f, 0.76f, 0.57f, 1f);

    private SimulationController controller;
    private Font interfaceFont;
    private Sprite roundedSprite;
    private Sprite circularSprite;

    private Text modeText;
    private Text speedValueText;
    private Text spawnValueText;
    private Text statisticsText;
    private Image statusDot;
    private Image buildButtonImage;
    private Image runButtonImage;

    private void Awake()
    {
        controller = GetComponent<SimulationController>();
        interfaceFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        roundedSprite = CreateRoundedSprite(32, 8, 9f);
        circularSprite = CreateRoundedSprite(16, 8, 0f);
        EnsureEventSystem();
    }

    private void Start()
    {
        BuildInterface();
    }

    private void Update()
    {
        bool isRunning = controller.IsRunning;

        modeText.text = isRunning ? "Running" : "Build mode";
        statusDot.color = isRunning ? RunningColor : AccentColor;
        SetButtonState(buildButtonImage, !isRunning);
        SetButtonState(runButtonImage, isRunning);

        speedValueText.text = $"{controller.SpeedMultiplier:0.00}×";
        spawnValueText.text = $"{controller.SpawnInterval:0.00}s";
        statisticsText.text =
            $"{controller.ActiveProducts} active   ·   " +
            $"{controller.CompletedProducts} completed   ·   " +
            $"{controller.ThroughputPerMinute:0.0}/min";
    }

    private void BuildInterface()
    {
        GameObject canvasObject = new("SimulationControlCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform panel = CreateRect("ControlPanel", canvasObject.transform);
        panel.anchorMin = new Vector2(0f, 1f);
        panel.anchorMax = new Vector2(0f, 1f);
        panel.pivot = new Vector2(0f, 1f);
        panel.anchoredPosition = new Vector2(24f, -24f);
        panel.sizeDelta = new Vector2(326f, 284f);

        Image panelImage = panel.gameObject.AddComponent<Image>();
        StyleRoundedImage(panelImage, PanelColor);

        VerticalLayoutGroup panelLayout =
            panel.gameObject.AddComponent<VerticalLayoutGroup>();
        panelLayout.padding = new RectOffset(18, 18, 15, 15);
        panelLayout.spacing = 10f;
        panelLayout.childAlignment = TextAnchor.UpperLeft;
        panelLayout.childControlWidth = true;
        panelLayout.childControlHeight = true;
        panelLayout.childForceExpandWidth = true;
        panelLayout.childForceExpandHeight = false;

        BuildHeader(panel);
        BuildModeControls(panel);

        BuildSliderControl(
            "Speed", "SpeedSlider", panel,
            0.25f, 3f, 1f,
            controller.SetSpeedMultiplier,
            out speedValueText);

        BuildSliderControl(
            "Spawn interval", "SpawnSlider", panel,
            0.25f, 5f, Mathf.Max(0.25f, controller.SpawnInterval),
            controller.SetSpawnInterval,
            out spawnValueText);

        CreateDivider(panel);

        statisticsText = CreateText(
            "0 active   ·   0 completed   ·   0.0/min",
            panel,
            11,
            FontStyle.Normal,
            MutedColor,
            23f,
            TextAnchor.MiddleLeft);

        Text hint = CreateText(
            "1–4 select  ·  R rotate  ·  Esc cancel",
            panel,
            10,
            FontStyle.Normal,
            new Color(MutedColor.r, MutedColor.g, MutedColor.b, 0.72f),
            18f,
            TextAnchor.MiddleLeft);
        hint.gameObject.SetActive(true);
    }

    private void BuildHeader(Transform parent)
    {
        RectTransform row = CreateRect("Header", parent);
        SetPreferredHeight(row, 28f);

        Text title = CreateText(
            "VACAC",
            row,
            17,
            FontStyle.Bold,
            TextColor,
            28f,
            TextAnchor.MiddleLeft);
        StretchToParent(title.rectTransform);

        RectTransform status = CreateRect("Status", row);
        status.anchorMin = new Vector2(1f, 0f);
        status.anchorMax = new Vector2(1f, 1f);
        status.pivot = new Vector2(1f, 0.5f);
        status.sizeDelta = new Vector2(104f, 0f);

        HorizontalLayoutGroup statusLayout =
            status.gameObject.AddComponent<HorizontalLayoutGroup>();
        statusLayout.spacing = 7f;
        statusLayout.childAlignment = TextAnchor.MiddleRight;
        statusLayout.childControlWidth = false;
        statusLayout.childControlHeight = false;
        statusLayout.childForceExpandWidth = false;
        statusLayout.childForceExpandHeight = false;

        RectTransform dot = CreateRect("StatusDot", status);
        SetPreferredSize(dot, 7f, 7f);
        dot.sizeDelta = new Vector2(7f, 7f);
        statusDot = dot.gameObject.AddComponent<Image>();
        statusDot.sprite = circularSprite;
        statusDot.color = AccentColor;

        modeText = CreateText(
            "Build mode",
            status,
            11,
            FontStyle.Normal,
            MutedColor,
            20f,
            TextAnchor.MiddleRight);
        SetPreferredWidth(modeText.rectTransform, 86f);
    }

    private void BuildModeControls(Transform parent)
    {
        RectTransform row = CreateRect("ModeControls", parent);
        SetPreferredHeight(row, 34f);

        HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 6f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        buildButtonImage = CreateButton(
            "Build", row, controller.SetBuildMode, false);
        runButtonImage = CreateButton(
            "Run", row, controller.SetRunMode, false);
        CreateButton(
            "Reset", row, controller.ResetSimulation, true);
    }

    private void BuildSliderControl(
        string label,
        string sliderName,
        Transform parent,
        float minimum,
        float maximum,
        float value,
        UnityEngine.Events.UnityAction<float> action,
        out Text valueText)
    {
        RectTransform root = CreateRect(label + "Control", parent);
        SetPreferredHeight(root, 43f);

        VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 3f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        RectTransform labelRow = CreateRect("LabelRow", root);
        SetPreferredHeight(labelRow, 19f);

        Text labelText = CreateText(
            label, labelRow, 11, FontStyle.Normal,
            MutedColor, 19f, TextAnchor.MiddleLeft);
        StretchToParent(labelText.rectTransform);

        valueText = CreateText(
            string.Empty, labelRow, 11, FontStyle.Normal,
            TextColor, 19f, TextAnchor.MiddleRight);
        StretchToParent(valueText.rectTransform);

        Slider slider = CreateSlider(sliderName, root, minimum, maximum, value);
        slider.onValueChanged.AddListener(action);
    }

    private Image CreateButton(
        string label,
        Transform parent,
        UnityEngine.Events.UnityAction action,
        bool isReset)
    {
        RectTransform rect = CreateRect(label + "Button", parent);
        Image image = rect.gameObject.AddComponent<Image>();
        StyleRoundedImage(image, SurfaceColor);

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f);
        colors.pressedColor = new Color(0.80f, 0.80f, 0.80f);
        colors.fadeDuration = 0.07f;
        button.colors = colors;

        RectTransform textRect = CreateRect("Label", rect);
        StretchToParent(textRect);
        Text text = textRect.gameObject.AddComponent<Text>();
        text.text = label;
        text.font = interfaceFont;
        text.fontSize = 11;
        text.fontStyle = FontStyle.Normal;
        text.color = isReset ? MutedColor : TextColor;
        text.alignment = TextAnchor.MiddleCenter;
        return image;
    }

    private Slider CreateSlider(
        string name,
        Transform parent,
        float minimum,
        float maximum,
        float value)
    {
        RectTransform root = CreateRect(name, parent);
        SetPreferredHeight(root, 17f);

        Slider slider = root.gameObject.AddComponent<Slider>();
        slider.minValue = minimum;
        slider.maxValue = maximum;
        slider.value = value;

        RectTransform background = CreateRect("Background", root);
        background.anchorMin = new Vector2(0f, 0.44f);
        background.anchorMax = new Vector2(1f, 0.56f);
        background.offsetMin = new Vector2(1f, 0f);
        background.offsetMax = new Vector2(-1f, 0f);
        Image backgroundImage = background.gameObject.AddComponent<Image>();
        backgroundImage.color = new Color(0.19f, 0.21f, 0.25f, 1f);

        RectTransform fillArea = CreateRect("FillArea", root);
        fillArea.anchorMin = new Vector2(0f, 0.44f);
        fillArea.anchorMax = new Vector2(1f, 0.56f);
        fillArea.offsetMin = new Vector2(1f, 0f);
        fillArea.offsetMax = new Vector2(-5f, 0f);

        RectTransform fill = CreateRect("Fill", fillArea);
        StretchToParent(fill);
        Image fillImage = fill.gameObject.AddComponent<Image>();
        fillImage.color = AccentColor;

        RectTransform handleArea = CreateRect("HandleArea", root);
        StretchToParent(handleArea);
        handleArea.offsetMin = new Vector2(5f, 0f);
        handleArea.offsetMax = new Vector2(-5f, 0f);

        RectTransform handle = CreateRect("Handle", handleArea);
        handle.anchorMin = new Vector2(0f, 0.5f);
        handle.anchorMax = new Vector2(0f, 0.5f);
        handle.pivot = new Vector2(0.5f, 0.5f);
        handle.sizeDelta = new Vector2(8f, 8f);
        Image handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.sprite = circularSprite;
        handleImage.preserveAspect = true;
        handleImage.color = TextColor;

        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private Text CreateText(
        string content,
        Transform parent,
        int fontSize,
        FontStyle fontStyle,
        Color color,
        float height,
        TextAnchor alignment)
    {
        RectTransform rect = CreateRect("Text", parent);
        SetPreferredHeight(rect, height);

        Text text = rect.gameObject.AddComponent<Text>();
        text.text = content;
        text.font = interfaceFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private void CreateDivider(Transform parent)
    {
        RectTransform divider = CreateRect("Divider", parent);
        SetPreferredHeight(divider, 1f);
        Image image = divider.gameObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.075f);
    }

    private void SetButtonState(Image image, bool selected)
    {
        image.color = selected ? AccentColor : SurfaceColor;
    }

    private void StyleRoundedImage(Image image, Color color)
    {
        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
        image.color = color;
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject = new("RuntimeEventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        InputSystemUIInputModule inputModule =
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        inputModule.AssignDefaultActions();
    }

    private static Sprite CreateRoundedSprite(int size, int radius, float border)
    {
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false)
        {
            name = "RuntimeMinimalUI",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Color clear = new(1f, 1f, 1f, 0f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float cornerX = Mathf.Max(radius - x, x - (size - radius - 1));
                float cornerY = Mathf.Max(radius - y, y - (size - radius - 1));
                float positiveX = Mathf.Max(0f, cornerX);
                float positiveY = Mathf.Max(0f, cornerY);
                float distance = Mathf.Sqrt(
                    positiveX * positiveX + positiveY * positiveY);
                float alpha = Mathf.Clamp01(radius + 0.5f - distance);
                texture.SetPixel(x, y, Color.Lerp(clear, Color.white, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(border, border, border, border));
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject gameObject = new(name, typeof(RectTransform));
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void SetPreferredHeight(RectTransform rect, float height)
    {
        LayoutElement element = GetOrAddLayoutElement(rect);
        element.preferredHeight = height;
    }

    private static void SetPreferredWidth(RectTransform rect, float width)
    {
        LayoutElement element = GetOrAddLayoutElement(rect);
        element.preferredWidth = width;
    }

    private static void SetPreferredSize(
        RectTransform rect,
        float width,
        float height)
    {
        LayoutElement element = GetOrAddLayoutElement(rect);
        element.preferredWidth = width;
        element.preferredHeight = height;
    }

    private static LayoutElement GetOrAddLayoutElement(RectTransform rect)
    {
        LayoutElement element = rect.gameObject.GetComponent<LayoutElement>();
        return element != null
            ? element
            : rect.gameObject.AddComponent<LayoutElement>();
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
