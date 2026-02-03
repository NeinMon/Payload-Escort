using UnityEditor;
using UnityEngine;

public static class HeroSkillEffectBuilder
{
    [MenuItem("Tools/FPS Game/Build Skill Ready Effects")]
    public static void BuildSkillReadyEffects()
    {
        string prefabPath = "Assets/Resources/Player.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot == null)
        {
            Debug.LogWarning("Player prefab not found at Assets/Resources/Player.prefab");
            return;
        }

        HeroSkillReadyEffects readyEffects = prefabRoot.GetComponentInChildren<HeroSkillReadyEffects>(true);
        if (readyEffects == null)
            readyEffects = prefabRoot.AddComponent<HeroSkillReadyEffects>();

        GameObject root = EnsureChild(prefabRoot.transform, "SkillReadyFX");
        root.transform.localPosition = Vector3.zero;

        HeroSkillReadyEffects.ReadyEffect[] effects = new HeroSkillReadyEffects.ReadyEffect[12];
        int index = 0;

        AddRoleEffects(PlayerRole.Engineer, new Color(1f, 0.85f, 0.2f), root.transform, effects, ref index);
        AddRoleEffects(PlayerRole.Heavy, new Color(1f, 0.5f, 0.1f), root.transform, effects, ref index);
        AddRoleEffects(PlayerRole.Medic, new Color(0.3f, 1f, 0.4f), root.transform, effects, ref index);
        AddRoleEffects(PlayerRole.Saboteur, new Color(0.7f, 0.4f, 1f), root.transform, effects, ref index);

        SerializedObject so = new SerializedObject(readyEffects);
        SerializedProperty effectsProp = so.FindProperty("effects");
        effectsProp.arraySize = effects.Length;
        for (int i = 0; i < effects.Length; i++)
        {
            SerializedProperty element = effectsProp.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("role").enumValueIndex = (int)effects[i].role;
            element.FindPropertyRelative("slot").enumValueIndex = (int)effects[i].slot;
            element.FindPropertyRelative("particle").objectReferenceValue = effects[i].particle;
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(readyEffects);
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Skill ready effects created on Player prefab.");
    }

    private static void AddRoleEffects(PlayerRole role, Color color, Transform parent, HeroSkillReadyEffects.ReadyEffect[] effects, ref int index)
    {
        CreateEffect(role, HeroSkillSlot.Q, color, parent, effects, ref index);
        CreateEffect(role, HeroSkillSlot.E, color, parent, effects, ref index);
        CreateEffect(role, HeroSkillSlot.R, color, parent, effects, ref index);
    }

    private static void CreateEffect(PlayerRole role, HeroSkillSlot slot, Color color, Transform parent, HeroSkillReadyEffects.ReadyEffect[] effects, ref int index)
    {
        string name = $"{role}_{slot}_ReadyFX";
        GameObject go = EnsureChild(parent, name);
        go.transform.localPosition = new Vector3(0f, 1.6f, 0f);

        ParticleSystem ps = go.GetComponent<ParticleSystem>();
        if (ps == null) ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.startLifetime = 0.6f;
        main.startSpeed = 0.2f;
        main.startSize = 0.25f;
        main.startColor = color;
        main.maxParticles = 30;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, 14)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.25f;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        effects[index] = new HeroSkillReadyEffects.ReadyEffect
        {
            role = role,
            slot = slot,
            particle = ps
        };
        index++;
    }

    private static GameObject EnsureChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null) return child.gameObject;
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }
}
