using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class GameplayHeroHUDBuilder
{
    private const string IconFolder = "Assets/UI/skillHero";
    private const string IconSetPath = "Assets/Resources/Heroes/UI/HeroUIIconSet.asset";

    [MenuItem("Tools/FPS Game/Build Gameplay Hero HUD")]
    public static void BuildGameplayHud()
    {
        LocalPlayerHUD localHud = Object.FindFirstObjectByType<LocalPlayerHUD>();
        bool usingPrefab = false;
        string prefabPath = "Assets/Resources/Player.prefab";
        GameObject prefabRoot = null;

        if (localHud == null)
        {
            prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            localHud = prefabRoot.GetComponentInChildren<LocalPlayerHUD>(true);
            if (localHud == null)
            {
                if (prefabRoot != null)
                    PrefabUtility.UnloadPrefabContents(prefabRoot);

                Debug.LogWarning("No LocalPlayerHUD found in scene or Player prefab.");
                return;
            }

            usingPrefab = true;
        }

        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Heroes");
        EnsureFolder("Assets/Resources/Heroes/UI");

        HeroUIIconAssigner.AssignIcons();
        HeroUIIconSet iconSet = CreateOrLoad<HeroUIIconSet>(IconSetPath);

        Transform root = localHud.transform;
        GameObject hudRoot = EnsureChild(root, "HeroHUDRoot");
        RectTransform hudRect = hudRoot.GetComponent<RectTransform>();
        if (hudRect == null) hudRect = hudRoot.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0f, 0f);
        hudRect.anchorMax = new Vector2(0f, 0f);
        hudRect.pivot = new Vector2(0f, 0f);
        hudRect.sizeDelta = new Vector2(560f, 220f);
        hudRect.anchoredPosition = new Vector2(20f, 20f);


        // Hero portrait
        Image heroPortrait = EnsureImage(hudRoot.transform, "HeroPortrait", new Vector2(72, 72), new Vector2(20, 130));
        heroPortrait.preserveAspect = true;

        // Health bar
        GameObject healthRoot = EnsureChild(hudRoot.transform, "HealthBar");
        RectTransform healthRect = healthRoot.GetComponent<RectTransform>();
        if (healthRect == null) healthRect = healthRoot.AddComponent<RectTransform>();
        healthRect.anchorMin = new Vector2(0f, 0f);
        healthRect.anchorMax = new Vector2(0f, 0f);
        healthRect.pivot = new Vector2(0f, 0f);
        healthRect.sizeDelta = new Vector2(340f, 26f);
        healthRect.anchoredPosition = new Vector2(110f, 150f);

        Image healthBg = EnsureImage(healthRoot.transform, "HealthBarBG", new Vector2(320, 26), Vector2.zero);
        Image healthFill = EnsureImage(healthRoot.transform, "HealthBarFill", new Vector2(320, 26), Vector2.zero);
        healthFill.type = Image.Type.Filled;
        healthFill.fillMethod = Image.FillMethod.Horizontal;
        healthFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        healthFill.fillAmount = 1f;

        TextMeshProUGUI healthText = EnsureText(hudRoot.transform, "HealthText", "100 / 100", 18, FontStyles.Bold, new Vector2(110, 118));

        // Skill bar
        GameObject skillRoot = EnsureChild(hudRoot.transform, "SkillBar");
        RectTransform skillRect = skillRoot.GetComponent<RectTransform>();
        if (skillRect == null) skillRect = skillRoot.AddComponent<RectTransform>();
        skillRect.anchorMin = new Vector2(0f, 0f);
        skillRect.anchorMax = new Vector2(0f, 0f);
        skillRect.pivot = new Vector2(0f, 0f);
        skillRect.sizeDelta = new Vector2(380f, 110f);
        skillRect.anchoredPosition = new Vector2(110f, 20f);


        SkillSlotData q = BuildSkillSlot(skillRoot.transform, "Slot_Q", new Vector2(0f, 0f));
        SkillSlotData e = BuildSkillSlot(skillRoot.transform, "Slot_E", new Vector2(130f, 0f));
        SkillSlotData r = BuildSkillSlot(skillRoot.transform, "Slot_R", new Vector2(260f, 0f));

        // HeroSkillHUD component
        HeroSkillHUD heroSkillHud = localHud.GetComponent<HeroSkillHUD>();
        if (heroSkillHud == null)
            heroSkillHud = localHud.gameObject.AddComponent<HeroSkillHUD>();

        SerializedObject hudObj = new SerializedObject(heroSkillHud);
        SerializedProperty slotsProp = hudObj.FindProperty("slots");
        slotsProp.arraySize = 3;
        AssignSlot(slotsProp, 0, HeroSkillSlot.Q, q);
        AssignSlot(slotsProp, 1, HeroSkillSlot.E, e);
        AssignSlot(slotsProp, 2, HeroSkillSlot.R, r);
        hudObj.ApplyModifiedPropertiesWithoutUndo();

        // HeroHUDIcons component
        HeroHUDIcons hudIcons = localHud.GetComponent<HeroHUDIcons>();
        if (hudIcons == null)
            hudIcons = localHud.gameObject.AddComponent<HeroHUDIcons>();

        hudIcons.GetType().GetField("iconSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(hudIcons, iconSet);
        hudIcons.GetType().GetField("skillHud", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(hudIcons, heroSkillHud);
        hudIcons.GetType().GetField("heroPortrait", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(hudIcons, heroPortrait);
        hudIcons.GetType().GetField("healthFill", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(hudIcons, healthFill);
        hudIcons.GetType().GetField("healthBackground", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(hudIcons, healthBg);

        // Assign LocalPlayerHUD
        localHud.healthBar = healthFill;
        localHud.healthText = healthText;

        EditorUtility.SetDirty(localHud);
        EditorUtility.SetDirty(heroSkillHud);
        EditorUtility.SetDirty(iconSet);

        if (usingPrefab)
        {
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Gameplay Hero HUD built and icons assigned.");
    }

    private static void AssignIcons(HeroUIIconSet set)
    {
        set.healthBarFill = LoadSprite("Health_bar.png");
        set.healthBarBackground = LoadSprite("Health_bar.png");

        set.roles = new HeroUIIconSet.RoleIcons[4];
        set.roles[0] = new HeroUIIconSet.RoleIcons
        {
            role = PlayerRole.Engineer,
            heroIcon = LoadSprite("Engineer_Hero.png"),
            skillQ = LoadSprite("Engineer_Q.png"),
            skillE = LoadSprite("Engineer_E.png"),
            skillR = LoadSprite("Engineer_R.png")
        };
        set.roles[1] = new HeroUIIconSet.RoleIcons
        {
            role = PlayerRole.Heavy,
            heroIcon = LoadSprite("Heavy_Hero.png"),
            skillQ = LoadSprite("Heavy_Q.png"),
            skillE = LoadSprite("Heavy_E.png"),
            skillR = LoadSprite("Heavy_R.png")
        };
        set.roles[2] = new HeroUIIconSet.RoleIcons
        {
            role = PlayerRole.Medic,
            heroIcon = LoadSprite("Medic_Hero.png"),
            skillQ = LoadSprite("Medic_Q.png"),
            skillE = LoadSprite("Medic_E.png"),
            skillR = LoadSprite("Medic_R.png")
        };
        set.roles[3] = new HeroUIIconSet.RoleIcons
        {
            role = PlayerRole.Saboteur,
            heroIcon = LoadSprite("Disruptor_Hero.png"),
            skillQ = LoadSprite("Disruptor_Q.png"),
            skillE = LoadSprite("Disruptor_E.png"),
            skillR = LoadSprite("Disruptor_R.png")
        };
    }

    private static Sprite LoadSprite(string fileName)
    {
        string path = $"{IconFolder}/{fileName}";
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null) return sprite;

        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is Sprite s)
                return s;
        }

        return null;
    }

    private static GameObject EnsureChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null) return child.gameObject;
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static Image EnsureImage(Transform parent, string name, Vector2 size, Vector2 anchoredPos)
    {
        GameObject go = EnsureChild(parent, name);
        Image image = go.GetComponent<Image>();
        if (image == null) image = go.AddComponent<Image>();
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;
        return image;
    }

    private static TextMeshProUGUI EnsureText(Transform parent, string name, string text, int size, FontStyles style, Vector2 anchoredPos)
    {
        GameObject go = EnsureChild(parent, name);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.sizeDelta = new Vector2(260f, 24f);
        rect.anchoredPosition = anchoredPos;
        return tmp;
    }

    private static SkillSlotData BuildSkillSlot(Transform parent, string name, Vector2 anchoredPos)
    {
        GameObject root = EnsureChild(parent, name);
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.sizeDelta = new Vector2(96f, 96f);
        rect.anchoredPosition = anchoredPos;

        Image icon = EnsureImage(root.transform, "Icon", new Vector2(80f, 80f), new Vector2(8f, 8f));
        icon.preserveAspect = true;
        Image overlay = EnsureImage(root.transform, "CooldownOverlay", new Vector2(80f, 80f), new Vector2(8f, 8f));
        overlay.type = Image.Type.Filled;
        overlay.fillMethod = Image.FillMethod.Radial360;
        overlay.fillOrigin = (int)Image.Origin360.Top;
        overlay.fillClockwise = false;
        overlay.fillAmount = 0f;
        overlay.color = new Color(0f, 0f, 0f, 0f);
        overlay.enabled = false;
        overlay.raycastTarget = false;
        TextMeshProUGUI nameText = EnsureText(root.transform, "Name", name.Replace("Slot_", ""), 16, FontStyles.Bold, new Vector2(8f, -2f));
        TextMeshProUGUI cooldownText = EnsureText(root.transform, "Cooldown", "", 18, FontStyles.Bold, new Vector2(8f, 30f));

        return new SkillSlotData
        {
            root = root,
            icon = icon,
            nameText = nameText,
            cooldownText = cooldownText,
            overlay = overlay
        };
    }

    private static void AssignSlot(SerializedProperty array, int index, HeroSkillSlot slot, SkillSlotData data)
    {
        SerializedProperty element = array.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("slot").enumValueIndex = (int)slot;
        element.FindPropertyRelative("root").objectReferenceValue = data.root;
        element.FindPropertyRelative("nameText").objectReferenceValue = data.nameText;
        element.FindPropertyRelative("cooldownText").objectReferenceValue = data.cooldownText;
        element.FindPropertyRelative("icon").objectReferenceValue = data.icon;
        element.FindPropertyRelative("cooldownOverlay").objectReferenceValue = data.overlay;
    }

    private static T CreateOrLoad<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string name = System.IO.Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private class SkillSlotData
    {
        public GameObject root;
        public Image icon;
        public Image overlay;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI cooldownText;
    }
}
