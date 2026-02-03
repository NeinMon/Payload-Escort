using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class HeroSkillHUD : MonoBehaviourPun
{
    [System.Serializable]
    public class SkillSlotUI
    {
        public HeroSkillSlot slot;
        public GameObject root;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI cooldownText;
        public Image icon;
        public Image cooldownOverlay;
        public Color readyColor = Color.white;
        public Color cooldownColor = new Color(1f, 1f, 1f, 0.4f);
        public Color usedFlashColor = new Color(1f, 0.9f, 0.4f, 1f);
        public float usedFlashDuration = 0.15f;
    }

    [SerializeField] private SkillSlotUI[] slots;
    [SerializeField] private HeroRuntime heroRuntime;

    private readonly System.Collections.Generic.Dictionary<HeroSkillSlot, Coroutine> flashBySlot = new System.Collections.Generic.Dictionary<HeroSkillSlot, Coroutine>();

    void Start()
    {
        if (!photonView.IsMine)
        {
            gameObject.SetActive(false);
            return;
        }

        if (heroRuntime == null)
            heroRuntime = GetComponentInParent<HeroRuntime>();

        EnsureSlotReferences();

        if (heroRuntime != null)
        {
            heroRuntime.HeroChanged += RefreshStaticTexts;
            heroRuntime.SkillActivated += OnSkillActivated;
            heroRuntime.SkillCooldownReady += OnSkillReady;
        }

        RefreshStaticTexts();
    }

    void OnDestroy()
    {
        if (heroRuntime != null)
        {
            heroRuntime.HeroChanged -= RefreshStaticTexts;
            heroRuntime.SkillActivated -= OnSkillActivated;
            heroRuntime.SkillCooldownReady -= OnSkillReady;
        }
    }

    void Update()
    {
        if (!photonView.IsMine || heroRuntime == null) return;
        UpdateCooldowns();
    }

    public void RefreshStaticTexts()
    {
        EnsureSlotReferences();
        DisableLegacySlotX();
        if (heroRuntime == null || slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            SkillSlotUI slot = slots[i];
            if (slot == null) continue;

            HeroSkillDefinition def = heroRuntime.GetSkill(slot.slot);
            bool usingFallbackF = false;
            if (def == null && slot.slot == HeroSkillSlot.Q)
            {
                def = heroRuntime.GetSkill(HeroSkillSlot.F);
                usingFallbackF = def != null;
            }
            if (slot.root != null)
                slot.root.SetActive(def != null);

            if (def != null && slot.nameText != null)
            {
                string hint = slot.slot == HeroSkillSlot.R ? "X" :
                    (!string.IsNullOrEmpty(def.inputHint) ? def.inputHint : (usingFallbackF ? "F" : slot.slot.ToString()));
                slot.nameText.text = hint;
            }

            if (def != null && slot.cooldownText != null)
                slot.cooldownText.text = string.Empty;
        }
    }

    public void SetSlotIcon(HeroSkillSlot slotType, Sprite sprite)
    {
        EnsureSlotReferences();
        if (slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            SkillSlotUI slot = slots[i];
            if (slot == null || slot.slot != slotType || slot.icon == null) continue;

            slot.icon.sprite = sprite;
            slot.icon.enabled = sprite != null;
            if (sprite != null)
                slot.icon.color = slot.readyColor.a <= 0.01f ? Color.white : slot.readyColor;
        }
    }

    private void EnsureSlotReferences()
    {
        DisableLegacySlotX();
        if (slots == null || slots.Length == 0)
        {
            slots = new SkillSlotUI[3];
            slots[0] = FindSlotUI(HeroSkillSlot.Q, "Slot_Q");
            slots[1] = FindSlotUI(HeroSkillSlot.E, "Slot_E");
            slots[2] = FindSlotUI(HeroSkillSlot.R, "Slot_R");
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            SkillSlotUI slot = slots[i];
            if (slot == null || slot.root != null) continue;

            string slotName = slot.slot == HeroSkillSlot.Q ? "Slot_Q" :
                slot.slot == HeroSkillSlot.E ? "Slot_E" :
                slot.slot == HeroSkillSlot.R ? "Slot_R" : null;

            if (!string.IsNullOrEmpty(slotName))
            {
                SkillSlotUI found = FindSlotUI(slot.slot, slotName);
                if (found != null)
                    slots[i] = found;
            }
        }
    }

    private SkillSlotUI FindSlotUI(HeroSkillSlot slotType, string slotName)
    {
        Transform slotRoot = transform.Find($"HeroHUDRoot/SkillBar/{slotName}") ??
                             transform.Find($"SkillBar/{slotName}") ??
                             transform.Find(slotName);

        if (slotRoot == null) return new SkillSlotUI { slot = slotType };

        SkillSlotUI slot = new SkillSlotUI
        {
            slot = slotType,
            root = slotRoot.gameObject,
            icon = FindImage(slotRoot, "Icon"),
            cooldownOverlay = FindImage(slotRoot, "CooldownOverlay"),
            nameText = FindText(slotRoot, "Name"),
            cooldownText = FindText(slotRoot, "Cooldown")
        };

        if (slot.icon != null && slot.icon.color.a <= 0.01f)
            slot.icon.color = slot.readyColor.a <= 0.01f ? Color.white : slot.readyColor;

        return slot;
    }

    private void DisableLegacySlotX()
    {
        Transform slotX = transform.Find("HeroHUDRoot/SkillBar/Slot_X") ??
                          transform.Find("SkillBar/Slot_X") ??
                          transform.Find("Slot_X");

        if (slotX != null && slotX.gameObject.activeSelf)
            slotX.gameObject.SetActive(false);
    }

    private Image FindImage(Transform root, string name)
    {
        Transform child = root.Find(name);
        return child != null ? child.GetComponent<Image>() : null;
    }

    private TextMeshProUGUI FindText(Transform root, string name)
    {
        Transform child = root.Find(name);
        return child != null ? child.GetComponent<TextMeshProUGUI>() : null;
    }

    void UpdateCooldowns()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            SkillSlotUI slot = slots[i];
            if (slot == null) continue;

            HeroSkillDefinition def = heroRuntime.GetSkill(slot.slot);
            if (def == null && slot.slot == HeroSkillSlot.Q)
                def = heroRuntime.GetSkill(HeroSkillSlot.F);
            if (def == null)
            {
                if (slot.cooldownOverlay != null)
                    slot.cooldownOverlay.enabled = false;
                continue;
            }

            float remaining = heroRuntime.GetCooldownRemaining(slot.slot);
            bool onCooldown = remaining > 0.01f;

            if (slot.cooldownText != null)
                slot.cooldownText.text = onCooldown ? remaining.ToString("0.0") : string.Empty;

            if (slot.icon != null)
            {
                slot.icon.enabled = slot.icon.sprite != null;
                if (slot.icon.sprite != null)
                {
                    Color baseColor = slot.readyColor.a <= 0.01f ? Color.white : slot.readyColor;
                    if (onCooldown)
                        slot.icon.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.5f);
                    else
                        slot.icon.color = baseColor;
                }
            }

            if (slot.cooldownOverlay != null)
            {
                slot.cooldownOverlay.enabled = onCooldown;
                float duration = def.cooldownSeconds > 0f ? def.cooldownSeconds : 1f;
                slot.cooldownOverlay.fillAmount = onCooldown ? Mathf.Clamp01(remaining / duration) : 0f;
                if (!onCooldown)
                {
                    Color c = slot.cooldownOverlay.color;
                    c.a = 0f;
                    slot.cooldownOverlay.color = c;
                }
            }
        }
    }

    private void OnSkillActivated(HeroSkillDefinition def)
    {
        if (def == null || slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            SkillSlotUI slot = slots[i];
            if (slot == null || slot.slot != def.slot || slot.icon == null) continue;

            if (flashBySlot.TryGetValue(slot.slot, out Coroutine running) && running != null)
            {
                StopCoroutine(running);
            }

            flashBySlot[slot.slot] = StartCoroutine(FlashIcon(slot));
            break;
        }
    }

    private System.Collections.IEnumerator FlashIcon(SkillSlotUI slot)
    {
        if (slot == null || slot.icon == null) yield break;
        Color original = slot.icon.color;
        slot.icon.color = slot.usedFlashColor;
        yield return new WaitForSeconds(slot.usedFlashDuration);
        slot.icon.color = original;
    }

    private void OnSkillReady(HeroSkillDefinition def)
    {
        if (def == null || slots == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            SkillSlotUI slot = slots[i];
            if (slot == null || slot.slot != def.slot || slot.icon == null) continue;

            if (flashBySlot.TryGetValue(slot.slot, out Coroutine running) && running != null)
            {
                StopCoroutine(running);
            }

            flashBySlot[slot.slot] = StartCoroutine(PulseReady(slot));
            break;
        }
    }

    private System.Collections.IEnumerator PulseReady(SkillSlotUI slot)
    {
        if (slot == null || slot.icon == null) yield break;

        Color baseColor = slot.readyColor;
        float duration = 0.35f;
        float t = 0f;
        while (t < duration)
        {
            float lerp = Mathf.PingPong(t * 8f, 1f);
            slot.icon.color = Color.Lerp(baseColor, slot.usedFlashColor, lerp);
            t += Time.deltaTime;
            yield return null;
        }

        slot.icon.color = baseColor;
    }
}
