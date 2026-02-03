public interface IHeroHoldSkill
{
    bool CanHold(HeroRuntime runtime);
    void BeginHold(HeroRuntime runtime);
    void UpdateHold(HeroRuntime runtime);
    void EndHold(HeroRuntime runtime);
}
