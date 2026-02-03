using UnityEngine;

public class EngineerTurretOverdriveSkill : HeroSkillBehaviour
{
    public float overdriveDuration = 12f;

    public override void Activate(HeroRuntime runtime)
    {
        if (runtime == null) return;

        EngineerTurretState state = runtime.GetComponent<EngineerTurretState>();
        if (state == null || state.LastTurret == null) return;

        TurretOverdriveEffect effect = state.LastTurret.GetComponent<TurretOverdriveEffect>();
        if (effect == null)
            effect = state.LastTurret.AddComponent<TurretOverdriveEffect>();

        effect.Activate(overdriveDuration);
    }
}
