using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[RequireComponent(typeof(SimulationController))]
public class SimulationControlPanel : MonoBehaviour
{
    private readonly Color panelColor =
        new Color(0.055f, 0.075f, 0.10f, 0.94f);

    private readonly Color textColor =
        new Color(0.92f, 0.95f, 0.98f, 1f);

    private readonly Color mutedTextColor =
        new Color(0.62f, 0.70f, 0.78f, 1f);

    private SimulationController controller;
    private Font interfaceFont;
    private Text modeText;
    private Text speedText;
    private Text spawnText;
    private Text activeText;
    private Text completedText;
    private Text throughputText;

    private void Awake()
    {
        controller = GetComponent<SimulationController>();
        interfaceFont =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf");

        EnsureEventSystem();
    }

    private void Start()
    {
        BuildInterface();
    }

    private void Update()
    {
        modeText.text = controller.IsRunning
            ? "RUN MODE"
            : "BUILD MODE";

        modeText.color = controller.IsRunning
            ? new Color(0.25f, 1f, 0.45f)
            : new Color(1f, 0.72f, 0.2f);

        speedText.text =
            $"Conveyor speed   {controller.SpeedMultiplier:0.00}x";

        spawnText.text =
            $"Spawn interval   {controller.SpawnInterval:0.00}s";

        activeText.text =
            $"Active products     {controller.ActiveProducts}";

        completedText.text =
            $"Completed products  {controller.CompletedProducts}";

        throughputText.text =
            $"Throughput           {controller.ThroughputPerMinute:0.0}/min";
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject =
            new GameObject("RuntimeEventSystem");

        eventSystemObject.AddComponent<EventSystem>();

        InputSystemUIInputModule inputModule =
            eventSystemObject.AddComponent<InputSystemUIInputModule>();

        inputModule.AssignDefaultActions();
    }

    private void BuildInterface()
    {
        GameObject canvasObject =
            new GameObject("SimulationControlCanvas");

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler =
            canvasObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform panel = CreateRect(
            "ControlPanel",
            canvasObject.transform);

        panel.anchorMin = new Vector2(0f, 1f);
        panel.anchorMax = new Vector2(0f, 1f);
        panel.pivot = new Vector2(0f, 1f);
        panel.anchoredPosition = new Vector2(22f, -22f);
        panel.sizeDelta = new Vector2(350f, 430f);

        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = panelColor;

        VerticalLayoutGroup layout =
            panel.gameObject.AddComponent<VerticalLayoutGroup>();

        layout.padding = new RectOffset(18, 18, 16, 16);
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateText(
            "VACAC CONVEYOR CONTROL",
            panel,
            22,
            FontStyle.Bold,
            textColor,
            34f);

        modeText = CreateText(
            "BUILD MODE",
            panel,
            17,
            FontStyle.Bold,
            textColor,
            28f);

        RectTransform buttonRow = CreateRect(
            "ModeButtons",
            panel);
        AddLayoutElement(buttonRow, 44f);

        HorizontalLayoutGroup buttonLayout =
            buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>();

        buttonLayout.spacing = 7f;
        buttonLayout.childControlWidth = true;
        buttonLayout.childControlHeight = true;
        buttonLayout.childForceExpandWidth = true;
        buttonLayout.childForceExpandHeight = true;

        CreateButton(
            "BUILD",
            buttonRow,
            new Color(0.75f, 0.43f, 0.08f),
            controller.SetBuildMode);

        CreateButton(
            "RUN",
            buttonRow,
            new Color(0.08f, 0.55f, 0.25f),
            controller.SetRunMode);

        CreateButton(
            "RESET",
            buttonRow,
            new Color(0.42f, 0.16f, 0.18f),
            controller.ResetSimulation);

        speedText = CreateText(
            "Conveyor speed",
            panel,
            15,
            FontStyle.Normal,
            textColor,
            24f);

        Slider speedSlider = CreateSlider(
            "SpeedSlider",
            panel,
            0.25f,
            3f,
            1f);

        speedSlider.onValueChanged.AddListener(
            controller.SetSpeedMultiplier);

        spawnText = CreateText(
            "Spawn interval",
            panel,
            15,
            FontStyle.Normal,
            textColor,
            24f);

        Slider spawnSlider = CreateSlider(
            "SpawnSlider",
            panel,
            0.25f,
            5f,
            Mathf.Max(0.25f, controller.SpawnInterval));

        spawnSlider.onValueChanged.AddListener(
            controller.SetSpawnInterval);

        CreateDivider(panel);

        activeText = CreateText(
            "Active products     0",
            panel,
            15,
            FontStyle.Normal,
            textColor,
            24f);

        completedText = CreateText(
            "Completed products  0",
            panel,
            15,
            FontStyle.Normal,
            textColor,
            24f);

        throughputText = CreateText(
            "Throughput           0.0/min",
            panel,
            15,
            FontStyle.Normal,
            textColor,
            24f);

        CreateText(
            "1–4 Select  •  R Rotate  •  Esc Cancel\n" +
            "RMB Look  •  WASD Move  •  Q/E Height",
            panel,
            13,
            FontStyle.Normal,
            mutedTextColor,
            42f);
    }

    private Text CreateText(
        string content,
        Transform parent,
        int fontSize,
        FontStyle fontStyle,
        Color color,
        float height)
    {
        RectTransform rect = CreateRect("Text", parent);
        AddLayoutElement(rect, height);

        Text text = rect.gameObject.AddComponent<Text>();
        text.text = content;
        text.font = interfaceFont;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = TextAnchor.MiddleLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        return text;
    }

    private void CreateButton(
        string label,
        Transform parent,
        Color color,
        UnityEngine.Events.UnityAction action)
    {
        RectTransform rect = CreateRect(label + "Button", parent);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f);
        button.colors = colors;

        RectTransform textRect = CreateRect("Label", rect);
        StretchToParent(textRect);

        Text text = textRect.gameObject.AddComponent<Text>();
        text.text = label;
        text.font = interfaceFont;
        text.fontSize = 14;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
    }

    private Slider CreateSlider(
        string name,
        Transform parent,
        float minimum,
        float maximum,
        float value)
    {
        RectTransform root = CreateRect(name, parent);
        AddLayoutElement(root, 26f);

        Slider slider = root.gameObject.AddComponent<Slider>();
        slider.minValue = minimum;
        slider.maxValue = maximum;
        slider.value = value;

        RectTransform background = CreateRect("Background", root);
        background.anchorMin = new Vector2(0f, 0.38f);
        background.anchorMax = new Vector2(1f, 0.62f);
        background.offsetMin = Vector2.zero;
        background.offsetMax = Vector2.zero;

        Image backgroundImage =
            background.gameObject.AddComponent<Image>();
        backgroundImage.color = new Color(0.18f, 0.23f, 0.28f);

        RectTransform fillArea = CreateRect("FillArea", root);
        fillArea.anchorMin = new Vector2(0f, 0.38f);
        fillArea.anchorMax = new Vector2(1f, 0.62f);
        fillArea.offsetMin = new Vector2(8f, 0f);
        fillArea.offsetMax = new Vector2(-8f, 0f);

        RectTransform fill = CreateRect("Fill", fillArea);
        StretchToParent(fill);
        Image fillImage = fill.gameObject.AddComponent<Image>();
        fillImage.color = new Color(0.12f, 0.66f, 0.88f);

        RectTransform handleArea = CreateRect("HandleArea", root);
        StretchToParent(handleArea);
        handleArea.offsetMin = new Vector2(8f, 0f);
        handleArea.offsetMax = new Vector2(-8f, 0f);

        RectTransform handle = CreateRect("Handle", handleArea);
        handle.sizeDelta = new Vector2(16f, 24f);
        Image handleImage = handle.gameObject.AddComponent<Image>();
        handleImage.color = Color.white;

        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private void CreateDivider(Transform parent)
    {
        RectTransform divider = CreateRect("Divider", parent);
        AddLayoutElement(divider, 2f);

        Image image = divider.gameObject.AddComponent<Image>();
        image.color = new Color(0.22f, 0.30f, 0.36f);
    }

    private static RectTransform CreateRect(
        string name,
        Transform parent)
    {
        GameObject gameObject =
            new GameObject(name, typeof(RectTransform));

        RectTransform rect =
            gameObject.GetComponent<RectTransform>();

        rect.SetParent(parent, false);
        return rect;
    }

    private static void AddLayoutElement(
        RectTransform rect,
        float preferredHeight)
    {
        LayoutElement element =
            rect.gameObject.AddComponent<LayoutElement>();

        element.preferredHeight = preferredHeight;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
