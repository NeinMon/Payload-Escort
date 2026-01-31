using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class PayloadEscortHudPrefabCreator
{
    private const string IconAtkPath = "Assets/UI/icon/ATK.png";
    private const string IconDefPath = "Assets/UI/icon/DEF.png";
    private const string IconPayloadPath = "Assets/UI/icon/Payload.png";
    private const string IconRepairPath = "Assets/UI/icon/Repair.png";
    private const string IconSabotagePath = "Assets/UI/icon/Sabotage.png";
    private const string IconTimerPath = "Assets/UI/icon/Timer.png";
    private const string IconRolePath = "Assets/UI/icon/Role.png";
    [MenuItem("Tools/Payload Escort/Create HUD Prefab")]
    public static void CreateHudPrefab()
    {
        GameObject canvasGo = new GameObject("PayloadEscortHUD_Canvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        PayloadEscortHUD hud = canvasGo.AddComponent<PayloadEscortHUD>();

        Slider progressSlider = CreateProgressSlider(canvasGo.transform);
        TextMeshProUGUI progressText = CreateProgressText(canvasGo.transform);
        TextMeshProUGUI stateText = CreateStateText(canvasGo.transform);
        TextMeshProUGUI timerText = CreateTimerText(canvasGo.transform);
        TextMeshProUGUI attackersScoreText = CreateAttackersScoreText(canvasGo.transform);
        TextMeshProUGUI defendersScoreText = CreateDefendersScoreText(canvasGo.transform);
        TextMeshProUGUI roleText = CreateRoleText(canvasGo.transform);
        TextMeshProUGUI payloadStatusText = CreatePayloadStatusText(canvasGo.transform);
        Slider sabotageBar = CreateStatusBar(canvasGo.transform, "SabotageBar", new Vector2(0f, 90f), new Color(1f, 0.45f, 0.25f, 0.95f));
        Slider repairBar = CreateStatusBar(canvasGo.transform, "RepairBar", new Vector2(0f, 70f), new Color(0.2f, 1f, 0.5f, 0.95f));

        CreatePayloadIcon(canvasGo.transform);
        CreateTimerIcon(timerText.transform);
        CreateRoleIcon(roleText.transform);
        CreateStatusIcon(sabotageBar.transform, IconSabotagePath, new Vector2(-230f, 0f));
        CreateStatusIcon(repairBar.transform, IconRepairPath, new Vector2(-230f, 0f));

        GameObject matchEndPanel = CreateMatchEndPanel(canvasGo.transform, out TextMeshProUGUI winnerText);

        hud.progressBar = progressSlider;
        hud.stateText = stateText;
        hud.timerText = timerText;
        hud.progressText = progressText;
        hud.attackersScoreText = attackersScoreText;
        hud.defendersScoreText = defendersScoreText;
        hud.roleText = roleText;
        hud.payloadStatusText = payloadStatusText;
        hud.sabotageProgressBar = sabotageBar;
        hud.repairProgressBar = repairBar;

        matchEndPanel.SetActive(false);

        string prefabPath = "Assets/Prefabs/UI/PayloadEscortHUD.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvasGo, prefabPath);
        Object.DestroyImmediate(canvasGo);

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    }

    [MenuItem("Tools/Payload Escort/Apply HUD Icons To Selection")]
    public static void ApplyHudIconsToSelection()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("Select a HUD root GameObject first.");
            return;
        }

        ApplyIcons(Selection.activeGameObject.transform);
    }

    private static Slider CreateProgressSlider(Transform parent)
    {
        GameObject sliderGo = new GameObject("PayloadProgressSlider", typeof(RectTransform), typeof(Slider));
        sliderGo.transform.SetParent(parent, false);

        RectTransform rect = sliderGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(780f, 34f);
        rect.anchoredPosition = new Vector2(0f, 35f);

        Slider slider = sliderGo.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;

        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(sliderGo.transform, false);
        Image bgImage = background.GetComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.7f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGo.transform, false);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0f);
        fillAreaRect.anchorMax = new Vector2(1f, 1f);
        fillAreaRect.offsetMin = new Vector2(4f, 4f);
        fillAreaRect.offsetMax = new Vector2(-4f, -4f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.GetComponent<Image>();
        fillImage.color = new Color(0.2f, 0.8f, 1f, 0.95f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;

        return slider;
    }

    private static TextMeshProUGUI CreateStateText(Transform parent)
    {
        GameObject textGo = new GameObject("PayloadStateText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(parent, false);

        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = new Vector2(460f, 64f);
        rect.anchoredPosition = new Vector2(0f, -16f);

        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "Stopped";
        tmp.fontSize = 46f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return tmp;
    }

    private static TextMeshProUGUI CreateProgressText(Transform parent)
    {
        GameObject textGo = new GameObject("PayloadProgressText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(parent, false);

        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(200f, 40f);
        rect.anchoredPosition = new Vector2(0f, 84f);

        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "0%";
        tmp.fontSize = 34f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return tmp;
    }

    private static TextMeshProUGUI CreateTimerText(Transform parent)
    {
        GameObject textGo = new GameObject("MatchTimerText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(parent, false);

        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.sizeDelta = new Vector2(260f, 68f);
        rect.anchoredPosition = new Vector2(-24f, -16f);

        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "10:00";
        tmp.fontSize = 40f;
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.color = Color.white;

        return tmp;
    }

    private static TextMeshProUGUI CreateAttackersScoreText(Transform parent)
    {
        GameObject textGo = new GameObject("AttackersScoreText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(parent, false);

        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(320f, 60f);
        rect.anchoredPosition = new Vector2(46f, -14f);

        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "ATK: 0";
        tmp.fontSize = 36f;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = new Color(1f, 0.35f, 0.35f, 1f);

        CreateScoreIcon(textGo.transform, "AtkIcon", new Color(1f, 0.3f, 0.3f, 1f), new Vector2(-20f, 0f), IconAtkPath);

        return tmp;
    }

    private static TextMeshProUGUI CreateDefendersScoreText(Transform parent)
    {
        GameObject textGo = new GameObject("DefendersScoreText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(parent, false);

        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(320f, 60f);
        rect.anchoredPosition = new Vector2(46f, -56f);

        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "DEF: 0";
        tmp.fontSize = 36f;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = new Color(0.35f, 0.65f, 1f, 1f);

        CreateScoreIcon(textGo.transform, "DefIcon", new Color(0.35f, 0.65f, 1f, 1f), new Vector2(-20f, 0f), IconDefPath);

        return tmp;
    }

    private static TextMeshProUGUI CreateRoleText(Transform parent)
    {
        GameObject textGo = new GameObject("RoleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(parent, false);

        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.sizeDelta = new Vector2(420f, 60f);
        rect.anchoredPosition = new Vector2(24f, 20f);

        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "Role: Engineer";
        tmp.fontSize = 34f;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = Color.white;

        return tmp;
    }

    private static TextMeshProUGUI CreatePayloadStatusText(Transform parent)
    {
        GameObject textGo = new GameObject("PayloadStatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(parent, false);

        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(860f, 56f);
        rect.anchoredPosition = new Vector2(0f, -78f);

        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = string.Empty;
        tmp.fontSize = 34f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return tmp;
    }

    private static Slider CreateStatusBar(Transform parent, string name, Vector2 anchoredPos, Color fillColor)
    {
        GameObject sliderGo = new GameObject(name, typeof(RectTransform), typeof(Slider));
        sliderGo.transform.SetParent(parent, false);

        RectTransform rect = sliderGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(560f, 20f);
        rect.anchoredPosition = anchoredPos;

        Slider slider = sliderGo.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;

        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(sliderGo.transform, false);
        Image bgImage = background.GetComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.65f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGo.transform, false);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0f);
        fillAreaRect.anchorMax = new Vector2(1f, 1f);
        fillAreaRect.offsetMin = new Vector2(4f, 3f);
        fillAreaRect.offsetMax = new Vector2(-4f, -3f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImage = fill.GetComponent<Image>();
        fillImage.color = fillColor;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;

        sliderGo.SetActive(false);
        return slider;
    }

    private static void CreateScoreIcon(Transform parent, string name, Color color, Vector2 anchoredPos, string spritePath)
    {
        GameObject iconGo = new GameObject(name, typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(parent, false);
        RectTransform rect = iconGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(80f, 60f);
        rect.anchoredPosition = anchoredPos;

        Image image = iconGo.GetComponent<Image>();
        Sprite sprite = LoadIconSprite(spritePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
        }
        else
        {
            image.color = color;
        }
    }

    private static void CreatePayloadIcon(Transform parent)
    {
        GameObject iconGo = new GameObject("PayloadIcon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(parent, false);
        RectTransform rect = iconGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(80f, 60f);
        rect.anchoredPosition = new Vector2(-450f, 35f);

        Image image = iconGo.GetComponent<Image>();
        Sprite sprite = LoadIconSprite(IconPayloadPath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
        }
        else
        {
            image.color = new Color(0.2f, 0.8f, 1f, 1f);
        }
    }

    private static void CreateTimerIcon(Transform parent)
    {
        GameObject iconGo = new GameObject("TimerIcon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(parent, false);
        RectTransform rect = iconGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.sizeDelta = new Vector2(80f, 60f);
        rect.anchoredPosition = new Vector2(-270f, 0f);

        Image image = iconGo.GetComponent<Image>();
        Sprite sprite = LoadIconSprite(IconTimerPath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
        }
        else
        {
            image.color = Color.white;
        }
    }

    private static void CreateRoleIcon(Transform parent)
    {
        GameObject iconGo = new GameObject("RoleIcon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(parent, false);
        RectTransform rect = iconGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(80f, 60f);
        rect.anchoredPosition = new Vector2(-40f, 0f);

        Image image = iconGo.GetComponent<Image>();
        Sprite sprite = LoadIconSprite(IconRolePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
        }
        else
        {
            image.color = Color.white;
        }
    }

    private static void CreateStatusIcon(Transform parent, string spritePath, Vector2 anchoredPos)
    {
        GameObject iconGo = new GameObject("StatusIcon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(parent, false);
        RectTransform rect = iconGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(80f, 60f);
        rect.anchoredPosition = anchoredPos;

        Image image = iconGo.GetComponent<Image>();
        Sprite sprite = LoadIconSprite(spritePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
        }
        else
        {
            image.color = Color.white;
        }
    }

    private static Sprite LoadIconSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null) return sprite;

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture == null) return null;

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private static void ApplyIcons(Transform root)
    {
        EnsureIcon(root, "AttackersScoreText", "AtkIcon", IconAtkPath, new Vector2(-50f, 0f), new Vector2(80f, 60f));
        EnsureIcon(root, "DefendersScoreText", "DefIcon", IconDefPath, new Vector2(-50f, 0f), new Vector2(80f, 60f));
        EnsureIcon(root, null, "PayloadIcon", IconPayloadPath, new Vector2(-450f, 35f), new Vector2(80f, 60f));
        EnsureIcon(root, "MatchTimerText", "TimerIcon", IconTimerPath, new Vector2(-270f, 0f), new Vector2(80f, 60f));
        EnsureIcon(root, "RoleText", "RoleIcon", IconRolePath, new Vector2(-40f, 0f), new Vector2(80f, 60f));

        Transform sabotageBar = root.Find("SabotageBar");
        if (sabotageBar != null)
            EnsureIcon(sabotageBar, null, "StatusIcon", IconSabotagePath, new Vector2(-300f, 0f), new Vector2(80f, 60f));

        Transform repairBar = root.Find("RepairBar");
        if (repairBar != null)
            EnsureIcon(repairBar, null, "StatusIcon", IconRepairPath, new Vector2(-300f, 0f), new Vector2(80f, 60f));
    }

    private static void SetIconByName(Transform root, string name, string spritePath)
    {
        Transform target = root.Find(name);
        if (target == null) return;

        Image image = target.GetComponent<Image>();
        if (image == null) return;

        Sprite sprite = LoadIconSprite(spritePath);
        if (sprite == null) return;

        image.sprite = sprite;
        image.color = Color.white;
        image.preserveAspect = true;
    }

    private static void EnsureIcon(Transform root, string parentName, string iconName, string spritePath, Vector2 anchoredPos, Vector2 size)
    {
        Transform parent = root;
        if (!string.IsNullOrEmpty(parentName))
        {
            Transform parentTransform = root.Find(parentName);
            if (parentTransform != null)
                parent = parentTransform;
        }

        Transform iconTransform = parent.Find(iconName);
        if (iconTransform == null)
        {
            GameObject iconGo = new GameObject(iconName, typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(parent, false);
            iconTransform = iconGo.transform;
        }

        RectTransform rect = iconTransform.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;

        Image image = iconTransform.GetComponent<Image>();
        Sprite sprite = LoadIconSprite(spritePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
        }
    }

    private static GameObject CreateMatchEndPanel(Transform parent, out TextMeshProUGUI winnerText)
    {
        GameObject panelGo = new GameObject("MatchEndUI", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(parent, false);

        RectTransform rect = panelGo.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panelGo.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.6f);

        GameObject textGo = new GameObject("WinnerText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(panelGo.transform, false);

        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(700f, 220f);
        textRect.anchoredPosition = Vector2.zero;

        winnerText = textGo.GetComponent<TextMeshProUGUI>();
        winnerText.text = "Winner: Attackers\nPayload reached destination";
        winnerText.fontSize = 48f;
        winnerText.alignment = TextAlignmentOptions.Center;
        winnerText.color = Color.white;

        return panelGo;
    }
}
