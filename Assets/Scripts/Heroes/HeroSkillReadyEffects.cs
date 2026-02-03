using UnityEngine;
using Photon.Pun;

public class HeroSkillReadyEffects : MonoBehaviour
{
    [System.Serializable]
    public class ReadyEffect
    {
        public PlayerRole role = PlayerRole.None;
        public HeroSkillSlot slot;
        public ParticleSystem particle;
        public AudioSource audioSource;
        public AudioClip clip;
    }

    [SerializeField] private HeroRuntime heroRuntime;
    [SerializeField] private ReadyEffect[] effects;
    [SerializeField] private PlayerRole defaultRole = PlayerRole.Engineer;

    private PhotonView photonView;

    void Awake()
    {
        if (heroRuntime == null)
            heroRuntime = GetComponent<HeroRuntime>();

        photonView = GetComponent<PhotonView>();
    }

    void OnEnable()
    {
        if (heroRuntime != null)
            heroRuntime.SkillCooldownReady += OnSkillReady;
    }

    void OnDisable()
    {
        if (heroRuntime != null)
            heroRuntime.SkillCooldownReady -= OnSkillReady;
    }

    private void OnSkillReady(HeroSkillDefinition def)
    {
        if (def == null || effects == null) return;

        if (photonView != null && !photonView.IsMine)
            return;

        PlayerRole role = defaultRole;
        if (PhotonNetwork.InRoom && PlayerRoleUtils.TryGetPlayerRole(PhotonNetwork.LocalPlayer, out PlayerRole r))
            role = r;

        for (int i = 0; i < effects.Length; i++)
        {
            ReadyEffect effect = effects[i];
            if (effect == null || effect.slot != def.slot) continue;
            if (effect.role != PlayerRole.None && effect.role != role) continue;

            if (effect.particle != null)
                effect.particle.Play();

            if (effect.audioSource != null)
            {
                if (effect.clip != null)
                    effect.audioSource.PlayOneShot(effect.clip);
                else
                    effect.audioSource.Play();
            }

            break;
        }
    }
}
