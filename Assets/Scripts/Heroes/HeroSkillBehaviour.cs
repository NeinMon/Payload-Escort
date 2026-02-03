using UnityEngine;

public abstract class HeroSkillBehaviour : MonoBehaviour
{
    [SerializeField] private HeroSkillSlot slot = HeroSkillSlot.Q;

    public HeroSkillSlot Slot => slot;

    public virtual void Initialize(HeroRuntime runtime) { }

    public virtual void Activate(HeroRuntime runtime) { }

    public virtual void Deactivate(HeroRuntime runtime) { }
}
