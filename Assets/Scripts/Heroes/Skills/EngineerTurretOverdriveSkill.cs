using UnityEngine;

public class EngineerTurretOverdriveSkill : HeroSkillBehaviour
{
    private const string SkillId = "engineer_turret_overdrive";
    public float overdriveDuration = 12f;

    public override void Activate(HeroRuntime runtime)
    {
        if (runtime == null) return;

        if (!IsCorrectSkill(runtime))
            return;

        EngineerTurretState state = runtime.GetComponent<EngineerTurretState>();
        if (state == null || state.LastTurret == null) return;

        TurretOverdriveEffect effect = state.LastTurret.GetComponent<TurretOverdriveEffect>();
        if (effect == null)
            effect = state.LastTurret.AddComponent<TurretOverdriveEffect>();

        effect.Activate(overdriveDuration);
    }

    private bool IsCorrectSkill(HeroRuntime runtime)
    {
        HeroSkillDefinition def = runtime.GetSkill(HeroSkillSlot.R);
        return def != null && def.skillId == SkillId;
    }
}
