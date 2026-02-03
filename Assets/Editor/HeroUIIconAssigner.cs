using UnityEditor;
using UnityEngine;

public static class HeroUIIconAssigner
{
    private const string IconFolder = "Assets/UI/skillHero";
    private const string IconSetPath = "Assets/Resources/Heroes/UI/HeroUIIconSet.asset";

    [MenuItem("Tools/FPS Game/Assign Hero UI Icons")]
    public static void AssignIcons()
    {
        string[] files =
        {
            "Health_bar.png",
            "Health_Background.png",
            "Engineer_Q.png",
            "Engineer_E.png",
            "Engineer_R.png",
            "Heavy_Q.png",
            "Heavy_E.png",
            "Heavy_R.png",
            "Medic_Q.png",
            "Medic_E.png",
            "Medic_R.png",
            "Disruptor_Q.png",
            "Disruptor_E.png",
            "Disruptor_R.png",
            "Engineer_Hero.png",
            "Heavy_Hero.png",
            "Medic_Hero.png",
            "Disruptor_Hero.png"
        };

        for (int i = 0; i < files.Length; i++)
        {
            string resolvedPath = ResolvePath(files[i]);
            if (!string.IsNullOrEmpty(resolvedPath))
            {
                EnsureSpriteImport(files[i], resolvedPath);
                ForceImport(resolvedPath);
            }
        }

        HeroUIIconSet iconSet = AssetDatabase.LoadAssetAtPath<HeroUIIconSet>(IconSetPath);
        if (iconSet == null)
        {
            iconSet = ScriptableObject.CreateInstance<HeroUIIconSet>();
            AssetDatabase.CreateAsset(iconSet, IconSetPath);
        }

        iconSet.healthBarFill = null;
        iconSet.healthBarBackground = null;
        iconSet.healthBarFill = LoadSpriteForce("Health_bar.png");
        iconSet.healthBarBackground = LoadSpriteForce("Health_Background.png");
        if (iconSet.healthBarBackground == null)
            iconSet.healthBarBackground = LoadSpriteForce("Health_bar.png");

        iconSet.roles = new HeroUIIconSet.RoleIcons[4];
        iconSet.roles[0] = new HeroUIIconSet.RoleIcons
        {
            role = PlayerRole.Engineer,
            heroIcon = LoadSpriteForce("Engineer_Hero.png"),
            skillQ = LoadSpriteForce("Engineer_Q.png"),
            skillE = LoadSpriteForce("Engineer_E.png"),
            skillR = LoadSpriteForce("Engineer_R.png")
        };
        iconSet.roles[1] = new HeroUIIconSet.RoleIcons
        {
            role = PlayerRole.Heavy,
            heroIcon = LoadSpriteForce("Heavy_Hero.png"),
            skillQ = LoadSpriteForce("Heavy_Q.png"),
            skillE = LoadSpriteForce("Heavy_E.png"),
            skillR = LoadSpriteForce("Heavy_R.png")
        };
        iconSet.roles[2] = new HeroUIIconSet.RoleIcons
        {
            role = PlayerRole.Medic,
            heroIcon = LoadSpriteForce("Medic_Hero.png"),
            skillQ = LoadSpriteForce("Medic_Q.png"),
            skillE = LoadSpriteForce("Medic_E.png"),
            skillR = LoadSpriteForce("Medic_R.png")
        };
        iconSet.roles[3] = new HeroUIIconSet.RoleIcons
        {
            role = PlayerRole.Saboteur,
            heroIcon = LoadSpriteForce("Disruptor_Hero.png"),
            skillQ = LoadSpriteForce("Disruptor_Q.png"),
            skillE = LoadSpriteForce("Disruptor_E.png"),
            skillR = LoadSpriteForce("Disruptor_R.png")
        };

        EditorUtility.SetDirty(iconSet);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        LogMissing(iconSet);
        Debug.Log("Hero UI icons assigned.");
    }

    [MenuItem("Tools/FPS Game/Debug Hero UI Icon Paths")]
    public static void DebugIconPaths()
    {
        string[] files =
        {
            "Health_bar.png",
            "Health_Background.png",
            "Engineer_Q.png",
            "Engineer_E.png",
            "Engineer_R.png",
            "Heavy_Q.png",
            "Heavy_E.png",
            "Heavy_R.png",
            "Medic_Q.png",
            "Medic_E.png",
            "Medic_R.png",
            "Disruptor_Q.png",
            "Disruptor_E.png",
            "Disruptor_R.png",
            "Engineer_Hero.png",
            "Heavy_Hero.png",
            "Medic_Hero.png",
            "Disruptor_Hero.png"
        };

        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i];
            string path = ResolvePath(file);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"[HeroUIIconAssigner] Not found: {file}");
                continue;
            }

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            string type = importer != null ? importer.textureType.ToString() : "<no importer>";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
            int spriteCount = 0;
            for (int j = 0; j < all.Length; j++)
                if (all[j] is Sprite) spriteCount++;

            Debug.Log($"[HeroUIIconAssigner] {file} -> {path} | Importer: {type} | Sprite: {(sprite != null ? "yes" : "no")} | SpritesInAsset: {spriteCount}");
        }
    }

    private static void EnsureSpriteImport(string fileName)
    {
        string path = ResolvePath(fileName);
        if (string.IsNullOrEmpty(path)) return;
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            changed = true;
        }

        if (changed)
            importer.SaveAndReimport();
    }

    private static void EnsureSpriteImport(string fileName, string resolvedPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(resolvedPath) as TextureImporter;
        if (importer == null) return;

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            changed = true;
        }

        if (changed)
            importer.SaveAndReimport();
    }

    private static Sprite LoadSprite(string fileName)
    {
        string path = $"{IconFolder}/{fileName}";
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void ForceImport(string path)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    private static Sprite LoadSpriteForce(string fileName)
    {
        string path = ResolvePath(fileName);
        if (string.IsNullOrEmpty(path)) return null;

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null) return sprite;

        Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] is Sprite s)
                return s;
        }

        string nameNoExt = System.IO.Path.GetFileNameWithoutExtension(fileName);
        string[] spriteGuids = AssetDatabase.FindAssets($"{nameNoExt} t:Sprite");
        for (int i = 0; i < spriteGuids.Length; i++)
        {
            string foundPath = AssetDatabase.GUIDToAssetPath(spriteGuids[i]);
            if (System.IO.Path.GetFileNameWithoutExtension(foundPath) != nameNoExt)
                continue;

            Sprite found = AssetDatabase.LoadAssetAtPath<Sprite>(foundPath);
            if (found != null) return found;
        }

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture == null) return null;

        string genFolder = "Assets/Resources/Heroes/UI/GeneratedSprites";
        EnsureFolder(genFolder);
        string genPath = $"{genFolder}/{nameNoExt}.asset";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(genPath);
        if (existing != null) return existing;

        Sprite generated = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        AssetDatabase.CreateAsset(generated, genPath);
        AssetDatabase.ImportAsset(genPath, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<Sprite>(genPath);
    }

    private static string ResolvePath(string fileName)
    {
        string direct = $"{IconFolder}/{fileName}";
        if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(direct)))
            return direct;

        string nameNoExt = System.IO.Path.GetFileNameWithoutExtension(fileName);
        string[] guids = AssetDatabase.FindAssets($"{nameNoExt} t:Texture2D");
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (System.IO.Path.GetFileName(path).Equals(fileName, System.StringComparison.OrdinalIgnoreCase))
                return path;
        }

        return null;
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

    private static void LogMissing(HeroUIIconSet set)
    {
        if (set == null) return;

        if (set.healthBarFill == null)
            Debug.LogWarning("Missing sprite: Health_bar.png");

        if (set.healthBarBackground == null)
            Debug.LogWarning("Missing sprite: Health_Background.png (fallback to Health_bar.png failed)");

        if (set.roles == null) return;

        for (int i = 0; i < set.roles.Length; i++)
        {
            var role = set.roles[i];
            if (role == null) continue;
            if (role.heroIcon == null)
                Debug.LogWarning($"Missing hero icon for role {role.role}");
            if (role.skillQ == null)
                Debug.LogWarning($"Missing Q icon for role {role.role}");
            if (role.skillE == null)
                Debug.LogWarning($"Missing E icon for role {role.role}");
            if (role.skillR == null)
                Debug.LogWarning($"Missing R icon for role {role.role}");
        }
    }
}
