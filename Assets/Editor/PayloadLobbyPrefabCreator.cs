using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class PayloadLobbyPrefabCreator
{
    [MenuItem("Tools/Payload Escort/Create Lobby Prefab")]
    public static void CreateLobbyPrefab()
    {
        GameObject canvasGo = new GameObject("PayloadLobby_Canvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("LobbyPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGo.transform, false);
        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.6f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(800f, 600f);
        panelRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI title = CreateText(panel.transform, "LobbyTitle", "PAYLOAD ESCORT", 42, new Vector2(0f, 240f), TextAlignmentOptions.Center);

        TMP_Dropdown teamDropdown = CreateDropdown(panel.transform, "TeamDropdown", new Vector2(0f, 140f), new string[] { "Auto", "Attackers", "Defenders" });
        TMP_Dropdown roleDropdown = CreateDropdown(panel.transform, "RoleDropdown", new Vector2(0f, 70f), new string[] { "Choose Role", "Engineer", "Heavy", "Medic", "Saboteur" });

        Button readyButton = CreateButton(panel.transform, "ReadyButton", "Ready", new Vector2(-120f, -40f));
        Button playButton = CreateButton(panel.transform, "PlayButton", "Play", new Vector2(120f, -40f));

        TextMeshProUGUI statusText = CreateText(panel.transform, "StatusText", "Connecting...", 24, new Vector2(0f, -120f), TextAlignmentOptions.Center);
        TextMeshProUGUI playersText = CreateText(panel.transform, "PlayersText", "", 20, new Vector2(0f, -200f), TextAlignmentOptions.TopLeft);
        RectTransform playersRect = playersText.GetComponent<RectTransform>();
        playersRect.sizeDelta = new Vector2(720f, 240f);

        LobbyManager lobby = canvasGo.AddComponent<LobbyManager>();
        lobby.teamDropdown = teamDropdown;
        lobby.roleDropdown = roleDropdown;
        lobby.readyButton = readyButton;
        lobby.playButton = playButton;
        lobby.statusText = statusText;
        lobby.playersText = playersText;

        string prefabPath = "Assets/Prefabs/UI/PayloadLobby.prefab";
        PrefabUtility.SaveAsPrefabAsset(canvasGo, prefabPath);
        Object.DestroyImmediate(canvasGo);

        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string content, float size, Vector2 anchoredPos, TextAlignmentOptions alignment)
    {
        GameObject textGo = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(parent, false);

        RectTransform rect = textGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(600f, 40f);
        rect.anchoredPosition = anchoredPos;

        TextMeshProUGUI tmp = textGo.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.alignment = alignment;
        tmp.color = Color.white;

        return tmp;
    }

    static TMP_Dropdown CreateDropdown(Transform parent, string name, Vector2 anchoredPos, string[] options)
    {
        GameObject dropdownGo = new GameObject(name, typeof(RectTransform), typeof(TMP_Dropdown), typeof(Image));
        dropdownGo.transform.SetParent(parent, false);
        RectTransform rect = dropdownGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(360f, 40f);
        rect.anchoredPosition = anchoredPos;

        Image image = dropdownGo.GetComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        TMP_Dropdown dropdown = dropdownGo.GetComponent<TMP_Dropdown>();
        dropdown.options.Clear();
        foreach (string option in options)
            dropdown.options.Add(new TMP_Dropdown.OptionData(option));

        TextMeshProUGUI label = CreateText(dropdownGo.transform, "Label", options[0], 22, Vector2.zero, TextAlignmentOptions.Left);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(10f, 0f);
        labelRect.offsetMax = new Vector2(-30f, 0f);

        GameObject arrowGo = new GameObject("Arrow", typeof(RectTransform), typeof(Image));
        arrowGo.transform.SetParent(dropdownGo.transform, false);
        RectTransform arrowRect = arrowGo.GetComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1f, 0.5f);
        arrowRect.anchorMax = new Vector2(1f, 0.5f);
        arrowRect.pivot = new Vector2(1f, 0.5f);
        arrowRect.sizeDelta = new Vector2(20f, 20f);
        arrowRect.anchoredPosition = new Vector2(-10f, 0f);
        Image arrowImage = arrowGo.GetComponent<Image>();
        arrowImage.color = Color.white;

        TextMeshProUGUI itemLabel;
        Image itemImage;
        GameObject template = CreateDropdownTemplate(dropdownGo.transform, out itemLabel, out itemImage);
        dropdown.template = template.GetComponent<RectTransform>();
        dropdown.captionText = label;

        dropdown.itemText = itemLabel;
        dropdown.itemImage = itemImage;

        return dropdown;
    }

    static GameObject CreateDropdownTemplate(Transform parent, out TextMeshProUGUI itemLabel, out Image itemImage)
    {
        GameObject template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        template.transform.SetParent(parent, false);
        RectTransform templateRect = template.GetComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0f, 0f);
        templateRect.anchorMax = new Vector2(1f, 0f);
        templateRect.pivot = new Vector2(0.5f, 1f);
        templateRect.sizeDelta = new Vector2(0f, 180f);
        templateRect.anchoredPosition = new Vector2(0f, -5f);

        Image templateImage = template.GetComponent<Image>();
        templateImage.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
        viewport.transform.SetParent(template.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = new Vector2(0f, 0f);
        viewportRect.anchorMax = new Vector2(1f, 1f);
        viewportRect.offsetMin = new Vector2(4f, 4f);
        viewportRect.offsetMax = new Vector2(-4f, -4f);

        Mask mask = viewport.GetComponent<Mask>();
        mask.showMaskGraphic = false;
        Image viewportImage = viewport.GetComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.2f);

        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 28f);

        GameObject item = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
        item.transform.SetParent(content.transform, false);
        RectTransform itemRect = item.GetComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0f, 0.5f);
        itemRect.anchorMax = new Vector2(1f, 0.5f);
        itemRect.pivot = new Vector2(0.5f, 0.5f);
        itemRect.sizeDelta = new Vector2(0f, 28f);

        GameObject itemBackground = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
        itemBackground.transform.SetParent(item.transform, false);
        RectTransform bgRect = itemBackground.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = itemBackground.GetComponent<Image>();
        bgImage.color = new Color(1f, 1f, 1f, 0.1f);

        GameObject itemCheckmark = new GameObject("Item Checkmark", typeof(RectTransform), typeof(Image));
        itemCheckmark.transform.SetParent(item.transform, false);
        RectTransform checkRect = itemCheckmark.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0f, 0.5f);
        checkRect.anchorMax = new Vector2(0f, 0.5f);
        checkRect.pivot = new Vector2(0f, 0.5f);
        checkRect.sizeDelta = new Vector2(16f, 16f);
        checkRect.anchoredPosition = new Vector2(10f, 0f);
        Image checkImage = itemCheckmark.GetComponent<Image>();
        checkImage.color = new Color(0.2f, 0.8f, 1f, 1f);

        itemLabel = CreateText(item.transform, "Item Label", "Option", 22, Vector2.zero, TextAlignmentOptions.Left);
        RectTransform itemLabelRect = itemLabel.GetComponent<RectTransform>();
        itemLabelRect.anchorMin = new Vector2(0f, 0f);
        itemLabelRect.anchorMax = new Vector2(1f, 1f);
        itemLabelRect.offsetMin = new Vector2(30f, 0f);
        itemLabelRect.offsetMax = new Vector2(-10f, 0f);

        Toggle toggle = item.GetComponent<Toggle>();
        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;

        itemImage = checkImage;

        ScrollRect scrollRect = template.GetComponent<ScrollRect>();
        scrollRect.content = contentRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        template.SetActive(false);
        return template;
    }

    static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos)
    {
        GameObject buttonGo = new GameObject(name, typeof(RectTransform), typeof(Button), typeof(Image));
        buttonGo.transform.SetParent(parent, false);

        RectTransform rect = buttonGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(180f, 50f);
        rect.anchoredPosition = anchoredPos;

        Image image = buttonGo.GetComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 1f, 0.9f);

        TextMeshProUGUI text = CreateText(buttonGo.transform, "Text", label, 24, Vector2.zero, TextAlignmentOptions.Center);
        text.rectTransform.sizeDelta = new Vector2(180f, 50f);

        return buttonGo.GetComponent<Button>();
    }
}
