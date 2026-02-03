using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroRuntime : MonoBehaviour
{
    [Header("Hero Data")]
    [SerializeField] private HeroDefinition heroDefinition;
    [SerializeField] private HeroSkillDefinition[] skillDefinitions;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerControllerNetwork playerController;

    public event Action<HeroSkillDefinition> SkillActivated;
    public event Action HeroChanged;
    public event Action<HeroSkillDefinition> SkillCooldownReady;

    private readonly Dictionary<HeroSkillSlot, HeroSkillDefinition> skillBySlot = new Dictionary<HeroSkillSlot, HeroSkillDefinition>();
    private readonly Dictionary<HeroSkillSlot, float> lastActivatedTime = new Dictionary<HeroSkillSlot, float>();
    private readonly Dictionary<HeroSkillSlot, HeroSkillBehaviour> behaviourBySlot = new Dictionary<HeroSkillSlot, HeroSkillBehaviour>();
    private readonly Dictionary<HeroSkillSlot, bool> wasOnCooldown = new Dictionary<HeroSkillSlot, bool>();

    void Awake()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (playerController == null)
            playerController = GetComponent<PlayerControllerNetwork>();

        InitializeSkills();
    }

    void Start()
    {
        ApplyHeroStats();
        InitializeBehaviours();
        HeroChanged?.Invoke();
    }

    void Update()
    {
        CheckCooldownTransitions();
    }

    public void SetHero(HeroDefinition definition, HeroSkillDefinition[] skills)
    {
        heroDefinition = definition;
        skillDefinitions = skills;
        InitializeSkills();
        ApplyHeroStats();
        InitializeBehaviours();
        HeroChanged?.Invoke();
    }

    public HeroDefinition GetHeroDefinition() => heroDefinition;

    public HeroSkillDefinition GetSkill(HeroSkillSlot slot)
    {
        return skillBySlot.TryGetValue(slot, out var def) ? def : null;
    }

    public HeroSkillBehaviour GetBehaviour(HeroSkillSlot slot)
    {
        return behaviourBySlot.TryGetValue(slot, out var behaviour) ? behaviour : null;
    }

    public float GetCooldownRemaining(HeroSkillSlot slot)
    {
        if (!skillBySlot.TryGetValue(slot, out var def))
            return 0f;

        if (!lastActivatedTime.TryGetValue(slot, out var lastTime))
            return 0f;

        float remaining = def.cooldownSeconds - (Time.time - lastTime);
        return Mathf.Max(0f, remaining);
    }

    public bool TryActivateSkill(HeroSkillSlot slot)
    {
        if (!skillBySlot.TryGetValue(slot, out var def) || def == null)
            return false;

        if (def.isPassive)
            return false;

        if (GetCooldownRemaining(slot) > 0f)
            return false;

        lastActivatedTime[slot] = Time.time;
        SkillActivated?.Invoke(def);

        if (behaviourBySlot.TryGetValue(slot, out var behaviour) && behaviour != null)
        {
            behaviour.Activate(this);
            if (def.durationSeconds > 0f)
            {
                StartCoroutine(DeactivateAfterDuration(behaviour, def.durationSeconds));
            }
        }

        return true;
    }

    public void StartCooldown(HeroSkillSlot slot)
    {
        if (!skillBySlot.TryGetValue(slot, out var def) || def == null)
            return;

        lastActivatedTime[slot] = Time.time;
        wasOnCooldown[slot] = true;
    }

    public void NotifySkillActivated(HeroSkillSlot slot)
    {
        if (!skillBySlot.TryGetValue(slot, out var def) || def == null)
            return;

        SkillActivated?.Invoke(def);
    }

    private IEnumerator DeactivateAfterDuration(HeroSkillBehaviour behaviour, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (behaviour != null)
            behaviour.Deactivate(this);
    }

    private void InitializeSkills()
    {
        skillBySlot.Clear();
        lastActivatedTime.Clear();
        wasOnCooldown.Clear();

        if (skillDefinitions == null)
            return;

        for (int i = 0; i < skillDefinitions.Length; i++)
        {
            var def = skillDefinitions[i];
            if (def == null)
                continue;

            skillBySlot[def.slot] = def;
            if (!lastActivatedTime.ContainsKey(def.slot))
                lastActivatedTime.Add(def.slot, -999f);

            if (!wasOnCooldown.ContainsKey(def.slot))
                wasOnCooldown.Add(def.slot, false);
        }
    }

    private void CheckCooldownTransitions()
    {
        if (skillBySlot.Count == 0) return;

        foreach (var kvp in skillBySlot)
        {
            HeroSkillSlot slot = kvp.Key;
            HeroSkillDefinition def = kvp.Value;
            if (def == null || def.isPassive) continue;

            float remaining = GetCooldownRemaining(slot);
            bool onCooldown = remaining > 0.01f;

            if (wasOnCooldown.TryGetValue(slot, out bool prevOnCooldown))
            {
                if (prevOnCooldown && !onCooldown)
                {
                    SkillCooldownReady?.Invoke(def);
                }
                wasOnCooldown[slot] = onCooldown;
            }
            else
            {
                wasOnCooldown[slot] = onCooldown;
            }
        }
    }

    private void InitializeBehaviours()
    {
        behaviourBySlot.Clear();
        HeroSkillBehaviour[] behaviours = GetComponentsInChildren<HeroSkillBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            var behaviour = behaviours[i];
            if (behaviour == null)
                continue;

            behaviour.Initialize(this);
            behaviourBySlot[behaviour.Slot] = behaviour;
        }
    }

    private void ApplyHeroStats()
    {
        if (heroDefinition == null)
            return;

        if (playerHealth != null)
        {
            playerHealth.SetMaxHealth(heroDefinition.maxHealth, true);
        }

        if (playerController != null)
        {
            playerController.moveSpeed = heroDefinition.moveSpeed;
            playerController.sprintMultiplier = heroDefinition.sprintMultiplier;
        }
    }
}
