using UnityEngine;

[CreateAssetMenu(fileName = "HeroSkill", menuName = "FPS Game/Heroes/Hero Skill")]
public class HeroSkillDefinition : ScriptableObject
{
    public string skillId;
    public string displayName;
    public HeroSkillSlot slot = HeroSkillSlot.Q;
    public float cooldownSeconds = 5f;
    public float durationSeconds = 0f;
    public bool isPassive = false;
    [TextArea(2, 6)]
    public string description;
    public string inputHint = "Q";
}
