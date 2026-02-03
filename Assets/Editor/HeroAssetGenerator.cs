using UnityEditor;
using UnityEngine;

public static class HeroAssetGenerator
{
    private const string BasePath = "Assets/Resources/Heroes";
    private const string DefinitionsPath = "Assets/Resources/Heroes/Definitions";
    private const string SkillsPath = "Assets/Resources/Heroes/Skills";
    private const string RosterPath = "Assets/Resources/HeroRoster.asset";

    [MenuItem("Tools/FPS Game/Generate Hero Assets")]
    public static void GenerateHeroAssets()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder(BasePath);
        EnsureFolder(DefinitionsPath);
        EnsureFolder(SkillsPath);

        // Hero Definitions
        HeroDefinition engineer = CreateOrLoad<HeroDefinition>($"{DefinitionsPath}/Engineer.asset");
        ConfigureHero(engineer, "engineer", "Engineer", HeroRole.PayloadControl, 175f, 5f, 1.5f,
            "Máy móc không mệt mỏi, con người thì có.");

        HeroDefinition heavy = CreateOrLoad<HeroDefinition>($"{DefinitionsPath}/Heavy.asset");
        ConfigureHero(heavy, "heavy", "Heavy", HeroRole.SpaceControl, 300f, 4f, 1.5f,
            "Áp đảo bằng sức mạnh, thắng bằng ý chí.");

        HeroDefinition medic = CreateOrLoad<HeroDefinition>($"{DefinitionsPath}/Medic.asset");
        ConfigureHero(medic, "medic", "Medic", HeroRole.AreaSupport, 150f, 6f, 1.5f,
            "Không ai bị bỏ lại phía sau.");

        HeroDefinition disruptor = CreateOrLoad<HeroDefinition>($"{DefinitionsPath}/Disruptor.asset");
        ConfigureHero(disruptor, "disruptor", "Disruptor", HeroRole.CrowdControl, 175f, 6f, 1.5f,
            "Hỗn loạn là thang máy đến chiến thắng.");

        // Skills - Engineer
        HeroSkillDefinition techKit = CreateOrLoad<HeroSkillDefinition>($"{SkillsPath}/Engineer_TechKit.asset");
        ConfigureSkill(techKit, "engineer_tech_kit", "Tech Kit", HeroSkillSlot.F, 0f, 0f, true,
            "Tăng 30% tốc độ sửa/phá Payload khi đứng gần và giữ F.", "F");

        HeroSkillDefinition deployTurret = CreateOrLoad<HeroSkillDefinition>($"{SkillsPath}/Engineer_DeployTurret.asset");
        ConfigureSkill(deployTurret, "engineer_deploy_turret", "Deploy Turret", HeroSkillSlot.E, 15f, 2f, false,
            "Đặt turret tự động bắn trong tầm 20m. HP 250, 30 DPS.", "E");

        HeroSkillDefinition turretOverdrive = CreateOrLoad<HeroSkillDefinition>($"{SkillsPath}/Engineer_TurretOverdrive.asset");
        ConfigureSkill(turretOverdrive, "engineer_turret_overdrive", "Turret Overdrive", HeroSkillSlot.R, 100f, 12f, false,
            "Nâng cấp turret: +50% damage, +30% tầm bắn, bắn 2 mục tiêu.", "X");

        // Skills - Heavy
        HeroSkillDefinition fortifyStance = CreateOrLoad<HeroSkillDefinition>($"{SkillsPath}/Heavy_FortifyStance.asset");
        ConfigureSkill(fortifyStance, "heavy_fortify_stance", "Fortify Stance", HeroSkillSlot.Q, 14f, 4f, false,
            "Giảm 50% sát thương, giảm 70% knockback, -15% tốc độ.", "Q");

        HeroSkillDefinition impactSlam = CreateOrLoad<HeroSkillDefinition>($"{SkillsPath}/Heavy_ImpactSlam.asset");
        ConfigureSkill(impactSlam, "heavy_impact_slam", "Impact Slam", HeroSkillSlot.E, 12f, 0.5f, false,
            "Đập đất gây 50 + 15% HP tối đa mục tiêu, đẩy lùi 8m.", "E");

        HeroSkillDefinition unbreakableWill = CreateOrLoad<HeroSkillDefinition>($"{SkillsPath}/Heavy_UnbreakableWill.asset");
        ConfigureSkill(unbreakableWill, "heavy_unbreakable_will", "Unbreakable Will", HeroSkillSlot.R, 120f, 5f, false,
            "Miễn nhiễm CC trong 5s, +200 HP tạm thời.", "X");

        // Skills - Medic
        HeroSkillDefinition healZone = CreateOrLoad<HeroSkillDefinition>($"{SkillsPath}/Medic_HealZone.asset");
        ConfigureSkill(healZone, "medic_heal_zone", "Heal Zone", HeroSkillSlot.Q, 16f, 10f, false,
            "Đặt vùng hồi máu 4m, 25 HP/s trong 10s.", "Q");

        HeroSkillDefinition lifeTether = CreateOrLoad<HeroSkillDefinition>($"{SkillsPath}/Medic_LifeTether.asset");
        ConfigureSkill(lifeTether, "medic_life_tether", "Life Tether", HeroSkillSlot.E, 8f, 0f, false,
            "Kết nối hồi máu 40 HP/s cho 1 đồng đội trong 20m.", "E");

        HeroSkillDefinition revive = CreateOrLoad<HeroSkillDefinition>($"{SkillsPath}/Medic_Revive.asset");
        ConfigureSkill(revive, "medic_revive", "Revive", HeroSkillSlot.R, 150f, 3f, false,
            "Hồi sinh tối đa 2 đồng đội trong 10m, hồi 50% HP.", "X");

        // Skills - Disruptor
        HeroSkillDefinition empPulse = CreateOrLoad<HeroSkillDefinition>($"{SkillsPath}/Disruptor_EMPPulse.asset");
        ConfigureSkill(empPulse, "disruptor_emp_pulse", "EMP Pulse", HeroSkillSlot.Q, 12f, 2.5f, false,
            "Silence 2.5s và slow 25% trong 1.5s ở tầm 20m.", "Q");

        HeroSkillDefinition kineticField = CreateOrLoad<HeroSkillDefinition>($"{SkillsPath}/Disruptor_KineticField.asset");
        ConfigureSkill(kineticField, "disruptor_kinetic_field", "Kinetic Field", HeroSkillSlot.E, 18f, 8f, false,
            "Dựng trường lực 5m chặn di chuyển trong 8s, HP 300.", "E");

        HeroSkillDefinition areaSuppression = CreateOrLoad<HeroSkillDefinition>($"{SkillsPath}/Disruptor_AreaSuppression.asset");
        ConfigureSkill(areaSuppression, "disruptor_area_suppression", "Area Suppression", HeroSkillSlot.R, 130f, 5f, false,
            "Vùng áp chế 10m: slow 35% và silence trong 5s.", "X");

        // Hero Roster
        HeroRoster roster = CreateOrLoad<HeroRoster>(RosterPath);
        roster.loadouts = new HeroRoster.HeroLoadout[4];

        roster.loadouts[0] = new HeroRoster.HeroLoadout
        {
            heroId = engineer.heroId,
            role = PlayerRole.Engineer,
            heroDefinition = engineer,
            skills = new[] { techKit, deployTurret, turretOverdrive }
        };

        roster.loadouts[1] = new HeroRoster.HeroLoadout
        {
            heroId = heavy.heroId,
            role = PlayerRole.Heavy,
            heroDefinition = heavy,
            skills = new[] { fortifyStance, impactSlam, unbreakableWill }
        };

        roster.loadouts[2] = new HeroRoster.HeroLoadout
        {
            heroId = medic.heroId,
            role = PlayerRole.Medic,
            heroDefinition = medic,
            skills = new[] { healZone, lifeTether, revive }
        };

        roster.loadouts[3] = new HeroRoster.HeroLoadout
        {
            heroId = disruptor.heroId,
            role = PlayerRole.Saboteur,
            heroDefinition = disruptor,
            skills = new[] { empPulse, kineticField, areaSuppression }
        };

        EditorUtility.SetDirty(roster);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Hero assets generated successfully.");
    }

    private static void ConfigureHero(HeroDefinition hero, string id, string name, HeroRole role, float hp, float moveSpeed, float sprintMultiplier, string tagline)
    {
        hero.heroId = id;
        hero.displayName = name;
        hero.role = role;
        hero.maxHealth = hp;
        hero.moveSpeed = moveSpeed;
        hero.sprintMultiplier = sprintMultiplier;
        hero.tagline = tagline;
        EditorUtility.SetDirty(hero);
    }

    private static void ConfigureSkill(HeroSkillDefinition skill, string id, string name, HeroSkillSlot slot, float cooldown, float duration, bool passive, string description, string inputHint)
    {
        skill.skillId = id;
        skill.displayName = name;
        skill.slot = slot;
        skill.cooldownSeconds = cooldown;
        skill.durationSeconds = duration;
        skill.isPassive = passive;
        skill.description = description;
        skill.inputHint = inputHint;
        EditorUtility.SetDirty(skill);
    }

    private static T CreateOrLoad<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
            return asset;

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string name = System.IO.Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, name);
    }
}
