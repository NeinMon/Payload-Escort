using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class LobbyHeroPreviewBuilder
{
    [MenuItem("Tools/FPS Game/Build Lobby Hero Preview Panel")]
    public static void BuildPanel()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("LobbyCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
        }

        GameObject panel = new GameObject("HeroPreviewPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.7f, 0.2f);
        panelRect.anchorMax = new Vector2(0.98f, 0.8f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.6f);

        TextMeshProUGUI title = CreateText(panel.transform, "HeroName", "Hero Name", 28, FontStyles.Bold);
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.8f);
        titleRect.anchorMax = new Vector2(0.95f, 0.95f);

        TextMeshProUGUI info = CreateText(panel.transform, "HeroInfo", "HP: --\nSpeed: --", 20, FontStyles.Normal);
        RectTransform infoRect = info.GetComponent<RectTransform>();
        infoRect.anchorMin = new Vector2(0.05f, 0.6f);
        infoRect.anchorMax = new Vector2(0.95f, 0.78f);

        TextMeshProUGUI skills = CreateText(panel.transform, "HeroSkills", "Q - Skill\nE - Skill\nR - Skill", 20, FontStyles.Normal);
        RectTransform skillsRect = skills.GetComponent<RectTransform>();
        skillsRect.anchorMin = new Vector2(0.05f, 0.1f);
        skillsRect.anchorMax = new Vector2(0.95f, 0.58f);

        LobbyManager lobby = Object.FindFirstObjectByType<LobbyManager>();
        if (lobby != null)
        {
            lobby.heroNameText = title;
            lobby.heroInfoText = info;
            lobby.heroSkillsText = skills;
            EditorUtility.SetDirty(lobby);
        }

        Selection.activeGameObject = panel;
        Debug.Log("Lobby Hero Preview Panel created.");
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int size, FontStyles style)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return tmp;
    }
}
