using UnityEngine;

public enum HeroRole
{
    PayloadControl,
    SpaceControl,
    AreaSupport,
    CrowdControl
}

[CreateAssetMenu(fileName = "Hero", menuName = "FPS Game/Heroes/Hero Definition")]
public class HeroDefinition : ScriptableObject
{
    public string heroId;
    public string displayName;
    public HeroRole role;
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.5f;
    [TextArea(2, 4)]
    public string tagline;
}
