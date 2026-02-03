using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class HeroHUDIcons : MonoBehaviourPun
{
    [SerializeField] private HeroUIIconSet iconSet;
    [SerializeField] private HeroSkillHUD skillHud;
    [SerializeField] private Image heroPortrait;
    [SerializeField] private Image healthFill;
    [SerializeField] private Image healthBackground;
    [SerializeField] private PlayerRole defaultRole = PlayerRole.Engineer;

    private HeroRuntime heroRuntime;

    void Start()
    {
        if (!photonView.IsMine)
        {
            gameObject.SetActive(false);
            return;
        }

        if (iconSet == null)
            iconSet = Resources.Load<HeroUIIconSet>("Heroes/UI/HeroUIIconSet");

        if (skillHud == null)
            skillHud = GetComponent<HeroSkillHUD>();

        heroRuntime = GetComponentInParent<HeroRuntime>();
        if (heroRuntime != null)
            heroRuntime.HeroChanged += ApplyIcons;

        ApplyIcons();
        StartCoroutine(ApplyIconsNextFrame());
    }

    void OnDestroy()
    {
        if (heroRuntime != null)
            heroRuntime.HeroChanged -= ApplyIcons;
    }

    private System.Collections.IEnumerator ApplyIconsNextFrame()
    {
        yield return null;
        ApplyIcons();
    }

    public void ApplyIcons()
    {
        if (iconSet == null) return;

        PlayerRole role = defaultRole;
        if (PhotonNetwork.InRoom && PlayerRoleUtils.TryGetPlayerRole(PhotonNetwork.LocalPlayer, out PlayerRole r))
            role = r;

        if (!iconSet.TryGetRoleIcons(role, out HeroUIIconSet.RoleIcons icons) || icons == null)
            return;

        if (heroPortrait != null)
            heroPortrait.sprite = icons.heroIcon;

        if (healthFill != null && iconSet.healthBarFill != null)
            healthFill.sprite = iconSet.healthBarFill;

        if (healthBackground != null && iconSet.healthBarBackground != null)
            healthBackground.sprite = iconSet.healthBarBackground;

        if (skillHud != null)
        {
            skillHud.SetSlotIcon(HeroSkillSlot.Q, icons.skillQ);
            skillHud.SetSlotIcon(HeroSkillSlot.E, icons.skillE);
            skillHud.SetSlotIcon(HeroSkillSlot.R, icons.skillR);
        }

        if (icons.skillE == null || icons.skillR == null)
            Debug.LogWarning($"[HeroHUDIcons] Missing skill icons for role {role}. Check HeroUIIconSet.");
    }
}
